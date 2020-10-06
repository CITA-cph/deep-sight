#include "Grid.h"

namespace DeepSight
{

	Grid::Grid()
	{
		openvdb::initialize();
	}

	Grid::~Grid()
	{

	}

	bool Grid::has_suffix(const std::string &str, const std::string &suffix)
	{
	    return str.size() >= suffix.size() &&
	           str.compare(str.size() - suffix.size(), suffix.size(), suffix) == 0;
	}

	Grid::Ptr Grid::from_multipage_tiff(const std::string path, double threshold)
	{
		bool verbose = false;

		if (verbose)
		{
			std::cout << "Opening multi-page TIFF '" << path << "'" << std::endl;
			std::cout << "Threshold: " << threshold << std::endl;
		}

	    using ValueT = typename openvdb_grid::ValueType;

        openvdb_grid::Ptr grid = openvdb_grid::create(/*background value=*/0.0);

	    openvdb_grid::Accessor accessor = (*grid).getAccessor();

	    openvdb::Coord ijk;
	    int& i = ijk[0], & j = ijk[1], & k = ijk[2];

	    TIFF* tif = TIFFOpen(path.c_str(), "r");

	    if (tif) {
            unsigned int width, height, samplesperpixel, bitspersample;
            ValueT max_val = 0.0;

	        do {
	            uint32* raster;

	            // get the size of the tiff
	            TIFFGetField(tif, TIFFTAG_IMAGEWIDTH, &width);
	            TIFFGetField(tif, TIFFTAG_IMAGELENGTH, &height);
	            TIFFGetField(tif, TIFFTAG_SAMPLESPERPIXEL, &samplesperpixel);
	            TIFFGetField(tif, TIFFTAG_BITSPERSAMPLE, &bitspersample);

	            if (verbose)
	            {
		            std::cout << "frame " << k << std::endl;
		            std::cout << "    width: " << width << std::endl;
		            std::cout << "    height: " << height << std::endl;
		            std::cout << "    samplesperpixel: " << samplesperpixel << std::endl;
		            std::cout << "    bitspersample: " << bitspersample << std::endl;
	            }

	            unsigned int npixels = width * height; // get the total number of pixels

	            raster = (uint32*)_TIFFmalloc(npixels * sizeof(uint32)); // allocate temp memory (must use the tiff library malloc)
	            if (raster == NULL) // check the raster's memory was allocaed
	            {
	                TIFFClose(tif);
	                std::cerr << "Could not allocate memory for raster of TIFF image" << std::endl;
	    			return Grid::Ptr(nullptr);
	            }

	            // Check the tif read to the raster correctly
	            if (!TIFFReadRGBAImage(tif, width, height, raster, 0))
	            {
	                TIFFClose(tif);
	                std::cerr << "Could not read raster of TIFF image" << std::endl;
	    			return Grid::Ptr(nullptr);
	            }

	            // itterate through all the pixels of the tif
	            for (i = 0; (unsigned int)i < width; i++)
	                for (j = 0; (unsigned int)j < height; j++)
	                {
	                    uint32& TiffPixel = raster[j * width + i]; // read the current pixel of the TIF

	                    ValueT val = ValueT(((float)(TIFFGetR(TiffPixel) + TIFFGetG(TiffPixel) + TIFFGetB(TiffPixel))) / (255. * 3));
	                    max_val = std::max(max_val, val);

	                    if (val < threshold) 
	                    	continue;

                        accessor.setValue(ijk, val);

	                }

	            _TIFFfree(raster); // release temp memory

	        	k ++;

	        } while (TIFFReadDirectory(tif)); // get the next tif
	        TIFFClose(tif); // close the tif file

	        std::cout << "Loaded " << k << " pages (" << width << " , " << height << ")" << std::endl;
	        std::cout << "Max value found: " << max_val << std::endl;

            grid->setGridClass(openvdb::GRID_FOG_VOLUME);
    		grid->setName("tiff");

    		auto ds_grid = std::make_shared<Grid>();
    		ds_grid->m_grid = grid;

    		return ds_grid;
	    }
	    else
	    {
	    	std::cerr << "Failed to load multi-page TIFF" << std::endl;
	    	return Grid::Ptr(nullptr);
	    }
	}

	std::vector<Grid::Ptr> Grid::from_vdb(const std::string path)
	{
		
    	openvdb::io::File file(path);
    	file.open();

    	if (!file.isOpen())
	    	return std::vector<Grid::Ptr>();

	    openvdb::GridBase::Ptr baseGrid;

	    std::vector<Grid::Ptr> grids;

	    for (openvdb::io::File::NameIterator nameIter = file.beginName();
	        nameIter != file.endName(); ++nameIter)
	    {
	        baseGrid = file.readGrid(nameIter.gridName());

	        Ptr ds_grid = std::make_shared<Grid>();
	        ds_grid->m_grid = openvdb::gridPtrCast<openvdb_grid>(baseGrid);
	        ds_grid->name = nameIter.gridName();

	        grids.push_back(ds_grid);
	    }

	    file.close();

	    return grids;
	}


	Grid::Ptr Grid::read(const std::string path)
	{
		openvdb::initialize();

	    if (has_suffix(path, "vdb"))
	    	return Grid::from_vdb(path)[0];
	    else if (has_suffix(path, "tif") || has_suffix(path, "tiff") || has_suffix(path, "TIF") || has_suffix(path, "TIFF"))
	    	return Grid::from_multipage_tiff(path);

	    return Grid::Ptr(nullptr);
	}


	void Grid::write(const std::string path)
	{
    	openvdb::io::File file(path);
    	openvdb::GridPtrVec grids_out;
    	grids_out.push_back(m_grid);

	    // Add the grid pointer to a container.
	    // openvdb::GridPtrVec grids_out;
	    // std::map<std::string, openvdb::FloatGrid::Ptr>::iterator it;
	    // for (it = grids.begin(); it != grids.end(); ++it)
	    // 	grids_out.push_back(it->second);

	    // Write out the contents of the container.
	    file.write(grids_out);
	    file.close();
	}

	float Grid::getValue(Eigen::Vector3i xyz)
	{
		typename openvdb_grid::Accessor accessor = m_grid->getAccessor();
		return accessor.getValue(openvdb::math::Coord(
			xyz.x(), 
			xyz.y(), 
			xyz.z() 
			));
	}

	std::vector<float> Grid::getDense(Eigen::Vector3i min, Eigen::Vector3i max)
	{
		openvdb::tools::Dense<float, openvdb::tools::MemoryLayout::LayoutZYX> dense(openvdb::CoordBBox(
		//openvdb::tools::Dense<float> dense(openvdb::CoordBBox(
        openvdb::Coord(min.x(), min.y(), min.z()), 
        openvdb::Coord(max.x(), max.y(), max.z()))
        );

		openvdb::tools::copyToDense(*(m_grid), dense, false);
		//return dense.data();

	    std::vector<float> data;
	    data.assign(dense.data(), dense.data() + dense.valueCount());

	    return data;
	}


	float Grid::getInterpolatedValue(Eigen::Vector3f xyz)
	{
		typename openvdb_grid::Accessor accessor = m_grid->getAccessor();
		return openvdb::tools::BoxSampler::sample(
			accessor, 
			openvdb::Vec3R(
				xyz.x(), 
				xyz.y(), 
				xyz.z() 
			));
	}

	std::vector<float>  Grid::getValues(std::vector<Eigen::Vector3i> &xyz)
	{
		std::vector<float> values;
		typename openvdb_grid::Accessor accessor = m_grid->getAccessor();

		for (auto iter = xyz.begin(); 
			iter != xyz.end();
			iter++)
		{
			values.push_back(
				accessor.getValue(
					openvdb::math::Coord(
						(int)(*iter).x(), 
						(int)(*iter).y(), 
						(int)(*iter).z() 
					)
				)
			);

		}

		return values;
	}	

	std::vector<float> Grid::getInterpolatedValues(std::vector<Eigen::Vector3f> &xyz)
	{
		std::vector<float> values;
		typename openvdb_grid::Accessor accessor = m_grid->getAccessor();

		for (auto iter = xyz.begin(); 
			iter != xyz.end();
			iter++)
		{
			values.push_back(
				openvdb::tools::BoxSampler::sample(
				accessor, 
				openvdb::Vec3R(
					(*iter).x(), 
					(*iter).y(), 
					(*iter).z() 
				)));

		}

		return values;
	}

	std::tuple<Eigen::Vector3i, Eigen::Vector3i> Grid::getBoundingBox()
	{
		openvdb::math::CoordBBox bb = m_grid->evalActiveVoxelBoundingBox();
		Eigen::Vector3i bbmin;
		bbmin << bb.min().x(), bb.min().y(), bb.min().z();
		Eigen::Vector3i bbmax;
		bbmax << bb.max().x(), bb.max().y(),bb.max().z();

		return std::tuple<Eigen::Vector3i, Eigen::Vector3i>(bbmin, bbmax);
	}

	Eigen::Matrix4d Grid::transform()
	{
		auto mat = m_grid->transform().baseMap()->getAffineMap()->getMat4();

		return Eigen::Matrix4d(mat.asPointer());
	}

	void Grid::transform_grid(Eigen::Matrix4d xform)
	{
		openvdb_grid outGrid;
		openvdb::Mat4R mat(xform.data());
		openvdb::tools::GridTransformer transformer(mat);
		transformer.transformGrid<openvdb::tools::BoxSampler>(*m_grid, outGrid);

		m_grid = std::make_shared<openvdb_grid>(outGrid);
	}


}
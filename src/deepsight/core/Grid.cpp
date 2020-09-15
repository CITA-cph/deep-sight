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

	void Grid::read(std::string path)
	{
    	// openvdb::initialize();
		
    	openvdb::io::File file(path);
    	file.open();

    	if (!file.isOpen())
    		return;

	    openvdb::GridBase::Ptr baseGrid;

	    for (openvdb::io::File::NameIterator nameIter = file.beginName();
	        nameIter != file.endName(); ++nameIter)
	    {
	        baseGrid = file.readGrid(nameIter.gridName());
	        grids[nameIter.gridName()] = openvdb::gridPtrCast<openvdb::FloatGrid>(baseGrid);
	    }
	    file.close();
	}

	void Grid::write(std::string grid_name, std::string path)
	{
    	openvdb::io::File file(path);

	    // Add the grid pointer to a container.
	    openvdb::GridPtrVec grids_out;
	    std::map<std::string, openvdb::FloatGrid::Ptr>::iterator it;
	    for (it = grids.begin(); it != grids.end(); ++it)
	    	grids_out.push_back(it->second);

	    //for (int i = 0; i < grids.size(); ++i)
	    //	grids_out.push_back(grids[i]);
	    // Write out the contents of the container.
	    file.write(grids_out);
	    file.close();
	}


	void Grid::from_multipage_tiff(std::string path, std::string id, double threshold)
	{
		bool verbose = false;

		if (verbose)
		{
			std::cout << "Opening multi-page TIFF '" << path << "'" << std::endl;
			std::cout << "Threshold: " << threshold << std::endl;
		}

	    using ValueT = typename openvdb::FloatGrid::ValueType;

        openvdb::FloatGrid::Ptr grid = openvdb::FloatGrid::create(/*background value=*/0.0);

	    openvdb::FloatGrid::Accessor accessor = (*grid).getAccessor();

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
	                return;
	            }

	            // Check the tif read to the raster correctly
	            if (!TIFFReadRGBAImage(tif, width, height, raster, 0))
	            {
	                TIFFClose(tif);
	                std::cerr << "Could not read raster of TIFF image" << std::endl;
	                return;
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

            //grid->setGridClass(openvdb::GRID_LEVEL_SET);
            grid->setGridClass(openvdb::GRID_FOG_VOLUME);
    		grid->setName(id);


    	    grids[id] = grid;
	    }
	    else
	    {
	    	std::cerr << "Failed to load multi-page TIFF" << std::endl;
	    }
	}

	std::vector<std::string> Grid::grid_names()
	{
		std::vector<std::string> names;
	    for ( auto item : grids )
	    {
	    	names.push_back(item.first);
	    }
	    return names;
	}

	float Grid::getValue(std::string grid_name, Eigen::Vector3i xyz)
	{
		typename openvdb::FloatGrid::Accessor accessor = grids[grid_name]->getAccessor();
		return accessor.getValue(openvdb::math::Coord(
			xyz.x(), 
			xyz.y(), 
			xyz.z() 
			));
	}

	std::vector<float> Grid::getDense(std::string grid_name, Eigen::Vector3i min, Eigen::Vector3i max)
	{
		openvdb::tools::Dense<float, openvdb::tools::MemoryLayout::LayoutZYX> dense(openvdb::CoordBBox(
		//openvdb::tools::Dense<float> dense(openvdb::CoordBBox(
        openvdb::Coord(min.x(), min.y(), min.z()), 
        openvdb::Coord(max.x(), max.y(), max.z()))
        );

		openvdb::tools::copyToDense(*(grids[grid_name]), dense, false);
		//return dense.data();

	    std::vector<float> data;
	    data.assign(dense.data(), dense.data() + dense.valueCount());

	    return data;
	}


	float Grid::getInterpolatedValue(std::string grid_name, Eigen::Vector3f xyz)
	{
		typename openvdb::FloatGrid::Accessor accessor = grids[grid_name]->getAccessor();
		return openvdb::tools::BoxSampler::sample(
			accessor, 
			openvdb::Vec3R(
				xyz.x(), 
				xyz.y(), 
				xyz.z() 
			));
	}

	std::vector<float>  Grid::getValues(std::string grid_name, std::vector<Eigen::Vector3i> &xyz)
	{
		std::vector<float> values;
		size_t i = 0;
		typename openvdb::FloatGrid::Accessor accessor = grids[grid_name]->getAccessor();

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

	std::vector<float> Grid::getInterpolatedValues(std::string grid_name, std::vector<Eigen::Vector3f> &xyz)
	{
		std::vector<float> values;
		size_t i = 0;
		typename openvdb::FloatGrid::Accessor accessor = grids[grid_name]->getAccessor();

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

	std::tuple<Eigen::Vector3i, Eigen::Vector3i> Grid::getBoundingBox(std::string grid_name)
	{
		openvdb::math::CoordBBox bb = grids[grid_name]->evalActiveVoxelBoundingBox();
		Eigen::Vector3i bbmin;
		bbmin << bb.min().x(), bb.min().y(), bb.min().z();
		Eigen::Vector3i bbmax;
		bbmax << bb.max().x(), bb.max().y(),bb.max().z();

		return std::tuple<Eigen::Vector3i, Eigen::Vector3i>(bbmin, bbmax);
	}




}
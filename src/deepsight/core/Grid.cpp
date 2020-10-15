#include "Grid.h"

namespace DeepSight
{

	template<typename T>
	Grid<T>::Grid()
	{
		openvdb::initialize();
	}

	template<typename T>
	Grid<T>::~Grid()
	{

	}

	template<typename T>
	bool Grid<T>::has_suffix(const std::string &str, const std::string &suffix)
	{
	    return str.size() >= suffix.size() &&
	           str.compare(str.size() - suffix.size(), suffix.size(), suffix) == 0;
	}

	template<typename T>
	std::shared_ptr<Grid<T>> Grid<T>::from_multipage_tiff(const std::string path, double threshold)
	{
		bool verbose = false;

		if (verbose)
		{
			std::cout << "Opening multi-page TIFF '" << path << "'" << std::endl;
			std::cout << "Threshold: " << threshold << std::endl;
		}

	    using ValueT = typename openvdb_grid::ValueType;

        GridT::Ptr grid = GridT::create(/*background value=*/0.0);

	    GridT::Accessor accessor = (*grid).getAccessor();

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
	    			return std::shared_ptr<Grid<T>>(nullptr);
	            }

	            // Check the tif read to the raster correctly
	            if (!TIFFReadRGBAImage(tif, width, height, raster, 0))
	            {
	                TIFFClose(tif);
	                std::cerr << "Could not read raster of TIFF image" << std::endl;
	    			return std::shared_ptr<Grid<T>>(nullptr);
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

    		auto ds_grid = std::make_shared<Grid<T>>();
    		ds_grid->m_grid = grid;

    		return ds_grid;
	    }
	    else
	    {
	    	std::cerr << "Failed to load multi-page TIFF" << std::endl;
	    	return std::shared_ptr<Grid<T>>(nullptr);
	    }
	}

	template <typename T>
	std::shared_ptr<Grid<T>> Grid<T>::from_many_tiffs(std::vector<std::string> paths, double threshold)
	{	

		bool verbose = false;

	    using ValueT = typename openvdb_grid::ValueType;

        GridT::Ptr grid = openvdb_grid::create(/*background value=*/0.0);

	    GridT::Accessor accessor = (*grid).getAccessor();

	    openvdb::Coord ijk;
	    int& i = ijk[0], & j = ijk[1], & k = ijk[2];

	    // ======== Compile list of TIFFs to open ========
	    /*
	    string directory;

		const size_t last_slash_idx = filename.rfind('\\/');
		if (std::string::npos != last_slash_idx)
		{
		    directory = filename.substr(0, last_slash_idx);
		}

	    std::vector<std::string> tiff_paths;
    	for (const auto & entry : std::filesystem::directory_iterator(directory))
    	{

    		tiff_paths.push_back(entry.path());
    	}
    	*/

    	for (int f = 0; f < paths.size(); ++f)
    	{

		    TIFF* tif = TIFFOpen(paths[f].c_str(), "r");

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

		            unsigned int npixels = width * height; // get the total number of pixels

		            raster = (uint32*)_TIFFmalloc(npixels * sizeof(uint32)); // allocate temp memory (must use the tiff library malloc)
		            if (raster == NULL) // check the raster's memory was allocaed
		            {
		                TIFFClose(tif);
		                std::cerr << "Could not allocate memory for raster of TIFF image" << std::endl;
		    			return std::shared_ptr<Grid<T>>(nullptr);
		            }

		            // Check the tif read to the raster correctly
		            if (!TIFFReadRGBAImage(tif, width, height, raster, 0))
		            {
		                TIFFClose(tif);
		                std::cerr << "Could not read raster of TIFF image" << std::endl;
		    			return std::shared_ptr<Grid<T>>(nullptr);
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
		    }
		    else
		    {
		    	std::cerr << "Failed to load TIFF" << std::endl;
		    	continue;
		    }
		}

        grid->setGridClass(openvdb::GRID_FOG_VOLUME);
		grid->setName("tiff");

		auto ds_grid = std::make_shared<Grid<T>>();
		ds_grid->m_grid = grid;

		return ds_grid;

	}


	template <typename T>
	std::vector<std::shared_ptr<Grid<T>>> Grid<T>::from_vdb(const std::string path)
	{
		
    	openvdb::io::File file(path);
    	file.open();

    	if (!file.isOpen())
	    	return std::vector<std::shared_ptr<Grid<T>>>();

	    openvdb::GridBase::Ptr baseGrid;

	    std::vector<std::shared_ptr<Grid<T>>> grids;

	    for (openvdb::io::File::NameIterator nameIter = file.beginName();
	        nameIter != file.endName(); ++nameIter)
	    {
	        baseGrid = file.readGrid(nameIter.gridName());

	        std::shared_ptr<Grid<T>> ds_grid = std::make_shared<Grid<T>>();
	        ds_grid->m_grid = openvdb::gridPtrCast<GridT>(baseGrid);
	        //ds_grid->set_name(nameIter.gridName());

	        grids.push_back(ds_grid);
	    }

	    file.close();

	    return grids;
	}


	template <typename T>
	std::shared_ptr<Grid<T>> Grid<T>::read(const std::string path)
	{
		openvdb::initialize();

	    if (has_suffix(path, "vdb"))
	    	return Grid<T>::from_vdb(path)[0];
	    else if (has_suffix(path, "tif") || has_suffix(path, "tiff") || has_suffix(path, "TIF") || has_suffix(path, "TIFF"))
	    	return Grid<T>::from_multipage_tiff(path);

	    return std::shared_ptr<Grid<T>>(nullptr);
	}


	template <typename T>
	void Grid<T>::write(const std::string path)
	{
    	openvdb::io::File file(path);
    	//openvdb::GridPtrVec grids_out;
    	//grids_out.push_back(m_grid);

	    // Add the grid pointer to a container.
	    // openvdb::GridPtrVec grids_out;
	    // std::map<std::string, openvdb::FloatGrid::Ptr>::iterator it;
	    // for (it = grids.begin(); it != grids.end(); ++it)
	    // 	grids_out.push_back(it->second);

	    // Write out the contents of the container.
	    //file.write(grids_out);
	    m_grid->tree().prune();
	    file.setCompression(openvdb::io::COMPRESS_ACTIVE_MASK | openvdb::io::COMPRESS_BLOSC);
	    file.write({m_grid});
	    file.close();

	    //openvdb::io::File(path).write({m_grid});
	}

	template <typename T>
	T Grid<T>::getValue(Eigen::Vector3i xyz)
	{
		typename openvdb_grid::Accessor accessor = m_grid->getAccessor();
		return accessor.getValue(openvdb::math::Coord(
			xyz.x(), 
			xyz.y(), 
			xyz.z() 
			));
	}

	template <typename T>
	std::vector<T> Grid<T>::getDense(Eigen::Vector3i min, Eigen::Vector3i max)
	{
		openvdb::tools::Dense<T, openvdb::tools::MemoryLayout::LayoutZYX> dense(openvdb::CoordBBox(
		//openvdb::tools::Dense<float> dense(openvdb::CoordBBox(
        openvdb::Coord(min.x(), min.y(), min.z()), 
        openvdb::Coord(max.x(), max.y(), max.z()))
        );

		openvdb::tools::copyToDense(*(m_grid), dense, false);
		//return dense.data();

	    std::vector<T> data;
	    data.assign(dense.data(), dense.data() + dense.valueCount());

	    return data;
	}


	template <typename T>
	T Grid<T>::getInterpolatedValue(Eigen::Vector3f xyz)
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

	template <typename T>
	std::vector<T> Grid<T>::getValues(std::vector<Eigen::Vector3i> &xyz)
	{
		std::vector<T> values;
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

	template <typename T>
	std::vector<T> Grid<T>::getInterpolatedValues(std::vector<Eigen::Vector3f> &xyz)
	{
		std::vector<T> values;
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

	template <typename T>
	std::tuple<Eigen::Vector3i, Eigen::Vector3i> Grid<T>::getBoundingBox()
	{
		openvdb::math::CoordBBox bb = m_grid->evalActiveVoxelBoundingBox();
		Eigen::Vector3i bbmin;
		bbmin << bb.min().x(), bb.min().y(), bb.min().z();
		Eigen::Vector3i bbmax;
		bbmax << bb.max().x(), bb.max().y(),bb.max().z();

		return std::tuple<Eigen::Vector3i, Eigen::Vector3i>(bbmin, bbmax);
	}

	template <typename T>
	Eigen::Matrix4d Grid<T>::get_transform()
	{
		auto mat = m_grid->transform().baseMap()->getAffineMap()->getMat4();

		return Eigen::Matrix4d(mat.asPointer());
	}

	template <typename T>
	void Grid<T>::set_transform(Eigen::Matrix4d mat)
	{
		openvdb::Mat4R omat(mat.data());
		openvdb::math::Transform::Ptr linearTransform =
    		openvdb::math::Transform::createLinearTransform(omat);

		m_grid->setTransform(linearTransform);
	}

	template <typename T>
	void Grid<T>::transform_grid(Eigen::Matrix4d xform)
	{
		GridT outGrid;
		openvdb::Mat4R mat(xform.data());
		openvdb::tools::GridTransformer transformer(mat);
		transformer.transformGrid<openvdb::tools::BoxSampler>(*m_grid, outGrid);

		m_grid = std::make_shared<GridT>(outGrid);
	}

	template <typename T>
	void Grid<T>::denseFill(Eigen::Vector3i min, Eigen::Vector3i max, double value, bool active)
	{
		openvdb::math::CoordBBox bb(
			openvdb::math::Coord(min.data()),
			openvdb::math::Coord(max.data()));

		m_grid->denseFill(bb, (T)value, active);
	}

	template <typename T>
	std::string Grid<T>::get_name()
	{
		return m_grid->getName();
	}

	template <typename T>
	void Grid<T>::set_name(std::string name)
	{
		m_grid->setName(name);
	}

	template class Grid<float>;
}
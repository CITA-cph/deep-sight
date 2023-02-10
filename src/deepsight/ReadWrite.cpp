#include "ReadWrite.h"


namespace DeepSight
{
	//template<typename T>
	Grid<float>::Ptr load_scalar_tiff(const std::string path, double threshold, unsigned int crop, bool verbose)
	{
		//bool verbose = false;
		unsigned int crop_x = crop, crop_y = crop;

		if (!verbose)
		{
			std::cout << "Opening scalar multi-page TIFF '" << path << "'" << std::endl;
			std::cout << "Threshold: " << threshold << std::endl;
		}

		using TreeT = openvdb::tree::Tree< openvdb::tree::RootNode< openvdb::tree::InternalNode< openvdb::tree::InternalNode< openvdb::tree::LeafNode< float, 3 >, 4 >, 5 >>>;
		using GridT = openvdb::Grid<TreeT>;
		using ValueT = typename GridT::ValueType;

		typename GridT::Ptr grid = GridT::create();

		typename GridT::Accessor accessor = (*grid).getAccessor();

		openvdb::Coord ijk;
		int& i = ijk[0], & j = ijk[1], & k = ijk[2];

		TIFF* tif = TIFFOpen(path.c_str(), "r");

		if (tif) {
			try
			{
				unsigned int width, height, samplesperpixel, bitspersample;
				ValueT max_val = 0.0;

				do {
					uint32_t* raster;

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

					raster = (uint32_t*)_TIFFmalloc(npixels * sizeof(uint32_t)); // allocate temp memory (must use the tiff library malloc)
					if (raster == NULL) // check the raster's memory was allocaed
					{
						TIFFClose(tif);
						std::cerr << "Could not allocate memory for raster of TIFF image" << std::endl;
						return std::shared_ptr<Grid<ValueT>>(nullptr);
					}

					// Check the tif read to the raster correctly
					if (!TIFFReadRGBAImage(tif, width, height, raster, 0))
					{
						TIFFClose(tif);
						std::cerr << "Could not read raster of TIFF image" << std::endl;
						return std::shared_ptr<Grid<ValueT>>(nullptr);
					}
					if (crop_x > width)
						crop_x = 0;
					if (crop_y > height)
						crop_y = 0;

					// itterate through all the pixels of the tif
					for (i = 0; (unsigned int)i < width; i++)
						for (j = 0; (unsigned int)j < height; j++)
						{
							uint32_t& TiffPixel = raster[j * width + i]; // read the current pixel of the TIF

							ValueT val = ValueT(((float)(TIFFGetR(TiffPixel) + TIFFGetG(TiffPixel) + TIFFGetB(TiffPixel))) / (255. * 3));
							max_val = std::max(max_val, val);

							if (val < threshold)
								continue;

							//j = height-true_j - 1;

							accessor.setValue(ijk, val);

						}

					_TIFFfree(raster); // release temp memory

					k++;

				} while (TIFFReadDirectory(tif)); // get the next tif
				TIFFClose(tif); // close the tif file

				std::cout << "Loaded " << k << " pages (" << width << " , " << height << ")" << std::endl;
				std::cout << "Max value found: " << max_val << std::endl;

				grid->setGridClass(openvdb::GRID_FOG_VOLUME);
				grid->setName("density");
				grid->pruneGrid(threshold);

				auto ds_grid = std::make_shared<Grid<ValueT>>();
				ds_grid->m_grid = grid;

				return ds_grid;
			}
			catch (std::exception e)
			{
				std::cout << e.what() << std::endl;
				return Grid<ValueT>::Ptr(nullptr);
			}
		}
		else
		{
			std::cerr << "Failed to load multi-page TIFF" << std::endl;
			return Grid<ValueT>::Ptr(nullptr);
		}
	}

	std::shared_ptr<Grid<openvdb::Vec3f>> load_vector_tiff(const std::string path, double threshold, unsigned int crop)
	{
		bool verbose = false;
		unsigned int crop_x = crop, crop_y = crop;

		if (verbose)
		{
			std::cout << "Opening vector multi-page TIFF '" << path << "'" << std::endl;
			std::cout << "Threshold: " << threshold << std::endl;
		}

		using TreeT = openvdb::tree::Tree< openvdb::tree::RootNode< openvdb::tree::InternalNode< openvdb::tree::InternalNode< openvdb::tree::LeafNode< openvdb::Vec3f, 3 >, 4 >, 5 >>>;
		using GridT = openvdb::Grid<TreeT>;
		using ValueT = typename GridT::ValueType;

		typename GridT::Ptr grid = GridT::create();

		typename GridT::Accessor accessor = (*grid).getAccessor();

		openvdb::Coord ijk;
		int& i = ijk[0], & j = ijk[1], & k = ijk[2];

		TIFF* tif = TIFFOpen(path.c_str(), "r");

		if (tif) {
			unsigned int width, height, samplesperpixel, bitspersample;
			ValueT max_val = openvdb::Vec3f(0,0,0);

			do {
				uint32_t* raster;

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

				raster = (uint32_t*)_TIFFmalloc(npixels * sizeof(uint32_t)); // allocate temp memory (must use the tiff library malloc)
				if (raster == NULL) // check the raster's memory was allocaed
				{
					TIFFClose(tif);
					std::cerr << "Could not allocate memory for raster of TIFF image" << std::endl;
					return std::shared_ptr<Grid<ValueT>>(nullptr);
				}

				// Check the tif read to the raster correctly
				if (!TIFFReadRGBAImage(tif, width, height, raster, 0))
				{
					TIFFClose(tif);
					std::cerr << "Could not read raster of TIFF image" << std::endl;
					return std::shared_ptr<Grid<ValueT>>(nullptr);
				}
				if (crop_x > width)
					crop_x = 0;
				if (crop_y > height)
					crop_y = 0;

				// itterate through all the pixels of the tif
				for (i = crop_x; (unsigned int)i < width - crop_x; i++)
					for (j = crop_y; (unsigned int)j < height - crop_y; j++)
					{
						uint32_t& TiffPixel = raster[j * width + i]; // read the current pixel of the TIF

						ValueT val = ValueT(
							(float)TIFFGetR(TiffPixel) / 255. - 0.5,
							(float)TIFFGetR(TiffPixel) / 255. - 0.5,
							(float)TIFFGetR(TiffPixel) / 255. - 0.5
							
							);
						max_val = std::max(max_val, val);

						if (val.length() < threshold)
							continue;

						accessor.setValue(ijk, val);

					}

				_TIFFfree(raster); // release temp memory

				k++;

			} while (TIFFReadDirectory(tif)); // get the next tif
			TIFFClose(tif); // close the tif file

			std::cout << "Loaded " << k << " pages (" << width << " , " << height << ")" << std::endl;
			std::cout << "Max value found: " << max_val << std::endl;

			grid->setGridClass(openvdb::GRID_FOG_VOLUME);
			grid->setName("tiff");

			auto ds_grid = std::make_shared<Grid<ValueT>>();
			ds_grid->m_grid = grid;

			return ds_grid;
		}
		else
		{
			std::cerr << "Failed to load multi-page TIFF" << std::endl;
			return std::shared_ptr<Grid<ValueT>>(nullptr);
		}
	}

	void read_pith(TIFF* tif, RawLam::InfoLog::Ptr infolog, uint32_t height)
	{
		uint16_t s, nsamples;
		tdata_t buf = _TIFFmalloc(TIFFScanlineSize(tif));

		int16_t* data;
		TIFFGetField(tif, TIFFTAG_SAMPLESPERPIXEL, &nsamples);
		for (s = 0; s < nsamples; s++)
		{
			for (uint32_t row = 0; row < height; row++)
			{
				TIFFReadScanline(tif, buf, row, s);
				data = (int16_t*)buf;
				infolog->pith.push_back(Eigen::Vector2f((float)data[0] * 0.1, (float)data[1]) * 0.1);
			}
		}

		_TIFFfree(buf);

		//std::cout << "Found " << infolog->pith.size() << " pith points." << std::endl;
	}

	void read_knots(TIFF* tif, RawLam::InfoLog::Ptr infolog, uint32_t height)
	{
		uint16_t s, nsamples;
		tdata_t buf = _TIFFmalloc(TIFFScanlineSize(tif));

		float* data;
		TIFFGetField(tif, TIFFTAG_SAMPLESPERPIXEL, &nsamples);
		for (s = 0; s < nsamples; s++)
		{
			for (uint32_t row = 0; row < height; row++)
			{
				TIFFReadScanline(tif, buf, row, s);
				data = (float*)buf;

				RawLam::knot k;
				k.index = (int)data[0];
				k.start = Eigen::Vector3f((float)data[1], (float)data[2], (float)data[3]);
				k.end = Eigen::Vector3f((float)data[4], (float)data[5], (float)data[6]);
				k.dead_knot_border = data[7];
				k.radius = (float)data[8];
				k.length = (float)data[9];
				k.volume = (float)data[10];

				infolog->knots.push_back(k);
			}
		}

		_TIFFfree(buf);

		//std::cout << "Found " << infolog->knots.size() << " knots." << std::endl;
	}


	RawLam::InfoLog::Ptr load_infolog(const std::string path, bool verbose)
	{
		if (verbose)
		{
			std::cout << "Opening InfoLog TIFF '" << path << "'" << std::endl;
		}

		TIFF* tif = TIFFOpen(path.c_str(), "r");

		uint32_t num_pages = 0;

		auto infolog = std::make_shared<RawLam::InfoLog>();

		enum mode {
			PITH,
			KNOTS,
			NONE
		};

		if (tif) {
			try
			{
				uint32_t width, height;
				uint16_t bitspersample, samplesperpixel, sampleformat;
				uint32_t config;
				va_list c_page_name;

				mode m = mode::NONE;

				do {
					//char** c_page_name;

					// get the size of the tiff
					TIFFGetField(tif, TIFFTAG_IMAGEWIDTH, &width);
					TIFFGetField(tif, TIFFTAG_IMAGELENGTH, &height);
					TIFFGetField(tif, TIFFTAG_SAMPLESPERPIXEL, &samplesperpixel);
					TIFFGetField(tif, TIFFTAG_SAMPLEFORMAT, &sampleformat);
					TIFFGetField(tif, TIFFTAG_BITSPERSAMPLE, &bitspersample);
					TIFFGetField(tif, TIFFTAG_PAGENAME, &c_page_name);

					//std::cout << c_page_name << std::endl;
					std::string page_name = std::string(c_page_name);

					if (page_name == "pith")
						m = mode::PITH;
					else if (page_name == "knots")
						m = mode::KNOTS;
					else
						m = mode::NONE;

					if (verbose)
					{
						std::cout << page_name << std::endl;
						std::cout << "    width: " << width << std::endl;
						std::cout << "    height: " << height << std::endl;
						std::cout << "    samplesperpixel: " << samplesperpixel << std::endl;
						std::cout << "    sampleformat: " << sampleformat << std::endl;
						std::cout << "    bitspersample: " << bitspersample << std::endl;
					}

					TIFFGetField(tif, TIFFTAG_PLANARCONFIG, &config);

					switch (m)
					{
					case(mode::PITH):
						read_pith(tif, infolog, height);
						break;
					case(mode::KNOTS):
						read_knots(tif, infolog, height);
						break;
					default:
						break;
					}

					num_pages++;
	

				} while (TIFFReadDirectory(tif)); // get the next tif
				TIFFClose(tif); // close the tif file

				if (verbose)
					std::cout << "Loaded " << num_pages << " pages (" << width << " , " << height << ")" << std::endl;

				return infolog;
			}
			catch (std::exception e)
			{
				std::cout << e.what() << std::endl;
				return std::shared_ptr<RawLam::InfoLog>(nullptr);
			}
		}
		else
		{
			std::cerr << "Failed to load multi-page TIFF" << std::endl;
			return std::shared_ptr<RawLam::InfoLog>(nullptr);
		}
		return std::shared_ptr<RawLam::InfoLog>(nullptr);
	}

	std::vector<GridBase*> read_vdb(const std::string path)
	{
		openvdb::io::File file(path);
		file.open();

		std::vector<GridBase*> grids;

		//auto grid_ptr_vec = file.getGrids();

		//for(auto iter=grid_ptr_vec->begin(); iter != grid_ptr_vec->end(); ++iter)

		for (openvdb::io::File::NameIterator nameIter = file.beginName();
			nameIter != file.endName(); ++nameIter)
		{
			//std::cout << "Reading " << (*iter)->type() << std::endl;
			auto grid = new GridBase();
			//grid->m_grid = openvdb::GridBase::Ptr( * iter);
			//std::cout << "Grid set: " << grid->m_grid->type() << std::endl;
			//std::cout << "Grid ptr: " << grid << std::endl;
			grid->m_grid = file.readGrid(nameIter.gridName());
			grids.push_back(grid);
		}

		file.close();

		return grids;
	}

	SAFEARRAY* ReadWrite_ReadVdb(const char* path)
	{
		std::vector<GridBase*> grids = read_vdb(path);
		//std::cout << "Found " << grids.size() << " grids... " << std::endl;
		//std::cout << "Size of grids array: " << sizeof(grids) << std::endl;
		//std::cout << "Size of single ptr : " << sizeof(GridBase*) << std::endl;

		SAFEARRAY* psa = SafeArrayCreateVector(VT_I4, 0, grids.size());
		void* data;
		SafeArrayAccessData(psa, &data);
		CopyMemory(data, grids.data(), grids.size() * sizeof(GridBase*));
		SafeArrayUnaccessData(psa);
		return psa;
	}

	void ReadWrite_WriteVdb(const char* path, int num_grids, GridBase** grids, int float_as_half)
	{
		openvdb::io::File file(path);
		openvdb::GridPtrVec grids_out;

		for (int i = 0; i < num_grids; ++i)
		{
			auto grid = grids[i]->m_grid;
			grid->pruneGrid();
			grid->setSaveFloatAsHalf(float_as_half != 0);
			grids_out.push_back(grid);
		}

		file.setCompression(openvdb::io::COMPRESS_ACTIVE_MASK | openvdb::io::COMPRESS_BLOSC);

		file.write(grids_out);
		file.close();
	}
}
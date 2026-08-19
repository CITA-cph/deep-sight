#include "ReadWrite.h"


namespace DeepSight
{
	//template<typename T>
	Grid<float>::Ptr load_scalar_tiff(const std::string path, double threshold, unsigned int crop, bool verbose)
	{
		//bool verbose = false;
		unsigned int crop_x = crop, crop_y = crop;

		// Was `if (!verbose)`: the banner printed only when the caller asked for
		// *quiet* output, and was suppressed in verbose mode. Compare
		// load_vector_tiff below, which has the same block written correctly.
		if (verbose)
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
				// TIFFGetField is varargs: it writes exactly the width the tag's
				// type declares, with no conversion and no diagnostic. IMAGEWIDTH
				// and IMAGELENGTH are LONG (uint32), but SAMPLESPERPIXEL and
				// BITSPERSAMPLE are SHORT (uint16). Passing `unsigned int*` for the
				// latter two wrote 2 bytes into a 4-byte slot and left the high half
				// uninitialised, which is why the verbose dump printed garbage.
				// Initialised as well, so a missing tag leaves a defined value.
				uint32_t width = 0, height = 0;
				uint16_t samplesperpixel = 0, bitspersample = 0;
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

					// width * height in 32-bit arithmetic silently wraps for large
					// scans (a 70k x 70k page is enough), producing a raster buffer
					// far smaller than the loop below then indexes into. Compute in
					// 64-bit and reject anything that will not fit.
					const uint64_t npixels64 = static_cast<uint64_t>(width) * static_cast<uint64_t>(height);
					if (npixels64 == 0 || npixels64 > (SIZE_MAX / sizeof(uint32_t)))
					{
						TIFFClose(tif);
						std::cerr << "TIFF page has invalid or unsupported dimensions ("
							<< width << " x " << height << ")" << std::endl;
						return Grid<ValueT>::Ptr(nullptr);
					}
					const size_t npixels = static_cast<size_t>(npixels64); // total number of pixels

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
						_TIFFfree(raster);	// was leaked on this path
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
			// Was `catch (std::exception e)` -- catching by value slices any derived
			// exception (openvdb::IoError, std::bad_alloc) down to its base, so
			// what() reported the generic base message instead of the real cause.
			catch (const std::exception& e)
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
			// See the note in load_scalar_tiff: SAMPLESPERPIXEL and BITSPERSAMPLE
			// are 16-bit tags and must not be read into an `unsigned int`.
			uint32_t width = 0, height = 0;
			uint16_t samplesperpixel = 0, bitspersample = 0;
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

				// See the note in load_scalar_tiff on 32-bit overflow here.
				const uint64_t npixels64 = static_cast<uint64_t>(width) * static_cast<uint64_t>(height);
				if (npixels64 == 0 || npixels64 > (SIZE_MAX / sizeof(uint32_t)))
				{
					TIFFClose(tif);
					std::cerr << "TIFF page has invalid or unsupported dimensions ("
						<< width << " x " << height << ")" << std::endl;
					return std::shared_ptr<Grid<ValueT>>(nullptr);
				}
				const size_t npixels = static_cast<size_t>(npixels64); // total number of pixels

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
					_TIFFfree(raster);	// was leaked on this path
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
	
	void read_border(TIFF* tif, RawLam::InfoLog::Ptr infolog, uint32_t height, uint32_t width)
	{
		uint16_t s, nsamples;
		tdata_t bufx = _TIFFmalloc(TIFFScanlineSize(tif));
		tdata_t bufy = _TIFFmalloc(TIFFScanlineSize(tif));

		int16_t* datax, *datay;
		TIFFGetField(tif, TIFFTAG_SAMPLESPERPIXEL, &nsamples);
		for (s = 0; s < nsamples; s++)
		{
			for (uint32_t row = 0; row < height; row+=2)
			{
				TIFFReadScanline(tif, bufx, row, s);
				TIFFReadScanline(tif, bufy, row + 1, s);
				datax = (int16_t*)bufx;
				datay = (int16_t*)bufy;

				std::vector<Eigen::Vector2f> outline;

				for (uint32_t i = 0; i < width; i += 1)
				{
					outline.push_back(Eigen::Vector2f(static_cast<float>(datax[i]), static_cast<float>(datay[i])));
				}

				infolog->border.push_back(outline);
			}
		}

		_TIFFfree(bufx);
		_TIFFfree(bufy);
	}

	void read_sapwood(TIFF* tif, RawLam::InfoLog::Ptr infolog, uint32_t height, uint32_t width)
	{
		uint16_t s, nsamples;
		tdata_t bufx = _TIFFmalloc(TIFFScanlineSize(tif));
		tdata_t bufy = _TIFFmalloc(TIFFScanlineSize(tif));

		int16_t* datax, * datay;
		TIFFGetField(tif, TIFFTAG_SAMPLESPERPIXEL, &nsamples);
		for (s = 0; s < nsamples; s++)
		{
			for (uint32_t row = 0; row < height; row += 2)
			{
				TIFFReadScanline(tif, bufx, row, s);
				TIFFReadScanline(tif, bufy, row + 1, s);
				datax = (int16_t*)bufx;
				datay = (int16_t*)bufy;

				std::vector<Eigen::Vector2f> outline;

				for (uint32_t i = 0; i < width; i += 1)
				{
					outline.push_back(Eigen::Vector2f(static_cast<float>(datax[i]), static_cast<float>(datay[i])));
				}

				infolog->sapwood.push_back(outline);
			}
		}

		_TIFFfree(bufx);
		_TIFFfree(bufy);
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
			BORDER,
			SAPWOOD,
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
					else if (page_name == "border")
						m = mode::BORDER;
					else if (page_name == "sapwood")
						m = mode::SAPWOOD;
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
					case(mode::SAPWOOD):
						read_sapwood(tif, infolog, height, width);
						break;
					case(mode::BORDER):
						read_border(tif, infolog, height, width);
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
			// Was `catch (std::exception e)` -- catching by value slices any derived
			// exception (openvdb::IoError, std::bad_alloc) down to its base, so
			// what() reported the generic base message instead of the real cause.
			catch (const std::exception& e)
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

	std::vector<GridBase*> read_vdb(const std::string& path)
	{
		openvdb::io::File file(path);
		file.open();		// throws openvdb::IoError on a missing/corrupt file

		// unique_ptrs while building. If readGrid() throws part-way through a
		// multi-grid file (truncated stream, unregistered grid type), the
		// GridBase objects already constructed used to be leaked outright --
		// and each one can own hundreds of MB of tree.
		std::vector<std::unique_ptr<GridBase>> owned;

		try
		{
			for (openvdb::io::File::NameIterator nameIter = file.beginName();
				nameIter != file.endName(); ++nameIter)
			{
				std::unique_ptr<GridBase> grid(new GridBase());
				grid->m_grid = file.readGrid(nameIter.gridName());
				owned.push_back(std::move(grid));
			}
		}
		catch (...)
		{
			file.close();	// was left open on the throwing path
			throw;
		}

		file.close();

		std::vector<GridBase*> grids;
		grids.reserve(owned.size());
		for (auto& g : owned) grids.push_back(g.release());

		return grids;
	}

	// -----------------------------------------------------------------------
	// C ABI entry points.
	//
	// Both of these call straight into OpenVDB file I/O, which throws
	// openvdb::IoError for the single most common failure in the whole library
	// -- a path that does not exist. Previously that exception unwound out of
	// an extern "C" frame and into the CLR, killing Rhino outright. They are
	// now wrapped, and report failure through DeepSight_GetLastError().
	// -----------------------------------------------------------------------

	extern "C"
	{
		void DEEPSIGHT_CALL ReadWrite_ReadVdb(const char* path, int* num_grids, GridBase*** grid_ptrs)
		{
			api_guard([&] {
				if (path == nullptr) throw std::invalid_argument("ReadWrite_ReadVdb: null path.");
				if (num_grids == nullptr) throw std::invalid_argument("ReadWrite_ReadVdb: null num_grids.");
				if (grid_ptrs == nullptr) throw std::invalid_argument("ReadWrite_ReadVdb: null grid_ptrs.");

				// Report zero before doing any work, so that a caller that
				// ignores the error state still sees a consistent (empty)
				// result rather than a stale count with a null buffer.
				*num_grids = 0;
				*grid_ptrs = nullptr;

				std::vector<GridBase*> grids = read_vdb(path);

				if (grids.empty())
					return;		// CoTaskMemAlloc(0) may return null, and the
								// old code CopyMemory'd into it unconditionally

				const size_t bytes = sizeof(GridBase*) * grids.size();
				GridBase** buffer = static_cast<GridBase**>(::CoTaskMemAlloc(bytes));
				if (buffer == nullptr)
				{
					// Do not leak the grids we just read if we cannot hand
					// them back.
					for (GridBase* g : grids) delete g;
					throw std::bad_alloc();
				}

				std::memcpy(buffer, grids.data(), bytes);

				*grid_ptrs = buffer;
				*num_grids = static_cast<int>(grids.size());
			});
		}

		void DEEPSIGHT_CALL ReadWrite_WriteVdb(const char* path, int num_grids, GridBase* const* grids, int float_as_half)
		{
			api_guard([&] {
				if (path == nullptr) throw std::invalid_argument("ReadWrite_WriteVdb: null path.");
				if (num_grids < 0) throw std::invalid_argument("ReadWrite_WriteVdb: negative grid count.");
				if (num_grids > 0 && grids == nullptr) throw std::invalid_argument("ReadWrite_WriteVdb: null grid array.");

				openvdb::GridPtrVec grids_out;
				grids_out.reserve(static_cast<size_t>(num_grids));

				for (int i = 0; i < num_grids; ++i)
				{
					// A null or empty entry used to be dereferenced blind.
					if (grids[i] == nullptr || !grids[i]->m_grid)
						throw std::invalid_argument(
							"ReadWrite_WriteVdb: grid " + std::to_string(i) + " is null or empty.");

					auto grid = grids[i]->m_grid;
					grid->pruneGrid();
					grid->setSaveFloatAsHalf(float_as_half != 0);
					grids_out.push_back(grid);
				}

				openvdb::io::File file(path);
				file.setCompression(openvdb::io::COMPRESS_ACTIVE_MASK | openvdb::io::COMPRESS_BLOSC);
				file.write(grids_out);
				file.close();
			});
		}
	}
}

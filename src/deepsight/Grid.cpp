#include "Grid.h"

namespace DeepSight
{

	template<typename T>
	Grid<T>::Grid()
	{
		openvdb::initialize();
		m_grid = GridT::create(openvdb::zeroVal<T>());
	}

	template<typename T>
	Grid<T>::Grid(T background)
	{
		openvdb::initialize();
		m_grid = GridT::create(background);
	}

	//template<typename T>
	//Grid<T>::Grid(GridT* grid)
	//{
	//	openvdb::initialize();
	//	m_grid.reset(grid);
	//	//m_accessor = m_grid->getAccessor();
	//}

	template<typename T>
	std::shared_ptr<Grid<T>> Grid<T>::duplicate()
	{
		typename Grid<T>::Ptr grid = std::make_shared<Grid<T>>();
		grid->m_grid = std::shared_ptr<GridT>(m_grid->deepCopy());

		return grid;
	}

	template<typename T>
	Grid<T>::~Grid()
	{
	}

	template<typename T>
	bool Grid<T>::has_suffix(const std::string& str, const std::string& suffix)
	{
		return str.size() >= suffix.size() &&
			str.compare(str.size() - suffix.size(), suffix.size(), suffix) == 0;
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
	std::shared_ptr<Grid<T>> Grid<T>::read(const std::string path, double threshold, unsigned int crop)
	{
		openvdb::initialize();

		if (has_suffix(path, "vdb"))
			return Grid<T>::from_vdb(path)[0];

		return std::shared_ptr<Grid<T>>(nullptr);
	}

	template <typename T>
	void Grid<T>::write(const std::string path, bool float_as_half)
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
		if (float_as_half)
			m_grid->setSaveFloatAsHalf(float_as_half);
		file.write({ m_grid });
		file.close();

		//openvdb::io::File(path).write({m_grid});
	}

	template <typename T>
	void Grid<T>::write(const std::string path, std::vector<Grid<T>*> grids, bool float_as_half)
	{
		openvdb::io::File file(path);
		openvdb::GridPtrVec grids_out;

		for (auto grid: grids)
		{
			grid->m_grid->tree().prune();
			grid->m_grid->setSaveFloatAsHalf(float_as_half);
			grids_out.push_back(grid->m_grid);
		}

		file.setCompression(openvdb::io::COMPRESS_ACTIVE_MASK | openvdb::io::COMPRESS_BLOSC);

		file.write(grids_out);
		file.close();
	}

/*
	template <typename T>
	void Grid<T>::write_many(const std::string path, std::vector<Grid<T>> grids, bool float_as_half)
	{
		openvdb::io::File file(path);
		openvdb::GridPtrVec grids_out;
		typename std::vector<Grid<T>>::iterator it;
		for (it = grids.begin(); it != grids.end(); ++it)
		{
			if (float_as_half)
				it->m_grid->setSaveFloatAsHalf(float_as_half);
			grids_out.push_back(it->m_grid);
		}

		// Add the grid pointer to a container.
		// openvdb::GridPtrVec grids_out;
		// std::map<std::string, openvdb::FloatGrid::Ptr>::iterator it;
		// for (it = grids.begin(); it != grids.end(); ++it)
		// 	grids_out.push_back(it->second);

		// Write out the contents of the container.
		//file.write(grids_out);
		//m_grid->tree().prune();
		
		file.setCompression(openvdb::io::COMPRESS_ACTIVE_MASK | openvdb::io::COMPRESS_BLOSC);
		file.write(grids_out);
		file.close();

		//openvdb::io::File(path).write({m_grid});
	}
	*/
	template <typename T>
	T Grid<T>::get_value(Eigen::Vector3i xyz)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();
		return accessor.getValue(openvdb::math::Coord(
			xyz.x(),
			xyz.y(),
			xyz.z()
		));
	}

	template <typename T>
	void Grid<T>::set_value(Eigen::Vector3i xyz, T value)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();
		return accessor.setValue(openvdb::math::Coord(
			xyz.x(),
			xyz.y(),
			xyz.z()
		), value);
	}

	template<typename T>
	Eigen::Matrix<T, 27, 1> Grid<T>::get_neighbourhood(Eigen::Vector3i xyz)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();

		int x = xyz[0], y = xyz[1], z = xyz[2];
		Eigen::Matrix<T, 27, 1> neighbourhood;

		neighbourhood[0] = accessor.getValue(openvdb::Coord(x - 1,	y - 1,	z - 1));
		neighbourhood[1] = accessor.getValue(openvdb::Coord(x,		y - 1,	z - 1));
		neighbourhood[2] = accessor.getValue(openvdb::Coord(x + 1,	y - 1,	z - 1));

		neighbourhood[3] = accessor.getValue(openvdb::Coord(x - 1,	y,	z - 1));
		neighbourhood[4] = accessor.getValue(openvdb::Coord(x,		y,	z - 1));
		neighbourhood[5] = accessor.getValue(openvdb::Coord(x + 1,	y,	z - 1));

		neighbourhood[6] = accessor.getValue(openvdb::Coord(x - 1,	y + 1,	z - 1));
		neighbourhood[7] = accessor.getValue(openvdb::Coord(x,		y + 1,	z - 1));
		neighbourhood[8] = accessor.getValue(openvdb::Coord(x + 1,	y + 1,	z - 1));

		neighbourhood[9] = accessor.getValue(openvdb::Coord(x - 1,	y - 1,	z));
		neighbourhood[10] = accessor.getValue(openvdb::Coord(x,		y - 1,	z));
		neighbourhood[11] = accessor.getValue(openvdb::Coord(x + 1,	y - 1,	z));

		neighbourhood[12] = accessor.getValue(openvdb::Coord(x - 1,		y,	z));
		neighbourhood[13] = accessor.getValue(openvdb::Coord(x,			y,	z));
		neighbourhood[14] = accessor.getValue(openvdb::Coord(x + 1,		y,	z));

		neighbourhood[15] = accessor.getValue(openvdb::Coord(x - 1,		y + 1,	z));
		neighbourhood[16] = accessor.getValue(openvdb::Coord(x,			y + 1,	z));
		neighbourhood[17] = accessor.getValue(openvdb::Coord(x + 1,		y + 1,	z));

		neighbourhood[18] = accessor.getValue(openvdb::Coord(x - 1,		y - 1,	z + 1));
		neighbourhood[19] = accessor.getValue(openvdb::Coord(x,			y - 1,	z + 1));
		neighbourhood[20] = accessor.getValue(openvdb::Coord(x + 1,		y - 1,	z + 1));

		neighbourhood[21] = accessor.getValue(openvdb::Coord(x - 1,		y,	z + 1));
		neighbourhood[22] = accessor.getValue(openvdb::Coord(x,			y,	z + 1));
		neighbourhood[23] = accessor.getValue(openvdb::Coord(x + 1,		y,	z + 1));

		neighbourhood[24] = accessor.getValue(openvdb::Coord(x - 1,		y + 1,	z + 1));
		neighbourhood[25] = accessor.getValue(openvdb::Coord(x,			y + 1,	z + 1));
		neighbourhood[26] = accessor.getValue(openvdb::Coord(x + 1,		y + 1,	z + 1));

		return neighbourhood;
	}

	template <typename T>
	std::vector<T> Grid<T>::get_dense(Eigen::Vector3i min, Eigen::Vector3i max)
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

	template<typename T>
	std::vector<Eigen::Vector3i> Grid<T>::get_active_voxels()
	{
		std::vector<Eigen::Vector3i> values;

		for (typename GridT::ValueOnCIter iter = m_grid->cbeginValueOn(); iter.test(); ++iter)
		{
			if (iter.isVoxelValue())
			{
				values.push_back(Eigen::Vector3i(iter.getCoord().data()));
			}
		}
		return values;
	}

	template <typename T>
	T Grid<T>::get_interpolated_value(Eigen::Vector3f xyz)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();
		return openvdb::tools::BoxSampler::sample(
			accessor,
			openvdb::Vec3R(
				xyz.x(),
				xyz.y(),
				xyz.z()
			));
	}

	template <typename T>
	std::vector<T> Grid<T>::get_values(std::vector<Eigen::Vector3i>& xyz)
	{
		std::vector<T> values;
		typename GridT::Accessor accessor = m_grid->getAccessor();

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
	void Grid<T>::set_values(std::vector<Eigen::Vector3i>& xyz, std::vector<T> values)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();


		for (auto iter = std::make_pair(xyz.cbegin(), values.cbegin());
			iter.first != xyz.end() && iter.second != values.end();
			++iter.first, ++iter.second)
		{
				accessor.setValue(
					openvdb::math::Coord(
						(int)(*iter.first).x(),
						(int)(*iter.first).y(),
						(int)(*iter.first).z()
					), *iter.second
			);

		}
	}

	template <typename T>
	bool Grid<T>::get_active_state(Eigen::Vector3i xyz)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();
		return accessor.isValueOn(openvdb::math::Coord(xyz.data()));
	}

	template <typename T>
	std::vector<bool> Grid<T>::get_active_state(std::vector<Eigen::Vector3i>& xyz)
	{
		std::vector<bool> states;
		typename GridT::Accessor accessor = m_grid->getAccessor();

		for (auto iter = xyz.begin(); iter != xyz.end(); ++iter)
		{
			states.push_back(accessor.isValueOn(openvdb::math::Coord((* iter).data())));
		}

		return states;
	}

	template <typename T>
	void Grid<T>::set_active_state(Eigen::Vector3i xyz, bool state)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();
		accessor.setActiveState(
			openvdb::math::Coord(xyz.data()), state);
	}

	template <typename T>
	void Grid<T>::set_active_state(std::vector<Eigen::Vector3i>& xyz, std::vector<bool>& states)
	{
		typename GridT::Accessor accessor = m_grid->getAccessor();
		for (auto iter = std::make_pair(xyz.cbegin(), states.cbegin());
			iter.first != xyz.end() && iter.second != states.end();
			++iter.first, ++iter.second)
		{
			accessor.setActiveState(
				openvdb::math::Coord((*iter.first).data()
					//(int)(*iter.first).x(),
					//(int)(*iter.first).y(),
					//(int)(*iter.first).z()
				), *iter.second
			);
		}

	}

	template <typename T>
	std::vector<T> Grid<T>::get_interpolated_values(std::vector<Eigen::Vector3f>& xyz, unsigned int sample_type)
	{
		auto accessor = m_grid->getConstAccessor();
		std::vector<ValueT> values(xyz.size());

		switch (sample_type)
		{
		case(1):
		{
			openvdb::tools::GridSampler<GridT::ConstAccessor, openvdb::tools::BoxSampler> sampler1(accessor, m_grid->transform());
			for (int i = 0; i < xyz.size(); ++i)
			{
				values[i] = sampler1.wsSample(
					openvdb::Vec3R(xyz[i].x(), xyz[i].y(), xyz[i].z()));
			}
			break;
		}
		case(2):
		{
			openvdb::tools::GridSampler<GridT::ConstAccessor, openvdb::tools::QuadraticSampler> sampler2(accessor, m_grid->transform());

			for (int i = 0; i < xyz.size(); ++i)
			{
				values[i] = sampler2.wsSample(
					openvdb::Vec3R(xyz[i].x(), xyz[i].y(), xyz[i].z()));
			}
			break;
		}
		default:
		{
			openvdb::tools::GridSampler<GridT::ConstAccessor, openvdb::tools::PointSampler> sampler3(accessor, m_grid->transform());

			for (int i = 0; i < xyz.size(); ++i)
			{
				values[i] = sampler3.wsSample(
					openvdb::Vec3R(xyz[i].x(), xyz[i].y(), xyz[i].z()));
			}
		}
		}
		return values;

		/*
		std::vector<T> values;
		typename GridT::Accessor accessor = m_grid->getAccessor();

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
		*/
	}

	template <typename T>
	const std::tuple<Eigen::Vector3i, Eigen::Vector3i> Grid<T>::bounding_box()
	{
		openvdb::math::CoordBBox bb = m_grid->evalActiveVoxelBoundingBox();
		Eigen::Vector3i bbmin;
		bbmin << bb.min().x(), bb.min().y(), bb.min().z();
		Eigen::Vector3i bbmax;
		bbmax << bb.max().x(), bb.max().y(), bb.max().z();

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
	void Grid<T>::dense_fill(Eigen::Vector3i min, Eigen::Vector3i max, T value, bool active)
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

	template <typename T>
	void Grid<T>::gradient()
	{
		//openvdb::tools::Gradient<Grid<T>::GridT, openvdb::BoolGrid> tool(*m_grid);
		//tool.process(true);
	}

	template <typename T>
	std::shared_ptr<Grid<T>> Grid<T>::laplacian()
	{

		//openvdb::tools::Laplacian <Grid<T>::GridT, openvdb::BoolGrid> tool(*m_grid);
		auto ds_grid = std::make_shared<Grid<T>>();

		ds_grid->m_grid = m_grid;
		//ds_grid->m_grid = tool.process(true);

		return ds_grid;
	}

	template <typename T>
	std::shared_ptr<Grid<T>> Grid<T>::mean_curvature()
	{
		auto ds_grid = std::make_shared<Grid<T>>();
		//ds_grid->m_grid = openvdb::tools::meanCurvature(*m_grid);

		ds_grid->m_grid = m_grid;
		return ds_grid;
	}

	template <typename T>
	void Grid<T>::normalize()
	{

		return;

		// auto ds_grid = std::make_shared<Grid<T>>();
		// openvdb::tools::normalize<Grid<T>::GriT>(*m_grid);
		// ds_grid->m_grid = openvdb::tools::normalize<Grid<T>::GridT>(*m_grid);

		// return ds_grid;
	}

	template <typename T>
	void Grid<T>::filter(int width, int iterations, int type)
	{

		openvdb::tools::Filter <Grid<T>::GridT> tool(*m_grid);
		switch (type)
		{
		case(1):
			tool.mean(width, iterations);
			break;
		case(2):
			tool.median(width, iterations);
			break;
		default:
			tool.gaussian(width, iterations);
		}
	}

	template <typename T>
	void Grid<T>::dilate(int iterations)
	{
		openvdb::tools::dilateActiveValues(m_grid->tree(), iterations, openvdb::tools::NearestNeighbors::NN_FACE_EDGE_VERTEX);
	}

	template <typename T>
	void Grid<T>::prune(T value)
	{
		m_grid->pruneGrid(0.1);
	}

	template <typename T>
	void Grid<T>::erode(int iterations)
	{
		openvdb::tools::erodeActiveValues(m_grid->tree(), iterations, openvdb::tools::NearestNeighbors::NN_FACE_EDGE_VERTEX);
	}

	//template <typename T>
	void Grid<float>::to_mesh(float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& faces)
	{
		openvdb::tools::VolumeToMesh mesher(isovalue);
		mesher(*m_grid);

		openvdb::Coord ijk;

		for (size_t i = 0, N = mesher.pointListSize(); i < N; ++i)
		{
			const openvdb::Vec3s& vert = mesher.pointList()[i];
			verts.push_back(Eigen::Vector3f((float)vert.x(), (float)vert.y(), (float)vert.z()));
		}

		// Copy primitives
		openvdb::tools::PolygonPoolList& polygonPoolList = mesher.polygonPoolList();

		size_t numQuads = 0;
		for (size_t n = 0, N = mesher.polygonPoolListSize(); n < N; ++n) {
			numQuads += polygonPoolList[n].numQuads();
		}

		for (size_t i = 0, N = mesher.polygonPoolListSize(); i < N; ++i) {
			const openvdb::tools::PolygonPool& polygons = polygonPoolList[i];
			for (size_t j = 0, I = polygons.numQuads(); j < I; ++j) {
				const openvdb::Vec4I& quad = polygons.quad(j);

				faces.push_back(Eigen::Vector4i((int)quad.x(), (int)quad.y(), (int)quad.z(), (int)quad.w()));
			}
		}
	}

	void Grid<double>::to_mesh(float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& faces)
	{
		openvdb::tools::VolumeToMesh mesher(isovalue);
		mesher(* m_grid);

		openvdb::Coord ijk;

		for (size_t i = 0, N = mesher.pointListSize(); i < N; ++i)
		{
			const openvdb::Vec3s& vert = mesher.pointList()[i];
			verts.push_back(Eigen::Vector3f((float)vert.x(), (float)vert.y(), (float)vert.z()));
		}

		// Copy primitives
		openvdb::tools::PolygonPoolList& polygonPoolList = mesher.polygonPoolList();

		size_t numQuads = 0;
		for (size_t n = 0, N = mesher.polygonPoolListSize(); n < N; ++n) {
			numQuads += polygonPoolList[n].numQuads();
		}

		for (size_t i = 0, N = mesher.polygonPoolListSize(); i < N; ++i) {
			const openvdb::tools::PolygonPool& polygons = polygonPoolList[i];
			for (size_t j = 0, I = polygons.numQuads(); j < I; ++j) {
				const openvdb::Vec4I& quad = polygons.quad(j);

				faces.push_back(Eigen::Vector4i((int)quad.x(), (int)quad.y(), (int)quad.z(), (int)quad.w()));
			}
		}
	}

	void Grid<int>::to_mesh(float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& faces)
	{
		openvdb::tools::VolumeToMesh mesher(isovalue);
		mesher(*m_grid);

		openvdb::Coord ijk;

		for (size_t i = 0, N = mesher.pointListSize(); i < N; ++i)
		{
			const openvdb::Vec3s& vert = mesher.pointList()[i];
			verts.push_back(Eigen::Vector3f((float)vert.x(), (float)vert.y(), (float)vert.z()));
		}

		// Copy primitives
		openvdb::tools::PolygonPoolList& polygonPoolList = mesher.polygonPoolList();

		size_t numQuads = 0;
		for (size_t n = 0, N = mesher.polygonPoolListSize(); n < N; ++n) {
			numQuads += polygonPoolList[n].numQuads();
		}

		for (size_t i = 0, N = mesher.polygonPoolListSize(); i < N; ++i) {
			const openvdb::tools::PolygonPool& polygons = polygonPoolList[i];
			for (size_t j = 0, I = polygons.numQuads(); j < I; ++j) {
				const openvdb::Vec4I& quad = polygons.quad(j);

				faces.push_back(Eigen::Vector4i((int)quad.x(), (int)quad.y(), (int)quad.z(), (int)quad.w()));
			}
		}
	}

	template <typename T>
	Grid<T>* Grid<T>::resample(float scale)
	{
		typename GridT::Ptr target = GridT::create(m_grid->background());

		target->setTransform(
			openvdb::math::Transform::createLinearTransform(scale));

		const openvdb::math::Transform
			& sourceXform = m_grid->transform(),
			& targetXform = target->transform();

		openvdb::Mat4R xform =
			sourceXform.baseMap()->getAffineMap()->getMat4() *
			targetXform.baseMap()->getAffineMap()->getMat4().inverse();

		openvdb::tools::GridTransformer transformer(xform);

		// Resample using triquadratic interpolation.
		//transformer.transformGrid<openvdb::tools::QuadraticSampler, GridType>(
		//	*m_grid, *target);

		//target->tree().prune();

		openvdb::tools::resampleToMatch<openvdb::tools::QuadraticSampler>(*m_grid, *target);
		
		Grid<T>* new_grid = new Grid<T>(m_grid->background());
		new_grid->m_grid = target;

		return new_grid;

	}

	Grid<float>::Ptr from_mesh(std::vector<openvdb::Vec3f> verts, std::vector <openvdb::Vec4I> faces, openvdb::math::Transform xform, float isovalue, float exteriorBandWidth, float interiorBandWidth)
	{
		openvdb::tools::VolumeToMesh mesher(isovalue);
		using MeshType = openvdb::tools::QuadAndTriangleDataAdapter<openvdb::Vec3f, openvdb::Vec4I>;
		MeshType mesh(verts, faces);

		openvdb::FloatGrid::Ptr new_grid = openvdb::tools::meshToVolume<openvdb::FloatGrid, MeshType>
			(mesh, xform, exteriorBandWidth, interiorBandWidth);

		Grid<float>::Ptr grid = std::make_shared<Grid<float>>();
		grid->m_grid = std::shared_ptr<openvdb::FloatGrid>(new_grid->deepCopy());

		return grid;
	}

	template class Grid<double>;
	template class Grid<float>;
	template class Grid<int>;
	//template class Grid<bool>;
	template class Grid<openvdb::Vec3f>;
	template class Grid <openvdb::Vec4f>;
	//template class Grid<openvdb::Vec3d>;

}
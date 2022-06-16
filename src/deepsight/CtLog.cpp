#include "CtLog.h"
#include "openvdb/openvdb.h"



DeepSight::CtLog::CtLog()
	{
		openvdb::initialize();
	}

DeepSight::CtLog::~CtLog()
	{
		//if (initialized)
		//	delete m_grid;
	}

	void DeepSight::CtLog::readVDB(const std::string filename)
	{
		openvdb::GridBase::Ptr baseGrid;

		openvdb::io::File file(filename);

		file.open();

		for (auto nameIter = file.beginName(); nameIter != file.endName(); ++nameIter)
		{
			baseGrid = file.readGrid(nameIter.gridName());
			break;
		}

		file.close();

		m_grid = openvdb::gridPtrCast<GridType>(baseGrid);
		//m_grid->tree().prune(0.01);
		m_grid->pruneGrid(0.01f);
	}

	std::vector<ValueT> DeepSight::CtLog::evaluate(std::vector<Eigen::Vector3f> coords, int sample_type)
	{
		auto accessor = m_grid->getConstAccessor();


		std::vector<ValueT> values(coords.size());

		switch (sample_type)
		{
		case(1):
		{
			openvdb::tools::GridSampler<GridType::ConstAccessor, openvdb::tools::BoxSampler> sampler1(accessor, m_grid->transform());
			for (int i = 0; i < coords.size(); ++i)
			{
				values[i] = sampler1.wsSample(
					openvdb::Vec3R(coords[i].x(), coords[i].y(), coords[i].z()));
			}
			break;
		}
		case(2):
		{
			openvdb::tools::GridSampler<GridType::ConstAccessor, openvdb::tools::QuadraticSampler> sampler2(accessor, m_grid->transform());

			for (int i = 0; i < coords.size(); ++i)
			{
				values[i] = sampler2.wsSample(
					openvdb::Vec3R(coords[i].x(), coords[i].y(), coords[i].z()));
			}
			break;
		}
		default:
		{
			openvdb::tools::GridSampler<GridType::ConstAccessor, openvdb::tools::PointSampler> sampler3(accessor, m_grid->transform());

			for (int i = 0; i < coords.size(); ++i)
			{
				values[i] = sampler3.wsSample(
					openvdb::Vec3R(coords[i].x(), coords[i].y(), coords[i].z()));
			}
			break;
		}
		}
		return values;
	}

	void DeepSight::CtLog::filter(int width, int iterations, int type)
	{
		openvdb::tools::Filter <GridType> tool(*m_grid);
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

	void DeepSight::CtLog::offset(ValueT amount)
	{
		openvdb::tools::Filter<GridType> tool(*m_grid);
		tool.offset(amount);
	}

	openvdb::math::CoordBBox DeepSight::CtLog::bounding_box()
	{
		return m_grid->evalActiveVoxelBoundingBox();
	}

	Eigen::Matrix4d DeepSight::CtLog::get_transform()
	{
		auto mat = m_grid->transform().baseMap()->getAffineMap()->getMat4();

		return Eigen::Matrix4d(mat.asPointer());
	}

	void DeepSight::CtLog::set_transform(Eigen::Matrix4d mat)
	{
		openvdb::Mat4R omat(mat.data());
		openvdb::math::Transform::Ptr linearTransform =
			openvdb::math::Transform::createLinearTransform(omat);

		m_grid->setTransform(linearTransform);
	}

	void DeepSight::CtLog::to_mesh(float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& faces)
	{
		openvdb::tools::VolumeToMesh mesher(isovalue);
		mesher(*m_grid);

		openvdb::Coord ijk;

		//verts.clear();
		//verts.resize(mesher.pointListSize());

		for (size_t i = 0, N = mesher.pointListSize(); i < N; ++i) 
		{
			const openvdb::Vec3s& vert = mesher.pointList()[i];
			//Eigen::Vector3f v3(vert.asPointer());
			//verts.push_back(Eigen::Vector3f(v3));

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
				//Eigen::Vector4i v4(quad.asPointer());

				//faces.push_back(v4);

				faces.push_back(Eigen::Vector4i((int)quad.x(), (int)quad.y(), (int)quad.z(), (int)quad.w()));
			}
		}

	}

	DeepSight::CtLog* DeepSight::CtLog::resample(float scale)
	{
		GridType::Ptr target = GridType::create();

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

		DeepSight::CtLog* new_log = new DeepSight::CtLog();
		new_log->m_grid = target;

		return new_log;

		/*
		openvdb::FloatGrid::Ptr dest = openvdb::FloatGrid::create();
		dest->setTransform( openvdb::math::Transform::createLinearTransform( 2.0f ) ); // org voxel size is 1.0f
		openvdb::tools::resampleToMatch<openvdb::tools::BoxSampler>( *org, *dest );
		*/
	}


/* Export functions */
/* --------------------------------------- */
namespace DeepSight
{
	CtLog* CtLog_Create()
	{
		debug_grid_counter++;
		return new CtLog();
	}

	void CtLog_Delete(CtLog* ptr)
	{
		debug_grid_counter--;
		delete ptr;
	}

	int CtLog_debug_grid_counter()
	{
		return debug_grid_counter;
	}

	void CtLog_readVDB(CtLog* ptr, const char* filename)
	{
		ptr->readVDB(filename);
	}

	void CtLog_evaluate(CtLog* ptr, int num_coords, float* coords, ValueT* results, int sample_type)
	{
		//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords);
		//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords);
		//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map);

		std::vector<Eigen::Vector3f> xyz(num_coords);
		for (int i = 0; i < num_coords; ++i)
		{
			xyz[i] = Eigen::Vector3f(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
		}

		auto res = ptr->evaluate(xyz, sample_type);
		std::copy(res.begin(), res.end(), results);
	}

	void CtLog_filter(CtLog* ptr, int width, int iterations, int type)
	{
		ptr->filter(width, iterations, type);
	}

	void CtLog_offset(CtLog* ptr, ValueT amount)
	{
		ptr->offset(amount);
	}

	void CtLog_bounding_box(CtLog* ptr, float* min, float* max)
	{
		openvdb::CoordBBox bb = ptr->bounding_box();

		min[0] = bb.min().x();
		min[1] = bb.min().y();
		min[2] = bb.min().z();

		max[0] = bb.max().x();
		max[1] = bb.max().y();
		max[2] = bb.max().z();
	}

	void CtLog_set_transform(CtLog* ptr, float* mat)
	{
		Eigen::Matrix4f matf(mat);
		ptr->set_transform(matf.cast<double>());
		//ptr->set_transform(Eigen::Matrix4d(mat));
	}

	void CtLog_get_transform(CtLog* ptr, float* mat)
	{
		Eigen::Matrix4f matf = ptr->get_transform().cast<float>();
		std::copy(matf.data(), matf.data() + matf.size(), mat);
		//Eigen::Matrix4d matd = ptr->get_transform();
		//std::copy(matd.data(), matd.data() + matd.size(), mat);
	}

	void CtLog_to_mesh(CtLog* ptr, DeepSight::QuadMesh* mesh_ptr, float isovalue)
	{
		ptr->to_mesh(isovalue, *(mesh_ptr->vertices), *(mesh_ptr->faces));
	}

	CtLog* CtLog_resample(CtLog* ptr, float scale)
	{
		return ptr->resample(scale);
	}

}




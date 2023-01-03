#include "pyGrid.h"

#include <pybind11/pybind11.h>
#include <pybind11/eigen.h>
#include <pybind11/stl.h>
#include <pybind11/numpy.h> 

#include <iostream>

#include "../deepsight/Grid.h"
#include "pyutil.h"

#if defined(_WIN32) || defined(_WIN64)
# if defined(_WIN64)
typedef __int64 LONG_PTR;
# else
typedef long LONG_PTR;
# endif
typedef LONG_PTR SSIZE_T;
typedef SSIZE_T ssize_t;
#endif

namespace py = pybind11;

/**
 * \brief Returns py:array<T> from vector<T>. Efficient as zero-copy.
 * - Uses std::move to obtain ownership of said vector and transfer everything to the heap.
 * - Only accepts parameter using std::move(...), or else the vector metadata on the stack will go out of scope (heap data will always be fine).
 * \tparam T Type.
 * \param passthrough Numpy array.
 * \return py::array_t<T> with a clean and safe reference to contents of Numpy array.
 */
template<typename T>
inline py::array_t<T> toPyArray(std::vector<T>&& passthrough)
{
	// Pass result back to Python.
	// Ref: https://stackoverflow.com/questions/54876346/pybind11-and-stdvector-how-to-free-data-using-capsules
	auto* transferToHeapGetRawPtr = new std::vector<T>(std::move(passthrough));
	// At this point, transferToHeapGetRawPtr is a raw pointer to an object on the heap. No unique_ptr or shared_ptr, it will have to be freed with delete to avoid a memory leak.

	// Alternate implementation: use a shared_ptr or unique_ptr, but this appears to be more difficult to reason about as a raw pointer (void *) is involved - how does C++ know which destructor to call?

	const py::capsule freeWhenDone(transferToHeapGetRawPtr, [](void* toFree) {
		delete static_cast<std::vector<T>*>(toFree);
		//fmt::print("Free memory."); // Within Python, clear memory to check free: sys.modules[__name__].__dict__.clear()
		});

	auto passthroughNumpy = py::array_t<T>(/*shape=*/{ transferToHeapGetRawPtr->size() }, /*strides=*/{ sizeof(T) }, /*ptr=*/transferToHeapGetRawPtr->data(), freeWhenDone);
	return passthroughNumpy;
}

template<typename T>
void grid(py::module& m, std::string class_name)
{
	using GridType = typename DeepSight::Grid<T>;
	using GridPtr = typename GridType::Ptr;

	py::class_<GridType, GridPtr>(m, class_name.c_str())
		.def(py::init())
		.def_property("name", &GridType::get_name, &GridType::set_name)
		.def("read", &GridType::read)
		.def("write", &GridType::write)
		.def("from_vdb", &GridType::from_vdb)
		//.def("grid_names", &DeepSight::Grid::grid_names)
		.def_property_readonly("bounding_box", &GridType::bounding_box)
		.def("get_value", &GridType::get_value)
		.def("get_values", &GridType::get_values)
		.def("get_neighbourhood", &GridType::get_neighbourhood)
		.def("set_value", &GridType::set_value)
		.def("set_values", &GridType::set_values)
		.def("get_dense", [](GridType& grid, Eigen::Vector3i min, Eigen::Vector3i max)
			{
				std::vector<ssize_t> shape{ max.x() - min.x() + 1, max.y() - min.y() + 1, max.z() - min.z() + 1 };
				std::vector<T> data = grid.get_dense(min, max);

				return py::array_t<T, py::array::c_style | py::array::forcecast>(shape, // shape
					{ shape[2] * shape[1] * sizeof(T), shape[2] * sizeof(T), sizeof(T) },  // strides
					//& data[0]
					data.data()
					);
			})
		.def("get_interpolated_value", &GridType::get_interpolated_value)
		.def("get_interpolated_values", &GridType::get_interpolated_values)
		.def_property("transform", &GridType::get_transform, &GridType::set_transform)
		.def("transform_grid", &GridType::transform_grid)
		.def("get_dense", &GridType::get_dense)
		.def("get_active_voxels", &GridType::get_active_voxels)
		//.def("transform", (void (DeepSight::Grid::*)(Eigen::Matrix4d)) &DeepSight::Grid::transform)
		.def("gradient", &GridType::gradient)
		.def("laplacian", &GridType::laplacian)
		.def("mean_curvature", &GridType::mean_curvature)
		.def("normalize", &GridType::normalize)
		.def("filter", &GridType::filter)
		.def("dilate", &GridType::dilate)
		.def("erode", &GridType::erode);
		//.def("__iter__", [](const GridType::active_voxels & s) { return py::make_iterator(s.begin(), s.end()); },
					//py::keep_alive<0, 1>() /* Essential: keep object alive while iterator exists */)
}




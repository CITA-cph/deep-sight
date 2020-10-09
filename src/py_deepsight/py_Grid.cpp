#include <pybind11/pybind11.h>
#include <pybind11/eigen.h>
#include <pybind11/stl.h>
#include <pybind11/numpy.h>

#include <iostream>

#include "core/Grid.h"

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
	
	const py::capsule freeWhenDone(transferToHeapGetRawPtr, [](void *toFree) {				
		delete static_cast<std::vector<T> *>(toFree);
		//fmt::print("Free memory."); // Within Python, clear memory to check free: sys.modules[__name__].__dict__.clear()
	});
	
	auto passthroughNumpy = py::array_t<T>(/*shape=*/{transferToHeapGetRawPtr->size()}, /*strides=*/{sizeof(T)}, /*ptr=*/transferToHeapGetRawPtr->data(), freeWhenDone);
	return passthroughNumpy;	
}


void grid(py::module &m)
{
	py::class_<DeepSight::Grid, DeepSight::Grid::Ptr>(m, "Grid")
	    .def(py::init())
	    .def_property("name", &DeepSight::Grid::get_name, &DeepSight::Grid::set_name)
	    .def("read", &DeepSight::Grid::read)
	    .def("write", &DeepSight::Grid::write)
	    .def("from_multipage_tiff", &DeepSight::Grid::from_multipage_tiff)
	    .def("from_many_tiffs", &DeepSight::Grid::from_many_tiffs)
	    .def("from_vdb", &DeepSight::Grid::from_vdb)
	    //.def("grid_names", &DeepSight::Grid::grid_names)
	    .def("get_bounding_box", &DeepSight::Grid::getBoundingBox)
	    .def("get_value", &DeepSight::Grid::getValue)
	    .def("get_dense", [](DeepSight::Grid &grid, Eigen::Vector3i min, Eigen::Vector3i max)
	    	{
	    		std::vector<ssize_t> shape {max.x() - min.x() + 1, max.y() - min.y() + 1, max.z() - min.z() + 1};
	    		std::vector<float> data = grid.getDense(min, max);

    		    return py::array_t<float, py::array::c_style | py::array::forcecast>(shape, // shape
                      {shape[2] * shape[1] * sizeof(float), shape[2] * sizeof(float), sizeof(float)},  // strides
                      data.data()
                      ); 
	    	})
	    .def("get_interpolated_value", &DeepSight::Grid::getInterpolatedValue)
	    .def("get_values", &DeepSight::Grid::getValues)
	    .def_property("transform", &DeepSight::Grid::get_transform, &DeepSight::Grid::set_transform)
	    .def("transform_grid", &DeepSight::Grid::transform_grid)
	    //.def("transform", (void (DeepSight::Grid::*)(Eigen::Matrix4d)) &DeepSight::Grid::transform)
	    .def("get_interpolated_values", &DeepSight::Grid::getInterpolatedValues);

}


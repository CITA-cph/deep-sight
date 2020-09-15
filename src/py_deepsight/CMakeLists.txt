cmake_minimum_required (VERSION 2.6)

cmake_policy(SET CMP0081 NEW)
cmake_policy(SET CMP0015 NEW)
cmake_policy(SET CMP0019 NEW)

project(_deepsight)

set (PyDeepSight_VERSION_MAJOR 0)
set (PyDeepSight_VERSION_MINOR 1)

message(STATUS ${PROJECT_SOURCE_DIR}/../../bin)

set(DEEPSIGHT_DIR ${PROJECT_SOURCE_DIR}/../..)
set(DEEPSIGHT_DEPS_DIR ${DEEPSIGHT_DIR}/dep/)

include_directories(${PROJECT_SOURCE_DIR})

include_directories(
	"C:/lib/cpp/boost_1_72_0"
	"C:/lib/cpp/eigen-3.3.7"
	"C:/git/eigen"
	"${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/include"
	"${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/include/OpenEXR"
	"${DEEPSIGHT_DEPS_DIR}openvdb/include"
	"${DEEPSIGHT_DEPS_DIR}tbb/include"
	"${DEEPSIGHT_DEPS_DIR}tiff-4.1.0/include"
	"${PROJECT_SOURCE_DIR}/../deepsight"
	)

link_directories(
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib
	${DEEPSIGHT_DEPS_DIR}openvdb/lib
	${DEEPSIGHT_DEPS_DIR}tbb/lib/intel64/vc14
	${DEEPSIGHT_DEPS_DIR}tiff-4.1.0/lib
	)

set(DEEPSIGHT_LIBS 
	${DEEPSIGHT_DEPS_DIR}openvdb/lib/openvdb.lib
	${DEEPSIGHT_DEPS_DIR}openvdb/lib/libopenvdb.lib
	${DEEPSIGHT_DEPS_DIR}tbb/lib/intel64/vc14/tbb.lib
	${DEEPSIGHT_DEPS_DIR}tiff-4.1.0/lib/tiff.lib
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib/Half-2_4.lib
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib/Iex-2_4.lib
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib/IexMath-2_4.lib
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib/IlmImf-2_4.lib
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib/IlmImfUtil-2_4.lib
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib/IlmThread-2_4.lib
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib/Imath-2_4.lib
	)

#add_definitions(-DDEBUG)

add_subdirectory(${DEEPSIGHT_DEPS_DIR}/pybind11 ${DEEPSIGHT_DIR}/build/pybind11)
pybind11_add_module(_deepsight
	../deepsight/config.h
	../deepsight/core/Grid.cpp

	py_DeepSight.cpp
	py_Grid.cpp
)

target_link_libraries(_deepsight ${DEEPSIGHT_LIBS})

set_target_properties( _deepsight PROPERTIES
  LIBRARY_OUTPUT_DIRECTORY_DEBUG   ${CMAKE_SOURCE_DIR}/../../bin 
  LIBRARY_OUTPUT_DIRECTORY_RELEASE ${CMAKE_SOURCE_DIR}/../../bin
  ARCHIVE_OUTPUT_DIRECTORY_DEBUG   ${CMAKE_SOURCE_DIR}/../../bin 
  ARCHIVE_OUTPUT_DIRECTORY_RELEASE ${CMAKE_SOURCE_DIR}/../../bin
  RUNTIME_OUTPUT_DIRECTORY_DEBUG   ${CMAKE_SOURCE_DIR}/../../bin 
  RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_SOURCE_DIR}/../../bin
)
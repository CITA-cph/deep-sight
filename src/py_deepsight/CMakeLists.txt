cmake_minimum_required (VERSION 3.9)

cmake_policy(SET CMP0081 NEW)
cmake_policy(SET CMP0015 NEW)
cmake_policy(SET CMP0019 NEW)


#======== VCPKG setup ========#
message(" [INFO] VCPKG ENV = $ENV{VCPKG_ROOT}")
if(DEFINED ENV{VCPKG_ROOT} AND NOT DEFINED CMAKE_TOOLCHAIN_FILE)
  set(CMAKE_TOOLCHAIN_FILE "$ENV{VCPKG_ROOT}/scripts/buildsystems/vcpkg.cmake"
    CACHE STRING "")
endif()
message(" [INFO] VCPKG CMAKE_TOOLCHAIN_FILE = ${CMAKE_TOOLCHAIN_FILE}")

set(VCPKG_TARGET_TRIPLET "x64-windows")

#======== Python paths ========#
if(DEFINED ENV{PYTHON_PATH})
	set(PYTHON_EXECUTABLE "$ENV{PYTHON_PATH}/python.exe")
else()
	message(" [INFO] PYTHON_PATH not found. Setting manually...")
	set(PYTHON_EXECUTABLE "$ENV{LOCALAPPDATA}/Programs/Python/Python38/python.exe")
endif()
message(" [INFO] PYTHON_EXECUTABLE = ${PYTHON_EXECUTABLE}")

#======== project configuration ========#
project(_deepsight)
set(CMAKE_CXX_STANDARD 17)     

set (PyDeepSight_VERSION_MAJOR 0)
set (PyDeepSight_VERSION_MINOR 1)

message(STATUS ${PROJECT_SOURCE_DIR}/../../bin)

set(CMAKE_GENERATOR_PLATFORM "x64")
set(CMAKE_BUILD_TYPE "Release")

#======== dependencies ========#

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
	"${DEEPSIGHT_DEPS_DIR}glfw-3.3.2.bin.WIN64/include"
	"${DEEPSIGHT_DEPS_DIR}glad/include"	
	"${PROJECT_SOURCE_DIR}/../deepsight"
	)

link_directories(
	${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/lib
	${DEEPSIGHT_DEPS_DIR}openvdb/lib
	${DEEPSIGHT_DEPS_DIR}tbb/lib/intel64/vc14
	${DEEPSIGHT_DEPS_DIR}tiff-4.1.0/lib
	${DEEPSIGHT_DEPS_DIR}glfw-3.3.2.bin.WIN64/lib-vc2019
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
	${DEEPSIGHT_DEPS_DIR}glfw-3.3.2.bin.WIN64/lib-vc2019/glfw3dll.lib
	${DEEPSIGHT_DEPS_DIR}glfw-3.3.2.bin.WIN64/lib-vc2019/glfw3.lib
	)

#add_definitions(-DDEBUG)

add_subdirectory(${DEEPSIGHT_DEPS_DIR}/pybind11 ${DEEPSIGHT_DIR}/build/pybind11)
pybind11_add_module(_deepsight
	../deepsight/config.h
	../deepsight/core/Grid.cpp
	../deepsight/core/Window.cpp

	py_DeepSight.cpp
	py_Grid.cpp
	py_Window.cpp
)

target_link_libraries(_deepsight PRIVATE ${DEEPSIGHT_LIBS})

set_target_properties( _deepsight PROPERTIES
  LIBRARY_OUTPUT_DIRECTORY_DEBUG   ${CMAKE_SOURCE_DIR}/../../bin 
  LIBRARY_OUTPUT_DIRECTORY_RELEASE ${CMAKE_SOURCE_DIR}/../../bin
  ARCHIVE_OUTPUT_DIRECTORY_DEBUG   ${CMAKE_SOURCE_DIR}/../../bin 
  ARCHIVE_OUTPUT_DIRECTORY_RELEASE ${CMAKE_SOURCE_DIR}/../../bin
  RUNTIME_OUTPUT_DIRECTORY_DEBUG   ${CMAKE_SOURCE_DIR}/../../bin 
  RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_SOURCE_DIR}/../../bin
)
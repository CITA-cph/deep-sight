cmake_minimum_required(VERSION 3.10)

project(deepsight_all)

#======== VCPKG setup ========#

message(" [INFO] VCPKG ENV = $ENV{VCPKG_ROOT}")
if(DEFINED ENV{VCPKG_ROOT} AND NOT DEFINED CMAKE_TOOLCHAIN_FILE)
  set(CMAKE_TOOLCHAIN_FILE "$ENV{VCPKG_ROOT}/scripts/buildsystems/vcpkg.cmake"
    CACHE STRING "")
endif()
message(" [INFO] VCPKG CMAKE_TOOLCHAIN_FILE = ${CMAKE_TOOLCHAIN_FILE}")
include("${CMAKE_TOOLCHAIN_FILE}")

set(VCPKG_TARGET_TRIPLET "x64-windows")
set(VCPKG_LIB_DIR "${_VCPKG_INSTALLED_DIR}/${VCPKG_TARGET_TRIPLET}/lib")

#======== DEEPSIGHT FILES ========

set(DEEPSIGHT_DIR ${CMAKE_CURRENT_SOURCE_DIR})

set(DEEPSIGHT_DEPS_DIR ${DEEPSIGHT_DIR}/dep)

set(DEEPSIGHT_SOURCE_FILES
	"${DEEPSIGHT_DEPS_DIR}/glad/src/glad.c"
	"${DEEPSIGHT_DIR}/src/deepsight/core/Grid.cpp"
	"${DEEPSIGHT_DIR}/src/deepsight/core/Window.cpp"
	)

set(DEEPSIGHT_INCLUDE_DIR
	"${DEEPSIGHT_DIR}/src/deepsight"
	"${DEEPSIGHT_DEPS_DIR}/glad/include"
	"${DEEPSIGHT_DEPS_DIR}/glfw/include"
	"C:/git/eigen"
	"${_VCPKG_INSTALLED_DIR}/${VCPKG_TARGET_TRIPLET}/include"
	)

set(DEEPSIGHT_LINK_DIR
	"${_VCPKG_INSTALLED_DIR}/${VCPKG_TARGET_TRIPLET}/lib"
	"${DEEPSIGHT_DEPS_DIR}/glfw/lib"
	)

set(DEEPSIGHT_LIBS 
	openvdb.lib
	tbb.lib
	tiff.lib
	glfw3.lib
	Half-2_5.lib
	)

set(DEEPSIGHT_OUTPUT_DIR ${DEEPSIGHT_DIR}/bin)

#======== Set warning level, disable some warnings ========

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -MP -W4 -wd4251 -wd4146 -wd4127")

#======== Set some defines ========

add_compile_definitions(_USE_MATH_DEFINES)


#======== Add targets ========

add_subdirectory("src/py_deepsight" "_deepsight")
#add_subdirectory("src/dsTest" "dsTest")

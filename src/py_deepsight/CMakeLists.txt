cmake_minimum_required (VERSION 3.10)

cmake_policy(SET CMP0081 NEW)
cmake_policy(SET CMP0015 NEW)
cmake_policy(SET CMP0019 NEW)


#======== Python paths ========#

if(DEFINED ENV{PYTHON_PATH})
	set(PYTHON_EXECUTABLE "$ENV{PYTHON_PATH}/python.exe")
else()
	message(" [INFO] PYTHON_PATH not found. Setting manually...")
	set(PYTHON_EXECUTABLE "$ENV{LOCALAPPDATA}/Programs/Python/Python38/python.exe")
endif()
message(" [INFO] PYTHON_EXECUTABLE = ${PYTHON_EXECUTABLE}")


#======== project configuration ========#

project(_deepsight LANGUAGES CXX C)


find_package(OpenGL REQUIRED)


# Set the global minimum C++ standard for the project (this is C++11, since we use nullptr)
if(NOT DEFINED CMAKE_CXX_STANDARD)
    set(CMAKE_CXX_STANDARD 17)
    set(CMAKE_CXX_STANDARD_REQUIRED ON)
endif()

set (PyDeepSight_VERSION_MAJOR 0)
set (PyDeepSight_VERSION_MINOR 1)

message(STATUS ${PROJECT_SOURCE_DIR}/../../bin)

set(CMAKE_GENERATOR_PLATFORM "x64")
set(CMAKE_BUILD_TYPE "Release, Debug")

#======== target configurations ========#


# include_directories(${PROJECT_SOURCE_DIR})

# include_directories(
# 	"C:/lib/cpp/boost_1_72_0"
# 	"C:/lib/cpp/eigen-3.3.7"
# 	"C:/git/eigen"
# 	"${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/include"
# 	"${DEEPSIGHT_DEPS_DIR}openexr-2.4.0/include/OpenEXR"
# 	"${DEEPSIGHT_DEPS_DIR}openvdb/include"
# 	"${DEEPSIGHT_DEPS_DIR}tbb/include"
# 	"${DEEPSIGHT_DEPS_DIR}tiff-4.1.0/include"
# 	"${DEEPSIGHT_DEPS_DIR}glfw-3.3.2.bin.WIN64/include"
# 	"${DEEPSIGHT_DEPS_DIR}glad/include"	
# 	"${PROJECT_SOURCE_DIR}/../deepsight"
# 	)

link_directories(${DEEPSIGHT_LINK_DIR}
	)


add_subdirectory(${DEEPSIGHT_DEPS_DIR}/pybind11 ${CMAKE_BINARY_DIR}/build/pybind11)

pybind11_add_module(_deepsight
	${DEEPSIGHT_SOURCE_FILES}

	py_DeepSight.cpp
	py_Grid.cpp
	py_Window.cpp
	)

target_sources(_deepsight
	PUBLIC
	${DEEPSIGHT_SOURCE_FILES}
	)

target_include_directories(_deepsight
	PUBLIC ${DEEPSIGHT_INCLUDE_DIR}
	)

target_link_libraries(_deepsight PRIVATE ${DEEPSIGHT_LIBS})

set_target_properties( _deepsight PROPERTIES
  LIBRARY_OUTPUT_DIRECTORY_DEBUG   ${DEEPSIGHT_OUTPUT_DIR}
  LIBRARY_OUTPUT_DIRECTORY_RELEASE ${DEEPSIGHT_OUTPUT_DIR}
  ARCHIVE_OUTPUT_DIRECTORY_DEBUG   ${DEEPSIGHT_OUTPUT_DIR}
  ARCHIVE_OUTPUT_DIRECTORY_RELEASE ${DEEPSIGHT_OUTPUT_DIR}
  RUNTIME_OUTPUT_DIRECTORY_DEBUG   ${DEEPSIGHT_OUTPUT_DIR}
  RUNTIME_OUTPUT_DIRECTORY_RELEASE ${DEEPSIGHT_OUTPUT_DIR}
)

install(TARGETS _deepsight DESTINATION ${DEEPSIGHT_OUTPUT_DIR})


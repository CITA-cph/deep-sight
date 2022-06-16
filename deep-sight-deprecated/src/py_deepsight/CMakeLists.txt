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

set(PROJECT_NAME _deepsight)
project(${PROJECT_NAME} LANGUAGES CXX C)

#set(PROJECT_OUTPUT_DIR "${DEEPSIGHT_OUTPUT_DIR}/${PROJECT_NAME}")
set(PROJECT_OUTPUT_DIR "${DEEPSIGHT_OUTPUT_DIR}")

# find_package(OpenGL REQUIRED)

# Set the global minimum C++ standard for the project (this is C++11, since we use nullptr)
if(NOT DEFINED CMAKE_CXX_STANDARD)
    set(CMAKE_CXX_STANDARD 17)
    set(CMAKE_CXX_STANDARD_REQUIRED ON)
endif()

set(CMAKE_GENERATOR_PLATFORM "x64")
set(CMAKE_BUILD_TYPE "Release, Debug")


link_directories(${DEEPSIGHT_LINK_DIR}
	)


add_subdirectory(${DEEPSIGHT_DEPS_DIR}/pybind11 ${CMAKE_BINARY_DIR}/build/pybind11)

pybind11_add_module(${PROJECT_NAME}
	${DEEPSIGHT_SOURCE_FILES}

	#pyGrid.cpp
	#pyGrid-impl.cpp
	pyDeepSight.cpp
	pyDeepSightGrid.cpp
	#pyWindow.cpp
	)

target_sources(${PROJECT_NAME}
	PUBLIC
	${DEEPSIGHT_SOURCE_FILES}
	)

target_include_directories(${PROJECT_NAME}
	PUBLIC ${DEEPSIGHT_INCLUDE_DIR}
	)

target_link_libraries(${PROJECT_NAME} PRIVATE ${DEEPSIGHT_LIBS})

set_target_properties(${PROJECT_NAME} PROPERTIES
  LIBRARY_OUTPUT_DIRECTORY_DEBUG   ${PROJECT_OUTPUT_DIR}
  LIBRARY_OUTPUT_DIRECTORY_RELEASE ${PROJECT_OUTPUT_DIR}
  ARCHIVE_OUTPUT_DIRECTORY_DEBUG   ${PROJECT_OUTPUT_DIR}
  ARCHIVE_OUTPUT_DIRECTORY_RELEASE ${PROJECT_OUTPUT_DIR}
  RUNTIME_OUTPUT_DIRECTORY_DEBUG   ${PROJECT_OUTPUT_DIR}
  RUNTIME_OUTPUT_DIRECTORY_RELEASE ${PROJECT_OUTPUT_DIR}
)

install(TARGETS ${PROJECT_NAME} DESTINATION ${PROJECT_OUTPUT_DIR})


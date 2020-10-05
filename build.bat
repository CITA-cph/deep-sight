if not exist build mkdir build
cd build
cmake ..
cmake --build . --config Release
cd ..
copy_depends.bat

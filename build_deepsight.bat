if not exist build mkdir build
cd build
cmake ../src/py_deepsight 
cmake --build . --config Release
cd ..
copy_depends.bat

if not exist build mkdir build
cd build
cmake ../src/py_deepsight -DCMAKE_GENERATOR_PLATFORM=x64 -DCMAKE_BUILD_TYPE=Release -DPYTHON_EXECUTABLE:FILEPATH="%APPDATA%/../Local/Programs/Python/Python38/python.exe"
cmake --build . --config Release
cd ..
copy_depends.bat

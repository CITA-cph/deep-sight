@echo off
if not exist bin mkdir bin
if not exist bin\blosc.dll copy dep\openvdb\bin\blosc.dll bin\blosc.dll
if not exist bin\tbb.dll copy dep\openvdb\bin\tbb.dll bin\tbb.dll
if not exist bin\zlib.dll copy dep\openvdb\bin\zlib.dll bin\zlib.dll
if not exist bin\openvdb.dll copy dep\\openvdb\bin\openvdb.dll bin\openvdb.dll
if not exist bin\Half-2_4.dll copy dep\openvdb\bin\Half-2_4.dll bin\Half-2_4.dll
if not exist bin\Iex-2_4.dll copy dep\openvdb\bin\Iex-2_4.dll bin\Iex-2_4.dll
if not exist bin\IexMath-2_4.dll copy dep\openvdb\bin\IexMath-2_4.dll bin\IexMath-2_4.dll
if not exist bin\IlmImf-2_4.dll copy dep\openvdb\bin\IlmImf-2_4.dll bin\IlmImf-2_4.dll
if not exist bin\IlmImfUtil-2_4.dll copy dep\openvdb\bin\IlmImfUtil-2_4.dll bin\IlmImfUtil-2_4.dll
if not exist bin\IlmThread-2_4.dll copy dep\openvdb\bin\IlmThread-2_4.dll bin\IlmThread-2_4.dll
if not exist bin\Imath-2_4.dll copy dep\openvdb\bin\Imath-2_4.dll bin\Imath-2_4.dll
if not exist bin\tiff.dll copy dep\tiff-4.1.0\lib\tiff.dll bin\tiff.dll
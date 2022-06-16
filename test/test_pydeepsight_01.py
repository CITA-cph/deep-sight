'''
Copyright 2022 CITA

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
'''
try:
    import pydeepsight
except ImportError:
    print("Importing local build...")
    import importlib.util, sys
    spec = importlib.util.spec_from_file_location("pydeepsight", os.path.abspath("../bin/pydeepsight.cp{}{}-win_amd64.pyd".format(sys.version_info.major, sys.version_info.minor)))
    pydeepsight = importlib.util.module_from_spec(spec)
from pydeepsight import FloatGrid, IntGrid, VectorGrid, load_scalar_tiff, load_infolog

import math

def test_tiff():
	tiff_path = r"C:\Users\tsvi\OneDrive - Det Kongelige Akademi\03_Projects\2019_RawLam\Data\Microtec\20220301.142735.DK.feb.log01_char\20220301.142735.DK.feb.log01_char_mp.tiff"

	grid = load_scalar_tiff(tiff_path, 1.0e-3, 0)
	bb = grid.bounding_box
	print(bb)

def test_infolog():
	tiff_path = r"C:\Users\tsvi\OneDrive - Det Kongelige Akademi\03_Projects\2019_RawLam\Data\Microtec\20220301.142735.DK.feb.log01_char\infolog@20220301.142735.DK.feb.log01_char.tiff"
	ilog = load_infolog(tiff_path, True)

	print(ilog.pith)
	print(ilog.knots)


def test_vdb():
	log_path = r"C:\Users\tsvi\OneDrive - Det Kongelige Akademi\03_Projects\2019_RawLam\Data\Microtec\20220301.142735.DK.feb.log01_char\20220301.142735.DK.feb.log01_char.vdb"

	print("Loading VDB...")
	grid = FloatGrid.read(log_path, 1.0e-3, 0)
	bb = grid.bounding_box
	print(bb)

	grid.set_value([0,0,0], 120)
	grid.set_value([0,1,0], 150)
	grid.set_value([0,1,1], 40)
	grid.set_value([0,0,1], 170)

	v = grid.get_value([0,0,0])
	print(v)

	n = grid.get_neighbourhood([0,0,0])
	print(n)

def test_create():
	grid = FloatGrid()
	grid.name = "density"
	for x in range(50):
		for y in range(50):
			for z in range(4):
				r = math.sqrt((x -25)**2 + (y - 25)**2 + (z - 2)**2) * 0.1
				grid.set_value([x, y, z], r)

	grid.transform = [[0.1,0,0,2],
					[0,0.1,0,0],
					[0,0,0.1,3],
					[0,0,0,1]]
	grid.write(r"C:/tmp/create_grid.vdb", False)

def convert_fungar():
	fungar_path = r"Q:\CITA\PROJECTS\BEHAVING_ARCHITECTURES\2020-DeepSight\01_DataSets\Fungar\00_GlaAI.tif"
	grid = load_scalar_tiff(fungar_path, 0.5e-1, 0)
	grid.name = "density"
	grid.transform = [[0.01,0,0,-0.5],
					[0,0.01,0,-0.5],
					[0,0,0.01,0],
					[0,0,0,1]]

	grid.filter(2, 2, 1)
	grid.write(r"C:/tmp/fungar7.vdb", True)

def main():
	print(dir(pyrawlam))
	print(dir(FloatGrid))

	vgrid = VectorGrid()

	v = None
	vgrid.set_value([0,0,0], [10,11,12])
	vgrid.set_value([1,1,1], [1,1,3])

	print("Got this far!")
	v = vgrid.get_value([0,0,0])
	print(v)
	v = vgrid.get_value([1,1,1])
	print(v)

	#test_tiff()
	#test_vdb()
	#convert_fungar()
	test_create()
	#test_infolog()





if __name__=="__main__":
	main()
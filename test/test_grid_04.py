'''
Copyright 2020 CITA

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
import os
os.chdir(os.path.dirname(os.path.abspath(__file__)))
DEEPSIGHT_DIR = os.getenv('DEEPSIGHT_DIR')
if DEEPSIGHT_DIR is None:
    DEEPSIGHT_DIR = ""
    
try:
    import _deepsight as deepsight
except ImportError:
    print("Importing local build.")
    import importlib.util, sys
    spec = importlib.util.spec_from_file_location("_deepsight", os.path.abspath("../bin/_deepsight.cp{}{}-win_amd64.pyd".format(sys.version_info.major, sys.version_info.minor)))
    deepsight = importlib.util.module_from_spec(spec)

import numpy

'''
Plot a slice of a grid
'''
def plot_image(grid, z):
    bb = grid.getBoundingBox()

    bmin = bb[0]
    bmax = bb[1]

    bmin[2] = z
    bmax[2] = z

    w = bmax[0] - bmin[0]
    h = bmax[1] - bmin[1]
    d = bmax[2] - bmin[2]

    dense = grid.getDense(bmin, bmax)

    import matplotlib.pyplot as plt
    import matplotlib.image as mpimg
    imgplot = plt.imshow(dense, interpolation='none')
    plt.show()

def main():
    # Pick a file with a file dialog
    from tkinter import Tk, filedialog

    root = Tk()
    file_path = filedialog.askopenfilename(
        initialdir = os.getcwd(),
        title = "Select file",
        filetypes = (("TIFF files","*.tif, *.tiff"),("VDB files","*.vdb")))
    root.destroy()

    if not file_path:
        return

    # Load grid
    print("Loading {}...".format(file_path))
    grid = deepsight.Grid.read(file_path)

    bmin, bmax = grid.get_bounding_box()
    print("x {}-{}\ny {}-{}\nz {}-{}".format(bmin[0], bmax[0], bmin[1], bmax[1], bmin[2], bmax[2]))

    mat = grid.get_transform()
    print(mat)

    # Get some interpolated values at various points
    pts = [
        [350.5,600.2,212.5],
        [400.5,650.89,200.13],
        [300.0,700.2,200.4]]

    res = grid.getInterpolatedValues(pts)
    print("Interpolated values: {}".format(res))    

    # Transform the grid with a 4x4 matrix

    xform = [
        [1, 0, 0, 30],
        [0, 1, 0, -20],
        [0, 0, 1, 10],
        [0, 0, 0, 1],
        ]

    print("Transforming...")
    grid.transform_grid(xform);

    bmin, bmax = grid.getBoundingBox()
    print("x {}-{}\ny {}-{}\nz {}-{}".format(bmin[0], bmax[0], bmin[1], bmax[1], bmin[2], bmax[2]))

    res = grid.getInterpolatedValues(pts)
    print("Interpolated values: {}".format(res))   

if __name__ == "__main__":
    main()
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
    from os.path import abspath
    spec = importlib.util.spec_from_file_location("_deepsight", abspath("../bin/_deepsight.cp{}{}-win_amd64.pyd".format(sys.version_info.major, sys.version_info.minor)))
    deepsight = importlib.util.module_from_spec(spec)

import numpy

def query_grid(grid, grid_name):

    bb = grid.getBoundingBox(grid_name)

    bmin = bb[0]
    bmax = bb[1]

    print(bmin)
    print(bmax)

    return grid.getDense(grid_name, bmin, bmax)

'''
Plot a slice of a grid
'''
def plot_image(grid, grid_name, z):
    bb = grid.getBoundingBox(grid_name)

    bmin = bb[0]
    bmax = bb[1]

    bmin[2] = z
    bmax[2] = z

    w = bmax[0] - bmin[0]
    h = bmax[1] - bmin[1]
    d = bmax[2] - bmin[2]

    dense = grid.getDense(grid_name, bmin, bmax)

    import matplotlib.pyplot as plt
    import matplotlib.image as mpimg
    imgplot = plt.imshow(dense, interpolation='none')
    plt.show()

def main():

    # Density at which to filter values
    density_cutoff = 0.0

    # Switch to use either VDB or TIFF
    vdb_path = r"data\p15_.vdb"    
    tiff_path = r"data\p15_.tiff"

    if True:
        path = vdb_path
    else:
        path = tiff_path

    print("Loading {}...".format(path))

    # Create grid object
    grid = deepsight.Grid()

    # Load appropriate file
    if path.endswith('vdb'):
        grid.read(path)
    elif path.endswith('tiff'):
        grid.from_multipage_tiff(path, "computed_tomography", 1.0e-4)

    # Print out all available grid names
    names = grid.grid_names()
    print("Found {} grid(s):".format(len(names)))

    for name in names:
        print("   {}".format(name))

    # Get dense representation of first grid
    q = query_grid(grid, names[0])

    # Plot values
    #plot_scatter3d(q, density_cutoff)
    plot_image(grid, names[0], 200)

if __name__ == "__main__":
    main()
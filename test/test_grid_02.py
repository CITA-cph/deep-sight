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

try:
    import _deepsight as deepsight
except ImportError:
    import importlib.util, sys
    from os.path import abspath
    spec = importlib.util.spec_from_file_location("_deepsight", abspath("../bin/_deepsight.cp{}{}-win_amd64.pyd".format(sys.version_info.major, sys.version_info.minor)))
    deepsight = importlib.util.module_from_spec(spec)

import numpy, os

def main():

    # Switch to use either VDB or TIFF
    vdb_path = r"data\p15_.vdb" 
    tiff_path = r"data\p15_.tiff"
    #tiff_path = r"data\00_GlaAI.tif"

    if False:
        path = vdb_path
    else:
        path = tiff_path

    print("Loading {}...".format(os.path.abspath(path)))

    # Create grid object
    grid = deepsight.Grid()

    # Load appropriate file
    if path.endswith('vdb'):
        grid.load(path)
    elif path.endswith('tiff') or path.endswith('tif'):
        grid.from_multipage_tiff(path, "computed_tomography", 1.0e-4)

    # Print out all available grid names
    names = grid.grid_names()
    print("Found {} grid(s):".format(len(names)))

    if len(names) < 1:
        print("No grids are loaded.")
        return

    for name in names:
        print("   {}".format(name))

    # Choose first loaded grid
    grid_name = names[0]

    # Find extents of non-zero data in grid
    bmin, bmax = grid.getBoundingBox(grid_name)

    # Define Z-coordinate for an image slice
    z = 20

    # Get width, height, and depth dimensions
    w, h, d = bmax[0] - bmin[0], bmax[1] - bmin[1], bmax[2] - bmin[2]
    print("w {} h {} d {}".format(w, h, d))

    # Get dense representation of grid
    dense = grid.getDense(grid_name, bmin, bmax)

    print("Got dense grid.")

    # Visualize results
    import pyvista
    plotter = pyvista.Plotter(shape=(1,2))

    # Show full volume
    opacity = [0, 0, 0, 0.1, 0.3, 0.6, 1]
    #opacity = [0, 0, 0, 0.0, 0.0, 0.0, 1]
    plotter.add_volume(dense, cmap="viridis", opacity=opacity, show_scalar_bar=True, shade=False, resolution=(1,1,10))

    # Get image slice
    img_section = dense[0:dense.shape[0], 0:dense.shape[1], z:z+2]

    # Create pyvista grid
    pgrid = pyvista.UniformGrid()
    pgrid.dimensions = numpy.array(img_section.shape) + 1
    pgrid.cell_arrays["values"] = img_section.flatten(order="F")  # Flatten the array!

    plotter.subplot(0,1)
    opacity = [0,0,0,0,0.2,1]
    plotter.add_volume(pgrid, cmap="cool", opacity=opacity, show_scalar_bar=True, shade=False, resolution=(1,1,10))

    # Link and display
    plotter.link_views()
    plotter.show()

if __name__ == "__main__":
    main()




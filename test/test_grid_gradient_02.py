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

test_grid_gradient_01.py

This script shows how to extract the gradient and Laplacian of a grid.

Author: Tom Svilans

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

def load_grid_ui():
    # Pick a file with a file dialog
    from tkinter import Tk, filedialog

    root = Tk()
    file_path = filedialog.askopenfilename(
        initialdir = os.getcwd(),
        title = "Select file",
        filetypes = (("VDB files","*.vdb"), ("TIFF files","*.tif, *.tiff")))
    root.destroy()
    if not file_path:
        return (None, file_path)

    return (deepsight.Grid.read(file_path), file_path)

'''
Plot a slice of a grid
'''
def plot_image(grid, z):
    bb = grid.bounding_box

    bmin = bb[0].copy()
    bmax = bb[1].copy()

    bmin[2] = z
    bmax[2] = z

    w = bmax[0] - bmin[0]
    h = bmax[1] - bmin[1]
    d = bmax[2] - bmin[2]

    dense = grid.get_dense(bmin, bmax)

    import matplotlib.pyplot as plt
    import matplotlib.image as mpimg

    imgplot = plt.imshow(dense, interpolation='none', vmin=-2000, vmax=2000)
    imgplot.set_cmap("binary")
    plt.axis('off')    

    plt.show()

def main():

    grid, filepath = load_grid_ui()
    if not grid:
        return

    SLICE = 200
    FILTER = 2
 
    print("Filtering grid...")
    grid.dilate(FILTER * 2) # We need to dilate the grid by a small amount to prevent edge artefacts
    grid.filter(FILTER, 2) # Blur the grid using a Gaussian kernel of width FILTER

    print("Calculating Laplacian...")
    grid_laplacian = grid.laplacian() # Get the Laplacian of the grid

    # Visualize results
    import pyvista
    plotter = pyvista.Plotter(shape=(1,1))

    # Show full volume
    opacity = [1, 0.6, 0, 0.0, 0, 0.0, 1]
    dense = grid_laplacian.get_dense(grid.bounding_box[0], grid.bounding_box[1])

    plotter.add_volume(dense, cmap="viridis", opacity=opacity, show_scalar_bar=True, shade=False, resolution=(1,1,10))

    # Link and display
    plotter.link_views()
    plotter.update_scalar_bar_range(clim=(0,2000))
    plotter.show()

if __name__ == "__main__":
    main()
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

test_grid_05.py

This script shows how to generate a log histogram using matplotlib.

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

def main():

    grid, filepath = load_grid_ui()
    if not grid:
        return

    bmin, bmax = grid.getBoundingBox()
    dense = grid.getDense(bmin, bmax)


    # Create histogram along the Z-axis
    histogram = dense.sum(axis=(0,1))
    histogram = histogram / numpy.max(histogram)
    histogram_median = numpy.median(histogram)
    histogram_max = numpy.max(histogram)

    import matplotlib.pyplot as plt
    import matplotlib.image as mpimg

    cmap = 'binary'

    plt.figure(os.path.splitext(os.path.basename(filepath))[0] + "_histogram")
    plt.subplot(311)
    img = plt.imshow(dense.sum(axis=0), aspect=0.1)
    img.set_cmap(cmap)
    plt.axis('off')
    plt.subplot(312)
    img = plt.imshow(dense.sum(axis=1), aspect=0.1)
    img.set_cmap(cmap)
    plt.axis('off')    
    plt.subplot(313)
    #plt.yscale("logit")
    plt.ylim((histogram_median - histogram_max, (histogram_max - histogram_median) * 2))
    plt.plot(histogram - numpy.median(histogram), 'k', linewidth=1.0)
    plt.axis('off')
    ax = plt.gca();
    ax.set_xlim(0, dense.shape[2]);
    plt.subplots_adjust(hspace=0.1)

    # Toggle full screen (press F to exit fullscreen and close)
    mng = plt.get_current_fig_manager()
    mng.full_screen_toggle()

    # Show the plot
    plt.show()

if __name__ == "__main__":
    main()
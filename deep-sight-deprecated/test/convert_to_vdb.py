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
    print(sys.version_info)
    spec = importlib.util.spec_from_file_location("_deepsight", os.path.abspath("../bin/_deepsight.cp{}{}-win_amd64.pyd".format(sys.version_info.major, sys.version_info.minor)))
    deepsight = importlib.util.module_from_spec(spec)

import numpy

def main():
    # Pick a file with a file dialog
    from tkinter import Tk, filedialog

    root = Tk()
    file_path = filedialog.askopenfilename(
        initialdir = os.getcwd(),
        title = "Select file",
        filetypes = (("TIFF files","*.tif, *.tiff"),("VDB files","*.vdb")))

    if not file_path:
        return

    # Load grid
    print("Loading {}...".format(file_path))
    grid = deepsight.Grid.read(file_path)

    mat = [
        [0.001,0,0,0],
        [0,0.001,0,0],
        [0,0,0.01,0],
        [0,0,0,1]]

    grid.transform = mat
    grid.name = "density"

    file_path = filedialog.asksaveasfilename(
        #defaultextension=".vdb",
        initialdir = os.getcwd(),
        title = "Select file",
        filetypes = [("VDB files (*.vdb)","*.vdb")])

    if not file_path:
        return

    if not file_path.endswith(".vdb"):
        file_path = file_path + ".vdb"

    grid.write(file_path, True)

    root.destroy()


if __name__ == "__main__":
    main()
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

import numpy, filetype

def main():
    # Pick a file with a file dialog
    from tkinter import Tk, filedialog

    root = Tk()
    directory = filedialog.askdirectory(
        initialdir = os.getcwd(),
        title = "Select file")

    if not directory:
        return

    files = os.listdir(directory)

    files = [os.path.join(directory, f) for f in files if f.lower().endswith('tif') or f.lower().endswith('tiff')]
    files.sort()


    # Load grid
    grid = deepsight.Grid.from_many_tiffs(files, 0.001)

    mat = [
        [1,0,0,0],
        [0,1,0,0],
        [0,0,1,0],
        [0,0,0,1]]

    grid.set_transform(mat)
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

    grid.write(file_path)

    root.destroy()


if __name__ == "__main__":
    main()
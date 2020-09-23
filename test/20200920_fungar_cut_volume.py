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
 

20200920_fungar_cut_volume.py

This script shows how to load a volume from images and how to cut it with a cutter. It also shows how to implement a button.

Author: Sebastian Gatz

'''

import numpy as np
from vedo import *
from vedo.pyplot import histogram
import os
import glob
import cv2
import pyvista as pv


all_loaded_images = []

x_scale = 2
y_scale = 1
z_scale = 1
res = 2 #read every nth pixel in x,y,z


#set path
all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/all/"
os.chdir(all_images)


#load all images
types = ('*.gif', '*.png', '*.jpg', "*bmp", "*tif")
files = []
for f in types:
    files.extend(glob.glob(f))
print (len(files))



#create image stack
img_stack = 0
for i in range(0,len(files),res):
    print ("img loaded: ", i)
    img = cv2.imread(files[i],0)
    
    img = cv2.resize(img, (0,0), fx=1/res, fy=1/res) 

    if i == 0:
        img.fill(0)
        img_stack = img
    else:
        img_stack = np.dstack((img_stack,img))

img = cv2.imread(files[0],0)
img = cv2.resize(img, (0,0), fx=1/res, fy=1/res) 
img.fill(0)
img_stack = np.dstack((img_stack,img))
print ("image stack created")


#create volume
vol = Volume(img_stack, spacing=(x_scale,y_scale,z_scale), mapper="smart", mode=1, alpha= [0.0, 0.0, 0.5, 0.7, 1], c= "jet")

vp = Plotter(N=2, axes=True, bg="black", size="fullscreen") #(1000,1000))
vp.show("Voxel Crop", axes=0, at=0, viewup="y")

def buttonfunc():
    vp.close()
    bu.switch()

bu = vp.addButton(
    buttonfunc,
    pos=(0.5, 0.05),  # x,y fraction from bottom left corner
    states=["done"],
    c=["black"],
    bc=["w"],  # colors of states
    font="courier",   # arial, courier, times
    size=25,
    bold=False,
    italic=False,
)


vp.show(vol, "Voxel Render", axes=1, at=1)
vp.addCutterTool(vol)

vp.show(interactive=True)


"""

print ("nice")
vp = Plotter(N=1, axes=True, bg="black", size="fullscreen")
vp.show("Voxel Crop", axes=0, at=0)
"""
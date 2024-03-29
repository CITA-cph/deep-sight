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
 

20200923_fungar_network.py

Example Script to load images as a volume, extract points (based on brightness) for iterative skeletonization of points.

Author: Sebastian Gatz

'''

import numpy as np
from vedo import *
import os
import glob
import cv2

def main():
    all_loaded_images = []

    x_scale = 1
    y_scale = 1
    z_scale = 1
    res = 5 #read every nth pixel in x,y,z


    #set path
    all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/all/"
    #all_images = "C:/Users/sgat/OneDrive - KADK/Sebastian_after_Sync/06_bones_to_flesh/browsing male/(VKH) CT Images (494 X 281)/"
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


    #points from voxel
    pts = np.nonzero(img_stack > 120) #all voxels brigner than 100
    pts = tuple(zip(pts[2],pts[1],pts[0]))
    pts = Points(pts)


    #create plotter 
    vp = Plotter(N=6, axes=True, bg="black", size="fullscreen", sharecam=True) #(1000,1000))
    vp.show(vol, "Voxel Render", axes=1, at=0, viewup ="y")


    #skeltonize points
    N = 5    # nr of iterations
    f = 0.2  # fraction of neighbours

    for i in range(N):
        print ("skeletonized ", i, "of", N)
        pts = pts.clone().smoothMLS1D(f=f).color(i)
        vp.show(pts, "Skeletonization Step " + str(i), at=i+1, N=N)



    vp.show(interactive=True)

if __name__ == "__main__":
    main()
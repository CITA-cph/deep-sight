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
 

20201019_desnity.py

Shows Historgam of CT scan density

Author: Sebastian Gatz

'''

import numpy as np
from vedo import *
from vedo.pyplot import histogram
import os
import glob
import cv2
from natsort import natsorted, ns

all_loaded_images = []

x_scale = 1
y_scale = 1
z_scale = 1
res = 10 #read every nth pixel in x,y,z


#set path
#all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/all/"
#all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/zebicon-7a12a3/31853_2020-09-18_Wood_1_Top/"
#all_images = "C:/Users/sgat/OneDrive - KADK/Sebastian_after_Sync/06_bones_to_flesh/browsing male/(VKH) CT Images (494 X 281)/"
#all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/31853_2020-09-18_Cellulose_2_Top/"
#all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/31853_2020-09-18_Cellulose_1_Top/"
#all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/zebicon-ccc195/31853_2020-10_02_Fungar_1_Top/"
all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/zebicon-ccc195/31853_2020-10_02_Fungar_2_Top/"
#all_images = "C:/Users/sgat/OneDrive - KADK/03_CITA/59_DataViz/03_data/zebicon-ccc195/cropped_jpeg/"

os.chdir(all_images)


#load all images
types = ('*.gif', '*.png', '*.jpg', "*bmp", "*tif")
files = []
for f in types:
    files.extend(glob.glob(f))
print (len(files))

files = natsorted(files, key=lambda y: y.lower())


#create image stack
img_stack = 0
for i in range(0,len(files),res):
    if i % 100 == 0:
        print ("img loaded: ", i)
    img = cv2.imread(files[i],0)
    
    if res > 1:
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
print ("image stack created. shape:", img_stack.shape)


#prepare data for historgam
data = img_stack.copy()
data = data.flatten()
data = data [data > 5] #only data highe than 5

#create historgam
hist = histogram(data,
                 bins=25,
                 errors=False,
                 c = "white",
                 bc = "white",
                 ytitle= "Voxel Counts",
                 xtitle= "Density"
                )



#create volume
vol = Volume(img_stack, spacing=(x_scale,y_scale,z_scale), mapper="smart", mode=1, alpha= [0.0, 0.9, 1.0], c= "coolwarm")


#render and show
vp = Plotter(N=2, axes=False, bg="black", size="fullscreen", sharecam=False) #(1000,1000))
vp.show(vol, "Voxel Render", axes=0, at=0)
vp.show(hist,"Density Historgam", axes=0, at=1)
vp.show(interactive=True)

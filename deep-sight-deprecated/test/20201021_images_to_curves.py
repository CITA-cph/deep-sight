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
 

20201021_images_to_curves.py

Find circles in image stack, export as text file for grasshopper (20201026_images_to_curves.gh).

Author: Sebastian Gatz

'''



import numpy as np
import cv2
import os
import glob 
import matplotlib.pyplot as plt
import time

res = 2

#set image folder
#all_images = "C:/Users/sgat/OneDrive - KADK/Sebastian_after_Sync/13_gcode/SE000001_png-20200530T122127Z-001/SE000001_png"
all_images = "C:/Users/sgat/Desktop/03_data/zebicon-ccc195/cropped_jpeg/"

os.chdir(all_images)
types = ('*.gif', '*.png', '*.jpg', "*bmp", "*tif")

#load all images
img_names = []
for files in types:
	img_names.extend(glob.glob(files))

curves = []
txt_out = open("img_to_crv.txt", "w")


for z in range (400,len(img_names),200):
    print(z)

    img = cv2.imread(img_names[z],0)
    img = cv2.resize(img, (0,0), fx=1/res, fy=1/res) 
    img = cv2.medianBlur(img,5)
    cimg = cv2.cvtColor(img,cv2.COLOR_GRAY2BGR)

    circles = cv2.HoughCircles(img,cv2.HOUGH_GRADIENT,1,25,
                                param1=100,param2=39,minRadius=5,maxRadius=40) #50 30

    circles = np.uint16(np.around(circles))
    for i in circles[0,:]:
        cv2.circle(cimg,(i[0],i[1]),i[2],(0,255,0),2)
        cv2.circle(cimg,(i[0],i[1]),2,(0,0,255),3)

        print (z, i[0],i[1],i[2])
        txt_out.write(str(z) + " " + str(i[0]) + " " + str(i[1]) + " " + str(i[2]) + "\n")


    cv2.imshow('detected circles',cimg)
    cv2.waitKey(0)
    cv2.destroyAllWindows()

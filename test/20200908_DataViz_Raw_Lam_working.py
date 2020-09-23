import numpy as np
# from vedo import *
import vedo
from vedo.pyplot import histogram
import os
import glob
import cv2


all_loaded_images = []

z_scale = 10

def find_contour(img, val, col):
    all_knot_curves = []
    all_knot_curves_center = []
    all_knot_curves_center_obj = []

    #find knots
    height, width = img.shape
    img_blur = cv2.medianBlur(img, 5)
    ret, img_tre = cv2.threshold(img_blur, val,255,cv2.THRESH_BINARY)

    for w in range(width):
        img_tre[0,w] = 0
        img_tre[height-1, w] = 0
    
    for h in range(height):
        img_tre[h,0] = 0
        img_tre[h,width-1] = 0

    img_c = img.copy()
    contours, hierachy = cv2.findContours (img_tre, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)[-2:]

    for j, c in enumerate(contours): #j == 0 is the biggest outer curve
            if len(c) > 3:
                lst = []
                
                #calculate average aka center point of curves
                pt_len = 0
                x_a = 0
                y_a = 0
                z_a = 0
                for p in c:
                    p_x = p[0][0]
                    p_y = p[0][1]
                    p_z = i * z_scale
                    pt = []
                    
                    #flipping and mirroring to match voxel volume orientation
                    pt.append(p_z)
                    pt.append((p_x*-1)+img_stack.shape[0])
                    pt.append(p_y)

                    x_a += p_x
                    y_a += p_y
                    z_a += p_z
                    pt_len += 1

                    lst.append(pt)
                
                if i != 0:
                    cnt_pt = []
                    x_a /= pt_len
                    y_a /= pt_len
                    z_a /= pt_len
                    cnt_pt.append(z_a)
                    cnt_pt.append((x_a*-1)+img_stack.shape[0])
                    cnt_pt.append(y_a)

                    cnt_pt_obj = vedo.Point(pos=cnt_pt, r=8, c=col, alpha=1)
                    all_knot_curves_center.append(cnt_pt)
                    all_knot_curves_center_obj.append(cnt_pt_obj)

                knot_curves = vedo.shapes.KSpline(lst, continuity=-1, tension=0, bias=0, closed=True, res=None)
                all_knot_curves.append(knot_curves)
    
    return all_knot_curves, all_knot_curves_center_obj



#set path
all_images = "C:/Users/martin.tamke\OneDrive - KADK/GitHub/deep-sight/test/data/20200505_S05"
#all_images = "C:/Users/sgat\OneDrive - KADK/Sebastian_after_Sync/06_bones_to_flesh/browsing male/(VKH) Anatomical Images (1,000 X 570)/"
os.chdir(all_images)
# os.chdir(os.path.dirname(os.path.abspath(__file__)))



#load all images
types = ('*.gif', '*.png', '*.jpg', "*bmp", "*tif")
files = []
for f in types:
    files.extend(glob.glob(f))
print (len(files))



#create image stack
img_stack = 0
for i in range(len(files)): #(60):
    print ("img loaded: ", i)
    img = cv2.imread(files[i],0)
    all_loaded_images.append(img)

    if i == 0:
        img.fill(0)
        img_stack = img
    else:
        img_stack = np.dstack((img_stack,img))
img = cv2.imread(files[0],0)
img.fill(0)
img_stack = np.dstack((img_stack,img))

print ("all ", len(files), "images loaded") #Debugging


#find contours
all_contours = []
all_centers = []
for i in range(0, len(all_loaded_images), 5):
    contours, centers = find_contour(all_loaded_images[i], 127, "red")[-2:]
    all_contours.append(contours)
    all_centers.append(centers)

print ("contours and centers found")



#find pit
pith_points = []
for i in range(1,len(all_loaded_images)):
    contours, centers = find_contour(all_loaded_images[i],60, "green")[-2:]
    pith_points.append(centers[0])



#create volumes
vol1 = vedo.Volume(img_stack, spacing=(z_scale,1,1), mapper="smart", mode=1, alpha= [0.0, 0.0, 0.5, 0.7, 1], c= "jet")
vol2 = vedo.Volume(img_stack, spacing=(z_scale,1,1), mapper="smart", mode=3, alpha= [0.0, 0.0, 0.5, 0.7, 1])
#vol1.crop(0.1, 0.1, 0.1, 0.1, 0.1, 0.1) #crop volume



#create isosurfaces and slider
def slider0(widget, event):
    value = int(widget.GetRepresentation().GetValue()+0.5)

    for i in range(10):
        isos[i].alpha(0)
        if i == value:
            isos[i].alpha(1)

isos = []
for i in range (0,250,25):
    iso = vedo.Volume(img_stack).isosurface(threshold=i, largest=True)
    iso.alpha(0)
    iso.scale([z_scale,1,1])
    isos.append(iso)

isos[5].alpha(1)

print("created isosurfaces")


#test sliders
def slider1(widget, event):
    value = int(widget.GetRepresentation().GetValue()+0.5)



#points from isosurface
pts = isos[5].clone().points()



#render
vp = vedo.Plotter(N=6, axes=True, bg="black", size="fullscreen")

vp.show(vol1, "Voxel Render", axes=1, at=0, viewup="z")
vp.show(vol2, "Volume Ghost", axes=0, at=1)

vp.show(isos, "Isosurface", __doc__, at=2)
vp.addSlider2D(slider0,
               1, 9,            # slider range
               value=5,         # initial value
               pos=([0.1,0.05],  # first point of slider in the renderer
                    [0.25,0.05]), # 0.4 = 40% of the window size width
               title="Isosurface Threshold")


vp.show(pts, "Points", at=3)
vp.show(all_contours, "Contours", at=4)
vp.show(all_centers, pith_points, "Contour Centers + Pith Points", at=5)

vp.show(interactive=True)




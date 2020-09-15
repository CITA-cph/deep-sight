# Deep Sight
This project seeks to develop design-focused visualisation techniques enabling designers to non-intrusively gain understandings of the interior heterogeneous structure of familiar and emerging bio-materials. Methods from the field of biological imaging will be tested and extended to support visualisation and manipulation of 3D volumetric datasets for application within design and making contexts of KADK.

# Background
In response to global challenges of resource scarcity, many design practices are turning attention to finding viable and sustainable alternatives to current resource intensive material practices. Particular attention is being focused on bio-based materials - a domain that covers both familiar and established materials (such as timber) and emerging novel materials (such as mycelium composites). A common characteristic of bio-materials is their heterogeneous and variegated structure. The ability to observe and design with this interior heterogeneity, for practical and aesthetic purposes, would provide a deep reservoir of creative and technical innovation potentials across a broad range of design fields. It would also support a deepening and enriching of scales of material engagement tuned towards sustainable objectives such as minimising material processing through targeted intervention, or gaining insight into capabilities of new materials for application in familiar domains.  
 
Methods for non-intrusively acquiring 3d data of complex biological structures include Computed Tomography (CT) scanning, Confocal Microscopy and Magnetic Resonance Imaging (MRI). However, the datasets produced by these approaches require complex processing and segmentation before quantitative and qualitative information can be extracted. A further complication is that data-processing and segmentation methods differ depending on the type of structural element being investigated. Typically, the workflow here is 1) data acquisition; 2) dataset alignment; 3) pre-processing (for noise reduction, typically using Gaussian smoothing and thresholding); 4) segmentation (with a range of approaches dependent on the character of the features being targeted, for example: boundaries, regions, groups, networks, etc.). 

Data-processing and segmentation methods have matured in the field of biological imaging, but their transfer and application within design fields is still limited and only few attempts to apply these to design and manufacturing of  bio-materials took place, for instance at CITA. Here, we observed that the established approaches towards data-processing and segmentation work in general for design purposes, but that tedious programming is necessary to adapt these to the specific questions of each project. What is needed is a framework, which allows to interface the commonly used design programming platforms, such as Python or Grasshopper, with the tools for processing volumetric image data. We find furthermore, that 2D display technologies complicate the understanding and interaction with volumetric data and disassociate finally the analysis and design from the material itself. Alternatives might be found within 3D display technologies or the overlay of the physical artifact with data by means of Augmented Reality.

# Research Question
The research question for this project can therefore be stated as:

How can we effectively transfer existing 3d volumetric data processing methods from the field of biological imaging and tailor these for: 
1) analysis of a broad range of bio-materials
2) the translation of acquired datasets into data structures commonly associated with digital design workflows   
3) to serve the purpose of visualisation and the manipulation of data 

# Dependencies
Core library:
- [Cmake](https://cmake.org/)
- [Eigen](http://eigen.tuxfamily.org/) ([latest stable version](https://github.com/libigl/eigen))
- [Boost](https://www.boost.org/)

Python examples:
- matplotlib
- pyvista
- numpy

Python dependencies can be installed using `pip`: i.e. `> pip install matplotlib`.




<img align="left" src="misc/logo.jpg">
    
    
<br /><br /><br /><br /><br /><br />
    
    

### [CVISS Research](http://www.cviss.net/)

## Interactive Defect Quantification through Extended Reality

[![](http://img.youtube.com/vi/vOv0GKCy_r0/0.jpg)](https://www.youtube.com/watch?v=vOv0GKCy_r0)

### Introduction

This repository contains the source code used by the authors to run Extended Reality Inspection and Visualization (XRIV) developed for the [paper](https://www.sciencedirect.com/science/article/abs/pii/S1474034621002238),

Al-Sabbag, Z. A., Yeum, C. M., & Narasimhan, S. (2022). Interactive defect quantification through extended reality. *Advanced Engineering Informatics*, *51*, 101473.

XRIV allows you to perform interactive segmentation on objects of interest (defects such as spalling, cracks, etc.) and measure their areas automatically. XRIV was implemented on Microsoft's HoloLens 2.

### Deployment

XRIV requires an XR device (HoloLens 2) and a local computer (server) for deployment. 

### Installation and Building From Source

#### Server

First, recursively clone this package:

```
git clone --recursive https://github.com/cviss-lab/XRIV.git
```

Then install the dependencies listed in the **requirements.txt** in the **server** folder.

To deploy the server, run the following:

```
cd server
python server.py 
```
When prompted, enter the **IP address** of the HoloLens 2 device on your local network.

#### HoloLens 2

The unity project was built using **Unity 2020.3.24f1**. 

To deploy the project on your device, use *File -> Build Settings -> Build* on the Universal Windows Platform (UWP) and select a place to store the build. Then, open the resulting *XRIV.sln* on Visual Studio 2019 and deploy it to your device using USB or Wi-Fi. 

Once you start the server, it will ask you to enter the *IP address of the HoloLens 2*, and if prompted, the *IP address of the server*.

Once you start the HL2 application, you can select positive and negative seed points by selecting their option in the menu, and then click on the location to place them. Then, use *Analyze Image* and click to capture an image and send it to the server for analysis.

<img align="left" src="misc/img2.jpg">

## License

The code is released under the MPL 2.0 License. MPL is a copyleft license that is easy to comply with. You must make the source code for any of your changes available under MPL, but you can combine the MPL software with proprietary code, as long as you keep the MPL code in separate files.

### BibTeX Citations

If you use any of our code, please cite our paper as:

```
@article{al2022interactive,
  title={Interactive defect quantification through extended reality},
  author={Al-Sabbag, Zaid Abbas and Yeum, Chul Min and Narasimhan, Sriram},
  journal={Advanced Engineering Informatics},
  volume={51},
  pages={101473},
  year={2022},
  publisher={Elsevier}
}
@article{al2022enabling,
  title={Enabling human--machine collaboration in infrastructure inspections through mixed reality},
  author={Al-Sabbag, Zaid Abbas and Yeum, Chul Min and Narasimhan, Sriram},
  journal={Advanced Engineering Informatics},
  volume={53},
  pages={101709},
  year={2022},
  publisher={Elsevier}
}
```

Welcome to the Harold wiki!

Get your pencils out! Harold is a machine learning app using Generative Adversarial Network (GAN) to transform your (very) flat 2D sketches into vibrant 3D models.

# About the implementation

## GAN model

The app uses a GAN to transform a input picture of a 2D sketch into a output codified image where elements are easily identifiable by their color. This output image is then post-processed with image-processing techniques to extract structural elements and build the 3D model.

The GAN is based on the [pix2pix](https://phillipi.github.io/pix2pix/) by Isola et al, and using its tensorflow implementation [pix2pix-tensorflow](https://github.com/affinelayer/pix2pix-tensorflow) by [Christopher Hesse](https://github.com/christopherhesse).

## Storage of ML model

Due to size issues, storing the machine learning model within the app is not a solution. Therefore the trained machine learning model is uploaded in an Azure container with Tensorflow-Serving. The app sends requests to the container through gRPC framework.

# Getting Started

## Connect your smartphone camera to your PC

To improve your experience you can connect your smartphone camera to your computer thanks to a third party app (like [ivCam](https://www.e2esoft.com/ivcam/)). You need to install the app both on your iPhone and your PC. Connection can be established through WiFi (both your PC and your iPhone must be connected to the same network, detection is then automatic). Your PC camera will always work as a default option.

## How to use the app

Select your camera source in the drop-down list, then click the green PLAY button. Once you are happy with the picture shown on the capture window, click the CAMERA button to take a screenshot. That’s it! You can then play with the number of stories and the horizontal scaling of your model.

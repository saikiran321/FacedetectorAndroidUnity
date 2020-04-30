#pragma once

#include <iostream>
#include <string>
#include <vector>
#include <stdlib.h>
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/dnn.hpp>

using namespace cv;
using namespace std;

struct Color32
{
    uchar red;
    uchar green;
    uchar blue;
    uchar alpha;
};


class FaceDetector
{
public:
    FaceDetector();
    ~FaceDetector();
    void detectFace(Mat& imgframe);
    void loadModelFile(std::string caffeConfigFile, std::string caffeWeightFile);
    cv::Mat loadImgTest(std::string imgfile);
private:
    const size_t inWidth = 300;
    const size_t inHeight = 300;
    const double inScaleFactor = 1.0;
    const float confidenceThreshold = 0.7;
    cv::Scalar meanVal;
    cv::Mat testImg; // for testing purpose
    dnn::Net net;

};




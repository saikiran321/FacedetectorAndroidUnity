#include "SharedOcv.h"
#include<fstream>
#include<iostream>
using namespace cv;

#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "SharedOcv", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "SharedOcv", __VA_ARGS__))



extern "C" {

    uint8_t* resultBuffer;
    char* buffer;


    FaceDetector::FaceDetector()
    {
        meanVal = cv::Scalar(104.0, 177.0, 123.0);
    }

    FaceDetector:: ~FaceDetector()
    {

    }

    void FaceDetector::detectFace(cv::Mat& imgframe)
    {

        cv::resize(imgframe, imgframe, cv::Size(270,480));
        int frameHeight = imgframe.rows;
        int frameWidth = imgframe.cols;

        cv::Mat inputBlob = cv::dnn::blobFromImage(imgframe, inScaleFactor, cv::Size(inWidth, inHeight), meanVal, false, false);

        net.setInput(inputBlob, "data");
        cv::Mat detection = net.forward("detection_out");

        cv::Mat detectionMat(detection.size[2], detection.size[3], CV_32F, detection.ptr<float>());

        for (int i = 0; i < detectionMat.rows; i++)
        {
            float confidence = detectionMat.at<float>(i, 2);

            if (confidence > confidenceThreshold)
            {
                int x1 = static_cast<int>(detectionMat.at<float>(i, 3) * frameWidth);
                int y1 = static_cast<int>(detectionMat.at<float>(i, 4) * frameHeight);
                int x2 = static_cast<int>(detectionMat.at<float>(i, 5) * frameWidth);
                int y2 = static_cast<int>(detectionMat.at<float>(i, 6) * frameHeight);

                cv::rectangle(imgframe, cv::Point(x1, y1), cv::Point(x2, y2), cv::Scalar(0, 255, 0), 2, 4);
            }
        }

        cv::resize(imgframe, imgframe, cv::Size(1080, 1920));

    }
    void FaceDetector::loadModelFile(std::string caffeConfigFile, std::string caffeWeightFile)
    {
        net = cv::dnn::readNetFromCaffe(caffeConfigFile, caffeWeightFile);
    }


    static FaceDetector facedetector_;

    void startDetector(char* configfile, char* weightfile)
    {
        facedetector_.loadModelFile(configfile, weightfile);
    }



    uint8_t* facedetector(int width, int height, uint8_t* buffer) {
        Mat inFrame = Mat(height, width, CV_8UC4, buffer);
        Mat outImage = inFrame.clone();
        
        std::cerr << "read image" << std::endl;
        cv::rotate(outImage, outImage, cv::ROTATE_90_CLOCKWISE);
        cv::cvtColor(outImage, outImage, COLOR_RGBA2BGR);
        //cv::resize(outImage, outImage, cv::Size(width / 4.0, height / 4.0));
        std::cerr << "detection started" << std::endl;
        facedetector_.detectFace(outImage);
        //cv::resize(outImage, outImage, cv::Size(width, height));
        cv::cvtColor(outImage, outImage, COLOR_BGR2RGBA);
        std::cerr << "detection ended" << std::endl;
        size_t size = width * height * 4;
        memcpy(resultBuffer, outImage.data, size);
        outImage.release();
        return resultBuffer;
    }










    int initBuffer(int width, int height) {
        size_t size = width * height * 4;
        resultBuffer = new uint8_t[size];
        return 0;
    }


    int deleteBuffer() {
        
        resultBuffer = NULL;
        return 0;
    }

    uint8_t* processFrame(int width, int height, uint8_t* buffer) {
        Mat inFrame = Mat(height, width, CV_8UC4, buffer);
        cv::Mat outFrame;//(inFrame.rows, inFrame.cols, CV_8UC4, Scalar(0, 0, 0));

        Mat processingFrame;

        cv::rotate(inFrame, inFrame, cv::ROTATE_90_CLOCKWISE);
        cvtColor(inFrame, processingFrame, COLOR_RGBA2GRAY);
        //Canny(processingFrame, processingFrame, 0, 30, 3);
        cvtColor(processingFrame, outFrame, COLOR_GRAY2RGBA);

        size_t size = width * height * 4;
        memcpy(resultBuffer, outFrame.data, size);

        inFrame.release();
        outFrame.release();
        processingFrame.release();

        return resultBuffer;
    }

    uint8_t* rotate90Degree(int width, int height, uint8_t* buffer) {
        Mat inFrame = Mat(height, width, CV_8UC4, buffer);
        cv::rotate(inFrame, inFrame, cv::ROTATE_90_CLOCKWISE);
        size_t size = width * height * 4;
        memcpy(resultBuffer, inFrame.data, size);
        inFrame.release();
        return resultBuffer;
    }

  


    
}

#include <cstdio>
#include <FaceTracker/Tracker.h>
#include "opencv2/opencv.hpp"
#include "Kinoko/morph.h"

#define dll extern "C" __declspec(dllexport)

using namespace std;
using namespace cv;
using namespace FACETRACKER;

bool failed = true;
bool fcheck = false;
bool show = false;
int fnum = 0;
int fpd = -1;
int nIter = 5;
double clamp = 3;
double fps = 0;
double fTol = 0.01;
double scale = 1;
char sss[256];

int64 t1, t0;
Mat frame, gray, im, tri, con;
VideoCapture capture;
Tracker tracker("./model/face2.tracker"); // Initialize()で初期化したら死んだ　なんやこのクソライブラリ
Morph mp;
string text;


// プロトタイプ宣言　#本当はヘッダーを作るべきだが
void Draw(Mat &image, Mat &shape, Mat &con, Mat &tri, Mat &visi);


dll void Initialize() {
	capture = VideoCapture();
	int nIter = 5; double clamp = 3, fTol = 0.01;
	tri = IO::LoadTri("./model/face.tri");
	con = IO::LoadCon("./model/face.con");
}

dll void Enabled() {
	capture.open(0);
	t0 = cvGetTickCount();
}

dll void Disabled() {
	if (capture.isOpened()) {
		capture.release();
	}
	destroyAllWindows();
}

dll void Dispose() {
	if (capture.isOpened()) {
		capture.release();
	}
	destroyAllWindows();
}

dll void ModelReset() {
	tracker.FrameReset();
	mp.FrameReset();
}

dll void Track() {
	if (capture.isOpened()) {
		// start!
		capture >> frame;
		if (frame.total() == 0) return;
		Mat img;
		im = frame;
		flip(im, im, 1); cvtColor(im, gray, CV_BGR2GRAY);
		vector<int> wSize1(1);
		vector<int> wSize2(3);
		wSize1[0] = 7;
		wSize2[0] = 11;
		wSize2[1] = 9;
		wSize2[2] = 7;
		vector<int> wSize; if (failed)wSize = wSize2; else wSize = wSize1;
		if (tracker.Track(gray, wSize, fpd, nIter, clamp, fTol, show) == 0) {
			int idx = tracker._clm.GetViewIdx(); failed = false;
			img = im.clone();
			if (show) { Draw(im, tracker._shape, con, tri, tracker._clm._visi[idx]); }
		}
		else {
			if (show) { Mat R(im, cvRect(0, 0, 150, 50)); R = Scalar(0, 0, 255); }
			tracker.FrameReset(); failed = true;
			mp.FrameReset();
		}
		if (fnum >= 9) {
			t1 = cvGetTickCount();
			fps = 10.0 / ((double(t1 - t0) / cvGetTickFrequency()) / 1e+6);
			t0 = t1; fnum = 0;
		}
		else fnum += 1;
		if (mp.frame < 5) {
			mp.Calibrate(tracker._shape);
			mp.frame++;
		}
		if (mp.frame >= 5) {
			mp.EyeOpenL(img, tracker._shape);
			mp.EyeOpenR(img, tracker._shape);
			mp.MouthOpen(img, tracker._shape);
			mp.FaceRotation(tracker._shape);
			mp.OutPutValues();
		}
		if (show) {
			sprintf(sss, "%d frames/sec", (int)round(fps)); text = sss;
			putText(im, text, Point(10, 20),
				CV_FONT_HERSHEY_SIMPLEX, 0.5, CV_RGB(0, 0, 0));
			imshow("TrackingInfo", im);
		}
		if (waitKey(1) >= 0);
	}
}

dll void ShowTrackingInfoWindow() {
	show = true;
	cvNamedWindow("TrackingInfo", 1);
}

dll void CloseTrackingInfoWindow() {
	show = false;
	destroyAllWindows();
}

struct Status {
	//total 16bytes
	float EyeL; //4bytes
	float EyeR; //4bytes
	float mouth; //4bytes
	float leanX; //4bytes
	float leanY; //4bytes
	float leanZ; //4bytes
};

dll void GetStatus(Status* statusPtr) {
	statusPtr->EyeL = mp.LeftEyeHeight;
	statusPtr->EyeR = mp.RightEyeHeight;
	statusPtr->mouth = mp.mouth;
	statusPtr->leanX = mp.cRotation.x;
	statusPtr->leanY = mp.cRotation.y;
	statusPtr->leanZ = mp.cRotation.z;
}

dll void ReOpenDevice(int deviceId) {
	if (capture.isOpened()) {
		capture.release();
	}
	capture.open(deviceId);
}

void Draw(Mat &image, Mat &shape, Mat &con, Mat &tri, Mat &visi) {
	int i, n = shape.rows / 2; Point p1, p2; Scalar c;
	Mat empty = Mat::zeros(Size(image.cols, image.rows), CV_8UC3);

	//draw triangulation
	c = CV_RGB(0, 0, 0);
	for (i = 0; i < tri.rows; i++) {
		if (visi.at<int>(tri.at<int>(i, 0), 0) == 0 ||
			visi.at<int>(tri.at<int>(i, 1), 0) == 0 ||
			visi.at<int>(tri.at<int>(i, 2), 0) == 0)continue;
		p1 = Point(shape.at<double>(tri.at<int>(i, 0), 0),
			shape.at<double>(tri.at<int>(i, 0) + n, 0));
		p2 = Point(shape.at<double>(tri.at<int>(i, 1), 0),
			shape.at<double>(tri.at<int>(i, 1) + n, 0));
		//line(image,p1,p2,c);
		//line(empty, p1, p2, Scalar(255,255,255));
		p1 = Point(shape.at<double>(tri.at<int>(i, 0), 0),
			shape.at<double>(tri.at<int>(i, 0) + n, 0));
		p2 = Point(shape.at<double>(tri.at<int>(i, 2), 0),
			shape.at<double>(tri.at<int>(i, 2) + n, 0));
		//line(image,p1,p2,c);
		//line(empty, p1, p2, Scalar(255, 255, 255));
		p1 = Point(shape.at<double>(tri.at<int>(i, 2), 0),
			shape.at<double>(tri.at<int>(i, 2) + n, 0));
		p2 = Point(shape.at<double>(tri.at<int>(i, 1), 0),
			shape.at<double>(tri.at<int>(i, 1) + n, 0));
		//line(image,p1,p2,c);
		//line(empty, p1, p2, Scalar(255, 255, 255));
	}
	//draw connections
	c = CV_RGB(0, 0, 255);
	for (i = 0; i < con.cols; i++) {
		if (visi.at<int>(con.at<int>(0, i), 0) == 0 ||
			visi.at<int>(con.at<int>(1, i), 0) == 0)continue;
		p1 = Point(shape.at<double>(con.at<int>(0, i), 0),
			shape.at<double>(con.at<int>(0, i) + n, 0));
		p2 = Point(shape.at<double>(con.at<int>(1, i), 0),
			shape.at<double>(con.at<int>(1, i) + n, 0));
		line(image, p1, p2, c);
		//line(empty, p1, p2, c);
		//imshow("lines", empty);
	}
	//draw points
	for (i = 0; i < n; i++) {
		if (visi.at<int>(i, 0) == 0)continue;
		p1 = Point(shape.at<double>(i, 0), shape.at<double>(i + n, 0));
		c = CV_RGB(255, 0, 0); circle(image, p1, 2, c);
		//string text = to_string(i);
		//putText(image, text, p1,CV_FONT_HERSHEY_SIMPLEX, 0.3, CV_RGB(0, 0, 0));

	}
}
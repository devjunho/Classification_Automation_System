using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Windows.Threading;
using Opencv_Pj.Models;

namespace Opencv_Pj
{

    public partial class Videocam : System.Windows.Window
    {
        VideoCapture cam;
        DispatcherTimer timer;
        bool is_initCam, is_initTimer;
        static public Mat frame;
        //static public string save_name;
        static public OpenCvSharp.Rect enlargedRect; // 전역 변수로 선언

        OpenCV faceDetector;

        public Videocam()
        {
            InitializeComponent();
            faceDetector = new OpenCV();
        }

        private void windows_loaded(object sender, RoutedEventArgs e)
        {
            // 카메라, 타이머(0.01ms 간격) 초기화
            is_initCam = init_camera();
            is_initTimer = init_Timer(0.01);

            // 초기화 완료면 타이머 실행
            if (is_initTimer && is_initCam) timer.Start();
        }

        private bool init_Timer(double interval_ms)
        {
            try
            {
                timer = new DispatcherTimer();

                timer.Interval = TimeSpan.FromMilliseconds(interval_ms);
                timer.Tick += new EventHandler(timer_tick);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool init_camera()
        {
            try
            {
                // 0번 카메라로 VideoCapture 생성 (카메라가 없으면 안됨)
                cam = new VideoCapture(1);
                cam.FrameHeight = (int)Cam_1.Height;
                cam.FrameWidth = (int)Cam_1.Width;

                // 카메라 영상을 담을 Mat 변수 생성
                frame = new Mat();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void timer_tick(object sender, EventArgs e)
        {
            if (cam.IsOpened())
            {
                cam.Read(frame);

                if (!frame.Empty())
                {
                    // 얼굴 감지
                    OpenCvSharp.Rect[] faces = faceDetector.DetectFaces(frame);

                    // 감지된 얼굴 주위에 사각형 그리기
                    foreach (OpenCvSharp.Rect face in faces)
                    {
                        // 좀 더 큰 사각형을 그리기 위해 위치 확장
                        int expandX = 30; // X 방향으로 확장할 픽셀 수
                        int expandY = 30; // Y 방향으로 확장할 픽셀 수
                        int newX = Math.Max(face.X - expandX, 0); // 새로운 X 위치
                        int newY = Math.Max(face.Y - expandY, 0); // 새로운 Y 위치
                        int newWidth = Math.Min(face.Width + 2 * expandX, frame.Width - newX); // 새로운 너비
                        int newHeight = Math.Min(face.Height + 2 * expandY, frame.Height - newY); // 새로운 높이

                        //OpenCvSharp.enlargedRect = new OpenCvSharp.Rect(newX, newY, newWidth, newHeight);

                        enlargedRect = new OpenCvSharp.Rect(newX, newY, newWidth, newHeight);
                        Cv2.Rectangle(frame, enlargedRect, Scalar.Blue, 2);
                    }

                    // Mat 데이터를 WPF Image에 출력
                    Cam_1.Source = frame.ToWriteableBitmap();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            cam.Release();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp;
using Tesseract;
using Opencv_Pj.Models;
using Network_;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.IO;

namespace Opencv_Pj
{

    public partial class Classify : System.Windows.Controls.Page
    {
        Network stream = new Network();
        JObject protocol = new JObject();
        JObject temp = new JObject();
        System.Windows.Window videocam = new Videocam();

        private TesseractEngine ocr;
        private string save_name;
        private string recognizedText;

        public Classify()
        {
            InitializeComponent();
            videocam.Show();
            ocr = new TesseractEngine(@"C:/Users/aiot/source/repos/Opencv_Pj/bin/Debug/tessdata", "kor", EngineMode.Default);
        }

        public Classify(Socket socket) : this()
        {
            this.stream.Socket = socket;  
        }
        private void Recognize_btn_Click(object sender, RoutedEventArgs e)
        {
            string originalFilePath;
            string croppedFilePath;
            string faceFilePath;

            Information_box.Text = "";
            CardPhoto.Source = null;
            try
            {
                // 현재 시간을 이용하여 파일 이름 생성
                save_name = DateTime.Now.ToString("yyyy-MM-dd-hh시mm분ss초");

                // 현재 프레임 저장
                if (Videocam.frame != null)
                {
                    originalFilePath = @"C:\Users\aiot\OneDrive\사진\카메라 앨범\" + save_name + ".jpg";
                    Videocam.frame.SaveImage(@"C:\Users\aiot\OneDrive\사진\카메라 앨범\" + save_name + ".jpg");
                }
                else
                {
                    MessageBox.Show("이미지를 캡처할 수 없습니다.");
                    return; // 이미지 캡처 실패 시 종료
                }

                Mat ms = new Mat(originalFilePath, ImreadModes.Grayscale);
                ms = ms.Canny(75, 200, 3, true);

                OpenCvSharp.Point testpoint = new OpenCvSharp.Point();
                OpenCvSharp.Point[][] contours0;
                HierarchyIndex[] hierarchy;

                Cv2.FindContours(ms, out contours0, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, testpoint);

                Mat ms2 = new Mat(originalFilePath, ImreadModes.Color);

                for (int i = 0; i < contours0.Length; i++)
                {
                    double peri = Cv2.ArcLength(contours0[i], true);
                    OpenCvSharp.Point[] pp = Cv2.ApproxPolyDP(contours0[i], 0.02 * peri, true);
                    RotatedRect rrect = Cv2.MinAreaRect(pp);
                    double areaRatio = Math.Abs(Cv2.ContourArea(contours0[i], false)) / (rrect.Size.Width * rrect.Size.Height);

                    if (pp.Length == 4)
                    {
                        // Contour 주위에 빨간색 사각형 그리기
                        Cv2.DrawContours(ms2, contours0, i, Scalar.Red, 2, LineTypes.AntiAlias, hierarchy, 100, testpoint);
                        // 사각형이 표시된 영역 자르기
                        OpenCvSharp.Rect boundingRect = Cv2.BoundingRect(contours0[i]);
                        Mat croppedRegion = new Mat(ms2, boundingRect);
                        croppedFilePath = @"C:\Users\aiot\OneDrive\사진\카메라 앨범\" + save_name + "_cropped.jpg";
                        Cv2.ImWrite(croppedFilePath, croppedRegion);

                        // Tesseract OCR로 글자 인식
                        using (var img = Pix.LoadFromFile(lier))
                        //using (var img = Pix.LoadFromFile(croppedFilePath))
                        {
                            using (var page = ocr.Process(img))
                            {
                                recognizedText = page.GetText();
                                string[] lines = recognizedText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                if (lines.Length > 0)
                                {
                                    string idcard = lines[0];
                                    string idcard_name = lines[1];
                                    string idcard_issue_date = lines[4];
                                    string idcard_issuer = lines[5];

                                    string idcard_idnumber;
                                    int index = lines[2].IndexOf('-');
                                    if (index != -1)
                                    {
                                        idcard_idnumber = lines[2].Substring(0, index).Trim();
                                    }
                                    else
                                    {
                                        idcard_idnumber = lines[2].Trim();
                                    }

                                    string idcard_address;
                                    int firstSpaceIndex = lines[3].IndexOf(' ');
                                    if (firstSpaceIndex != -1)
                                    {
                                        int secondSpaceIndex = lines[3].IndexOf(' ', firstSpaceIndex + 1);
                                        if (secondSpaceIndex != -1)
                                        {
                                            idcard_address = lines[3].Substring(0, secondSpaceIndex).Trim();
                                        }
                                        else
                                        {
                                            idcard_address = lines[3].Trim();
                                        }
                                    }
                                    else
                                    {
                                        idcard_address = lines[3].Trim();
                                    }

                                    string[] information = {"이름: " + idcard_name, "주민번호: " + idcard_idnumber,
                                        "주소: " + idcard_address, "발급일자: " + idcard_issue_date, "발급기관장: " + idcard_issuer};
                                    foreach (string info in information)
                                    {
                                        Information_box.Text += Environment.NewLine + info + Environment.NewLine + Environment.NewLine;
                                    }

                                    protocol["protocol"] = 12;
                                    protocol["name"] = idcard_name;
                                    protocol["idnumber"] = idcard_idnumber;
                                    protocol["address"] = idcard_address;
                                    protocol["issue_date"] = idcard_issue_date;
                                    protocol["issuer"] = idcard_issuer;

                                    try
                                    {
                                        stream.JsonWrite(protocol);
                                    }
                                    catch (SocketException ex)
                                    {
                                        MessageBox.Show("SocketException: " + ex.Message);
                                    }
                                }
                            }
                        }

                        faceFilePath = "C:\\Users\\aiot\\OneDrive\\사진\\카메라 앨범\\" + save_name + "_face.jpg";


                        Mat capturedFace = new Mat(Videocam.frame, Videocam.enlargedRect);
                        capturedFace.SaveImage(faceFilePath);

                        string idcard_number = protocol["idnumber"].ToString();

                        BitmapImage bitmap = new BitmapImage(new Uri(faceFilePath));
                        byte[] buffer = stream.ImageToByteString(bitmap);
                        stream.ImageWrite(buffer, idcard_number);
                        CardPhoto.Source = bitmap;

                        File.Delete(originalFilePath);
                        File.Delete(croppedFilePath);

                        break; // 첫 번째로 발견된 사각형 처리 후 루프 종료
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생: " + ex.Message);
            }
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            Menu menu = new Menu(stream.Socket);
            videocam.Close();
            NavigationService.Navigate(menu);

        }
    }
}

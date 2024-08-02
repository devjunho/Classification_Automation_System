using LiveCharts.Defaults;
using LiveCharts;
using Network_;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp;
using System.Diagnostics;
using System.Xml.Linq;

namespace Opencv_Pj
{

    public partial class History : Page
    {
        Network stream = new Network();
        Data data = new Data();
        List <Data> Listdata = new List<Data>();

        public History()
        {
            InitializeComponent();
        }

        public History(Socket socket) : this()
        {
            this.stream.Socket = socket;

            Listdata = stream.ListRead();
            History_List.ItemsSource = Listdata;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Infor_tb.Text = "";
            var selectedItem = History_List.SelectedItem as Data;
            Infor_tb.Text = Environment.NewLine + selectedItem.name + Environment.NewLine + Environment.NewLine
                + selectedItem.idnumber + Environment.NewLine + Environment.NewLine
                + selectedItem.address + Environment.NewLine + Environment.NewLine
                + selectedItem.issue_date + Environment.NewLine + Environment.NewLine
                + selectedItem.issuer;

            //byte[] bytess = stream.ImageRead(selectedItem.name);
            //BitmapImage bitmapImage = stream.ByteToImage(bytess);
            JObject protocol = new JObject();
            protocol["protocol"] = 20;
            protocol["name"] = selectedItem.name;
            stream.JsonWrite(protocol);
            LoadImageFromServer();
            //Dispatcher.Invoke(() =>
            //{
            //    if (bitmapImage != null)
            //    {
            //        Photo.Source = bitmapImage;
            //        MessageBox.Show("됨");
            //    }
            //    else
            //    {
            //        // 이미지 로드 실패 시 처리할 내용
            //        Photo.Source = null; // 혹은 기본 이미지 등을 설정
            //        MessageBox.Show("안됨");
            //    }
            //});

        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            Menu menu = new Menu(stream.Socket);
            NavigationService.Navigate(menu);
        }

        private void LoadImageFromServer()
        {
            try
            {
                MessageBox.Show("이미지 로드 성공");

                // 이미지 데이터 크기 수신
                byte[] sizeBuffer = new byte[sizeof(int)];
                stream.Socket.Receive(sizeBuffer);
                int imageSize = BitConverter.ToInt32(sizeBuffer.Reverse().ToArray(), 0);
                //sizeBuffer를 역순으로 변환한 후, BitConverter.ToInt32를 사용해 네트워크 바이트 순서에서 호스트 바이트 순서로 변환하여 imageSize에 저장
                // 역순으로 변환하는 이유는 바이트순서(엔디언 차이) 때문
                // 네트워크 바이트 순서는 빅 엔디언으로 인코딩 된다. 하지만 대부분의 pc는 리틀 엔딩언 방식이기에 변환해줘야 읽기 가능
                // 0x12345678을 리틀엔디언 - '78 56 34 12' 빅엔디언 '12 34 56 78' 저장 방식
                Debug.WriteLine("*******************");
                Debug.WriteLine(imageSize);

                byte[] buffer = new byte[1000];
                using (MemoryStream ms = new MemoryStream())
                // 메모리 스트림 ms 생성.
                // 이 스트림은 이미지 데이터를 메모리에 저장
                {
                    int bytesRead;
                    int totalBytesRead = 0;
                    while (totalBytesRead < imageSize && (bytesRead = stream.Socket.Receive(buffer)) > 0)
                    // 총 읽은 바이트수가 이미지 크기보다 작고, 바이트수를 계속 읽어올 수 있으면 루프
                    {
                        ms.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        //Debug.WriteLine("&&&&&&&&&&&&&&&&&&&&");
                        //Debug.WriteLine(bytesRead);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    // 스트림 포인터를 스트림 시작으로 이동
                    // 이전 코드에서 이미지를 수신하여 ms에 저장할떄 스크림 포인터는 스트림의 끝에 위치하게 됨.
                    // 이미지를 로드하려면 스트림의 시작부터 읽어야 하기에 스트림 포인터를 시작위치로 되돌림.
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    // BitmapImage 초기화
                    bitmap.StreamSource = ms;
                    // 메모리 스트림 ms를 BitmapImagedml 소스로 설정
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    // 이미지를 메모리에 로드하고, 스트림을 닫아도 이미지를 사용할 수 있도록 설정합니다.
                    bitmap.EndInit();
                    // 초기화 완료
                   
                    Photo.Source = bitmap;
                    Debug.WriteLine("이미지 로드 완료");

                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"이미지 로드 실패 - 네트워크 오류: {ex.Message}");
                Debug.WriteLine($"네트워크 오류: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 로드 실패: {ex.Message}");
                Debug.WriteLine($"예외 발생: {ex.Message}");
            }
        }
    }

}

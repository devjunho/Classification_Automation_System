using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Collections;
using System.ComponentModel.Design;
using Opencv_Pj;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Runtime.InteropServices;

namespace Network_
{
    public class Network
    {
        static public List<string> list_region = new List<string>();

        static public List<int> list_number = new List<int>();

        private const int IMAGE_SIZE = 1000;

        private string serverIp;


        public string ServerIP { get { return serverIp; } set { serverIp = value; } }

        private int port;

        public int Port { get { return port; } set { port = value; } }

        public String Name { get; set; }

        public Socket Socket { get; set; }

        public Network() { }

        public Network(Socket Socket)
        {
            this.Socket = Socket;
        }

        public void Connected()
        {

            IPAddress address = IPAddress.Parse("10.10.21.125");
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(address, 12345);

        }

        public void Disconnect()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }

        public byte[] ImageToByteString(BitmapImage bitmapimage)
        {
            byte[] bytes;
            var image = new BitmapImage();

            // 이미지를 byte로 전환
            using (var mem = new MemoryStream())
            {
                MemoryStream memoryStream = new MemoryStream();
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapimage));
                encoder.Save(memoryStream);
                bytes = memoryStream.ToArray();
                // 마지막에 bite로 바뀐다.
            }
            return bytes;
        }

        public BitmapImage ByteToImage(byte[] bytes)
        {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(bytes))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.Freeze();
            }
            return image;
        }

        public void Write(string msg)
        {
            byte[] data = Encoding.Default.GetBytes(msg);
            Socket.Send(data);

        }

        public void ImageWrite(byte[] image, string idnumber)
        {
            byte[] buffer = new byte[IMAGE_SIZE];
            try
            {
                int len = image.Length / 1000;
                int modulo = image.Length % 1000;
                JObject protocol = new JObject();
                protocol["protocol"] = 10;
                protocol["idnumber"] = idnumber;
                protocol["count"] = len + (modulo > 0 ? 1 : 0);
                JsonWrite(protocol);
                Thread.Sleep(100);
                for (int i = 0; i < len; i++)
                {
                    Buffer.BlockCopy(image, i * IMAGE_SIZE, buffer, 0, IMAGE_SIZE);
                    Socket.Send(buffer);
                    Thread.Sleep(100);
                }
                if (modulo > 0)
                {
                    buffer = new byte[modulo];
                    Buffer.BlockCopy(image, len * IMAGE_SIZE, buffer, 0, modulo);
                    Socket.Send(buffer);
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                JObject tmp = new JObject();
                tmp["protocol"] = 35;
                byte[] bytes = Encoding.UTF8.GetBytes(tmp.ToString());
                Socket.Send(bytes);
                MessageBox.Show("ImageWrite error");
                MessageBox.Show(ex.Message);
            }
        }

        public void JsonWrite(JObject jobject)
        {
            string jsonString = jobject.ToString(); // JObject를 JSON 문자열로 변환

            // UTF-8 인코딩으로 바이트 배열 얻기
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

            // Socket으로 바이트 배열 전송
            Socket.Send(bytes);
        }

        public string Read()
        {
            try
            {
                byte[] buffer = new byte[1024];
                Array.Clear(buffer, 0, buffer.Length);
                Socket.Receive(buffer, buffer.Length, SocketFlags.None);
                string message = Encoding.Default.GetString(buffer);
                message = message.Replace("\0", string.Empty);
                return message;
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public int GraphRead()
        {
            try
            {
                while(true)
                {
                    byte[] bytes = new byte[1024];
                    Array.Clear(bytes, 0, bytes.Length);
                    Socket.Receive(bytes, bytes.Length, SocketFlags.None);
                    string recv_msg = Encoding.UTF8.GetString(bytes);

                    //MessageBox.Show(recv_msg);

                    Socket.Send(bytes);

                    //MessageBox.Show(recv_msg);

                    JObject temp = JObject.Parse(recv_msg);
                    if (int.Parse(temp["protocol"].ToString()) == 19)
                    {
                        list_region.Add(temp["region"].ToString());
                        list_number.Add(Convert.ToInt32(temp["count"].ToString()));
                    }
                    else if (int.Parse(temp["protocol"].ToString()) == 18)
                    {
                        return 0;
                    }
                    else
                    {
                        MessageBox.Show("ListRead() Error");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ListRead() Catch: " + e.Message);
                return -1;
            }
        }

        public JObject JsonRead()
        {

            try
            {
                byte[] buffer = new byte[3000];
                Socket.Receive(buffer);
                string message = Encoding.UTF8.GetString(buffer);
                message = message.Replace("\0", string.Empty);
                var temp = JObject.Parse(message);
                return temp;
            }
            catch (Exception e)
            {
                JObject temp = new JObject();
                temp["protocol"] = 30;
                return temp;
            }
        }

        public bool ReadCheck(string temp)
        {
            if (temp.CompareTo("이미지전송완료") != 0)
                return true;
            else
                return false;
        }

        //public byte[] ImageRead(string name)
        //{
        //    try
        //    {
        //        List<byte[]> list = new List<byte[]>();
        //        JObject protocol = new JObject();
        //        protocol["protocol"] = 20;
        //        protocol["name"] = name;
        //        JsonWrite(protocol); // 프로토콜 전송

        //        // 서버로부터 파일 크기 수신
        //        byte[] buffer = new byte[1000];
        //        Socket.Receive(buffer); // 파일 크기 수신
        //        Socket.Send(new byte[1]); // 응답 전송

        //        // 파일 크기를 문자열에서 정수로 변환
        //        string message = Encoding.Default.GetString(buffer).TrimEnd('\0');
        //        int fileSize = int.Parse(message);

        //        // 전체 파일 데이터를 수신할 바이트 배열
        //        byte[] fileData = new byte[fileSize];
        //        int totalBytesReceived = 0;

        //        while (totalBytesReceived < fileSize)
        //        {
        //            buffer = new byte[Math.Min(IMAGE_SIZE, fileSize - totalBytesReceived)];
        //            int bytesRead = Socket.Receive(buffer); // 데이터 수신
        //            Array.Copy(buffer, 0, fileData, totalBytesReceived, bytesRead); // 수신된 데이터를 전체 파일 데이터 배열에 복사
        //            totalBytesReceived += bytesRead;
        //        }
        //        Socket.Send(new byte[1]); // 응답 전송
        //        // 데이터 수신 완료 후 반환
        //        return fileData;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("ImageRead Error");
        //        Console.WriteLine(ex.ToString());
        //        return null;
        //    }
        //}
        public byte[] ImageRead(string name)
        {
            try
            {
                List<byte[]> list = new List<byte[]>();
                JObject protocol = new JObject();
                protocol["protocol"] = 20;
                protocol["name"] = name;
                JsonWrite(protocol);

                byte[] buffer = new byte[1000];
                Socket.Receive(buffer);
                Socket.Send(new byte[1]);

                string message = Encoding.Default.GetString(buffer).TrimEnd('\0');
                int length = int.Parse(message);

                for (int i = 0; i*1000 < length; i++)
                {
                    buffer = new byte[IMAGE_SIZE];
                    int bytesRead = Socket.Receive(buffer);
                    if (bytesRead < IMAGE_SIZE)
                    {
                        Array.Resize(ref buffer, bytesRead);
                    }
                    list.Add(buffer);
                }
                Socket.Send(new byte[1]);
                byte[] returnbyte = new byte[length];
                int index = 0;
                foreach (var item in list)
                {
                    foreach (var b in item)
                    {
                        returnbyte[index++] = b;
                    }
                }
                MessageBox.Show(Encoding.UTF8.GetString(returnbyte));
                return returnbyte;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImageRead Error");
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public List <Data> ListRead()
        {
            List<Data> list = new List<Data>();
            try
            {
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    Array.Clear(bytes, 0, bytes.Length);
                    Socket.Receive(bytes, bytes.Length, SocketFlags.None);
                    string recv_msg = Encoding.UTF8.GetString(bytes);

                    Socket.Send(bytes);

                    JObject temp = JObject.Parse(recv_msg);

                    if (int.Parse(temp["protocol"].ToString()) == 19)
                    {
                        Data data = new Data();

                        data.name = temp["name"].ToString();
                        data.idnumber = temp["idnumber"].ToString();
                        data.address = temp["address"].ToString();
                        data.issue_date = temp["date"].ToString();
                        data.issuer = temp["issuer"].ToString();

                        //// 확인용
                        //MessageBox.Show(data.name, "이름");

                        list.Add(data);
                    }
                    else if (int.Parse(temp["protocol"].ToString()) == 18)
                    {
                        return list;
                    }
                    else
                    {
                        MessageBox.Show("ListRead() Error");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ListRead() Catch: " + e.Message);
                return null;
            }
        }
    }
}

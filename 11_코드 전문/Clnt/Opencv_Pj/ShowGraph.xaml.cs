using System;
using System.Collections.Generic;
using System.Windows.Controls;
using LiveCharts.Defaults;
using LiveCharts;
using System.Net.Sockets;
using Network_;
using System.Windows;

namespace Opencv_Pj
{
    public partial class ShowGraph : Page
    {
        public ChartValues<ObservableValue> Values { get; set; }

        public List<string> XLabel { get; set; } // List<string>으로 변경

        Network stream = new Network();

        public ShowGraph()
        {
            InitializeComponent();

            Network.list_region.Clear();
            Network.list_number.Clear();
        }

        public ShowGraph(Socket socket) : this()
        {
            this.stream.Socket = socket;

            int result;

            result = stream.GraphRead();

            //MessageBox.Show(result.ToString(), "결과 값");

            // 데이터 리스트 생성
            List<int> values = new List<int> { };

            // 데이터 리스트 추가
            for (int i = 0; i < Network.list_number.Count; i++)
            {
                values.Add(Network.list_number[i]);
                //MessageBox.Show(Network.list_number[i].ToString(), "value 값");
            }

            // ChartValues 초기화
            Values = new ChartValues<ObservableValue>();

            // 데이터 리스트 반복하여 ChartValues에 추가
            foreach (int value in values)
            {
                Values.Add(new ObservableValue(value));
            }

            // X 축 레이블 설정 (List<string>으로 변경)
            XLabel = new List<string> { };

            for (int i = 0; i < Network.list_region.Count; i++)
            {
                XLabel.Add(Network.list_region[i]);
                //MessageBox.Show(Network.list_region[i], "column 값");
            }

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Menu menu = new Menu(stream.Socket);
            NavigationService.Navigate(menu);
        }
    }
}

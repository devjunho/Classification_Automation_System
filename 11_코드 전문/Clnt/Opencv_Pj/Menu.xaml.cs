using Network_;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

namespace Opencv_Pj
{

    public partial class Menu : Page
    {
        static public bool check = false;
        Network stream = new Network();
        JObject protocol = new JObject();
        public Menu()
        {
            InitializeComponent();
            if(!check)
            {
                stream.Connected();
                check = true;
            }
                
        }

        public Menu(Socket socket) : this ()
        {
            this.stream.Socket = socket;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Classify classify = new Classify(stream.Socket);
            NavigationService.Navigate(classify);
        }

        private void Graph_btn_Click(object sender, RoutedEventArgs e)
        {
            protocol["protocol"] = 24;
            stream.JsonWrite(protocol);
            ShowGraph showGraph = new ShowGraph(stream.Socket);
            NavigationService.Navigate(showGraph);
        }

        private void Past_btn_Click(object sender, RoutedEventArgs e)
        {
            protocol["protocol"] = 26;
            stream.JsonWrite(protocol);
            History history = new History(stream.Socket);
            NavigationService.Navigate(history);
        }
    }
}

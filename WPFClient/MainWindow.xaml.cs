using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using System.ComponentModel;

namespace WPFClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8889;
        static TcpClient client;
        static NetworkStream stream;
        private bool close = true;
        public MainWindow()
        {
            InitializeComponent();
        }
        void CloseD(object sender, CancelEventArgs e)
        {
            Disconnect();
        }
        private void Button_Send(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }
        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            userName = TBN.Text;
            client = new TcpClient();
            client.Connect(host, port);
            stream = client.GetStream();
            string message = userName;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();
        }
        private void SendMessage()
        {
            string message = Msg5.Text;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; 
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    if (message.IndexOf("вошел в чат") != -1)
                    {
                        Dispatcher.Invoke(new Action(() => Connect.IsEnabled = false));
                        Dispatcher.Invoke(new Action(() => TBN.IsReadOnly = true));
                        Dispatcher.Invoke(new Action(() => Send.IsEnabled = true));
                    } 
                    Dispatcher.Invoke(new Action(() => Msg2.Items.Add(message)));
                }
                catch
                {
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }
    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;

using System.Net;
using System.Net.Sockets;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string serverIP = "localhost";
        int port = 8080;

        public MainWindow()
        {
            InitializeComponent();

            ThreadStart childref = new ThreadStart(CallToChildThread);
            Console.WriteLine("In Main: Creating the Child thread");
            Thread childThread = new Thread(childref);
            childThread.Start();

           


        }

        public void CallToChildThread()
        {
            Console.WriteLine("Child thread starts");

            IPAddress ip = Dns.GetHostEntry("localhost").AddressList[0];
            TcpListener server = new TcpListener(ip, 8080);
            TcpClient client = default(TcpClient);

            try
            {
                server.Start();
                Console.WriteLine("Server started...");
                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Read();
            }

            while (true)
            {
                
                client = server.AcceptTcpClient();

                byte[] receivedBuffer = new byte[100];
                NetworkStream stream = client.GetStream();

                stream.Read(receivedBuffer, 0, receivedBuffer.Length);

                string msg = Encoding.ASCII.GetString(receivedBuffer, 0, receivedBuffer.Length);
                Console.WriteLine(msg);
                this.Dispatcher.Invoke(() =>
                {
                    updateTextBox(msg);
                });
                Console.Read();
            }
        }
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            TcpClient client = new TcpClient(serverIP, port);

            int byteCount = Encoding.ASCII.GetByteCount(messageBox.Text);

            byte[] sendData = new byte[byteCount];

            sendData = Encoding.ASCII.GetBytes(messageBox.Text);

            NetworkStream stream = client.GetStream();

            stream.Write(sendData, 0, sendData.Length);

            stream.Close();
            client.Close();

        }
        private void updateTextBox(String text)
        {
            receivedText.Text = text;
        }
        private void receivedText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

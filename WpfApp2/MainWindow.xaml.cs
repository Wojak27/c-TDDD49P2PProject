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

using System.Windows;
using System.Windows.Media.Imaging;

using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //for local testing
        //string serverIP = "localhost";
        //string clientIP = "localhost";
        string serverIP = "localhost";
        string clientIP = "localhost";
        int port = 8080;

        public MainWindow()
        {
            InitializeComponent();
            conversationBox.HorizontalContentAlignment = HorizontalAlignment.Right;

            ThreadStart childref = new ThreadStart(CallToChildThread);
            Console.WriteLine("In Main: Creating the Child thread");
            Thread childThread = new Thread(childref);
            childThread.Start();

           


        }

        public void CallToChildThread()
        {
            Console.WriteLine("Child thread starts");

            IPAddress ip = Dns.GetHostEntry(clientIP).AddressList[0];
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

                StringBuilder msg = new StringBuilder();

                foreach(byte b in receivedBuffer)
                {
                    if (b.Equals(00))
                    {
                        break;
                    }
                    else
                    {
                        msg.Append(Convert.ToChar(b).ToString());
                    }
                }
                String message = msg.ToString();
                
                Console.WriteLine(message);
                this.Dispatcher.Invoke(() =>
                {
                    updateConversationBox(message);
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
        private void updateConversationBox(String text)
        {
            TextMessageBox newMessage = new TextMessageBox();
            newMessage.setText(text);
            conversationBox.Items.Add(newMessage);
        }
        private void receivedText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void loadImage()
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                ImageMessageBox messageBox = new ImageMessageBox();
                messageBox.setInMessageImage(new BitmapImage(new Uri(op.FileName)));
                conversationBox.Items.Add(messageBox);
            }
        }

        private void imageButton_Click(object sender, RoutedEventArgs e)
        {
            loadImage();
        }
    }
}

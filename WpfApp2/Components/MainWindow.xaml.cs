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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.ObjectModel;
using WpfApp2.Model;
using Image = System.Drawing.Image;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ArrayList addresses = new ArrayList();
        private ObservableCollection<Object> conversationList = new ObservableCollection<Object>();

        //for local testing
        string clientIP = "localhost";
        string hostIP = "localhost";
        //string hostIP = "localhost";

        int port = 8080;

        public MainWindow()
        {
            InitializeComponent();
            var t1 = new TextMessageBox("you", "Hello");
            var t2 = new TextMessageBox("you", "Hello1");
            var t3 = new TextMessageBox("you", "Hello2");
            conversationList.Add(t1);
            conversationList.Add(t2);
            conversationList.Add(t3);
            //hostIP = GetLocalIPAddress();
            conversationBox.ItemsSource = conversationList;
            conversationBox.HorizontalContentAlignment = HorizontalAlignment.Right;

            ThreadStart childref = new ThreadStart(CallToChildThread);
            Console.WriteLine("In Main: Creating the Child thread");
            Thread childThread = new Thread(childref);
            childThread.Start();

            MessageItem testItem = new MessageItem();
            Image newImage = Image.FromFile(@"C:\Users\Windows1\source\repos\WpfApp2\WpfApp2\Components\userIcon.png");
            testItem.MessageText = "Hello World";
            testItem.Image = (Utilities.imageToBase64(newImage));
            string jsonTestItem = Utilities.convertMessageItemToJSON(testItem);
            Console.WriteLine("json item: " + jsonTestItem);
            MessageItem testItemRetrieved = Utilities.convertJSONToMessageItem(jsonTestItem);
            Console.WriteLine("Retrieved item: " + testItemRetrieved.MessageText);
            Console.WriteLine("Image item: " + testItemRetrieved.hasImage());

            Console.WriteLine(GetLocalIPAddress());

        }

        //for real ipv6 address
        
        private string GetLocalIPV6Address()
        {
            return Dns.GetHostEntry(hostIP).AddressList[0].ToString();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public void CallToChildThread()
        {

            IPAddress ip = Dns.GetHostEntry(hostIP).AddressList[0];
            hostIP = ip.ToString();
            TcpListener server = new TcpListener(ip, port);
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
                string address = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine("Client connected with IP {0}", ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                if (!addresses.Contains(address))
                {
                    showConnectionRequestDialogBox(address);
                }
                else
                {
                    byte[] receivedBuffer = new byte[10000000];
                    NetworkStream stream = client.GetStream();

                    stream.Read(receivedBuffer, 0, receivedBuffer.Length);
                    
                    this.Dispatcher.Invoke(() =>
                    {

                        updateConversationBox(receivedBuffer, address);
                    });
                    Console.Read();
                }
                
            }
        }

        private void showConnectionRequestDialogBox(string address)
        {
            string messageBoxText = "Do you want to accept the request from "+ address+"?";
            string caption = "New Connection request";
            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;

            // Display message box
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

            // Process message box results
            switch (result)
            {
                case MessageBoxResult.Yes:
                    addresses.Add(address);
                    this.Dispatcher.Invoke(() =>
                    {
                        contactList.Items.Add(address);
                    });
                    
                    break;
                case MessageBoxResult.No:
                    
                    break;
                case MessageBoxResult.Cancel:
                    
                    break;
            }
        }
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageItem messageItem = new MessageItem();
            messageItem.MessageText = messageBox.Text;
            messageItem.UserName = this.hostIP;
            sendMessage(messageItem);
            messageBox.Text = "";
        }

        private void sendMessage(MessageItem messageItem)
        {
            Console.WriteLine("client ip: "+ clientIP);
            try
            {
                TcpClient client = new TcpClient(clientIP, port);

                var jsonObjectToSend = Utilities.convertMessageItemToJSON(messageItem);
                Console.WriteLine("json to send: " + jsonObjectToSend);
                byte[] sendData = Utilities.stringToBytes(jsonObjectToSend);
                //byte[] sendData = Encoding.ASCII.GetBytes(messageBox.Text);

                NetworkStream stream = client.GetStream();
                Console.WriteLine("byte len sent: " + sendData.Length);
                stream.Write(sendData, 0, sendData.Length);

                stream.Close();
                client.Close();
            }
            catch(Exception e)
            {
                showNoValidIPDialogBox();
            }
           
        }

        private void showNoValidIPDialogBox()
        {
            string messageBoxText = "Given IP is invalid! Try adding new IP";
            string caption = "No IP found";
            MessageBoxButton button = MessageBoxButton.OK;

            MessageBoxImage icon = MessageBoxImage.Warning;

            // Display message box
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
        }
        private void updateConversationBox(byte[] bytes, string address)
        {

            Console.WriteLine("byte len recieved: " + bytes.Length);
            var json = Utilities.bytesToString(bytes);
            Console.WriteLine(json);
            var message = Utilities.convertJSONToMessageItem(json);

            Console.WriteLine("Started updating");
            Console.WriteLine("Message image: "+ message.Image);
            if (message.hasImage())
            {
                Image image = Utilities.Base64ToImage(message.Image);
                /*using (MemoryStream ms = new MemoryStream(bytes))
                {
                    var decoder = BitmapDecoder.Create(ms,
                        BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    image =  decoder.Frames[0];
                }*/

                ImageMessageBox messageBox = new ImageMessageBox();
                BitmapSource source = Utilities.GetImageStream(image);
                messageBox.setInMessageImage(source);
                conversationList.Add(messageBox);
                //conversationBox.Items.Add(messageBox);
                Console.Write("Have image");

            }
            else
            {
                TextMessageBox newMessage = new TextMessageBox(address, message.MessageText);
                conversationList.Add(newMessage);
                Console.WriteLine("Doesn't have image");
            }
            conversationBox.SelectedIndex = conversationBox.Items.Count - 1;
            conversationBox.ScrollIntoView(conversationBox.SelectedItem);

        }
        
        private void receivedText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void sendImage()
        {
            Console.WriteLine("Picking image");
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            
            if (op.ShowDialog() == true)
            {
                Console.WriteLine("Showing dialog");
                //ImageMessageBox messageBox = new ImageMessageBox();
                //messageBox.setInMessageImage(new BitmapImage(new Uri(op.FileName)));
                //conversationBox.Items.Add(messageBox);

                MessageItem message = new MessageItem();

                var wc = new WebClient();
                Image image = Image.FromStream(wc.OpenRead(new Uri(op.FileName)));
                var imageInBase64 = Utilities.imageToBase64(image);
                Console.WriteLine("image to send:" + imageInBase64);

                message.Image = imageInBase64;
                Console.WriteLine("message.image: " + message.Image);

                sendMessage(message);
            }
        }


        private void imageButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Image button click");
            sendImage();
        }

        private void myIPMenuButton_Click(object sender, RoutedEventArgs e)
        {
            YourIPDialog ipDialog = new YourIPDialog(hostIP);
            ipDialog.Show();
        }

        private void connectToNewIpMenuButton_Click(object sender, RoutedEventArgs e)
        {
            NewIPDialog ipDialog = new NewIPDialog();
            
            // Configure the dialog box
            ipDialog.Owner = this;

            // Open the dialog box modally
            var result = ipDialog.ShowDialog();

            if(ipDialog.DialogResult == true)
            {
                var senderIP = ipDialog.IP;
                // TODO: implement connection to the client if the IP address is valid
                Console.WriteLine("Sender: " + senderIP);
                sendRequestToIP(senderIP);
            }

        }

        private void sendRequestToIP(string ip)
        {
            MessageItem messageItem = new MessageItem();
            messageItem.MessageText = messageBox.Text;
            messageItem.UserName = this.hostIP;

            clientIP = ip;
            sendMessage(messageItem);
        }

        private void contactList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

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
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ArrayList addresses = new ArrayList();
        private ArrayList conversationList = new ArrayList();

        //for local testing
        string clientIP = "10.0.0.6";
        string hostIP = "localhost";
        //string hostIP = "localhost";

        int portClient = 8080;
        int portHost = 8080;

        public MainWindow()
        {
            InitializeComponent();
            var t1 = new TextMessageBox("you", "Hello");
            var t2 = new TextMessageBox("you", "Hello1");
            var t3 = new TextMessageBox("you", "Hello2");
            conversationList.Add(t1);
            conversationList.Add(t2);
            conversationList.Add(t3);
            hostIP = GetLocalIPAddress();
            //conversationBox.ItemsSource = conversationList;
            conversationBox.HorizontalContentAlignment = HorizontalAlignment.Right;

            ThreadStart childref = new ThreadStart(CallToChildThread);
            Console.WriteLine("In Main: Creating the Child thread");
            Thread childThread = new Thread(childref);
            childThread.Start();

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
            Console.WriteLine("Child thread starts");

            IPAddress ip = Dns.GetHostEntry(hostIP).AddressList[0];
            TcpListener server = new TcpListener(ip, portHost);
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
                    byte[] receivedBuffer = new byte[100];
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
            sendText(messageBox.Text);
            messageBox.Text = "";
        }

        private void sendText(String objectToSend)
        {
            TcpClient client = new TcpClient(clientIP, portClient);

            int byteCount = Encoding.ASCII.GetByteCount(objectToSend.ToString());
            MessageItem messageItem = new MessageItem();
           // messageItem.setMessageText(messageBox.Text);

            //byte[] sendData = ObjectToByteArray(messageItem);
            byte[] sendData = Encoding.ASCII.GetBytes(messageBox.Text);

            NetworkStream stream = client.GetStream();

            stream.Write(sendData, 0, sendData.Length);

            stream.Close();
            client.Close();
        }
        private void updateConversationBox(byte[] bytes, string address)
        {
            Console.WriteLine("Started updating");
            if (IsValidImage(bytes))
            {
                BitmapSource image;

                /*using (MemoryStream ms = new MemoryStream(bytes))
                {
                    var decoder = BitmapDecoder.Create(ms,
                        BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    image =  decoder.Frames[0];
                }*/

                Stream stream = new MemoryStream(bytes);
                var img = Bitmap.FromStream(stream);
                ImageMessageBox messageBox = new ImageMessageBox();
                BitmapImage bitmapImage = BitmapToImageSource((Bitmap)img);
                messageBox.setInMessageImage(bitmapImage);
                conversationBox.Items.Add(messageBox);
                Console.Write("Have image");

            }
            else
            {
                TextMessageBox newMessage = new TextMessageBox(address, bytesToText(bytes));
                //newMessage.setText(((MessageItem)ByteArrayToObject(bytes)).getMessageText());
                //conversationList.Add(newMessage);
                conversationBox.Items.Add(newMessage);
                Console.WriteLine("Doesn't have image");
            }
            conversationBox.SelectedIndex = conversationBox.Items.Count - 1;
            conversationBox.ScrollIntoView(conversationBox.SelectedItem);

        }

        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private BitmapImage BitmapFromSource(BitmapSource bitmapsource)
        {
            Console.WriteLine("bitmapsource" + bitmapsource.ToString());
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        private static String bytesToText(byte[] bytes)
        {
            StringBuilder msg = new StringBuilder();

            foreach (byte b in bytes)
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
            return msg.ToString();
        }


        public static bool IsValidImage(byte[] bytes)
        {
            try
            {
                Stream stream = new MemoryStream(bytes);
                var img = Bitmap.FromStream(stream);
                Console.WriteLine("valid image");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("not valid image");
                return false;
            }
            return true;
        }
        private void receivedText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private MessageItem ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return (MessageItem)binForm.Deserialize(memStream);
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
                Bitmap bitmap = BitmapImage2Bitmap(new BitmapImage(new Uri(op.FileName)));
                TcpClient client = new TcpClient(clientIP, portClient);

                byte[] sendData = ImageToByte(bitmap);

                NetworkStream stream = client.GetStream();

                stream.Write(sendData, 0, sendData.Length);

                stream.Close();
                client.Close();
                Console.WriteLine("Sending image");
            }
        }

        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        private void imageButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Image button click");
            sendImage();
        }

        private void myIPMenuButton_Click(object sender, RoutedEventArgs e)
        {
            YourIPDialog ipDialog = new YourIPDialog(GetLocalIPV6Address());
            ipDialog.Show();
        }

        private void connectToNewIpMenuButton_Click(object sender, RoutedEventArgs e)
        {
            NewIPDialog ipDialog = new NewIPDialog();
            ipDialog.Show();

        }

        private void contactList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

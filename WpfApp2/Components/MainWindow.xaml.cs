using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using WpfApp2.Model;
using Image = System.Drawing.Image;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using WpfApp2.Tools;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //for viewing the people in listbox
        private ObservableCollection<string> usersObservableList = new ObservableCollection<string>();

        //for query
        private ArrayList usersList = new ArrayList();

        //for showing the appropriate conversation
        private ObservableCollection<MessageInterface> conversationList = new ObservableCollection<MessageInterface>();

        //for linking the user to its conversation
        //private Dictionary<string, ArrayList> conversationDict = new Dictionary<string, ArrayList>();

        //for linking the user to its conversation
        private Dictionary<string, List<MessageItem>> conversationDict = new Dictionary<string, List<MessageItem>>();
        //for local testing
        string clientIP = "localhost";
        string hostIP = "localhost";

        int port = 8080;

        public MainWindow()
        {
            InitializeComponent();

            
            if (Utilities.hasSavedInstance())
            {
                LoadLastInstance();
            }
            //adding some fake data
            //InitiateFakeData();
            
            //linking the listboxes to the collections
            conversationBox.ItemsSource = conversationList;
            contactList.ItemsSource = usersObservableList;

            //load users to the users list box
            LoadUsers();
            
            conversationBox.HorizontalContentAlignment = HorizontalAlignment.Right;
            

            //starting new thread for the server
            ThreadStart hostServer = new ThreadStart(StartHostServer);
            Thread serverThread = new Thread(hostServer);
            serverThread.Start();

            Console.WriteLine(GetLocalIPAddress());

        }

        //Load instatnce for list
        private void LoadLastInstance()
        { 
            conversationDict = Utilities.retrieveInstanceFromDisk();
        }

        private void InitiateFakeData()
        {

            List<MessageItem> adamConversation = new List<MessageItem>();
            MessageItem item1 = new MessageItem();
            item1.MessageText = "Hello12";
            MessageItem item2 = new MessageItem();
            item2.MessageText = "Hello23";
            MessageItem item3 = new MessageItem();
            item3.MessageText = "Hello4";
            MessageItem item4 = new MessageItem();
            item4.MessageText = "Hello5";
            adamConversation.Add(item1);
            adamConversation.Add(item2);
            adamConversation.Add(item3);
            adamConversation.Add(item4);
            conversationDict.Add("Adam", adamConversation);

            List<MessageItem> evaConversation = new List<MessageItem>();
            MessageItem item5 = new MessageItem();
            item5.MessageText = "Hello14";
            MessageItem item6 = new MessageItem();
            item6.MessageText = "Hello45";
            MessageItem item7 = new MessageItem();
            item7.MessageText = "Hellsdfgo";
            MessageItem item8 = new MessageItem();
            item8.MessageText = "Helsdfgo";
            evaConversation.Add(item5);
            evaConversation.Add(item6);
            evaConversation.Add(item7);
            evaConversation.Add(item8);

            conversationDict.Add("Eva", evaConversation);
        }

        //Load users with list
        private void LoadUsers()
        {
            usersObservableList.Clear();
            foreach (KeyValuePair<string, List<MessageItem>> entry in conversationDict)
            {
                usersObservableList.Add(entry.Key);
                usersList.Add(entry.Key);
            }
        }

        private void ReloadUsers(IEnumerable<string> users)
        {
            usersObservableList.Clear();
            foreach(string user in users)
            {
                Console.WriteLine("User Reload: " + user);
                usersObservableList.Add(user);
                
            }
        }

        private void ReloadMessagesToConversationBox(List<MessageItem> array)
        {
            conversationList.Clear();
            foreach(MessageItem message in array)
            {
                updateConversationBox(message, message.UserName);
            }
            conversationBox.SelectedIndex = conversationBox.Items.Count - 1;
            conversationBox.ScrollIntoView(conversationBox.SelectedItem);
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

        public void StartHostServer()
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
                if (!usersObservableList.Contains(address))
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
                        //thanks to this the integrity of the message is preserved
                        try
                        {
                            var json = Utilities.bytesToString(receivedBuffer);
                            var decapsulatedMessage = KarolProtocol.decapsulateMessage(json);
                            var message = Utilities.convertJSONToMessageItem(decapsulatedMessage);
                            conversationDict[address].Add(message);
                            Utilities.saveInstanceOnDisk(conversationDict);
                            updateConversationBox(message, address);
                        }
                        catch(IncorrectProtocolException e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        
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
                    this.Dispatcher.Invoke(() =>
                    {
                        conversationDict.Add(address, new List<MessageItem>());
                        LoadUsers();
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
            sendMessage(messageItem);
            messageBox.Text = "";
        }

        private void sendMessage(MessageItem messageItem)
        {
            Console.WriteLine("client ip: "+ clientIP);
            try
            {
                TcpClient client = new TcpClient(clientIP, port);
                messageItem.UserName = this.hostIP;
                messageItem.MessageTime = DateTime.Now.ToString("h:mm:ss tt");
                var jsonObjectToSend = Utilities.convertMessageItemToJSON(messageItem);
                var encapsulatedMessage = KarolProtocol.encapsulteMessageInProtocol(jsonObjectToSend);
                Console.WriteLine("json to send: " + jsonObjectToSend);

                //byte[] sendData = Utilities.stringToBytes(encapsulatedMessage);
                //for testing that it works
                byte[] sendData = Utilities.stringToBytes(jsonObjectToSend);

                NetworkStream stream = client.GetStream();
                Console.WriteLine("byte len sent: " + sendData.Length);
                stream.Write(sendData, 0, sendData.Length);

                stream.Close();
                client.Close();
            }
            catch(System.Net.Sockets.SocketException e)
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
        private void updateConversationBox(MessageItem message, string address)
        {

            if (message.hasImage())
            {
                //Tried with sending the MessageItem object to the ImageMessageBox but did't
                //work for some reason :(
                //can show in the lab
                ImageMessageBox messageBox = new ImageMessageBox(message);
                
                conversationList.Add(messageBox);
                Console.Write("Have image");

            }
            else
            {
                TextMessageBox newMessage = new TextMessageBox(message);
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
            String user = (String)contactList.SelectedItem;
            Console.WriteLine(user);
            if(user != null)
            {
                ReloadMessagesToConversationBox(conversationDict[user]);
                clientIP = user;
            }
            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("TextField updated!");
            if(searchContactsTextField.Text == "")
            {
                LoadUsers();
            }
        }

        private void FindContactsFromSearchBox(string searchText)
        {

            Console.WriteLine("Seach text: " + searchText);
            if(searchText == "")
            {
                LoadUsers();
            }
            else
            {
                try
                {
                    string[] array = usersList.ToArray(typeof(string)) as string[];
                    foreach (string user in array)
                    {
                        Console.WriteLine("User: " + user);
                    }
                    var foundContacts =
                    from contact in array
                    where (contact.StartsWith(searchText))
                    select contact.ToString();
                    Console.WriteLine("User query: " + foundContacts);
                    ReloadUsers(foundContacts);
                }
                catch(ArgumentNullException e)
                {
                    Console.WriteLine("Null string provided to the query");
                }
                
            }
            

        }

        private void searchContactsButton_Click(object sender, RoutedEventArgs e)
        {
            FindContactsFromSearchBox(searchContactsTextField.Text);
        }
    }
}

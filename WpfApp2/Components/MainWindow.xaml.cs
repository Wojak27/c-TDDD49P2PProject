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
        //for viewing the people in listbox and following the MVVM design pattern
        private ObservableCollection<string> usersObservableList = new ObservableCollection<string>();

        //for showing the appropriate conversation and following the MVVM design pattern
        private ObservableCollection<MessageInterface> conversationList = new ObservableCollection<MessageInterface>();

        //for query listings
        private ArrayList usersList = new ArrayList();

        //for linking the user to its conversation
        private Dictionary<string, List<MessageItem>> conversationDict = new Dictionary<string, List<MessageItem>>();
        
        //for local testing, this can be changed to real ip if used in for example a vm
        string clientIP = "localhost";
        string hostIP = "localhost";

        //using the same port for development purpouses,but can have two different ones if needed 
        //just add another port and use it as a client/host
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

        //Load instatnce for list from drive
        private void LoadLastInstance()
        { 
            conversationDict = Utilities.retrieveInstanceFromDisk();
        }

        //to generate fake data. Comment out the method call in main function 
        //to not generate any
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

        //Load users with list when the application first starts
        private void LoadUsers()
        {
            usersObservableList.Clear();
            foreach (KeyValuePair<string, List<MessageItem>> entry in conversationDict)
            {
                usersObservableList.Add(entry.Key);
                usersList.Add(entry.Key);
            }
        }

        //reloads the users in search
        //used only in search
        private void ReloadUsers(IEnumerable<string> users)
        {
            usersObservableList.Clear();
            foreach(string user in users)
            {
                Console.WriteLine("User Reload: " + user);
                usersObservableList.Add(user);
                
            }
        }

        //reloads the messages that will show in the conversation box
        //this function is called every time we switch the user
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
        //not really used withe the localhost but can be used when using over the network
        //this will definatelly work if iv4 won't
        private string GetLocalIPV6Address()
        {
            return Dns.GetHostEntry(hostIP).AddressList[0].ToString();
        }

        //not really used withe the localhost but can be used when using over the network
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

        //this starts the host server to be able to receive messages
        //it lies on a separate thread and will not block the UI
        // this method could be rewritten for more compact/cleaner code
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
                        //thanks to this the integrity of the message is preserved haha ;)
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

        //when the first message from an unknown ip address is sent, this pops up
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
                        //creates a new conversation
                        conversationDict.Add(address, new List<MessageItem>());
                        LoadUsers();

                        //TODO: not implemented the callback to the sender with the same reques
                        //but was not specified in the requirements. For that simple send request can be used:
                        //sendRequestToIP(string address)
                    });
                    
                    break;
                case MessageBoxResult.No:
                    
                    break;
                case MessageBoxResult.Cancel:
                    
                    break;
            }
        }

        //sends a message on "Send" button click
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageItem messageItem = new MessageItem();
            messageItem.MessageText = messageBox.Text;
            sendMessage(messageItem);
            messageBox.Text = "";
        }

        //this method encapsulates the message sent to it as well as adds the essential metadata for every message
        //(independent if it's an image or text message like username or time)
        private void sendMessage(MessageItem messageItem)
        {
            Console.WriteLine("client ip: "+ clientIP);
            try
            {
                TcpClient client = new TcpClient(clientIP, port);
                messageItem.UserName = this.hostIP;
                messageItem.MessageTime = DateTime.Now.ToString("h:mm:ss tt");
                var jsonObjectToSend = Utilities.convertMessageItemToJSON(messageItem);

                //encapsulation with Karol's protocol. This is later decapsulated when recieving the message
                var encapsulatedMessage = KarolProtocol.encapsulteMessageInProtocol(jsonObjectToSend);

                byte[] sendData = Utilities.stringToBytes(encapsulatedMessage);

                //for testing that exeption works if you don't use Karol's protocol
                //byte[] sendData = Utilities.stringToBytes(jsonObjectToSend);

                NetworkStream stream = client.GetStream();
                Console.WriteLine("byte len sent: " + sendData.Length);
                stream.Write(sendData, 0, sendData.Length);

                stream.Close();
                client.Close();

                //for saving sent message on the host site if you're using vm
                //conversationDict[hostIP].Add(messageItem);
                //Utilities.saveInstanceOnDisk(conversationDict);
            }
            catch(System.Net.Sockets.SocketException e)
            {
                // if wrong ip provided this pops up (or if we use the fake data)
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

        //updates the conversation box. This method is called in couple of places like then changing the user
        //or when a new message is recieved
        private void updateConversationBox(MessageItem message, string address)
        {
            //this could be implemented in a better way, where we just have one messagebox
            //that is resizable if there is an image or not
            if (message.hasImage())
            {
                //creates new ImageMessageBox object
                ImageMessageBox messageBox = new ImageMessageBox(message);
                conversationList.Add(messageBox);

            }
            else
            {
                //creates new TextMessageBox object
                TextMessageBox newMessage = new TextMessageBox(message);
                conversationList.Add(newMessage);
            }
            //selects the newest item and scrolls to the buttom of the list
            conversationBox.SelectedIndex = conversationBox.Items.Count - 1;
            conversationBox.ScrollIntoView(conversationBox.SelectedItem);

        }
        
        //this is unused, but the program throws an error if not implemented
        private void receivedText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //standard image picker
        //MessageItem object created here is later sent to sendMessage() method
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

        //on imagebutton click
        private void imageButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Image button click");
            sendImage();
        }

        //shows the dialog with my ip
        private void myIPMenuButton_Click(object sender, RoutedEventArgs e)
        {
            YourIPDialog ipDialog = new YourIPDialog(hostIP);
            ipDialog.Show();
        }

        //shows the dialog enabeling you to connect to a new ip
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

                Console.WriteLine("Sender: " + senderIP);
                sendRequestToIP(senderIP);
            }

        }

        //just sends a request for the given ip address
        //later it forwards the messageitem object to sendMessage
        private void sendRequestToIP(string ip)
        {
            MessageItem messageItem = new MessageItem();
            messageItem.UserName = this.hostIP;

            clientIP = ip;
            sendMessage(messageItem);
        }

        //reloads the messages in the conversationbox for the selected user
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

        //doesn't work for some reason
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("TextField updated!");
            if(searchContactsTextField.Text == "")
            {
                LoadUsers();
            }
        }

        //method for searching in contacts
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

                    var foundContacts =
                    from contact in array
                    where (contact.StartsWith(searchText))
                    select contact.ToString();

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

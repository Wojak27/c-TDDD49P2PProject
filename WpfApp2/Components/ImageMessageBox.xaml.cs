
using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp2.Model;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImageMessageBox : UserControl, MessageInterface
    {
        public ImageMessageBox()
        {
            InitializeComponent();
        }

        //constructor with just the MessageItem for simplicity
        public ImageMessageBox(MessageItem message)
        {
            InitializeComponent();
            UserName = message.UserName;
            MessageTime = message.MessageTime;
            System.Drawing.Image image = Utilities.Base64ToImage(message.Image);
            BitmapSource source = Utilities.GetImageStream(image);
            setInMessageImage(source);
            Console.WriteLine("time image: " + MessageTime);
        }
        
        //getters and setters, implementation according to the interface
        public string UserName { get { return userNameTextField.Text; } set { userNameTextField.Text = value; } }
        public string MessageText { get; set; }
        public string MessageTime { get { return timeTextBoxImageMessage.Text; } set { timeTextBoxImageMessage.Text = value; } }
        public string Image { get { return null; } set { ; } }

        //sets the given image source to the image that is shown in the box
        public void setInMessageImage(BitmapSource source)
        {
            inmessageimage.Source = source;
        }

    }
}

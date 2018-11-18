
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

        public string UserName { get ; set; }
        public string MessageText { get; set; }
        public string MessageTime { get; set; }
        public string Image { get { return null; } set { ; } }

        public void setInMessageImage(BitmapSource source)
        {
            inmessageimage.Source = source;
        }

    }
}

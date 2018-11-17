using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing.Imaging;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImageMessageBox : UserControl
    {
        public ImageMessageBox()
        {
            InitializeComponent();
        }

        public void setInMessageImage(BitmapSource source)
        {
            inmessageimage.Source = source;
        }

    }
}

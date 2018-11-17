using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfApp2
{
    [Serializable]
    public class MessageItem
    {
        private string _image;

        public string UserName { get; set; }
        public string MessageText { get; set; }
        public string MessageTime { get; set; }
        public string Image { get { return _image; } set { _image = value; } }


        public bool hasImage()
        {
            return _image == null ? false : true;
        }
        
    }

}

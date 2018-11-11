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
    class MessageItem
    {
        private String messageText = "";

        /*public bool hasImage()
        {
            return image == null ? false : true;
        }*/

        public void setMessageText(String text)
        {
            messageText = text;
        }

        public String getMessageText()
        {
            return messageText;
        }
    }
}

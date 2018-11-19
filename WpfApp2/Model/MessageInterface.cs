using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.Model
{
    //an interface for unifying the MessageItem, TextMessageBox and ImageMessageBox
    interface MessageInterface
    {

        string UserName { get; set; }
        string MessageText { get; set; }
        string MessageTime { get; set; }
        string Image { get; set; }
        
    }
}

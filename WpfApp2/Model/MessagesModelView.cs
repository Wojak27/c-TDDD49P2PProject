using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp2.Model;

namespace WpfApp2
{
    public class MessagesModelView : BindableBase
    {
        private List<MessageItem> messageItems;
        public List<MessageItem> MessageItems
        {
            get
            {
                return messageItems;
            }
            set
            {
                SetProperty(ref messageItems, value);
            }
        }
        public MessagesModelView()
        {
            MessageItems = new List<MessageItem>();
            
        }
    }
}

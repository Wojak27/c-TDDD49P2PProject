using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.Tools
{
    class KarolProtocol
    {
        public static string encapsulteMessageInProtocol(string message)
        {
            return message + "Karol was here";
        }

        public static string decapsulateMessage(string messageToDeconvert)
        {
            if(!messageToDeconvert.Contains("Karol was here"))
            {
                throw new IncorrectProtocolException("Given string is not a valid protocol!");
            }else
                return messageToDeconvert.Replace("Karol was here", "");
        }
    }
}

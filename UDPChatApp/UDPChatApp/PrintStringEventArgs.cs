using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPChatApp
{
    public class PrintStringEventArgs : EventArgs
    {
        public string MessageToPrint { get; set; }

        public PrintStringEventArgs(string message) => MessageToPrint = message;
    }
}

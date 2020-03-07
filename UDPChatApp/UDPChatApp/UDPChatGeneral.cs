using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPChatApp
{
    public class UDPChatGeneral
    {
        public EventHandler<PrintStringEventArgs> RaisePrintStringEvent;

        protected virtual void OnRaisePrintStringEvent(PrintStringEventArgs e)
        {
            EventHandler<PrintStringEventArgs> handler = RaisePrintStringEvent;
            if(handler != null)
            {
                handler(this, e);
            }
        }
    }
}

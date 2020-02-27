using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPChatApp
{
    public class UDPChatServer
    {
        Socket mSockBroadcastReciever;
        IPEndPoint mIPEPLocal;
        private int retryCount;

        public UDPChatServer()
        {
            mSockBroadcastReciever = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            mIPEPLocal = new IPEndPoint(IPAddress.Any, 23000);

            mSockBroadcastReciever.EnableBroadcast = true;
        }

        public void StartRecievingData()
        {
            try
            {
                SocketAsyncEventArgs socketAEA = new SocketAsyncEventArgs();
                socketAEA.SetBuffer(new byte[1024], 0, 1024);
                socketAEA.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                if(!mSockBroadcastReciever.IsBound)
                {
                    mSockBroadcastReciever.Bind(mIPEPLocal);
                }
                socketAEA.Completed += RecieveCompletedCallback;

                if(!mSockBroadcastReciever.ReceiveFromAsync(socketAEA))
                {
                    Console.WriteLine($"Failed to reciev data - socket error: {socketAEA.SocketError}");
                    if(retryCount++ >= 10)
                    {
                        return;
                    }
                    else
                    {
                        StartRecievingData();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private void RecieveCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            string textRecieved = Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred);
            Console.WriteLine($"Text recieved: {textRecieved} \n " +
                $"Number of bytes recieved: {e.BytesTransferred} \n" +
                $"Recieved data from endpoint: {e.RemoteEndPoint}");

            StartRecievingData();
        }
    }
}

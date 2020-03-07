using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPChatApp
{
    public class UDPChatServer : UDPChatGeneral
    {
        Socket mSockBroadcastReciever;
        IPEndPoint mIPEPLocal;
        private int retryCount;

        List<EndPoint> listOfClients;

        public UDPChatServer()
        {
            mSockBroadcastReciever = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            mIPEPLocal = new IPEndPoint(IPAddress.Any, 23000);

            mSockBroadcastReciever.EnableBroadcast = true;

            listOfClients = new List<EndPoint>();
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
                    OnRaisePrintStringEvent(new PrintStringEventArgs($"Failed to reciev data - socket error: {socketAEA.SocketError}"));
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
                OnRaisePrintStringEvent(new PrintStringEventArgs(ex.ToString()));
                throw;
            }
        }

        private void RecieveCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            string textRecieved = Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred);
            Console.WriteLine($"Text recieved: {textRecieved} \n " +
                $"Number of bytes recieved: {e.BytesTransferred} \n" +
                $"Recieved data from endpoint: {e.RemoteEndPoint}");
            OnRaisePrintStringEvent(new PrintStringEventArgs($"Text recieved: {textRecieved} \n " +
                $"Number of bytes recieved: {e.BytesTransferred} \n" +
                $"Recieved data from endpoint: {e.RemoteEndPoint}"));

            if (textRecieved.Equals("<DISCOVER>"))
            {
                if(!listOfClients.Contains(e.RemoteEndPoint))
                {
                    listOfClients.Add(e.RemoteEndPoint);
                    Console.WriteLine("Total clients: " + listOfClients.Count);
                    OnRaisePrintStringEvent(new PrintStringEventArgs("Total clients: " + listOfClients.Count));
                }

                SendTextToEndPoint("<CONFIRM>", e.RemoteEndPoint);
            }
            else
            {
                foreach (var remoteEP in listOfClients)
                {
                    if (!remoteEP.Equals(e.RemoteEndPoint))
                    {
                        SendTextToEndPoint(textRecieved, remoteEP);
                    }
                }
            }

            StartRecievingData();
        }

        private void SendTextToEndPoint(string textToSend, EndPoint remoteEndPoint)
        {
            if(string.IsNullOrEmpty(textToSend) || remoteEndPoint == null)
            {
                return;
            }
            SocketAsyncEventArgs socketAEA = new SocketAsyncEventArgs();

            socketAEA.RemoteEndPoint = remoteEndPoint;

            var bytesToSend = Encoding.ASCII.GetBytes(textToSend);
            socketAEA.SetBuffer(bytesToSend, 0, bytesToSend.Length);

            socketAEA.Completed += SendTextToEnpointCompleted;

            mSockBroadcastReciever.SendToAsync(socketAEA);


        }

        private void SendTextToEnpointCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("Completed sending text to " + e.RemoteEndPoint);
            OnRaisePrintStringEvent(new PrintStringEventArgs("Completed sending text to " + e.RemoteEndPoint));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPChatApp
{
    public class UDPChatClient : UDPChatGeneral
    {
        Socket mSocketBroadCastSender;
        IPEndPoint miPEBroadcast;
        IPEndPoint mIPELocal;
        private EndPoint chatServerEP;

        public UDPChatClient(int localPort, int remotePort)
        {
            miPEBroadcast = new IPEndPoint(IPAddress.Broadcast, remotePort);
            mIPELocal = new IPEndPoint(IPAddress.Any, localPort);

            mSocketBroadCastSender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            mSocketBroadCastSender.EnableBroadcast = true;


        }

        public void SendBroadcast(string broadcastString)
        {
           if(string.IsNullOrEmpty(broadcastString))
            {
                return;
            }

            try
            {
                if(!mSocketBroadCastSender.IsBound)
                {
                    mSocketBroadCastSender.Bind(mIPELocal);
                }
                var dataBytes = Encoding.ASCII.GetBytes(broadcastString);
                SocketAsyncEventArgs socketAEA = new SocketAsyncEventArgs();
                socketAEA.SetBuffer(dataBytes, 0, dataBytes.Length);
                socketAEA.RemoteEndPoint = miPEBroadcast;

                socketAEA.Completed += SendCompletedCallback;

                mSocketBroadCastSender.SendToAsync(socketAEA);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                OnRaisePrintStringEvent(new PrintStringEventArgs(ex.ToString()));
                throw;
            }
        }

        private void SendCompletedCallback(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Data sent succesfully to: {e.RemoteEndPoint}");
            OnRaisePrintStringEvent(new PrintStringEventArgs($"Data sent succesfully to: {e.RemoteEndPoint}"));

            if (Encoding.ASCII.GetString(e.Buffer).Equals("<DISCOVER>"))
            {
                RecieveTextFromServer(expectedValue: "<CONFIRM>", IPERecieverLocal: mIPELocal);
            }
        }

        private void RecieveTextFromServer(string expectedValue, IPEndPoint IPERecieverLocal)
        {
            if(IPERecieverLocal == null)
            {
                Console.WriteLine("No IPEndpoint specified");
                OnRaisePrintStringEvent(new PrintStringEventArgs("No IPEndpoint specified"));
                return;
            }

            SocketAsyncEventArgs socketAEA = new SocketAsyncEventArgs();

            socketAEA.SetBuffer(new byte[1024], 0, 1024);
            socketAEA.RemoteEndPoint = IPERecieverLocal;

            socketAEA.UserToken = expectedValue;

            socketAEA.Completed += RecieveConfirmationCompleted;

            mSocketBroadCastSender.ReceiveFromAsync(socketAEA);
        }

        private void RecieveConfirmationCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(e.BytesTransferred == 0)
            {
                Debug.WriteLine("Zero bytes transferred, socket error" + e.SocketError);
                OnRaisePrintStringEvent(new PrintStringEventArgs("Zero bytes transferred, socket error" + e.SocketError));
                return;
            }

            var recievedText = Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred);
            var expectedText = Convert.ToString(e.UserToken);

            if (recievedText.Equals(expectedText))
            {
                Console.WriteLine("Recieved confirmation from server. " + e.RemoteEndPoint);
                OnRaisePrintStringEvent(new PrintStringEventArgs("Recieved confirmation from server. " + e.RemoteEndPoint));
                chatServerEP = e.RemoteEndPoint;
                RecieveTextFromServer(string.Empty, chatServerEP as IPEndPoint);
            }
            else if(string.IsNullOrEmpty(expectedText) && !string.IsNullOrEmpty(recievedText))
            {
                Console.WriteLine("Text recieved: " + recievedText);
                OnRaisePrintStringEvent(new PrintStringEventArgs("Text recieved: " + recievedText));

               RecieveTextFromServer(string.Empty, chatServerEP as IPEndPoint);
            }
            else if(!string.IsNullOrEmpty(expectedText) && !recievedText.Equals(expectedText))
            {
                Console.WriteLine("Expected token not returned by the server.");
                OnRaisePrintStringEvent(new PrintStringEventArgs("Expected token not returned by the server."));
            }
        }

        public void SendMessageToKnownServer(string message)
        {
            try
            {
                if(string.IsNullOrEmpty(message))
                {
                    return;
                }

                var bytesToSend = Encoding.ASCII.GetBytes(message);

                SocketAsyncEventArgs socketAEA = new SocketAsyncEventArgs();
                socketAEA.SetBuffer(bytesToSend, 0, bytesToSend.Length);

                socketAEA.RemoteEndPoint = chatServerEP;

                socketAEA.UserToken = message;

                socketAEA.Completed += SendMessageToKnownServerCompleted;
                mSocketBroadCastSender.SendToAsync(socketAEA);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                OnRaisePrintStringEvent(new PrintStringEventArgs(ex.ToString()));
               throw;
            }
        }

        private void SendMessageToKnownServerCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Sent: {e.UserToken} to Server: {e.RemoteEndPoint}");
            OnRaisePrintStringEvent(new PrintStringEventArgs($"Sent: {e.UserToken} to Server: {e.RemoteEndPoint}"));
        }
    }
}

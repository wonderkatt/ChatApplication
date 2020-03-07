using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UDPChatApp;

namespace UDPChatClientForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UDPChatApp.UDPChatClient chatClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(chatClient == null)
            {
                int.TryParse(tbLocalPort.Text, out var localPort);
                int.TryParse(tbRemotePort.Text, out var remotePort);
                chatClient = new UDPChatApp.UDPChatClient(localPort, remotePort);
                chatClient.RaisePrintStringEvent += chatClient_PrintString;
            }

            chatClient.SendBroadcast(tbBroadcastText.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            chatClient.SendMessageToKnownServer(tbMessage.Text);
        }

        private void chatClient_PrintString(object sender, PrintStringEventArgs e)
        {
            Action<string> print = PrintToTextBox;

            tbConsole.Dispatcher.Invoke(print, new String[] { e.MessageToPrint });
            //tbConsole.Text += $"{Environment.NewLine}{DateTime.Now} - {e.MessageToPrint}";
        }

        private void PrintToTextBox(string stringToPrint)
        {
            tbConsole.Text += $"{Environment.NewLine}{DateTime.Now} - {stringToPrint}";
        }

    }
}

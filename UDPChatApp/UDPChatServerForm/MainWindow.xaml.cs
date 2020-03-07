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

namespace UDPChatServerForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        UDPChatApp.UDPChatServer mUDPChatServer;

        public MainWindow()
        {
            mUDPChatServer = new UDPChatApp.UDPChatServer();
            mUDPChatServer.RaisePrintStringEvent += chatClient_PringString;
            InitializeComponent();
        }

        private void chatClient_PringString(object sender, PrintStringEventArgs e)
        {
            Action<string> print = PrintToTextBox;

            tbConsole.Dispatcher.Invoke(print, new String[] { e.MessageToPrint });
            //tbConsole.Text += $"{Environment.NewLine}{DateTime.Now} - {e.MessageToPrint}";
        }

        private void PrintToTextBox(string stringToPrint)
        {
            tbConsole.Text += $"{Environment.NewLine}{DateTime.Now} - {stringToPrint}";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mUDPChatServer.StartRecievingData();
        }
    }
}

using System.Threading;
using System.Windows;

namespace Server
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HandleServer myServer;
        public MainWindow()
        {
            InitializeComponent();

            myServer = new HandleServer();

            myServer.StartServer();

            Thread threadWaitClient = new Thread(myServer.waitForClient);
            threadWaitClient.Start();             
        }
    }
}
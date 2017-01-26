using System.Threading;
using System.Windows;

namespace Server
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class WindowServer : Window
    {
        HandleServer myServer;
        public WindowServer()
        {
            InitializeComponent();

            myServer = new HandleServer();
            DataContext = myServer;
        }
    }
}
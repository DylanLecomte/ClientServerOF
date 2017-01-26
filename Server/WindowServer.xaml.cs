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

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            myServer.db.connect();
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            myServer.db.insertUser("Jean", "Cloud");
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            myServer.db.updateBalance("Jean", 10);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
using System.Threading;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HandleConnection myConnection;
        public MainWindow()
        {
            InitializeComponent();

            myConnection = new HandleConnection();

            myConnection.Connect();
            DataContext = myConnection;

            Thread threadWaitClient = new Thread(myConnection.ManageConnection);
            threadWaitClient.Start();
        }
    }
}
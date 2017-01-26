using System.Threading;
using System.Windows;

namespace Client
{
    public partial class WindowClient : Window
    {
        HandleConnection myConnection;
        public WindowClient()
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
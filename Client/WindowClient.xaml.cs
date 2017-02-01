using System.Threading;
using System.Windows;

namespace Client
{
    public partial class WindowClient : Window
    {
        HandleConnection myConnection;
        public WindowClient(HandleConnection connection)
        {
            InitializeComponent();

            myConnection = connection;
            this.Closing += WindowClient_Closing;
            DataContext = myConnection;
        }

        private void WindowClient_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(myConnection.connected)
                myConnection.Clear();            
        }
    }
}
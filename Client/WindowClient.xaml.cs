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
            DataContext = myConnection;
        }
    }
}
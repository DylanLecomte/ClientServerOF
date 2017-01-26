using System.Windows;

namespace Client
{
    /// <summary>
    /// Logique d'interaction pour WindowClientConnection.xaml
    /// </summary>
    public partial class WindowClientConnection : Window
    {
        Connection myConnection;
        public WindowClientConnection()
        {
            InitializeComponent();

            myConnection = new Connection(this);
            DataContext = myConnection;
        }
    }
}

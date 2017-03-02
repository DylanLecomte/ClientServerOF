using System.Windows;

namespace Client
{
    /// <summary>
    /// Logique d'interaction pour WindowPayment.xaml
    /// </summary>
    public partial class WindowPayment : Window
    {
        HandleConnection myConnection;
        public WindowPayment(HandleConnection connection)
        {
            InitializeComponent();
            myConnection = connection;
            DataContext = myConnection;
        }
    }
}

using System.Windows;

namespace Client
{
    // Classe permettant de gérer la fenêtre d'ajout d'argent du client
    public partial class WindowPayment : Window
    {
        // Attributs
        HandleConnection myConnection;

        // Méthodes
        public WindowPayment(HandleConnection connection)
        {
            InitializeComponent();
            myConnection = connection;
            DataContext = myConnection;
        }

        // Méthode permettant d'afficher un message sur la fenêtre
        public void displayMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
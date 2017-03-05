using System.Windows;

namespace Client
{
    // Classe permettant de gérer la fenêtre de connexion
    public partial class WindowClientConnection : Window
    {
        // Attributs
        Connection myConnection;

        // Méthodes

        public WindowClientConnection()
        {
            InitializeComponent();

            myConnection = new Connection(this);
            DataContext = myConnection;
        }

        // Méthode permettant d'afficher un message sur la fenêtre
        public void displayMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
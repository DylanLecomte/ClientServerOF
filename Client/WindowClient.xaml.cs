using System.Windows;

namespace Client
{
    // Classe permettant de gérer la fenêtre principale du client
    public partial class WindowClient : Window
    {
        // Attributs
        HandleConnection myConnection;

        // Méthodes

        public WindowClient(HandleConnection connection)
        {
            InitializeComponent();

            myConnection = connection;
            this.Closing += WindowClient_Closing;
            DataContext = myConnection;
        }

        // Méthode permettant de fermer la connection lors de la fermeture de la fenêtre
        private void WindowClient_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(myConnection.connected)
                myConnection.Clear();            
        }
    }
}
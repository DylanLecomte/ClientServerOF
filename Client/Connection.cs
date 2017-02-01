using GalaSoft.MvvmLight.CommandWpf;
using System.Diagnostics;
using System.Threading;

namespace Client
{
    public class Connection
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public RelayCommand TryConnectionCommand { get; private set; }

        private readonly ClientFrameManager clientFrameManager;
        private readonly WindowClientConnection windowClientConnection;
        

        public Connection(WindowClientConnection _windowClientConnection)
        {
            TryConnectionCommand = new RelayCommand(TryConnection, CanTry);
            windowClientConnection = _windowClientConnection;
            clientFrameManager = new ClientFrameManager();
        }

        public void TryConnection()
        {
            WindowClient windowClient;
            HandleConnection myConnection;
            myConnection = new HandleConnection(Login);

            // tentative de connexion au serveur
            if(myConnection.Connect())
            {
                myConnection.SendMessage(clientFrameManager.ConnectionBuild(Login, Password));

                Thread.Sleep(1000);
                // tentative de login
                switch (clientFrameManager.ACKConnectionRead(myConnection.currentMessage))
                {
                    case "Ok":
                        myConnection.SendMessage(clientFrameManager.GetBalanceBuild());
                        windowClient = new WindowClient(myConnection);
                        windowClient.Show();
                        windowClientConnection.Close();
                        break;

                    case "Unknown":
                        myConnection.Clear();
                        windowClientConnection.displayMessage("Identifiant incorrect");
                        break;

                    case "PasswordFalse":
                        myConnection.Clear();
                        windowClientConnection.displayMessage("Mot de passe incorrect");
                        break;

                    default:
                        myConnection.Clear();
                        windowClientConnection.displayMessage("Erreur lors de la connection à la base de données");
                        break;
                }
            }
            // echec connection au serveur
            else
            {
                myConnection.Clear();
                Trace.WriteLine("Erreur lors de la connection au serveur");
            }            
        }

        public bool CanTry()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }
    }
}

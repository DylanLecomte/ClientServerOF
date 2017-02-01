using GalaSoft.MvvmLight.Command;
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
                Thread threadWaitClient = new Thread(myConnection.ManageConnection);
                threadWaitClient.Start();

                myConnection.SendMessage(clientFrameManager.ConnectionBuild(Login, Password));

                Thread.Sleep(1000);
                // tentative de login
                switch (clientFrameManager.ACKConnectionRead(myConnection.currentMessage))
                {
                    case "Ok":
                        windowClient = new WindowClient(myConnection);
                        windowClient.Show();
                        windowClientConnection.Close();
                        break;

                    case "Unknown":
                        myConnection.Clear(true);
                        windowClientConnection.displayMessage("Identifiant incorrect");
                        break;

                    case "PasswordFalse":
                        myConnection.Clear(true);
                        windowClientConnection.displayMessage("Mot de passe incorrect");
                        break;

                    default:
                        myConnection.Clear(true);
                        windowClientConnection.displayMessage("Erreur lors de la connection à la base de données");
                        break;
                }
            }
            // echec connection au serveur
            else
            {
                myConnection.Clear(false);
                Trace.WriteLine("Erreur lors de la connection au serveur");
            }            
        }

        public bool CanTry()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }
    }
}

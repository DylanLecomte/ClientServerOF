using GalaSoft.MvvmLight.CommandWpf;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;

namespace Client
{
    public class Connection
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public RelayCommand TryConnectionCommand { get; private set; }
        public RelayCommandPassword PasswordCommand { get; set; }

        private readonly ClientFrameManager clientFrameManager;
        private readonly WindowClientConnection windowClientConnection;
        
   
        public Connection(WindowClientConnection _windowClientConnection)
        {
            TryConnectionCommand = new RelayCommand(TryConnection, CanTry);
            PasswordCommand = new RelayCommandPassword(this.ExecutePasswordCommand);
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
                        windowClientConnection.displayMessage("Username incorrect");
                        break;

                    case "PasswordFalse":
                        myConnection.Clear();
                        windowClientConnection.displayMessage("Password incorrect");
                        break;

                    default:
                        myConnection.Clear();
                        windowClientConnection.displayMessage("Connection Failed");
                       break;
                }
            }
            // echec connection au serveur
            else
            {
                windowClientConnection.displayMessage("Impossible de se connecter au serveur");
                myConnection.Clear();
                Trace.WriteLine("Error when trying to connect to the server");
            }            
        }

        public bool CanTry()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }

        public void ExecutePasswordCommand(object obj)
        {
            PasswordBox _password;
            if (obj != null)
            {
                _password = (PasswordBox)obj;
                Password = _password.Password;
            }
        }
        
    }
}

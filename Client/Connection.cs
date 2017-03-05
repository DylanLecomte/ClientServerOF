using GalaSoft.MvvmLight.CommandWpf;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;

namespace Client
{
    public class Connection : INotifyPropertyChanged
    {
        private string _ContentButton;
        public string ContentButton
        {
            get { return _ContentButton; }
            set
            {
                _ContentButton = value;
                RaiseProperty(nameof(ContentButton));
            }
        }

        private bool _NewUser;

        public bool NewUser
        {
            get { return _NewUser; }
            set
            {
                if (value)
                    ContentButton = "Créer";
                else
                    ContentButton = "Connection";

                _NewUser = value;
                RaiseProperty(nameof(NewUser));
            }
        }

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
            ContentButton = "Connection";
            NewUser = false;
        }

        public void TryConnection()
        {
            WindowClient windowClient;
            HandleConnection myConnection;
            myConnection = new HandleConnection(Login);

            // tentative de connexion au serveur
            if(myConnection.Connect())
            {
                if(NewUser)
                {
                    if(IsPasswordValid(Password))
                        myConnection.SendMessage(clientFrameManager.CreationBuild(Login, Password));
                    else
                    {
                        windowClientConnection.displayMessage("Le mot de passe doit contenir au moins une majuscule, une chiffre et entre 8 et 15 caractères");
                        return;
                    }                  
                }
                else
                    myConnection.SendMessage(clientFrameManager.ConnectionBuild(Login, Password));

                Thread.Sleep(2000);
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

                    case "Duplication":
                        myConnection.Clear();
                        windowClientConnection.displayMessage("User already exist");
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

        public bool IsPasswordValid(string _mdp)
        {
            int nbNumb = 0;
            int nbMaj = 0;

            if (_mdp.Length < 8 || _mdp.Length > 15)
                return false;

            foreach (char digit in _mdp)
            {
                if (digit >= '0' && digit <= '9')
                    nbNumb++;
                if (char.IsUpper(digit))
                    nbMaj++;
            }
            if (nbNumb == 0 || nbMaj == 0)
                return false;

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperty(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));

    }
}
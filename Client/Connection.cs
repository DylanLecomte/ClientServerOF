using GalaSoft.MvvmLight.CommandWpf;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;

namespace Client
{
    // Clase permettant de gérer la connection au serveur de jeu
    public class Connection : INotifyPropertyChanged
    {
        // avec getters/setters
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

        // Méthodes

        // Constructeur
        public Connection(WindowClientConnection _windowClientConnection)
        {
            TryConnectionCommand = new RelayCommand(TryConnection, CanTry);
            PasswordCommand = new RelayCommandPassword(this.ExecutePasswordCommand);
            windowClientConnection = _windowClientConnection;
            clientFrameManager = new ClientFrameManager();
            ContentButton = "Connection";
            NewUser = false;
        }

        // Fonction appelée lors du clic sur le bouton "Connection" ou "Create"
        public void TryConnection()
        {
            WindowClient windowClient;
            HandleConnection myConnection;
            myConnection = new HandleConnection(Login);

            // tentative de connexion au serveur
            if(myConnection.Connect())
            {
                // S'il s'agit d'une création de compte, on vérifie que la structure du mot de passe est conforme
                // Lors d'une création, le serveur accepte directement la connection du client
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
                // Sinon connection normale
                else
                    myConnection.SendMessage(clientFrameManager.ConnectionBuild(Login, Password));

                // Tempo de deux secondes pour s'assurer que on reçoit bien la réponse du serveur
                Thread.Sleep(2000);
                // Réponser à la tentative de login
                switch (clientFrameManager.ACKConnectionRead(myConnection.currentMessage))
                {
                    // Connection Ok, lancement de la fenêtre de jeu, fermeture de celle-ci
                    case "Ok":
                        myConnection.SendMessage(clientFrameManager.GetBalanceBuild());
                        windowClient = new WindowClient(myConnection);
                        windowClient.Show();
                        windowClientConnection.Close();
                        break;
                    // Login rentré incorrect
                    case "Unknown":
                        myConnection.Clear();
                        windowClientConnection.displayMessage("Username incorrect");
                        break;
                    // Mot de passe rentré incorrect
                    case "PasswordFalse":
                        myConnection.Clear();
                        windowClientConnection.displayMessage("Password incorrect");
                        break;
                    // Dans le cas d'une création, login déjà existant
                    case "Duplication":
                        myConnection.Clear();
                        windowClientConnection.displayMessage("User already exist");
                        break;
                    // Autres erreurs
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

        // Méthode vérifier périodiquement que les valeurs des champs Login et MDP ne sont pas vide, cela grisera le champ de connection ou pas
        public bool CanTry()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }

        // Méthode appelée lors de la mise à jour du mot de passe pour le mettre à jour
        // Nécessaire car utilisation d'un PasswordBox qui ne permet pas de bind directement le mot de passe
        public void ExecutePasswordCommand(object obj)
        {
            PasswordBox _password;
            if (obj != null)
            {
                _password = (PasswordBox)obj;
                Password = _password.Password;
            }
        }

        // Méthode permettant de vérifier si le mot de passe rentré est conforme
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
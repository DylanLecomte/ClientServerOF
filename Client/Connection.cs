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
        private WindowClient windowClient;
        private readonly WindowClientConnection windowClientConnection;
        private HandleConnection myConnection;

        public Connection(WindowClientConnection _windowClientConnection)
        {
            TryConnectionCommand = new RelayCommand(TryConnection, CanTry);
            windowClientConnection = _windowClientConnection;
        }

        public void TryConnection()
        {
            myConnection = new HandleConnection();

            // tentative de connexion au serveur
            if(myConnection.Connect())
            {
                Thread threadWaitClient = new Thread(myConnection.ManageConnection);
                threadWaitClient.Start();

                myConnection.SendMessage("Try Login;" + Login + ";" + Password);
                System.Threading.Thread.Sleep(1000);
                // tentative de login
                if (myConnection.currentMessage.Equals("Login ok"))
                {
                    windowClient = new WindowClient(myConnection);
                    windowClient.Show();
                    windowClientConnection.Close();
                }
                // echec login
                else
                {
                    Trace.WriteLine(myConnection.currentMessage);
                }
            }
            // echec connection au serveur
            else
            {
                Trace.WriteLine("Erreur lors de la connection au serveur");
            }            
        }

        public bool CanTry()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }
    }
}

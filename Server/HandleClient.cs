using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Server
{
    // Classe de gestion d'un client
    class HandleClient
    {
        // Attribut
        TcpClient clientSocket;
        NetworkStream networkStream;
        readonly ServerFrameManager serverFrameManager;
        public string Username { get; private set; }
        private bool threadRunning { get; set; }
        private Thread ctThread;
        private readonly Database db;
        const string password = "Saucisse";
        DESEncrypt Encrypt;
        public string ConnectionConfirmed { get; private set; }

        private ConcurrentQueue<ThreadMessage> ActionQueue;

        public HandleClient(ref ConcurrentQueue<ThreadMessage> Queue)
        {
            ActionQueue = Queue;
            ConnectionConfirmed = "UNKNOW";
            Database.Error error;
            try
            {
                // Connection à la base données
                db = new Database("database.db");
                error = db.connect();
                // Instantciation des objets de gestion des trames et de chiffrement 
                serverFrameManager = new ServerFrameManager();
                Encrypt = new DESEncrypt();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error : " + ex.Message);
            }
        }

        public void startClient(TcpClient inClientSocket)
        {
            // Récupération de la socket
            this.clientSocket = inClientSocket;
            networkStream = clientSocket.GetStream();
            // Création du thread client
            ctThread = new Thread(ManageClient);
            // Démarrage de thread
            ctThread.Start();
        }

        private void ManageClient()
        {
            string message;
            string cryptedMessage;
            int requestCount = 0;
            string header;
            requestCount = 0;
            threadRunning = true;

            // Boucle principale du thread
            while (threadRunning)
            {
                try
                {
                    requestCount++;
                    if (networkStream.CanRead)
                    {
                        byte[] myReadBuffer = new byte[1024];
                        StringBuilder myCompleteMessage = new StringBuilder();
                        int numberOfBytesRead = 0;

                        do
                        {
                            // Récupération des données disponibles
                            numberOfBytesRead = networkStream.Read(myReadBuffer, 0, myReadBuffer.Length);
                            // Concaténation des données
                            myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                        }
                        while (networkStream.DataAvailable);
                        Trace.WriteLine("Frame recieved : " + myCompleteMessage.ToString());
                        // Récupération du chiffré
                        cryptedMessage = myCompleteMessage.ToString();
                        // Déchiffremment du message
                        message = Encrypt.DecryptString(cryptedMessage, password);

                        // Récupération du l'entête de la trame
                        header = serverFrameManager.GetFrameHeader(message);

                        // Traitement de la trame suivant l'entête
                        switch (header)
                        {
                            case "CREATE":
                                // Création d'un nouveau client
                                CreateUser(message);
                                break;
                            case "LOGIN":
                                // Vérification du login et mot de passe
                                CheckLogin(message);
                                break;
                            case "GBAL":
                                // Envoi du solde du client
                                SendBalance();
                                break;
                            case "UBAL":
                                // Mise à jour du solde du client
                                ManageBalance(message);
                                break;
                            case "LOGOUT":
                                // Déconnexion du client
                                threadRunning = false;
                                if(ConnectionConfirmed == "OK")
                                {
                                    // Envoi d'un message de déconnexion au serveur poeur mettre a jour l'IHM
                                    ActionQueue.Enqueue(new ThreadMessage(ThreadMessage.Action.Disconnection, Username, ""));
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
        }

        private void SendMessage(string message)
        {
            // Chriffrement
            string messageCrypted = Encrypt.EncryptString(message, password);

            // Vérification de la connexion
            if (clientSocket.Connected)
            {
                Byte[] sendBytes;

                // Préparation des doonnées pour envoi
                sendBytes = Encoding.ASCII.GetBytes(messageCrypted);
                // Envoi des données
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();
            }            
        }

        private void SendBalance()
        {
            Database.Error error;
            int Balance = 0;
            // Récupération du solde du client dans la base de doonées
            error = db.getBalance(Username, ref Balance);
            if (error == Database.Error.None)
            {
                // Envoie du solde au client
                SendMessage(serverFrameManager.SendBalanceBuild(Balance));

                // Envoie d'un message de mise à jour du solde au thread principal
                ActionQueue.Enqueue(new ThreadMessage(ThreadMessage.Action.Update, Username, Balance.ToString()));
            }
            else
            {
                // Envoi d'un message d'erreur
                SendMessage("Error");
            }
        }

        private void ManageBalance(string frame)
        {
            Database.Error error;
            int value;
            // Récupération de la différence dans le trame
            value = serverFrameManager.UpdatebalanceRead(frame);
            if(value > 0)
            {
                // Mise à jour du solde
                error = db.updateBalance(Username, value);
            } else
            {
                error = Database.Error.NonExistant;
            }

            // On log et renvoie la réponse au client
            if (error == Database.Error.None)
            {
                Trace.WriteLine(Username + " updated balance");
                SendMessage(serverFrameManager.ACKUpdateBalanceBuild(true));
            }
            else
            {
                Trace.WriteLine(Username + " failed to update balance");
                SendMessage(serverFrameManager.ACKUpdateBalanceBuild(false));
            }                
        }

        private void CheckLogin(string frame)
        {
            string login = "";
            string password = "";
            Database.Error error;

            // Analyse de la trame
            serverFrameManager.ConnectionRead(frame, ref login, ref password);

            // Tentative de connection avec le login et le mot de passe reçu
            error = db.checkLoginPwd(login, password);

            // On log
            if (error == Database.Error.None)
            {
                Trace.WriteLine(login + " sign in successful");
                Username = login;

                // Envoie d'un message de connection au thread principal
                ActionQueue.Enqueue(new ThreadMessage(ThreadMessage.Action.Connection, Username, "Loading..."));

                // Attente de la confirmation du serveur
                while (ConnectionConfirmed == "UNKNOW")
                {
                    Thread.Sleep(1);
                }

                if(ConnectionConfirmed == "OK")
                {
                    // On envoie la confirmation au client
                    SendMessage(serverFrameManager.ACKConnectionBuild(error));
                }
                else
                {
                    // On envoie un message d'erreur au client
                    SendMessage(serverFrameManager.ACKConnectionBuild(Database.Error.Duplication));
                }
            }
            else
            {
                // On envoie un message d'erreur au client
                SendMessage(serverFrameManager.ACKConnectionBuild(error));
                Trace.WriteLine(login + " failed to connect");
            }
        }

        private void CreateUser(string frame)
        {
            string login = "";
            string password = "";
            Database.Error error;

            // Analyse de la trame
            serverFrameManager.CreateRead(frame, ref login, ref password);

            // Tentative de création de compte
            error = db.insertUser(login, password);

            if(error == Database.Error.None)
                Trace.WriteLine(login + " created successful");
            else
            {
                Trace.WriteLine(login + " failed to create account");
                // On alerte le client
                SendMessage(serverFrameManager.ACKConnectionBuild(error));
                return;
            }               

            // Tentative de connection avec le login et le mot de passe reçu
            error = db.checkLoginPwd(login, password);

            // On log
            if (error == Database.Error.None)
            {
                Trace.WriteLine(login + " sign in successful");
                Username = login;

                // Envoie d'un message de connection au thread principal
                ActionQueue.Enqueue(new ThreadMessage(ThreadMessage.Action.Connection, Username, "Loading..."));

                while (ConnectionConfirmed == "UNKNOW") ;

                if(ConnectionConfirmed == "OK")
                {
                    // On renvoie la réponse au client
                    SendMessage(serverFrameManager.ACKConnectionBuild(error));
                }
                else
                {
                    // On renvoie la réponse au client
                    SendMessage(serverFrameManager.ACKConnectionBuild(Database.Error.Duplication));
                }
            }
            else
            {
                // On renvoie la réponse au client
                SendMessage(serverFrameManager.ACKConnectionBuild(error));
                Trace.WriteLine(login + " failed to connect");
            }
        }

        public void ConfirmConnection(string Confirmation)
        {

            // Mise à jour de la varaible de confirmation de connexion
            if (Confirmation == "OK" || Confirmation == "KO")
            {
                ConnectionConfirmed = Confirmation;
            }
            else
            {
                ConnectionConfirmed = "UNKNOW";
            }
        } 

        public void Clear()
        {
            if (ctThread != null && ctThread.IsAlive)
            {
                ctThread.Abort();
            }
            clientSocket.Close();
            networkStream.Close();
        }

        ~HandleClient()
        {
            clientSocket.Close();
        }
    }
}
using GalaSoft.MvvmLight.Command;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace Server
{
    // Classe de gestion du serveur
    class HandleServer : INotifyPropertyChanged
    {
        private TcpListener listener = null;
        private readonly List<HandleClient> listClients = new List<HandleClient>();
        private int connected { get; set; }
        private bool acceptClients { get; set; }
        public RelayCommand StartServerCommand { get; private set; }
        // Liste binder à la liste view de l'IHM
        public ObservableCollection<ListViewItem> Items { get; set; }
        // Liste des messages provenant des Clients
        private ConcurrentQueue<ThreadMessage> ActionQueue = new ConcurrentQueue<ThreadMessage>();
        private Thread threadWaitClient;
        private Thread threadMessageProcess;
        private CancellationTokenSource CancelTokenSource;
        private bool canStartServer=true;
        public bool CanStartServer
        {
            get
            {
                return canStartServer;
            }

            set
            {
                canStartServer = value;
                RaisePropertyChanged(nameof(CanStartServer));
            }
        }

        public HandleServer()
        {
            // Initialisation
            this.connected = 0;
            this.acceptClients = true;
            this.StartServerCommand = new RelayCommand(StartServer);

            this.Items = new ObservableCollection<ListViewItem>();
            this.CancelTokenSource = new CancellationTokenSource();
        }

        public void StartServer()
        {
            // Création de l'objet de gestion TCP
            listener = new TcpListener(IPAddress.Any, 1337);
            listener.Start();
            Trace.WriteLine("Server started");
            CanStartServer = false;

            // Démarrage du thread de gestion des nouveaux clients
            threadWaitClient = new Thread(() => waitForClient(CancelTokenSource.Token));
            threadWaitClient.Start();

            // Démarrage du thread de gestion de l'IHM
            threadMessageProcess = new Thread(() => MessageProcessing(CancelTokenSource.Token));
            threadMessageProcess.Start();

        }

        public void waitForClient(CancellationToken cancelToken)
        {
            TcpClient client;
            while (acceptClients)
            {
                if (cancelToken.IsCancellationRequested)
                    return;

                if (listener.Pending()) {
                    // Acception de la connexion du client
                    client = listener.AcceptTcpClient();
                    connected++;
                    Trace.WriteLine("New client accepted");
                    CancellationToken Token = CancelTokenSource.Token;
                    // Création d'un nouvel objet client
                    HandleClient newClient = new HandleClient(ref ActionQueue);
                    listClients.Add(newClient);
                    newClient.startClient(client);
                }
                Thread.Sleep(100);
            }

            Trace.WriteLine("End of threadWaitClient");
        }


        private void MessageProcessing(CancellationToken cancelToken)
        {
            ThreadMessage CurrentMsg;

            while (true) {

                if (cancelToken.IsCancellationRequested)
                    return;

                // Récupération du message
                if (ActionQueue.TryDequeue(out CurrentMsg))
                {
                    // Traitement du message
                    switch (CurrentMsg.ActionMsg)
                    {
                        // Connection d'un nouveau client
                        case ThreadMessage.Action.Connection:

                            int NbClientConnected = 0;
                            HandleClient NewClient = null;

                            // Récupération du nouveau client avec le username présent dans le message
                            foreach (var client in listClients)
                            {
                                if(client.Username == CurrentMsg.Username)
                                {
                                    NbClientConnected++;
                                    if (client.ConnectionConfirmed == "UNKNOW")
                                    {
                                        NewClient = client;
                                    }    
                                }
                            }

                            // Vérification qui n'est pas déja connecté
                            if (NewClient != null && NbClientConnected == 1)
                            {
                                // Confirmation de la connection
                                NewClient.ConfirmConnection("OK");
                                //Mise à jour de l'IHM
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Items.Add(new ListViewItem(CurrentMsg.Username, CurrentMsg.Balance));
                                });
                            }
                            else
                            {
                                NewClient.ConfirmConnection("KO");
                            }

                            break;
                        // Déconnection d'un client
                        case ThreadMessage.Action.Disconnection:
                            // Récupération du client qui va être déconnecter
                            var ClientToSupp = listClients.FirstOrDefault((item) => item.Username == CurrentMsg.Username);
                            if (ClientToSupp != null)
                            {
                                listClients.Remove(ClientToSupp);
                            }

                            // Suppression du client de l'IHM
                            var ItemToSupp = Items.FirstOrDefault((item) => item.Username == CurrentMsg.Username);
                            if (ItemToSupp != null)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Items.Remove(ItemToSupp);
                                });
                            }
                            break;
                       // Mise à jour du solde d'un client
                        case ThreadMessage.Action.Update:
                            var ItemToSet = Items.FirstOrDefault((item) => item.Username == CurrentMsg.Username);
                            if (ItemToSet != null)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    ItemToSet.Balance = CurrentMsg.Balance;
                                });
                            }
                            break;
                        default:
                            break;
                    }
                }
                Thread.Sleep(50);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void Clear() {

            // Envoi d'une demande d'arrêt
            CancelTokenSource.Cancel();

            // Attente de l'arret des threads
            if (threadMessageProcess != null && threadMessageProcess.IsAlive)
                threadMessageProcess.Join();
            if (threadWaitClient != null && threadWaitClient.IsAlive)
                threadWaitClient.Join();

            // Demande d'arrêt à tous les objets dans le liste des clients
            foreach (var item in listClients)
            {
                item.Clear();
            }

            if (listener != null)
                listener.Stop();
            CancelTokenSource.Dispose();

        }

        ~HandleServer()
        { }
    }

    
}
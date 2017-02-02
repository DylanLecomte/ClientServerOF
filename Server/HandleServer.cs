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
    class HandleServer : INotifyPropertyChanged
    {
        private TcpListener listener = null;
        private readonly List<HandleClient> listClients = new List<HandleClient>();
        private int connected { get; set; }
        private bool acceptClients { get; set; }
        public RelayCommand StartServerCommand { get; private set; }
        public ObservableCollection<ListViewItem> Items { get; set; }
        private bool terminate = false;
        private ConcurrentQueue<ThreadMessage> ActionQueue = new ConcurrentQueue<ThreadMessage>();
        private Thread threadWaitClient;
        private Thread threadMessageProcess;
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
            this.connected = 0;
            this.acceptClients = true;
            this.StartServerCommand = new RelayCommand(StartServer);

            this.Items = new ObservableCollection<ListViewItem>();
        }

        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 1337);
            listener.Start();
            Trace.WriteLine("Server started");
            CanStartServer = false;

            threadWaitClient = new Thread(waitForClient);
            threadWaitClient.Start();

            threadMessageProcess = new Thread(MessageProcess);
            threadMessageProcess.Start();

        }

        public void waitForClient()
        {
            TcpClient client;
            while (acceptClients && !terminate)
            {
                if (listener.Pending()) {
                    client = listener.AcceptTcpClient();
                    connected++;
                    Trace.WriteLine("New client accepted");
                    HandleClient newClient = new HandleClient(ref ActionQueue);
                    listClients.Add(newClient);
                    newClient.startClient(client);
                }
                Thread.Sleep(100);
            }

            Trace.WriteLine("End of threadWaitClient");
        }


        private void MessageProcess()
        {
            ThreadMessage CurrentMsg;

            while (!terminate) {
                if (ActionQueue.TryDequeue(out CurrentMsg))
                {
                    switch (CurrentMsg.ActionMsg)
                    {
                        case ThreadMessage.Action.Connection:
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Items.Add(new ListViewItem()
                                {
                                    Username = CurrentMsg.Username,
                                    Balance = CurrentMsg.Balance
                                });
                            });
                            break;
                        case ThreadMessage.Action.Disconnection:
                            var ClientToSupp = listClients.FirstOrDefault((item) => item.Username == CurrentMsg.Username);
                            if (ClientToSupp != null)
                            {
                                listClients.Remove(ClientToSupp);
                            }
                            var ItemToSupp = Items.FirstOrDefault((item) => item.Username == CurrentMsg.Username);
                            if (ItemToSupp != null)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Items.Remove(ItemToSupp);
                                });
                            }
                            break;
                        case ThreadMessage.Action.Update:
                            var ItemToUpdate = Items.FirstOrDefault((item) => item.Username == CurrentMsg.Username);
                            if (ItemToUpdate != null)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    ItemToUpdate.Balance = CurrentMsg.Balance;
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
            terminate = true;
            if (threadMessageProcess != null && threadMessageProcess.IsAlive)
                threadMessageProcess.Join();

            if (threadWaitClient != null && threadWaitClient.IsAlive)
                threadWaitClient.Join();

            if (listener != null)
                listener.Stop();
        }

        ~HandleServer()
        { }
    }

    
}

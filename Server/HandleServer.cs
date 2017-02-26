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
            this.connected = 0;
            this.acceptClients = true;
            this.StartServerCommand = new RelayCommand(StartServer);

            this.Items = new ObservableCollection<ListViewItem>();
            this.CancelTokenSource = new CancellationTokenSource();
        }

        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 1337);
            listener.Start();
            Trace.WriteLine("Server started");
            CanStartServer = false;

            threadWaitClient = new Thread(() => waitForClient(CancelTokenSource.Token));

            threadWaitClient.Start();

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
                    client = listener.AcceptTcpClient();
                    connected++;
                    Trace.WriteLine("New client accepted");
                    HandleClient newClient = new HandleClient(ref ActionQueue, CancelTokenSource.Token);
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

                if (ActionQueue.TryDequeue(out CurrentMsg))
                {
                    switch (CurrentMsg.ActionMsg)
                    {
                        case ThreadMessage.Action.Connection:
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Items.Add(new ListViewItem(CurrentMsg.Username, CurrentMsg.Balance));
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
            CancelTokenSource.Cancel();

            if (threadMessageProcess != null && threadMessageProcess.IsAlive)
                threadMessageProcess.Join();
            if (threadWaitClient != null && threadWaitClient.IsAlive)
                threadWaitClient.Join();
            
            foreach(var item in listClients)
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

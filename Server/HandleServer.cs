using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class HandleServer : INotifyPropertyChanged
    {
        private TcpListener listener = null;
        private readonly List<HandleClient> listClients = new List<HandleClient>();
        private int connected { get; set; }
        private bool acceptClients { get; set; }
        public RelayCommand StartServerCommand { get; private set; }
        public ObservableCollection<MyItem> Items { get; set; }
        private bool terminate = false;
        private Thread threadWaitClient;
        private Thread threadUpdateList;

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

            this.Items = new ObservableCollection<MyItem>();
        }

        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 1337);
            listener.Start();
            Trace.WriteLine("Server started");
            CanStartServer = false;

            threadWaitClient = new Thread(waitForClient);
            threadWaitClient.Start();

            threadUpdateList = new Thread(updateUserList);
            threadUpdateList.Start();
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
                    HandleClient newClient = new HandleClient();
                    listClients.Add(newClient);
                    newClient.startClient(client);
                }
                Thread.Sleep(100);
            }

            Trace.WriteLine("End of threadWaitClient");
        }

        public void updateUserList()
        {
            do
            {
                if (Items.Count != listClients.Count)
                {

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Items.Clear();
                    });

                    foreach (var item in listClients)
                    {

                        if (item.userName != null)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Items.Add(new MyItem() { userName = item.userName });
                            });
                        }

                    }
                }
                Thread.Sleep(100);
            } while (!terminate);
            Trace.WriteLine("End of threadUpdateList");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void Clean() {
            terminate = true;
            if (threadUpdateList != null && threadUpdateList.IsAlive)
                threadUpdateList.Join();

            if (threadUpdateList != null && threadWaitClient.IsAlive)
                threadWaitClient.Join();

            if (listener != null)
                listener.Stop();
        }

        ~HandleServer()
        { }
    }

    class MyItem
    {
        public string userName { get; set; }
    }
}

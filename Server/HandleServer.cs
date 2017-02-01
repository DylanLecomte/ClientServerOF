using GalaSoft.MvvmLight.Command;
using System;
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
        ObservableCollection<MyItem> Items;

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
            this.Items.Add(new MyItem() { Username = "Tartine" });

            Items.RemoveAt(0);
        }

        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 1337);
            listener.Start();
            Trace.WriteLine("Server started");
            CanStartServer = false;

            Thread threadWaitClient = new Thread(waitForClient);
            threadWaitClient.Start();

            Thread threadUpdateList = new Thread(updateUserList);
            threadUpdateList.Start();

            Thread threadDisconnection = new Thread(clientDisconnection);
            threadUpdateList.Start();
        }

        public void waitForClient()
        {
            TcpClient client;
            while (acceptClients)
            {
                client = listener.AcceptTcpClient();
                connected++;
                Trace.WriteLine("New client accepted");
                HandleClient newClient = new HandleClient();
                listClients.Add(newClient);
                newClient.startClient(client);
            }
        }

        private void updateUserList()
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

                        if (item.userName != "")
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Items.Add(new MyItem() { Username = item.userName });
                            });
                        }

                    }
                }
            } while (true);
        }

        private void clientDisconnection()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        ~HandleServer()
        {
            if(listener!=null)
                listener.Stop();
        }

        class MyItem
        {
            public string Username;
        }
    }
}

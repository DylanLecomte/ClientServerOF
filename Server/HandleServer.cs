using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
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
        }

        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 1337);
            listener.Start();
            Trace.WriteLine("Server started");
            CanStartServer = false;

            Thread threadWaitClient = new Thread(waitForClient);
            threadWaitClient.Start();
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
                newClient.startClient(client, Convert.ToString(connected));
            }
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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class HandleServer
    {
        private TcpClient client;
        private TcpListener listener;
        private readonly List<HandleClient> listClients = new List<HandleClient>();
        private int connected { get; set; }
        private bool acceptClients { get; set; }

        public HandleServer()
        {
            this.connected = 0;
            this.acceptClients = true;
        }

        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 1337);
            listener.Start();
            Trace.WriteLine("Server started");

        }

        public void waitForClient()
        {
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

        ~HandleServer()
        {
            listener.Stop();
        }
    }
}

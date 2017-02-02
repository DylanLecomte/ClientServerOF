using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class HandleClient
    {
        TcpClient clientSocket;
        NetworkStream networkStream;
        readonly ServerFrameManager serverFrameManager;
        public string userName { get; private set; }
        private bool threadRunning { get; set; }
        public bool disconnection { get; private set; }
        private readonly Database db;

        public HandleClient()
        {
            disconnection = false;
            Database.Error error;
            try
            {
                this.db = new Database();
                error = db.connect();
                this.serverFrameManager = new ServerFrameManager();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error : " + ex.Message);
            }
        }

        public void startClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            networkStream = clientSocket.GetStream();
            Thread ctThread = new Thread(ManageClient);
            ctThread.Start();
        }

        private void ManageClient()
        {
            int requestCount = 0;
            string header;
            requestCount = 0;
            threadRunning = true;

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
                            numberOfBytesRead = networkStream.Read(myReadBuffer, 0, myReadBuffer.Length);

                            myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                        }
                        while (networkStream.DataAvailable);
                        Trace.WriteLine("Frame recieved : " + myCompleteMessage.ToString());

                        header = serverFrameManager.GetFrameHeader(myCompleteMessage.ToString());

                        switch (header)
                        {
                            case "LOGIN":
                                CheckLogin(myCompleteMessage.ToString());
                                break;
                            case "GBAL":
                                SendBalance();
                                break;
                            case "UBAL":
                                ManageBalance(myCompleteMessage.ToString());
                                break;
                            case "LOGOUT":
                                disconnection = true;
                                threadRunning = false;
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
            if (clientSocket.Connected)
            {
                Byte[] sendBytes;

                sendBytes = Encoding.ASCII.GetBytes(message);
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();
            }            
        }

        private void SendBalance()
        {
            Database.Error error;
            int balance = 0;
            error = db.getBalance(userName, ref balance);
            if (error == Database.Error.None)
                SendMessage(serverFrameManager.SendBalanceBuild(balance));
            else
                SendMessage("Error");
        }

        private void ManageBalance(string frame)
        {
            Database.Error error;
            int value;
            value = serverFrameManager.UpdatebalanceRead(frame);
            error = db.updateBalance(userName, value);

            // On log et renvoie la réponse au client
            if (error == Database.Error.None)
            {
                Trace.WriteLine(userName + " updated balance");
                SendMessage(serverFrameManager.ACKUpdateBalanceBuild(true));
            }
            else
            {
                Trace.WriteLine(userName + " failed to update balance");
                SendMessage(serverFrameManager.ACKUpdateBalanceBuild(false));
            }                
        }

        private void CheckLogin(string frame)
        {
            string login = "";
            string password = "";
            Database.Error error;
            serverFrameManager.ConnectionRead(frame, ref login, ref password);
            
            // Tentative de connection avec le login et le mot de passe reçu
            error = db.checkLoginPwd(login, password);

            // On renvoie la réponse au client
            SendMessage(serverFrameManager.ACKConnectionBuild(error));
                
            // On log
            if (error == Database.Error.None)
            {
                Trace.WriteLine(login + " sign in successful");
                userName = login;
            }                
            else
                Trace.WriteLine(login + " failed to connect");
        }

        ~HandleClient()
        {
            clientSocket.Close();
        }
    }
}

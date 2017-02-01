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
        string clNo;
        private readonly Database db;
        public string Username { get; private set; }


        public HandleClient()
        {
            Username = "";
            try
            {
                this.db = new Database();
                this.db.connect();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error : " + ex.Message);
            }
        }

        public void startClient(TcpClient inClientSocket, string clientNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clientNo;
            networkStream = clientSocket.GetStream();
            Thread ctThread = new Thread(ManageClient);
            ctThread.Start();
        }
        private void ManageClient()
        {
            int requestCount = 0;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while (true)
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
                        // Traiter trame de caractère myCompleteMessage

                        string[] parameters;
                        string[] stringSeparators = new string[] { ";" };

       
                        if (myCompleteMessage.ToString().Contains("Try Login"))
                        {
                            parameters = myCompleteMessage.ToString().Split(stringSeparators, StringSplitOptions.None);
                            Trace.WriteLine("nb parameter : " + parameters.Length);
                            if(db.checkLoginPwd(parameters[1], parameters[2]) == Database.Error.None)
                            {
                                Trace.WriteLine("Sign in successful");
                                Username = parameters[1];
                            }
                            else
                            {
                                Trace.WriteLine("Sign in fail");
                            }
                        }
                    }

                    // Renvoyer trame en fonction de la trame
                    rCount = Convert.ToString(requestCount);
                    serverResponse = "Server to client(" + clNo + ") " + rCount;
                    SendMessage(serverResponse);
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

        ~HandleClient()
        {
            clientSocket.Close();
        }
    }
}

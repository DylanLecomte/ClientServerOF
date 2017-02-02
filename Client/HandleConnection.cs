using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class HandleConnection
    {
        private readonly TcpClient client;
        private NetworkStream networkStream;
        public string currentMessage { get; private set; }
        public bool connected { get; private set; }
        public bool valideThread { get; private set; }
        private readonly User user;
        private readonly ClientFrameManager clientFrameManager;
        private Thread ctThread;

        public RelayCommand<string> SendMessageCommand { get; private set; }

        public HandleConnection(string _userName)
        {
            user = new User(_userName);
            SendMessageCommand = new RelayCommand<string>(SendMessage);
            client = new TcpClient();
            clientFrameManager = new ClientFrameManager();
            valideThread = true;
        }

        public void Clear()
        {
            if(connected)
                SendMessage("LOGOUT;");
            Thread.Sleep(1000);
            this.client.Close();
            valideThread = false;
            if(ctThread!=null && ctThread.IsAlive)
                ctThread.Join();                
        }

        public bool Connect()
        {
            try
            {
                client.Connect("127.0.0.1", 1337);
                networkStream = client.GetStream();
                ctThread = new Thread(ManageConnection);
                ctThread.Start();
                this.connected = true;
                return true;
            }
            catch (Exception exc)
            {
                Trace.WriteLine(exc);
                return false;
            }            
        }

        public void ManageConnection()
        {
            string header;
            while (valideThread)
            {
                try
                {
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
                        currentMessage = myCompleteMessage.ToString();

                        header = clientFrameManager.GetFrameHeader(currentMessage);

                        switch (header)
                        {
                            case "SBAL":
                                getBalance(currentMessage);
                                break;
                            case "ACKUBAL":
                                ACKBalance(currentMessage);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
                Thread.Sleep(10);
            }            
        }

        public void getBalance(string message)
        {
            user.balance = clientFrameManager.SendBalanceRead(message);
            Trace.WriteLine("Balance updated : " + user.balance.ToString());
        }

        public bool ACKBalance(string message)
        {
            if(clientFrameManager.ACKUpdateBalanceBuild(message))
            {
                Trace.WriteLine("ACK update balance OK");
                return true;
            }
            else
            {
                Trace.WriteLine("ACK update balance KO");
                return false;
            }           
        }

        public void SendMessage(string message)
        {
            if (client.Connected)
            {
                byte[] sendBytes ;

                sendBytes = Encoding.ASCII.GetBytes(message);
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();
            }
        }
    }
}

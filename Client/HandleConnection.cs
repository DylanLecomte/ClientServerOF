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

        public RelayCommand<string> SendMessageCommand { get; private set; }

        public HandleConnection()
        {
            SendMessageCommand = new RelayCommand<string>(SendMessage);
            client = new TcpClient();
        }

        public bool Connect()
        {
            try
            {
                client.Connect("127.0.0.1", 1337);
                networkStream = client.GetStream();
                Thread ctThread = new Thread(ManageConnection);
                ctThread.Start();
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
            while (true)
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
                        // Traiter trame de caractère myCompleteMessage
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
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

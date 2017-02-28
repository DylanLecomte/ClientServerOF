﻿using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class HandleConnection : INotifyPropertyChanged
    {
        private readonly TcpClient client;
        private NetworkStream networkStream;
        public string currentMessage { get; private set; }
        public bool connected { get; private set; }
        public bool valideThread { get; private set; }
        public string betValue { get; set; }
        private bool _userCanBet;

        public string _infoPlayer { get; private set; }

        public string infoPlayer
        {
            get { return _infoPlayer; }
            set
            {
                _infoPlayer = value;
                RaisePropertyChanged("infoPlayer");
            }
        }


        public bool UserCanBet
        {
            get { return _userCanBet; }
            set { _userCanBet = value; }
        }

        public User user { get;}
        private readonly ClientFrameManager clientFrameManager;
        private Thread ctThread;
        private Thread playThread;

        public RelayCommand<string> SendMessageCommand { get; private set; }
        public RelayCommand BetCommand { get; private set; }

        public HandleConnection(string _userName)
        {
            user = new User(_userName);
            SendMessageCommand = new RelayCommand<string>(SendMessage);
            BetCommand = new RelayCommand(Bet,CanBet);
            client = new TcpClient();
            clientFrameManager = new ClientFrameManager();
            valideThread = true;
            UserCanBet = true;
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
                        //Trace.WriteLine("Frame recieved : " + myCompleteMessage.ToString());
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
            user.Balance = clientFrameManager.SendBalanceRead(message);
            Trace.WriteLine("Balance : " + user.Balance.ToString());
        }

        public bool ACKBalance(string message)
        {
            if(clientFrameManager.ACKUpdateBalanceBuild(message))
            {
                SendMessage(clientFrameManager.GetBalanceBuild());
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

        public void Bet()
        {
            Trace.WriteLine("");
            infoPlayer = "Playing...";
            UserCanBet = false;
            //SendMessage(clientFrameManager.UpdatebalanceBuild(-betValue));
            //Trace.WriteLine("Bet of : " + betValue.ToString());
            SendMessage(clientFrameManager.UpdatebalanceBuild(-(Int32.Parse(betValue))));
            Trace.WriteLine("Bet of : " + betValue);

            //Thread.Sleep(1000);
            //play();
            playThread = new Thread(Play);
            playThread.Start();
        }

        public bool CanBet()
        {
            //return betValue >= 1 && UserCanBet && betValue <= user.Balance;
            int value;
            if (int.TryParse(betValue, out value))
                return value >= 1 && UserCanBet && value <= user.Balance;
            else
                return false;           
        }

        public void Play()
        {
            Thread.Sleep(2000);
            Random gen = new Random();
            int prob = gen.Next(100);
            
            if (prob <= 50)
            {
                //SendMessage(clientFrameManager.UpdatebalanceBuild(betValue * 2));
                SendMessage(clientFrameManager.UpdatebalanceBuild(Int32.Parse(betValue) * 2));
                infoPlayer = "Partie gagnée !";
                Trace.WriteLine("Partie gagnée !");
            }
            else
            {
                Trace.WriteLine("Partie perdue !");
                infoPlayer = "Partie perdue !";
            }               

            UserCanBet = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
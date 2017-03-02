using GalaSoft.MvvmLight.CommandWpf;
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

        private string _betValue;

        public string BetValue
        {
            get { return _betValue; }
            set
            {
                _betValue = value;
                RaisePropertyChanged("BetValue");
            }
        }

        private bool _userCanBet;

        public string _infoPlayer;

        public string InfoPlayer
        {
            get { return _infoPlayer; }
            set
            {
                _infoPlayer = value;
                RaisePropertyChanged("InfoPlayer");
            }
        }


        public bool UserCanBet
        {
            get { return _userCanBet; }
            set { _userCanBet = value; }
        }

        public User user { get;}
        private readonly ClientFrameManager clientFrameManager;
        const string password = "Saucisse";
        DESEncrypt Encrypt ;
        private Thread ctThread;
        private Thread playThread;

        private WindowPayment windowPayment;

        public RelayCommand BetCommand { get; private set; }
        public RelayCommand AddMoneyCommand { get; private set; }
        public RelayCommand TempCommand { get; private set; }

        public HandleConnection(string _userName)
        {
            user = new User(_userName);
            BetCommand = new RelayCommand(Bet,CanBet);
            AddMoneyCommand = new RelayCommand(AddMoney);
            TempCommand = new RelayCommand(Temp);
            Encrypt = new DESEncrypt();
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
                        string cryptedMessage;
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
                        cryptedMessage= myCompleteMessage.ToString();
                        currentMessage = Encrypt.DecryptString(cryptedMessage, password);

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
            string messageCrypted = Encrypt.EncryptString(message, password);
            
            if (client.Connected)
            {
                byte[] sendBytes ;

                sendBytes = Encoding.ASCII.GetBytes(messageCrypted);
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();
            }
        }

        public void Bet()
        {
            Trace.WriteLine("");
            InfoPlayer = "Playing...";
            UserCanBet = false;
            SendMessage(clientFrameManager.UpdatebalanceBuild(-(Int32.Parse(BetValue))));
            Trace.WriteLine("Bet of : " + BetValue);

            playThread = new Thread(Play);
            playThread.Start();
        }

        public bool CanBet()
        {            
            int value;
            if (int.TryParse(BetValue, out value))
                return value >= 1 && UserCanBet && value <= user.Balance;
            else
                return false;           
        }

        public void Play()
        {
            Thread.Sleep(1500);
            Random gen = new Random();
            int prob = gen.Next(100);
            
            if (prob <= 50)
            {
                SendMessage(clientFrameManager.UpdatebalanceBuild(Int32.Parse(BetValue) * 2));
                InfoPlayer = "Partie gagnée !";
                Trace.WriteLine("Partie gagnée !");
            }
            else
            {
                Trace.WriteLine("Partie perdue !");
                InfoPlayer = "Partie perdue !";
            }               

            UserCanBet = true;
            BetValue = "";
        }

        public void AddMoney()
        {
            windowPayment = new WindowPayment(this);
            windowPayment.ShowDialog();
        }

        public void Temp()
        {
            string plainText = "Test of string to encrypt";
            string password = "Saucisse";

            DESEncrypt testEncrypt = new DESEncrypt();

            string encText = testEncrypt.EncryptString(plainText, password);
            string plain = testEncrypt.DecryptString(encText, password);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
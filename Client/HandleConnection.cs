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
        public Card card { get; }
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

        private bool _userCanAddMoney;
        public bool UserCanAddMoney
        {
            get { return _userCanAddMoney; }
            set
            {
                _userCanAddMoney = value;
                RaisePropertyChanged("UserCanAddMoney");
            }
        }

        private bool _userCanBet;
        public bool UserCanBet
        {
            get { return _userCanBet; }
            set { _userCanBet = value; }
        }

        private string _moneyToAdd;

        public string MoneyToAdd
        {
            get { return _moneyToAdd; }
            set { _moneyToAdd = value; }
        }


        public User user { get; }
        private readonly ClientFrameManager clientFrameManager;
        const string password = "Saucisse";
        DESEncrypt Encrypt;
        private Thread ctThread;
        private Thread playThread;

        private WindowPayment windowPayment;

        public RelayCommand BetCommand { get; private set; }
        public RelayCommand AddMoneyCommand { get; private set; }
        public RelayCommand PaymentCommand { get; private set; }

        public HandleConnection(string _userName)
        {
            user = new User(_userName);
            card = new Card();
            UserCanAddMoney = true;
            BetCommand = new RelayCommand(Bet, CanBet);
            AddMoneyCommand = new RelayCommand(AddMoney);
            PaymentCommand = new RelayCommand(Payment, CanPaid);
            Encrypt = new DESEncrypt();
            client = new TcpClient();
            clientFrameManager = new ClientFrameManager();
            valideThread = true;
            UserCanBet = true;
        }

        public void Clear()
        {
            if (connected)
                SendMessage("LOGOUT;");
            Thread.Sleep(1000);
            this.client.Close();
            valideThread = false;
            if (ctThread != null && ctThread.IsAlive)
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
                        cryptedMessage = myCompleteMessage.ToString();
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
                    InfoPlayer = "Connection lost. Restart client...";
                    UserCanBet = false;
                    UserCanAddMoney = false;

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
            if (clientFrameManager.ACKUpdateBalanceBuild(message))
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
                byte[] sendBytes;

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
            card.ResetCard();
            windowPayment = new WindowPayment(this);
            windowPayment.ShowDialog();
        }

        public void Payment()
        {
            int errorInfo =0 ;
            bool testCard = Card.CheckCardNumber(card.CardNumber);
            int year = Int32.Parse(card.CardDate.Substring(3, 4));
            int month = Int32.Parse(card.CardDate.Substring(0, 2));
            int currentMonth = Int32.Parse(DateTime.Now.Month.ToString()) ;
            int currentYear = Int32.Parse(DateTime.Now.Year.ToString()) ;

            if (testCard==false)
                errorInfo = 1;

            if (year < currentYear || year > currentYear + 3)
                errorInfo = 2;
            else if (year == currentYear)
            {
                if (month < currentMonth)
                    errorInfo = 2;
            }
            else if (year == currentYear + 2)
            {
                if (month > currentMonth)
                    errorInfo = 2;
            }

            if (Int32.Parse(MoneyToAdd) > 1000)
                errorInfo = 3;

            switch (errorInfo)
            {
                case 0:
                    SendMessage(clientFrameManager.UpdatebalanceBuild(Int32.Parse(MoneyToAdd)));
                    windowPayment.displayMessage("Balance updated");
                    windowPayment.Close();
                    break;
                case 1:
                    windowPayment.displayMessage("Invalid card number");
                    break;
                case 2:
                    windowPayment.displayMessage("Invalid card date");
                    break;
                case 3:
                    windowPayment.displayMessage("Invalid amount");
                    break;
            }
        }

        public bool CanPaid()
        {
            string month;
            string year;
            int monthZero;
            string date;

            if (UserCanAddMoney == false)
                return false;

            if (card.CardNumber == null || card.CardCrypto == null || card.CardDate == null || MoneyToAdd == null)
                return false;

            date = card.CardDate;

            if ( (IsNumeric(card.CardNumber) == false) || card.CardNumber.Length != 16 )
                return false;

            if (card.CardDate.Length != 7)
                return false;
            else
            {
                month = card.CardDate.Substring(0, 2);
                year = card.CardDate.Substring(3, 4);

                if (IsNumeric(month))
                    monthZero = getMonth(month);
                else
                    return false;

                if ((IsNumeric(month) == false) || monthZero < 1 || monthZero > 12 )
                    return false;

                if (card.CardDate.Substring(2, 1) != "/")
                    return false;

                if ( IsNumeric(month) == false )
                    return false;
            }

            if ( (IsNumeric(card.CardCrypto) == false) || card.CardCrypto.Length != 3 )
                return false;

            if ((IsNumeric(MoneyToAdd) == false) || Int32.Parse(MoneyToAdd) < 1)
                return false;

            return true;
        }

        public bool IsNumeric(string text)
        {
            if ((text == null) || text.Length ==0)
                return false;
            foreach (char digit in text)
            {
                if (digit < '0' || digit > '9')
                    return false;
            }
            return true;
        }

        public int getMonth(string _month)
        {
            if (card.CardDate.Substring(0, 1) == "0")
                return Int32.Parse(_month.Substring(1, 1));
            else
                return Int32.Parse(_month);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
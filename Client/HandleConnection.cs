﻿using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    // Classe permettant de gérer la connexion du client avec le serveur
    public class HandleConnection : INotifyPropertyChanged
    {
        // Attributs avec getters/setters
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
        // Mot de passe pour le chiffrement
        const string password = "Saucisse";
        readonly DESEncrypt Encrypt;
        private Thread ctThread;

        private WindowPayment windowPayment;

        public RelayCommand BetCommand { get; private set; }
        public RelayCommand AddMoneyCommand { get; private set; }
        public RelayCommand PaymentCommand { get; private set; }

        // Méthodes

        // Constructeur
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

        // Méthode permettant de fermer proprement le client, en annonçant la déconnexion au serveur et stoppant les threads en cours
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
    
        // Méthode permettant de se connecter au serveur
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

        // Méthode gérant les trames reçues du serveur
        public void ManageConnection()
        {
            string header;
            while (valideThread)
            {
                try
                {
                    // Réception d'une chaine de caractère du serveur
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
                        // Dechiffrement de la chaine
                        currentMessage = Encrypt.DecryptString(cryptedMessage, password);

                        // Récupération du header de la tram
                        header = clientFrameManager.GetFrameHeader(currentMessage);

                        // En fonction du header, on appel des fonctions pour gérer les trames
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
                // En cas de rupture de la connexion avec le serveur, on affiche un message pour lui dire de redemarer le client 
                // On l'interdit de jouer/ajouter de l'argent
                catch (Exception ex)
                {
                    InfoPlayer = "Connection lost. Restart serveur and client...";
                    UserCanBet = false;
                    UserCanAddMoney = false;

                    Trace.WriteLine(ex.ToString());
                }
                Thread.Sleep(10);
            }
        }

        // Méthode permettant de récupérer le montant en appelant la méthode de la classe clientFrameManager
        public void getBalance(string message)
        {
            user.Balance = clientFrameManager.SendBalanceRead(message);
            Trace.WriteLine("Balance : " + user.Balance.ToString());
        }

        // Méthode permettant savoir si la récupération du montant s'est bien passé en appelant la méthode de la classe clientFrameManager
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

        // Méthode permettant d'envoyer une trame au serveur.
        // On chiffre la trame avec la méthode EncryptString de la classe Encrypt
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

        // Méthode permettant de parier
        public void Bet()
        {
            Thread playThread;
            InfoPlayer = "Playing...";
            UserCanBet = false;
            // Appel de la fonction SendMessage et construction de la trame avec la méthode UpdatebalanceBuild de la classe clientFrameManager
            SendMessage(clientFrameManager.UpdatebalanceBuild(-(Int32.Parse(BetValue))));
            Trace.WriteLine("Bet of : " + BetValue);

            // On lance la partie
            playThread = new Thread(Play);
            playThread.Start();
        }

        // Méthode permettant de vérifier sur le client peut parier ou pas en fonction de la valeur rentrée et de son montant
        public bool CanBet()
        {
            int value;
            if (int.TryParse(BetValue, out value))
                return value >= 1 && UserCanBet && value <= user.Balance;
            else
                return false;
        }

        // Méthode permettant de lancer une partie
        public void Play()
        {
            // Tempo pour s'assurer que le serveur a bien débité le montant du pari avant de jouer
            Thread.Sleep(1500);
            Random gen = new Random();
            int prob = gen.Next(100);

            if (prob <= 50)
            {
                // Appel de la fonction SendMessage et construction de la trame avec la méthode UpdatebalanceBuild de la classe clientFrameManager
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

        // Méthode permettant de lancer la fenêtre d'ajout d'argent
        public void AddMoney()
        {
            card.ResetCard();
            windowPayment = new WindowPayment(this);
            windowPayment.ShowDialog();
        }

        // Méthode permettant de vérifier la cohérence des données rentrées grâce aux méthodes de la classe Card
        // Si c'est validé, on met à jour le montant du client
        public void Payment()
        {
            int errorInfo =0 ;
            bool testCardNumber = Card.CheckCardNumber(card.CardNumber);
            bool testCardDate = Card.CheckCardDate(card.CardDate);

            if (testCardNumber == false)
                errorInfo = 1;

            if (testCardDate == false)
                errorInfo = 2;

            if (Int32.Parse(MoneyToAdd) > 1000)
                errorInfo = 3;

            switch (errorInfo)
            {
                case 0:
                    // Appel de la fonction SendMessage et construction de la trame avec la méthode UpdatebalanceBuild de la classe clientFrameManager
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

        // Methode permettant de vérifier que les valeurs rentrées dans les champs d'ajout d'agent sont corrects
        // Cette méthode vérifie la structure des champs (numérique, taille ...) et non la cohérence des données
        // La méthode va permettre de griser ou non le bouton d'ajout d'argent
        public bool CanPaid()
        {
            string month;
            string year;
            string date;

            if (UserCanAddMoney == false)
                return false;

            if (card.CardNumber == null || card.CardCrypto == null || card.CardDate == null || MoneyToAdd == null)
                return false;

            date = card.CardDate;

            if ( (IsNumeric(card.CardNumber) == false) || card.CardNumber.Length != 16 )
                return false;

            if (date.Length != 7)
                return false;
            else
            {
                month = date.Substring(0, 2);
                year = date.Substring(3, 4);

                if (IsNumeric(month)==false)
                    return false;

                if (date.Substring(2, 1) != "/")
                    return false;

                if ( IsNumeric(year) == false )
                    return false;
            }

            if ( (IsNumeric(card.CardCrypto) == false) || card.CardCrypto.Length != 3 )
                return false;

            if ((IsNumeric(MoneyToAdd) == false) || Int32.Parse(MoneyToAdd) < 1)
                return false;

            return true;
        }

        // Methode permettant de savoir si une chaine de caractère est numérique
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
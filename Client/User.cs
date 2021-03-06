﻿using System.ComponentModel;

namespace Client
{
    // Classe permettant de représenter un utilisateur et son montant
    public class User : INotifyPropertyChanged
    {
        // Attributs avec getter/setter
        private string username;
        private int balance;

        public int Balance
        {
            get { return balance; }
            set {
                balance = value;
                RaiseProperty(nameof(Balance));
            }
        }

        public string Username
        {
            get { return username; }
            set {
                username = value;
                RaiseProperty(nameof(Username));
            }
        }

  
        public User(string _userName)
        {
            this.username = _userName;
            this.balance = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperty(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
using System.ComponentModel;

namespace Server
{
    class ListViewItem : INotifyPropertyChanged
    {
        private string m_Username;
        private string m_Balance;
        public event PropertyChangedEventHandler PropertyChanged;

        public ListViewItem(string Username, string Balance)
        {
            this.Username = Username;
            this.Balance = Balance;
        }

        public string Username
        {
            get { return m_Username; }
            set
            {
                if (m_Username != value)
                {
                    m_Username = value;
                    OnPropertyChanged("Username");
                }
            }
        }

        public string Balance
        {
            get { return m_Balance; }
            set
            {
                if (m_Balance != value)
                {
                    m_Balance = value;
                    OnPropertyChanged("Balance");
                }
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

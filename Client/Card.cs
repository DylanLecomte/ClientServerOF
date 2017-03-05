using System.ComponentModel;
using System.Linq;

namespace Client
{
    public class Card : INotifyPropertyChanged
    {
        private string _CardNumber;

        public string  CardNumber
        {
            get { return _CardNumber; }
            set
            {
                _CardNumber = value;
                RaiseProperty(nameof(CardNumber));
            }
        }

        private string _CardDate;

        public string CardDate
        {
            get { return _CardDate; }
            set
            {
                _CardDate = value;
                RaiseProperty(nameof(CardDate));
            }
        }

        private string _CardCrypto;

        public string CardCrypto
        {
            get { return _CardCrypto; }
            set
            {
                _CardCrypto = value;
                RaiseProperty(nameof(CardCrypto));
            }
        }

        public void ResetCard()
        {
            CardNumber = "";
            CardDate = "";
            CardCrypto = "";
        }

        static public bool CheckCardNumber(object value)//string _CardNumber)
        {

            if (value == null)
            {
                return true;
            }

            string ccValue = value as string;
            if (ccValue == null)
            {
                return false;
            }
            ccValue = ccValue.Replace("-", "");
            ccValue = ccValue.Replace(" ", "");

            int checksum = 0;
            bool evenDigit = false;

            foreach (char digit in ccValue.Reverse())
            {
                if (digit < '0' || digit > '9')
                {
                    return false;
                }

                int digitValue = (digit - '0') * (evenDigit ? 2 : 1);
                evenDigit = !evenDigit;

                while (digitValue > 0)
                {
                    checksum += digitValue % 10;
                    digitValue /= 10;
                }
            }

            return (checksum % 10) == 0;
        }

        static public bool CheckCardDate(string _CardDate)
        {
            return true;
        }

        static public bool CheckCardCrypto(string _CardCrypto)
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperty(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
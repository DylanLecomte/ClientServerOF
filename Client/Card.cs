using System;
using System.ComponentModel;
using System.Linq;

namespace Client
{
    // Classe permettant de représenter une carte de crédit
    public class Card : INotifyPropertyChanged
    {
        // Attributs avec getters/setters
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
        
        // Méthodes

        // Fonction permettant de reset les paramètres de la carte
        public void ResetCard()
        {
            CardNumber = "";
            CardDate = "";
            CardCrypto = "";
        }

        // Fonction permettant de vérifier le numéro de carte de crédit (Checksum)
        // Cette fonction vérifie la cohérence des données rentrées et non la structure
        // La validation de la structure sera faite la classe handleConnection dans la fonction CanPaid
        static public bool CheckCardNumber(object value)
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

        // Fonction permettant de vérifier la date de carte de crédit (Checksum)
        // Cette fonction vérifie la cohérence des données rentrées et non la structure
        // La validation de la structure sera faite la classe handleConnection dans la fonction CanPaid
        static public bool CheckCardDate(string _CardDate)
        {
            int year = Int32.Parse(_CardDate.Substring(3, 4));
            int month = Int32.Parse(_CardDate.Substring(0, 2));
            int currentMonth = Int32.Parse(DateTime.Now.Month.ToString());
            int currentYear = Int32.Parse(DateTime.Now.Year.ToString());

            if (year < currentYear || year > currentYear + 3)
                return false;
            else if (year == currentYear)
            {
                if (month < currentMonth)
                    return false;
            }
            else if (year == currentYear + 2)
            {
                if (month > currentMonth)
                    return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperty(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
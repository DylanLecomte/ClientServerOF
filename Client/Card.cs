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
                return true;

            string ccValue = value as string;
            if (ccValue == null)
            {
                return false;
            }

            int checksum = 0;
            bool evenDigit = false;

            foreach (char digit in ccValue.Reverse())
            {
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
            int month = Card.getMonth(_CardDate.Substring(0, 2));
            int currentMonth = Int32.Parse(DateTime.Now.Month.ToString());
            int currentYear = Int32.Parse(DateTime.Now.Year.ToString());

            if (month < 1 || month > 12)
                return false;

            if (year < currentYear || year > currentYear + 3)
                return false;

            if (year == currentYear && month < currentMonth)
                return false;

            if (year == currentYear + 2 && month > currentMonth)
                return false;

            return true;
        }

        // Méthode permettant de récupérer le mois d'une chaine de caractère de type MM/AAAA
        public static int getMonth(string _month)
        {
            if (_month.Substring(0, 1) == "0")
                return Int32.Parse(_month.Substring(1, 1));
            else
                return Int32.Parse(_month);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperty(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
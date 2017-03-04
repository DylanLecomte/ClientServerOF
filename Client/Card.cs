using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Card
    {
        private string _CardNumber;

        public string  CardNumber
        {
            get { return _CardNumber; }
            set { _CardNumber = value; }
        }

        private string _CardDate;

        public string CardDate
        {
            get { return _CardDate; }
            set { _CardDate = value; }
        }

        private string _CardCrypto;

        public string CardCrypto
        {
            get { return _CardCrypto; }
            set { _CardCrypto = value; }
        }

        static public bool CheckCardNumber(string _CardNumber)
        {
            return true;
        }

        static public bool CheckCardDate(string _CardDate)
        {
            return true;
        }

        static public bool CheckCardCrypto(string _CardCrypto)
        {
            return true;
        }
    }
}

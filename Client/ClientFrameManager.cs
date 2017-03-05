using System;

namespace Client
{
    // Classe permettant de gérer la structure des trames côté client
    class ClientFrameManager
    {
        // Méthodes de construction de trames à envoyer au serveur

        // Construction d'une trame pour se connecter
        public string ConnectionBuild(string login, string password)
        {
            return "LOGIN;" + login + ";" + password;
        }

        // Construction d'une trame pour créer un compte et se connecter
        public string CreationBuild(string login, string password)
        {
            return "CREATE;" + login + ";" + password;
        }

        // Construction d'une trame pour récupérer son montant
        public string GetBalanceBuild()
        {
            return "GBAL;";
        }

        // Construction d'une trame pour mettre à jour le montant
        public string UpdatebalanceBuild(int amount)
        {
            return "UBAL;" + amount.ToString();
        }

        // Méthodes de deconstruction de trames envoyé par le serveur

        // Récupération du header d'une trame
        public string GetFrameHeader(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            parameters = frame.Split(stringSeparators, StringSplitOptions.None);

            return parameters[0];
        }

        // Déconstruction d'une acquittement de connexion
        public string ACKConnectionRead(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            parameters = frame.Split(stringSeparators, StringSplitOptions.None);
            return parameters[1];
        }

        // Déconstruction d'une trame contenant le montant du client
        public int SendBalanceRead(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            parameters = frame.Split(stringSeparators, StringSplitOptions.None);
            return Int32.Parse(parameters[1]);
        }

        // Déconstruction d'une acquittement de réception du montant
        public bool ACKUpdateBalanceBuild(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            parameters = frame.Split(stringSeparators, StringSplitOptions.None);
            if (parameters[1].Equals("True"))
                return true;
            else
                return false;
        }
    }
}
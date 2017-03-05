using System;
using System.Diagnostics;

namespace Server
{
    // Classe de gestion des trames su serveur
    public class ServerFrameManager
    {
        public string ACKConnectionBuild(Database.Error connectionErrorReturn)
        {
            // Construction d'une trame d'acquitement de connection en fonction de l'erreur
            switch (connectionErrorReturn)
            {
                case Database.Error.None:
                    return "ACKLOGIN;Ok";
                case Database.Error.NonExistant:
                    return "ACKLOGIN;Unknown";
                case Database.Error.WrongPassword:
                    return "ACKLOGIN;PasswordFalse";
                case Database.Error.Duplication:
                    return "ACKLOGIN;Duplication";
                default:
                    return "ACKLOGIN;Ko";
            }
        }

        public string SendBalanceBuild(int amount)
        {
            // Construction de la trame contenant le solde du client
            return "SBAL;" + amount.ToString();
        }

        public string ACKUpdateBalanceBuild(bool balanceupdated)
        {
            // Construction de la trame d'acquitement de mise à jour du solde du client
            if (balanceupdated)
                return "ACKUBAL;True";
            else
                return "ACKUBAL;False";
        }

        public string GetFrameHeader(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            // Récupération des éléments de la trame
            parameters = frame.Split(stringSeparators, StringSplitOptions.None);

            // Retour de l'entête
            return parameters[0];
        }

        public void ConnectionRead(string frame, ref string login, ref string password)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            // Récupération du login et du password dans la trame
            parameters = frame.Split(stringSeparators, StringSplitOptions.None);
            login = parameters[1];
            password = parameters[2];
        }

        public void CreateRead(string frame, ref string login, ref string password)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            // Récupération du login et du password dans la trame
            parameters = frame.Split(stringSeparators, StringSplitOptions.None);
            login = parameters[1];
            password = parameters[2];
        }

        public int UpdatebalanceRead(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            // Récupération du montant à addition ou soustraire au solde du client
            parameters = frame.Split(stringSeparators, StringSplitOptions.None);

            if(parameters[1] == null || parameters[1] == "")
            {
                return -1;
            }
            else
            {
                return Int32.Parse(parameters[1]);
            }
        }
    }
}
using System;
using System.Diagnostics;

namespace Server
{
    public class ServerFrameManager
    {
        public string ACKConnectionBuild(Database.Error connectionErrorReturn)
        {
            switch (connectionErrorReturn)
            {
                case Database.Error.None:
                    return "ACKLOGIN;Ok";
                case Database.Error.NonExistant:
                    return "ACKLOGIN;Unknown";
                case Database.Error.WrongPassword:
                    return "ACKLOGIN;PasswordFalse";
                default:
                    return "ACKLOGIN;Ko";
            }
        }

        public string SendBalanceBuild(int amount)
        {
            return "SBAL;" + amount.ToString();
        }

        public string ACKUpdateBalanceBuild(bool balanceupdated)
        {
            if (balanceupdated)
                return "ACKUBAL;True";
            else
                return "ACKUBAL;False";
        }

        public string GetFrameHeader(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

            parameters = frame.Split(stringSeparators, StringSplitOptions.None);

            return parameters[0];
        }

        public void ConnectionRead(string frame, ref string login, ref string password)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

                parameters = frame.Split(stringSeparators, StringSplitOptions.None);
                login = parameters[1];
                password = parameters[2];
        }

        public int UpdatebalanceRead(string frame)
        {
            string[] parameters;
            string[] stringSeparators = new string[] { ";" };

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

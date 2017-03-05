using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Windows;

namespace Server
{
    // Classe de gestion de la base de données
    public class Database
    {
        private readonly SQLiteConnection con;

        // Enumération des erreurs possibless
        public enum Error
        {
            None,
            Duplication,
            NonExistant,
            WrongPassword,
            CommandFail,
            DBNotOpen
        }

        public Database(string DatabaseName)
        {
            // Build connection string with the name of the database
            string connectionString = @" Data Source = " + Environment.CurrentDirectory + @"\" + DatabaseName + "; Version = 3";
            con = new SQLiteConnection(connectionString);
        }

        ~Database()
        {
            con.Dispose();
        }

        public Error connect()
        {
            // Initialisation du code d'erreur a None
            Error errorCode = Error.None ;
            try
            {
                // Tentaive de connection à la base de doonées
                con.Open();
                if (con.State != System.Data.ConnectionState.Open)
                {
                    // Erreur "Database non ouverte"
                    errorCode = Error.DBNotOpen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return errorCode;
        }

        public Error insertUser(string username, string password)
        {
            Error errorCode = Error.None;
            // Vérification de le connection à la base de données 
            if (con.State != System.Data.ConnectionState.Open)
            {
                return Error.DBNotOpen;
            }

            //Create the salt value with a cryptographic PRNG
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            // Create the Rfc2898DeriveBytes and get the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            //Combine the salt and password bytes for later use:
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            //Turn the combined salt+hash into a string for storage
            string PasswordHash = Convert.ToBase64String(hashBytes);

            SQLiteCommand cmd = new SQLiteCommand();            
            try
            {
                // Préparation de la commande SQL
                cmd.CommandText = @"INSERT INTO User (username, password, balance) VALUES (@username, @password, 10)";
                cmd.Connection = con;
                // Configuration des paramètres de la requête
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));
                cmd.Parameters.Add(new SQLiteParameter(@"password", PasswordHash));
                 
                // Exécution de la requête
                int i = cmd.ExecuteNonQuery();

                // Retourne une erreur si la commande à échouée
                if (i != 1) { errorCode = Error.CommandFail; }

            }
            catch (Exception ex)
            {
                // Retourne une erreur sur une exception
                errorCode = Error.Duplication;
            }
            finally
            {
                cmd.Dispose();
            }

            return errorCode;
        }

        public Error deletetUser(string username)
        {
            Error errorCode = Error.None;
            // Vérification de le connection à la base de données 
            if (con.State != System.Data.ConnectionState.Open)
            {
                return Error.DBNotOpen;
            }

            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                // Préparation de la commande SQL
                cmd.CommandText = @"DELETE FROM User WHERE username=@username";
                cmd.Connection = con;
                // Configuration des paramètres de la requête
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));

                // Exécution de la requête
                int i = cmd.ExecuteNonQuery();

                // Retourne une erreur si la commande à échouée
                if (i != 1) { errorCode = Error.CommandFail; }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }

            return errorCode;
        }

        public Error updateBalance(string username, int differential)
        {
            Error errorCode = Error.None;
            if (con.State != System.Data.ConnectionState.Open)
            {
                return Error.DBNotOpen;
            }

            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                // Préparation de la commande SQL
                cmd.CommandText = @"UPDATE User SET balance = balance + @differential WHERE username = @username";
                cmd.Connection = con;
                // Configuration des paramètres de la requête
                cmd.Parameters.Add(new SQLiteParameter(@"differential", differential));
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));

                // Exécution de la requête
                int i = cmd.ExecuteNonQuery();

                // Retourne une erreur si la commande à échouée
                if (i != 1) { errorCode = Error.CommandFail; }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }

            return errorCode;
        }

        public Error getBalance(string username, ref int balance)
        {
            Error errorCode = Error.None;
            if (con.State != System.Data.ConnectionState.Open)
            {
                return Error.DBNotOpen;
            }

            SQLiteCommand cmd = new SQLiteCommand();

            try
            {
                // Préparation de la commande SQL
                cmd.CommandText = @"SELECT balance FROM User WHERE username = @username";
                cmd.Connection = con;
                // Configuration des paramètres de la requête
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));

                // Exécution de la requête
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    // Retourne une erreur si il y a deux résultats
                    if (reader.StepCount > 1) { errorCode = Error.Duplication; }
                    // Retourne une erreur si il n'y a pas de résultat
                    if (reader.StepCount == 0) { errorCode = Error.NonExistant; }

                    if (errorCode == Error.None && reader.Read())
                    {
                        // Récupération du montant
                        balance = reader.GetInt32(0);
                    }
                        
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }

            return errorCode;
        }


        public Error checkLoginPwd(string username, string password)
        {

            Error errorCode = Error.None;
            if (con.State != System.Data.ConnectionState.Open)
            {
                return Error.DBNotOpen;
            }

            SQLiteCommand cmd = new SQLiteCommand();

            try
            {
                // Préparation de la commande SQL
                cmd.CommandText = @"SELECT password FROM User WHERE username = @username";
                cmd.Connection = con;
                // Configuration des paramètres de la requête
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));

                // Exécution de la requête  
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                // Retourne une erreur si il y a deux résultats
                if (reader.StepCount > 1) { errorCode = Error.Duplication; }
                // Retourne une erreur si il n'y a pas de résultat
                if (reader.StepCount == 0) { errorCode = Error.NonExistant; }

                string dbPassword = "";

                if(errorCode == Error.None)
                {
                    if (reader.Read())
                    {
                        dbPassword = reader.GetString(0);
                    }

                    /* Extract the bytes */
                    byte[] hashBytes = Convert.FromBase64String(dbPassword);
                    /* Get the salt */
                    byte[] salt = new byte[16];
                    Array.Copy(hashBytes, 0, salt, 0, 16);
                    /* Compute the hash on the password the user entered */
                    var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                    byte[] hash = pbkdf2.GetBytes(20);
                    /* Compare the results */
                    for (int i = 0; i < 20; i++)
                        if (hashBytes[i + 16] != hash[i])
                                errorCode = Error.WrongPassword;

                }
                reader.Close();
                }

            }
            catch (Exception ex)
            {                    
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }

            return errorCode;
        }

        public void disconnect()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }
    }
}
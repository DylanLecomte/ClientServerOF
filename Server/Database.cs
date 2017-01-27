using System;
using System.Data.SQLite;
using System.Windows;

namespace Server
{
    class Database
    {

        private readonly string connectionString;
        private readonly SQLiteConnection con;

        public enum Error
        {
            None,
            Duplication,
            NonExistant,
            WrongPassword,
            CommandFail,
            DBNotOpen
        }

        public Database()
        {
            connectionString = @" Data Source = " + Environment.CurrentDirectory + @"\database.db; Version = 3";
            con = new SQLiteConnection(connectionString);
        }

        ~Database()
        {
            con.Dispose();
        }

        public Error connect()
        {
            Error errorCode = Error.None ;
            try
            {
                con.Open();
                if (con.State != System.Data.ConnectionState.Open)
                {
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
            if (con.State != System.Data.ConnectionState.Open)
            {
                return Error.DBNotOpen;
            }

            SQLiteCommand cmd = new SQLiteCommand();            
            try
            {
                cmd.CommandText = @"INSERT INTO User (username, password, balance) VALUES (@username, @password, 10)";
                cmd.Connection = con;
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));
                cmd.Parameters.Add(new SQLiteParameter(@"password", password));

                int i = cmd.ExecuteNonQuery();

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
                cmd.CommandText = @"UPDATE User SET balance = balance + @differential WHERE username = @username";
                cmd.Connection = con;
                cmd.Parameters.Add(new SQLiteParameter(@"differential", differential));
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));

                int i = cmd.ExecuteNonQuery();

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
                cmd.CommandText = @"SELECT balance FROM User WHERE username = @username";
                cmd.Connection = con;
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));
                   
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.StepCount > 1) { errorCode = Error.Duplication; }
                    if (reader.StepCount == 0) { errorCode = Error.NonExistant; }

                    if (errorCode == Error.None)
                    {
                        if(reader.Read())
                        {
                            balance = reader.GetInt32(0);
                        }
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
                cmd.CommandText = @"SELECT password FROM User WHERE username = @username";
                cmd.Connection = con;
                cmd.Parameters.Add(new SQLiteParameter(@"username", username));
                   
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {

                if (reader.StepCount > 1) { errorCode = Error.Duplication; }  
                if (reader.StepCount == 0) { errorCode = Error.NonExistant; }

                string dbPassword = "";

                if(errorCode == Error.None)
                {
                    if (reader.Read())
                    {
                        dbPassword = reader.GetString(0);
                    }
                    
                    if (!dbPassword.Equals(password))
                    {
                        errorCode = Error.WrongPassword;
                    }
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

    }
}
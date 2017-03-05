using Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SQLite;

namespace Server.Tests
{
    [TestClass()]
    public class DatabaseTests
    {

        [TestMethod()]
        public void insertUserTest()
        {
            // arrange
            Database.Error ResultInser;
            Database.Error ResultRead;
            string Username = "UserTest";
            string Password = "PwdTest";
            Database.Error Expected = Database.Error.None;
            Database db = new Database("databaseTest.db");
            db.connect();
            //act
            ResultInser = db.insertUser(Username, Password);
            ResultRead = db.checkLoginPwd(Username, Password);
            //assert           

            Assert.AreEqual(Expected, ResultInser);
            Assert.AreEqual(Expected, ResultRead);
            db.deletetUser(Username);
            db.disconnect();
        }

        [TestMethod()]
        public void checkLoginPwdTest()
        {
            // arrange
            Database.Error Result;
            string username = "user";
            string password = "Password123";
            Database.Error Expected = Database.Error.None;
            Database db = new Database("databaseTest.db");
            db.connect();
            //act
            Result = db.checkLoginPwd(username, password);
            //assert
            Assert.AreEqual(Expected, Result);
            db.disconnect();
        }

        [TestMethod()]
        public void getBalanceTest()
        {
            // arrange
            Database.Error ResultCommand;
            int ResultBalance = -1;
            string username = "user3";
            Database.Error ExpectedCommand = Database.Error.None;
            int ExpectedBalance = 10;
            Database db = new Database("databaseTest.db");
            db.connect();
            //act
            ResultCommand = db.getBalance(username, ref ResultBalance);
            //assert
            Assert.AreEqual(ExpectedCommand, ResultCommand);
            Assert.AreEqual(ExpectedBalance, ResultBalance);
            db.disconnect();
        }

    }
}
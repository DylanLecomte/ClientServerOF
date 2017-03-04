using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tests
{
    [TestClass()]
    public class DESEncryptTests
    {
        [TestMethod()]
        public void EncryptStringTest()
        {
            // arrange
            string Result;
            const string Password = "Saucisse";
            string Param = "Bonjour";
            string Expected = "uadfxoDmWciaQS80QhIJbg==";
            DESEncrypt Encrypt = new DESEncrypt();
            //act
            Result = Encrypt.EncryptString(Param, Password);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void DecryptStringTest()
        {
            // arrange
            string Result;
            const string Password = "Saucisse";
            string Param = "uadfxoDmWciaQS80QhIJbg==";
            string Expected = "Bonjour";
            DESEncrypt Encrypt = new DESEncrypt();
            //act
            Result = Encrypt.DecryptString(Param, Password);
            //assert
            Assert.AreEqual(Expected, Result);
        }
    }
}

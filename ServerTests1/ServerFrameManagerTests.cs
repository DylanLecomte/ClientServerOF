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
    public class ServerFrameManagerTests
    {
        [TestMethod()]
        public void ACKConnectionBuildTest_ErrorNone_ReturnOK()
        {
            //arrange
            string Result;
            string Expected = "ACKLOGIN;Ok";
            Database.Error Param = Database.Error.None;
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.ACKConnectionBuild(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void ACKConnectionBuildTest_ErrorNonExistant_ReturnUnknown()
        {
            //arrange
            string Result;
            string Expected = "ACKLOGIN;Unknown";
            Database.Error Param = Database.Error.NonExistant;
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.ACKConnectionBuild(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void ACKConnectionBuildTest_ErrorWrongPassword_ReturnPasswordFalse()
        {
            //arrange
            string Result;
            string Expected = "ACKLOGIN;PasswordFalse";
            Database.Error Param = Database.Error.WrongPassword;
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.ACKConnectionBuild(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void ACKConnectionBuildTest_ErrorDuplication_ReturnKO()
        {
            //arrange
            string Result;
            string Expected = "ACKLOGIN;Ko";
            Database.Error Param = Database.Error.Duplication;
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.ACKConnectionBuild(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void SendBalanceBuildTest_Amout50_ReturnFrameWith50()
        {
            //arrange
            string Result;
            int Param = 50;
            string Expected = "SBAL;" + Param.ToString();
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.SendBalanceBuild(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void ACKUpdateBalanceBuildTest_TrueParam_FrameWithTrue()
        {
            //arrange
            string Result;
            bool Param = true;
            string Expected = "ACKUBAL;True";
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.ACKUpdateBalanceBuild(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void ACKUpdateBalanceBuildTest_FalseParam_FrameWithFalse()
        {
            //arrange
            string Result;
            bool Param = false;
            string Expected = "ACKUBAL;False";
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.ACKUpdateBalanceBuild(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }
    }
}
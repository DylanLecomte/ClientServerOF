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
            string Expected = "ACKLOGIN;Duplication";
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

        [TestMethod()]
        public void GetFrameHeaderTest_FrameWithACKUBALHeader_ACKUBALHeader()
        {
            //arrange
            string Result;
            string Param = "ACKUBAL;False";
            string Expected = "ACKUBAL";
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.GetFrameHeader(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void GetFrameHeaderTest_FrameWithNoHeader_NoHeader()
        {
            //arrange
            string Result;
            string Param = ";False";
            string Expected = "";
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.GetFrameHeader(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void ConnectionReadTest_CorrectFrame_GoodLoginAndPassword()
        {
            //arrange
            string ResultLogin = "";
            string ResultPassword = "";
            string Param = "LOGIN;Jean;Cloud";
            string ExpectedLogin = "Jean";
            string ExpectedPassword = "Cloud";
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            FrameManager.ConnectionRead(Param, ref ResultLogin, ref ResultPassword);
            //assert
            Assert.AreEqual(ExpectedLogin, ResultLogin);
            Assert.AreEqual(ExpectedPassword, ResultPassword);
        }

        [TestMethod()]
        public void ConnectionReadTest_CorrectFrameNoPassword_NoPassword()
        {
            //arrange
            string ResultLogin = "";
            string ResultPassword = "";
            string Param = "LOGIN;Jean;";
            string ExpectedLogin = "Jean";
            string ExpectedPassword = "";
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            FrameManager.ConnectionRead(Param, ref ResultLogin, ref ResultPassword);
            //assert
            Assert.AreEqual(ExpectedLogin, ResultLogin);
            Assert.AreEqual(ExpectedPassword, ResultPassword);
        }

        [TestMethod()]
        public void UpdatebalanceReadTest_CorrectFrame_GoodAmout()
        {
            //arrange
            int Result;
            string Param = "UBAL;120";
            int Expected = 120;
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.UpdatebalanceRead(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }

        [TestMethod()]
        public void UpdatebalanceReadTest_NoAmout_GoodAmout()
        {
            //arrange
            int Result;
            string Param = "UBAL;";
            int Expected = -1;
            ServerFrameManager FrameManager = new ServerFrameManager();
            //act
            Result = FrameManager.UpdatebalanceRead(Param);
            //assert
            Assert.AreEqual(Expected, Result);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace IHMTests
{

    [CodedUITest]
    public class CodedUITest1
    {
        public CodedUITest1()
        {
        }

        [TestMethod]
        public void CodedUITestMethod1()
        {
            // Pour générer le code de ce test, sélectionnez "Générer le code pour le test codé de l'interface utilisateur" dans le menu contextuel et sélectionnez un des éléments de menu.
        }

        #region Attributs de tests supplémentaires

        // Vous pouvez utiliser les attributs supplémentaires suivants lorsque vous écrivez vos tests :

        ////Utilisez TestInitialize pour exécuter du code avant d'exécuter chaque test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // Pour générer le code de ce test, sélectionnez "Générer le code pour le test codé de l'interface utilisateur" dans le menu contextuel et sélectionnez un des éléments de menu.
        //}

        ////Utilisez TestCleanup pour exécuter du code après que chaque test a été exécuté
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // Pour générer le code de ce test, sélectionnez "Générer le code pour le test codé de l'interface utilisateur" dans le menu contextuel et sélectionnez un des éléments de menu.
        //}

        #endregion


        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;
    }
}

﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      Ce code a été généré par un générateur de test codé de l'interface utilisateur.
//      Version : 14.0.0.0 
//
//      Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//      le code est régénéré.
//  </auto-generated>
// ------------------------------------------------------------------------------

namespace IHMTests
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    [GeneratedCode("Générateur de test codé de l\'interface utilisateur", "14.0.23107.0")]
    public partial class UIMap
    {
        
        /// <summary>
        /// Connexion avec Login/MDP ok
        /// </summary>
        public void Connexion()
        {
            #region Variable Declarations
            WpfWindow uIWPFWindow = this.UIWPFWindow;
            WpfEdit uITextBoxLoginEdit = this.UIConnectionWindow.UITextBoxLoginEdit;
            WpfEdit uIPasswordBoxEdit = this.UIConnectionWindow.UIPasswordBoxEdit;
            #endregion

            // Clic 'WPF' fenêtre
            Mouse.Click(uIWPFWindow, new Point(179, 50));

            // Taper 'user' dans 'TextBoxLogin' zone de texte
            uITextBoxLoginEdit.Text = this.ConnexionParams.UITextBoxLoginEditText;

            // Taper '{Tab}' dans 'TextBoxLogin' zone de texte
            Keyboard.SendKeys(uITextBoxLoginEdit, this.ConnexionParams.UITextBoxLoginEditSendKeys, ModifierKeys.None);

            // Taper '********' dans 'PasswordBox' zone de texte
            Keyboard.SendKeys(uIPasswordBoxEdit, this.ConnexionParams.UIPasswordBoxEditSendKeys, true);

            // Clic 'WPF' fenêtre
            Mouse.Click(uIWPFWindow, new Point(156, 218));
        }
        
        #region Properties
        public virtual ConnexionParams ConnexionParams
        {
            get
            {
                if ((this.mConnexionParams == null))
                {
                    this.mConnexionParams = new ConnexionParams();
                }
                return this.mConnexionParams;
            }
        }
        
        public UIWPFWindow UIWPFWindow
        {
            get
            {
                if ((this.mUIWPFWindow == null))
                {
                    this.mUIWPFWindow = new UIWPFWindow();
                }
                return this.mUIWPFWindow;
            }
        }
        
        public UIConnectionWindow UIConnectionWindow
        {
            get
            {
                if ((this.mUIConnectionWindow == null))
                {
                    this.mUIConnectionWindow = new UIConnectionWindow();
                }
                return this.mUIConnectionWindow;
            }
        }
        #endregion
        
        #region Fields
        private ConnexionParams mConnexionParams;
        
        private UIWPFWindow mUIWPFWindow;
        
        private UIConnectionWindow mUIConnectionWindow;
        #endregion
    }
    
    /// <summary>
    /// Paramètres à passer dans 'Connexion'
    /// </summary>
    [GeneratedCode("Générateur de test codé de l\'interface utilisateur", "14.0.23107.0")]
    public class ConnexionParams
    {
        
        #region Fields
        /// <summary>
        /// Taper 'user' dans 'TextBoxLogin' zone de texte
        /// </summary>
        public string UITextBoxLoginEditText = "user";
        
        /// <summary>
        /// Taper '{Tab}' dans 'TextBoxLogin' zone de texte
        /// </summary>
        public string UITextBoxLoginEditSendKeys = "{Tab}";
        
        /// <summary>
        /// Taper '********' dans 'PasswordBox' zone de texte
        /// </summary>
        public string UIPasswordBoxEditSendKeys = "YN/mQM5J9PQyaTrMeVvgMxYZmM5e+OBQAMU264zVoqaYemox3yxxTqb3aqE41K3mxfxXEQVR2awHyGpOc" +
            "oba5WV8LGBQSzKs";
        #endregion
    }
    
    [GeneratedCode("Générateur de test codé de l\'interface utilisateur", "14.0.23107.0")]
    public class UIWPFWindow : WpfWindow
    {
        
        public UIWPFWindow()
        {
            #region Critères de recherche
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            #endregion
        }
    }
    
    [GeneratedCode("Générateur de test codé de l\'interface utilisateur", "14.0.23107.0")]
    public class UIConnectionWindow : WpfWindow
    {
        
        public UIConnectionWindow()
        {
            #region Critères de recherche
            this.SearchProperties[WpfWindow.PropertyNames.Name] = "Connection";
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("Connection");
            #endregion
        }
        
        #region Properties
        public WpfEdit UITextBoxLoginEdit
        {
            get
            {
                if ((this.mUITextBoxLoginEdit == null))
                {
                    this.mUITextBoxLoginEdit = new WpfEdit(this);
                    #region Critères de recherche
                    this.mUITextBoxLoginEdit.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "TextBoxLogin";
                    this.mUITextBoxLoginEdit.WindowTitles.Add("Connection");
                    #endregion
                }
                return this.mUITextBoxLoginEdit;
            }
        }
        
        public WpfEdit UIPasswordBoxEdit
        {
            get
            {
                if ((this.mUIPasswordBoxEdit == null))
                {
                    this.mUIPasswordBoxEdit = new WpfEdit(this);
                    #region Critères de recherche
                    this.mUIPasswordBoxEdit.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "PasswordBox";
                    this.mUIPasswordBoxEdit.WindowTitles.Add("Connection");
                    #endregion
                }
                return this.mUIPasswordBoxEdit;
            }
        }
        #endregion
        
        #region Fields
        private WpfEdit mUITextBoxLoginEdit;
        
        private WpfEdit mUIPasswordBoxEdit;
        #endregion
    }
}
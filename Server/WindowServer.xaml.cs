﻿using System.ComponentModel;
using System.Windows;

namespace Server
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class WindowServer : Window
    {
        HandleServer myServer;
        public WindowServer()
        {
            myServer = new HandleServer();
            DataContext = myServer;

            InitializeComponent();
        }

        public void WindowServer_Closing(object sender, CancelEventArgs e)
        {
            myServer.Clear();
        }
    }
}
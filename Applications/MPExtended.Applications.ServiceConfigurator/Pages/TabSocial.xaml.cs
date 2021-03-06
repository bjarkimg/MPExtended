﻿#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using MPExtended.Libraries.Social;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabSocial.xaml
    /// </summary>
    public partial class TabSocial : Page
    {
        private class TestCredentialsData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private string activeProvider;
        private BackgroundWorker worker;

        public TabSocial()
        {
            InitializeComponent();
            lblTestResult.Content = "";

            if (Configuration.Streaming.WatchSharing["type"] == "none" || Configuration.Streaming.WatchSharing["type"] == "debug") // debug isn't supported in UI
            {
                rbNone.IsChecked = true;
            }
            else
            {
                if (Configuration.Streaming.WatchSharing["type"] == "follwit")
                    rbFollwit.IsChecked = true;

                if (Configuration.Streaming.WatchSharing["type"] == "trakt")
                    rbTrakt.IsChecked = true;

                txtUsername.Text = Configuration.Streaming.WatchSharing["username"];
            }
        }

        private void radioButtonChanged(object sender, RoutedEventArgs e)
        {
            activeProvider = (string)((e.Source as RadioButton).Tag);
            InvalidateTestResults();
            switch (activeProvider)
            {
                case "none":
                    btnTest.IsEnabled = false;
                    btnSave.IsEnabled = false;
                    txtUsername.IsEnabled = false;
                    txtPassword.IsEnabled = false;
                    break;
                case "follwit":
                case "trakt":
                    btnTest.IsEnabled = true;
                    txtUsername.IsEnabled = true;
                    txtPassword.IsEnabled = true;
                    break;
            }
        }

        private void textChanged(object sender, RoutedEventArgs e)
        {
            InvalidateTestResults();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            lblTestResult.Foreground = Brushes.Black;
            lblTestResult.Content = "Testing credentials, please wait...";
            btnTest.IsEnabled = false;
            worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                TestCredentialsData data = args.Argument as TestCredentialsData;
                IWatchSharingService service = GetImplementation();
                args.Result = service.TestCredentials(data.Username, data.Password);
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if ((bool)args.Result)
                {
                    lblTestResult.Content = "Login successful!";
                    lblTestResult.Foreground = Brushes.Green;
                    btnSave.IsEnabled = true;
                }
                else
                {
                    lblTestResult.Content = "Login failed.";
                    lblTestResult.Foreground = Brushes.Red;
                }
                btnTest.IsEnabled = true;
            };
            worker.RunWorkerAsync(new TestCredentialsData()
            {
                Username = txtUsername.Text,
                Password = txtPassword.Password
            });
        }

        private IWatchSharingService GetImplementation()
        {
            if (activeProvider == "follwit")
            {
                return new FollwitSharingProvider();
            }
            else if (activeProvider == "trakt")
            {
                return new TraktSharingProvider();
            }
            return null;
        }

        private void InvalidateTestResults()
        {
            btnSave.IsEnabled = false;
            lblTestResult.Content = "";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Streaming.WatchSharing["username"] = txtUsername.Text;
            Configuration.Streaming.WatchSharing["passwordHash"] = GetImplementation().HashPassword(txtPassword.Password);
            Configuration.Streaming.WatchSharing["type"] = activeProvider;
            Configuration.Streaming.Save();
            lblTestResult.Content = "Saved successfully, restart the service for the changes to take affect.";
        }
    }
}

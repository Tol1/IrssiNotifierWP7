using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace IrssiNotifier
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
            browser.Navigate(new Uri("http://irssinotifierwp.appspot.com/client/login"));
        }

        private void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.EndsWith("client/login/loginsuccess"))
            {
                var cookies = (sender as WebBrowser).GetCookies();
                NavigationService.Navigate(new Uri("/Registration.xaml", UriKind.Relative));
            }
        }
    }
}
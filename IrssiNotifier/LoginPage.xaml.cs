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
using Microsoft.Phone.Shell;
using IrssiNotifier.Views;

namespace IrssiNotifier
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
			contentBorder.Child = new LoginView(this);
            //browser.Navigate(new Uri(App.BASEADDRESS+"client/login"));
        }

		public void Register()
		{
		}

        /*private void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.EndsWith("client/login/loginsuccess"))
            {
                var cookies = (sender as WebBrowser).GetCookies();
                var uri = (sender as WebBrowser).Source;
                PhoneApplicationService.Current.State["cookies"] = cookies;
                PhoneApplicationService.Current.State["cookiesUri"] = uri;
                NavigationService.Navigate(new Uri("/Registration.xaml", UriKind.Relative));
            }
        }*/
    }
}
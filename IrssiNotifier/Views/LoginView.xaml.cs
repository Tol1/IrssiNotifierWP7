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

namespace IrssiNotifier.Views
{
	public partial class LoginView : UserControl
	{
		public LoginView(LoginPage page)
		{
			InitializeComponent();
			FromPage = page;
			browser.Navigate(new Uri(App.BASEADDRESS + "client/login"));
		}

		public LoginPage FromPage { get; private set; }

		private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			if (e.Uri.AbsoluteUri.EndsWith("client/login/loginsuccess"))
			{
				var cookies = (sender as WebBrowser).GetCookies();
				var uri = (sender as WebBrowser).Source;
				PhoneApplicationService.Current.State["cookies"] = cookies;
				PhoneApplicationService.Current.State["cookiesUri"] = uri;
				FromPage.contentBorder.Child = new RegisterView(FromPage);
//				FromPage.contentBorder;
//				FromPage.NavigationService.Navigate(new Uri("/Registration.xaml", UriKind.Relative));
			}
		}
	}
}

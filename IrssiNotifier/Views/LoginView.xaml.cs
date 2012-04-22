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
using IrssiNotifier.Pages;

namespace IrssiNotifier.Views
{
	public partial class LoginView
	{
		public LoginView(LoginPage page)
		{
			InitializeComponent();
			FromPage = page;
			webBrowser.Navigate(new Uri(App.Baseaddress + "client/login"));
		}

		public LoginPage FromPage { get; private set; }

		private void BrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			if (e.Uri.AbsoluteUri.EndsWith("client/login/loginsuccess"))
			{
				var browser = sender as WebBrowser;
				var cookies = browser.GetCookies();
				var uri = browser.Source;
				PhoneApplicationService.Current.State["cookies"] = cookies;
				PhoneApplicationService.Current.State["cookiesUri"] = uri;
				FromPage.contentBorder.Child = new RegisterView(FromPage);
			}
		}
	}
}

using System;
using IrssiNotifier.Pages;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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

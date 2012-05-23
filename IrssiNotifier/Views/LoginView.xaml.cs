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
			webBrowser.Navigate(new Uri(App.Baseaddress + "client/login?version="+App.Version));
		}

		public LoginPage FromPage { get; private set; }

		private void BrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			var browser = sender as WebBrowser;
			if (e.Uri.AbsoluteUri.EndsWith("client/login/loginsuccess"))
			{
				var cookies = browser.GetCookies();
				var uri = browser.Source;
				PhoneApplicationService.Current.State["cookies"] = cookies;
				PhoneApplicationService.Current.State["cookiesUri"] = uri;
				FromPage.contentBorder.Child = new RegisterView(FromPage);
			}
			browser.IsEnabled = true;
			progressBar.IsIndeterminate = false;
		}

		private void WebBrowserNavigating(object sender, NavigatingEventArgs e)
		{
			progressBar.IsIndeterminate = true;
			(sender as WebBrowser).IsEnabled = false;
		}
	}
}

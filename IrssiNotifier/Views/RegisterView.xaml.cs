using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using IrssiNotifier.Pages;
using IrssiNotifier.Resources;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;

namespace IrssiNotifier.Views
{
	public partial class RegisterView : INotifyPropertyChanged
	{
		public RegisterView()
		{
			InitializeComponent();

			DataContext = this;
			IsBusy = true;
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender, args) =>
			{
				if(args.Error != null)
				{
					Dispatcher.BeginInvoke(() =>
						MessageBox.Show(string.Format(AppResources.ErrorRegistration, args.Error.Message) + "\n\n" + AppResources.ErrorRegistrationTryAgain, AppResources.ErrorTitle,
										MessageBoxButton.OK));
					PhoneApplicationService.Current.State["logout"] = true;
					App.GetCurrentPage().NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
					return;
				}
				var result = args.Result;
				var parsed = JObject.Parse(result);
				IsBusy = false;
				if (bool.Parse(parsed["success"].ToString()))
				{
					var loginPage = App.GetCurrentPage() as LoginPage;
					UserId = (string)parsed["userid"];
					if (loginPage != null)
					{
						loginPage.button.Content = AppResources.ContinueButtonText;
						loginPage.button.Visibility = Visibility.Visible;
						loginPage.button.Click += ButtonClick;
					}
				}
				else
				{
					Dispatcher.BeginInvoke(() => MessageBox.Show(string.Format(AppResources.ErrorRegistration, parsed["errorMessage"]), AppResources.ErrorTitle, MessageBoxButton.OK));
					PhoneApplicationService.Current.State["logout"] = true;		//Navigointi etusivulle, elegantimpia ratkaisuja?
					App.GetCurrentPage().NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
				}
			};
			var cookies = PhoneApplicationService.Current.State["cookies"] as CookieCollection;
			PhoneApplicationService.Current.State.Remove("cookies");
			if (cookies != null)
			{
				var cookieHeader = cookies.Cast<Cookie>().Aggregate("", (current, cookie) => current + (cookie.Name + "=" + cookie.Value + "; "));
				webclient.Headers["Cookie"] = cookieHeader;
			}
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/register"), "guid=" + App.AppGuid + "&version=" + App.Version);
		}

		private string _userId;
		public string UserId
		{
			get { return _userId; }
			set
			{
				_userId = value;
				IsolatedStorageSettings.ApplicationSettings["userID"] = value;
				NotifyPropertyChanged("UserId");
			}
		}

		private bool _isBusy;

		public bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				_isBusy = value;
				NotifyPropertyChanged("IsBusy");
			}
		}

		private void ButtonClick(object sender, EventArgs e)
		{
			MessageBox.Show(AppResources.InfoNotificationChannelAutoOpened);
			SettingsView.GetInstance().IsPushEnabled = true;
			PhoneApplicationService.Current.State["registered"] = true;
			App.GetCurrentPage().NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
		}

		public void NotifyPropertyChanged(string property)
		{
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}

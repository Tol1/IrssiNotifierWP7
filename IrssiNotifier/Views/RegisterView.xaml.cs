using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using IrssiNotifier.Pages;
using IrssiNotifier.PushNotificationContext;
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
				var result = args.Result;
				var parsed = JObject.Parse(result);
				IsBusy = false;
				if (bool.Parse(parsed["success"].ToString()))
				{
					var loginPage = App.GetCurrentPage() as LoginPage;
					UserId = (string)parsed["userid"];
					if (loginPage != null)
					{
						loginPage.button.Content = "Jatka";
						loginPage.button.Visibility = Visibility.Visible;
						loginPage.button.Click += ButtonClick;
					}
				}
				else
				{
					Dispatcher.BeginInvoke(() => MessageBox.Show("Virhe rekisteröinnissä: " + parsed["errorMessage"]));
					//TODO unsuccessful
				}
			};
			var cookies = PhoneApplicationService.Current.State["cookies"] as CookieCollection;
			PhoneApplicationService.Current.State.Remove("cookies");
			var cookieHeader = cookies.Cast<Cookie>().Aggregate("", (current, cookie) => current + (cookie.Name + "=" + cookie.Value + "; "));
			webclient.Headers["Cookie"] = cookieHeader;
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
			MessageBox.Show("Notifikaatiokanava avataan automaattisesti. Tarkista asetusnäkymästä muut asetukset.");
			PushContext.Current.IsPushEnabled = true;
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

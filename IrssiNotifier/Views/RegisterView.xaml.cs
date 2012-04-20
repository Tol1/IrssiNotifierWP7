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
using System.ComponentModel;
using System.IO.IsolatedStorage;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Shell;
using IrssiNotifier.Pages;

namespace IrssiNotifier.Views
{
	public partial class RegisterView : UserControl, INotifyPropertyChanged
	{
		public RegisterView(LoginPage page)
		{
			InitializeComponent();
			FromPage = page;

			DataContext = this;
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender, args) =>
			{
				var result = args.Result;
				var parsed = JObject.Parse(result);	//TODO unsuccessful
				UserId = (string)parsed["success"];
				FromPage.button.Content = "Jatka";
				FromPage.button.Visibility = Visibility.Visible;
				FromPage.button.Click += ButtonClick;
			};
			var cookies = PhoneApplicationService.Current.State["cookies"] as CookieCollection;
			var cookieHeader = "";
			foreach (Cookie cookie in cookies)
			{
				cookieHeader += cookie.Name + "=" + cookie.Value + "; ";
			}
			webclient.Headers["Cookie"] = cookieHeader;
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.BASEADDRESS + "client/register"), "guid=" + App.AppGuid/* + "&PushChannelURI=" + IsolatedStorageSettings.ApplicationSettings["NotificationChannelUri"].ToString()*/);
		}
		public LoginPage FromPage { get; private set; }

		private string _userId;
		public string UserId
		{
			get { return _userId; }
			set
			{
				_userId = value;
				IsolatedStorageSettings.ApplicationSettings["userID"] = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("UserId"));
				}
			}
		}

		private void ButtonClick(object sender, EventArgs e)
		{
			PhoneApplicationService.Current.State["registered"] = true;
			FromPage.NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}

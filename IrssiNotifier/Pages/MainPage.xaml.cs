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
using IrssiNotifier.PushNotificationContext;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Notification;
using System.Text;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class MainPage
	{

		private readonly IsolatedStorageSettings _appSettings = IsolatedStorageSettings.ApplicationSettings;
		// Constructor
		public MainPage()
		{
			InitializeComponent();
			if (!_appSettings.Contains("userID"))
			{
				ShowInitialView();
			}
			else
			{
				ShowMainView();
			}
		}

		private void ShowInitialView()
		{
			contentBorder.Child = new InitialView();
			firstButton.Content = "Rekisteröidy";
			firstButton.Visibility = Visibility.Visible;
			firstButton.Click += (sender, args) => NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
			ApplicationBar.IsVisible = false;
		}

		private void ShowMainView()
		{
			firstButton.Content = "Hilitet";
			firstButton.Visibility = Visibility.Visible;
			firstButton.Click +=
				(sender, args) => NavigationService.Navigate(new Uri("/Pages/HilitePage.xaml", UriKind.Relative));
			if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
			{
				PushContext.Current.Connect(Dispatcher, c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher));
			}
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (PhoneApplicationService.Current.State.ContainsKey("registered") || PhoneApplicationService.Current.State.ContainsKey("logout"))
			{
				while (NavigationService.CanGoBack)
				{
					NavigationService.RemoveBackEntry(); //clear backstack
				}
				PhoneApplicationService.Current.State.Remove("registered");
				PhoneApplicationService.Current.State.Remove("logout");
			}
		}
		/*
		private void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
		{
			var message = new StringBuilder();
			var relativeUri = string.Empty;

			message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

			// Parse out the information that was part of the message.
			foreach (string key in e.Collection.Keys)
			{
				message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

				if (string.Compare(
					key,
					"wp:Param",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.CompareOptions.IgnoreCase) == 0)
				{
					relativeUri = e.Collection[key];
				}
			}

			// Display a dialog of all the fields in the toast.
			Dispatcher.BeginInvoke(() => MessageBox.Show(message.ToString()));

		}*/

		private void SettingsButtonClick(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
		}
	}
}
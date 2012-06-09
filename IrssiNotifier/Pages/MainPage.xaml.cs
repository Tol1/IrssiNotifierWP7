using System;
using System.IO.IsolatedStorage;
using IrssiNotifier.PushNotificationContext;
using IrssiNotifier.Views;
using Microsoft.Phone.Shell;

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
			ApplicationBar.IsVisible = false;
		}

		private void ShowMainView()
		{
			if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
			{
				PushContext.Current.Connect(Dispatcher, c => SettingsView.GetInstance().RegisterChannelUri(c.ChannelUri, Dispatcher));
			}
			var view = new HiliteView();
			ApplicationBar = view.ApplicationBar;
			contentBorder.Child = view;
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
				foreach (var tile in ShellTile.ActiveTiles)
				{
					tile.Update(new StandardTileData { Count = 0 });
				}
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
	}
}
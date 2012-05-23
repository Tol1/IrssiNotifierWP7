using System;
using System.IO.IsolatedStorage;
using IrssiNotifier.PushNotificationContext;
using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class HilitePage
	{
		public HilitePage()
		{
			InitializeComponent();
			DataContext = this;
			if (!IsolatedStorageSettings.ApplicationSettings.Contains("userID"))
			{
				contentBorder.Child = new InitialView(this);
				ApplicationBar.IsVisible = false;
			}
			else
			{
				if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
				{
					PushContext.Current.Connect(Dispatcher, c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher, this));
				}
				contentBorder.Child = new HiliteView(this);
			}
		}

		private void SettingsButtonClick(object sender, EventArgs e)
		{
			((HiliteView)contentBorder.Child).SettingsButtonClick(sender, e);
		}

		private void RefreshButtonClick(object sender, EventArgs e)
		{
			((HiliteView)contentBorder.Child).RefreshButtonClick(sender, e);
		}
	}
}
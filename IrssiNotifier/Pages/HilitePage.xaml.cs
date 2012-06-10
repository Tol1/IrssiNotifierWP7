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
				contentBorder.Child = new InitialView();
			}
			else
			{
				if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
				{
					PushContext.Current.Connect(Dispatcher, c => SettingsView.GetInstance().RegisterChannelUri(c.ChannelUri, Dispatcher));
				}
				var view = new HiliteView();
				ApplicationBar = view.ApplicationBar;
				contentBorder.Child = view;
			}
		}
	}
}
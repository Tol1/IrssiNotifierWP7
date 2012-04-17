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
using WindowsPhone.Recipes.Push.Client;
using IrssiNotifier.Views;

namespace IrssiNotifier
{
	public partial class Page2 : PhoneApplicationPage
	{
		public Page2()
		{
			InitializeComponent();
			try
			{
				var pushContext = new PushContext(App.CHANNELNAME, App.SERVICENAME, App.AllowedDomains, Dispatcher);
			}
			catch (InvalidOperationException)
			{

			}
		}
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (PushContext.Current.IsTileEnabled)
			{
				PushContext.Current.Connect(c =>
				{
					SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher, false);
				});
			}
		}
	}
}
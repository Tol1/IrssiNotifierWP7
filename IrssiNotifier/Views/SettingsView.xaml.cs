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
using WindowsPhone.Recipes.Push.Client;
using System.IO.IsolatedStorage;
using System.Windows.Threading;

namespace IrssiNotifier.Views
{
	public partial class SettingsView : UserControl
	{
		public SettingsView()
		{
			DataContext = PushContext.Current;
			InitializeComponent();

			PushContext.Current.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "IsPushEnabled")
				{
					if (PushContext.Current.IsConnected)
					{
						PushContext.Current.Disconnect();
					}
					else
					{
						PushContext.Current.Connect(c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher));
					}
				}
				if (args.PropertyName == "IsToastEnabled")
				{
					SettingsView.UpdateSettings("toast", PushContext.Current.IsToastEnabled, Dispatcher);
				}
			};
		}

		public static void RegisterChannelUri(Uri ChannelUri, Dispatcher dispatcher)
		{
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				dispatcher.BeginInvoke(() => MessageBox.Show(args.Result));
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.BASEADDRESS + "client/update"), "POST", "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"].ToString() + "&guid=" + App.AppGuid + "&newUrl=" + ChannelUri.ToString());
		}

		public static void UpdateSettings(string param, bool enabled, Dispatcher dispatcher)
		{
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				dispatcher.BeginInvoke(() => MessageBox.Show("Vastaanotto keskeytetty"));
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.BASEADDRESS + "client/settings"), "POST", "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"].ToString() + "&guid=" + App.AppGuid + "&enable=" + enabled);
		}
	}
}

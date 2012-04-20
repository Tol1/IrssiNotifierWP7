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
using IrssiNotifier.Views;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using System.ComponentModel;

namespace IrssiNotifier.Pages
{
	public partial class HilitePage : PhoneApplicationPage, INotifyPropertyChanged
	{
		public HilitePage()
		{
			InitializeComponent();
			DataContext = this;
			try
			{
				var pushContext = new PushContext(App.Channelname, App.Servicename, App.AllowedDomains, Dispatcher);
			}
			catch (InvalidOperationException)
			{

			}
			HiliteCollection = new ObservableCollection<Hilite>();
		}
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (PushContext.Current.IsTileEnabled)
			{
				PushContext.Current.Connect(c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher));
			}
			long lastFetch = 0;
			if (IsolatedStorageSettings.ApplicationSettings.Contains("LastHiliteFetch"))
			{
				try
				{
					lastFetch = long.Parse(IsolatedStorageSettings.ApplicationSettings["LastHiliteFetch"].ToString());
				}
				catch (FormatException)
				{
					lastFetch = 0;
				}
			}
			FetchHilitesSince(lastFetch);
		}

		public class Hilite
		{
			public string Channel { get; set; }
			public string Nick { get; set; }
			public string Message { get; set; }
			public string From { get { return Nick + " @ " + Channel; } }
			public DateTime Timestamp { get; set; }
		}

		private ObservableCollection<Hilite> _hiliteCollection;
		public ObservableCollection<Hilite> HiliteCollection
		{
			get { return _hiliteCollection; }
			set
			{
				_hiliteCollection = value;
				if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("HiliteCollection")); }
			}
		}

		private void FetchHilitesSince(long last)
		{
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				if (args.Error != null)
				{
					return;
				}
				try
				{
					var collection = new ObservableCollection<Hilite>();
					var result = JObject.Parse(args.Result);
					IsolatedStorageSettings.ApplicationSettings["LastHiliteFetch"] = result["currentTimestamp"].ToString();
					var messages = JArray.Parse(result["messages"].ToString());
					foreach (var hilite in messages.Select(hiliteRow => JObject.Parse(hiliteRow.ToString())))
					{
						var hiliteObj = new Hilite
						             	{
						             		Channel = hilite["channel"].ToString(),
						             		Nick = hilite["nick"].ToString(),
						             		Message = hilite["message"].ToString()
						             	};
						collection.Insert(0, hiliteObj);
					}
					HiliteCollection = collection;
				}
				catch(Exception){
				}
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/messages"), "POST", "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"] + "&guid=" + App.AppGuid + "&since=" + last);
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
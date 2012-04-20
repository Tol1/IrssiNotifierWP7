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
				var pushContext = new PushContext(App.CHANNELNAME, App.SERVICENAME, App.AllowedDomains, Dispatcher);
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
				PushContext.Current.Connect(c =>
				{
					SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher);
				});
			}
			FetchHilitesSince(0);
		}

		public class Hilite
		{
			public string Channel { get; set; }
			public string Nick { get; set; }
			public string Message { get; set; }
			public string From { get { return Nick + " @ " + Channel; } }
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
					var result = JArray.Parse(args.Result);
					foreach(var hiliteRow in result){
						var hilite = JObject.Parse(hiliteRow.ToString());
						collection.Add(new Hilite() { Channel = hilite["channel"].ToString(), Nick = hilite["nick"].ToString(), Message = hilite["message"].ToString() });
					}
					HiliteCollection = collection;
				}
				catch(Exception){
				}
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.BASEADDRESS + "client/messages"), "POST", "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"].ToString() + "&guid=" + App.AppGuid/* + "&since=" + last*/);
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
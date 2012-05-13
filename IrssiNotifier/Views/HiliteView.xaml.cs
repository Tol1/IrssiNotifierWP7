using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using IrssiNotifier.Model;
using IrssiNotifier.PushNotificationContext;
using Newtonsoft.Json.Linq;

namespace IrssiNotifier.Views
{
	public partial class HiliteView : INotifyPropertyChanged
	{
		private ObservableCollection<Hilite> _hiliteCollection;
		private bool _isBusy;
		private long _lastFetch;

		public HiliteView(Page fromPage)
		{
			InitializeComponent();
			DataContext = this;
			FromPage = fromPage;
			if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
			{
				PushContext.Current.Connect(Dispatcher, c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher, FromPage));
			}
			Fetch(true);
		}

		public HiliteView()
		{
			InitializeComponent();
			DataContext = this;
			if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
			{
				PushContext.Current.Connect(Dispatcher, c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher, FromPage));
			}
			Fetch(true);
		}

		public Page FromPage { get; set; }

		public bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				_isBusy = value;
				NotifyPropertyChanged("IsBusy");
				NotifyPropertyChanged("IsListEnabled");
			}
		}

		public bool IsListEnabled
		{
			get { return !IsBusy; }
		}

		public ObservableCollection<Hilite> HiliteCollection
		{
			get { return _hiliteCollection; }
			set
			{
				_hiliteCollection = value;
				NotifyPropertyChanged("HiliteCollection");
				NotifyPropertyChanged("NewHilites");
			}
		}

		public ObservableCollection<Hilite> NewHilites
		{
			get
			{
				if (_nextHilite != null && _nextHilite.Id < _lastFetch)
				{
					newListBox.ItemTemplate = (DataTemplate)Resources["ButtonlessHiliteTemplate"];
				}
				else
				{
					newListBox.ItemTemplate = (DataTemplate)Resources["HiliteTemplate"];
				}
				return HiliteCollection != null
						? new ObservableCollection<Hilite>(HiliteCollection.Where(hilite => hilite.Id > _lastFetch))
						: new ObservableCollection<Hilite>();
			}
		}

		private Hilite _nextHilite;

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		#endregion

		private void Fetch(bool ignoreCache)
		{
			if (ignoreCache || HiliteCollection == null)
			{
				HiliteCollection = new ObservableCollection<Hilite>();
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
				_lastFetch = lastFetch;
				FetchHilites();
			}
		}

		private void ParseResult(string response)
		{
			try
			{
				var collection = new ObservableCollection<Hilite>();
				var result = JObject.Parse(response);
				if (!bool.Parse(result["isNextFetch"].ToString()))
				{
					IsolatedStorageSettings.ApplicationSettings["LastHiliteFetch"] = result["currentTimestamp"].ToString();
				}
				else
				{
					collection = HiliteCollection;
					var last = collection.LastOrDefault();
					if (last != null)
					{
						last.IsLast = false;
					}
				}
				var messages = JArray.Parse(result["messages"].ToString());
				foreach (var hilite in messages.Select(hiliteRow => JObject.Parse(hiliteRow.ToString())))
				{
					var hiliteObj = new Hilite
					{
						Channel = hilite["channel"].ToString(),
						Nick = hilite["nick"].ToString(),
						Message = hilite["message"].ToString(),
						TimestampString = hilite["timestamp"].ToString(),
						Id = long.Parse(hilite["id"].ToString())
					};
					collection.Add(hiliteObj);
				}
				if (result["nextMessage"].Type != JTokenType.Null)
				{
					var nextHilite = JObject.Parse(result["nextMessage"].ToString());
					_nextHilite = new Hilite
					{
						Channel = nextHilite["channel"].ToString(),
						Nick = nextHilite["nick"].ToString(),
						Message = nextHilite["message"].ToString(),
						TimestampString = nextHilite["timestamp"].ToString(),
						Id = long.Parse(nextHilite["id"].ToString())
					};
					var last = collection.LastOrDefault();
					if (last != null)
					{
						last.IsLast = true;
					}
				}
				else
				{
					_nextHilite = null;
					var last = collection.LastOrDefault();
					if (last != null)
					{
						last.IsLast = false;
					}
				}
				HiliteCollection = collection;
			}
			catch (Exception e)
			{
				Dispatcher.BeginInvoke(() => MessageBox.Show("Virhe viestien noutamisessa: " + e));
			}
			IsBusy = false;
		}

		private void FetchHilites(long starting = 0)
		{
			IsBusy = true;
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				if (args.Error != null)
				{
					IsBusy = false;
					Dispatcher.BeginInvoke(() => MessageBox.Show("Virhe viestien noutamisessa"));
					return;
				}
				ParseResult(args.Result);
			};
			var postMessage = "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"] + "&guid=" +
							  App.AppGuid;
			if (starting != 0)
			{
				postMessage += "&starting=" + starting;
			}
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/messages"), "POST",
										postMessage);
		}

		public void RefreshButtonClick(object sender, EventArgs e)
		{
			Fetch(true);
			SettingsView.ClearTileCount(Dispatcher);
		}

		public void SettingsButtonClick(object sender, EventArgs e)
		{
			FromPage.NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
		}

		private void MoreClick(object sender, RoutedEventArgs e)
		{
			FetchHilites(_nextHilite.Id);
		}
	}
}

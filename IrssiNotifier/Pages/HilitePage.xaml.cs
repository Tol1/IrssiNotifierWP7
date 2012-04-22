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
	public partial class HilitePage : INotifyPropertyChanged
	{
		public HilitePage()
		{
			InitializeComponent();
			DataContext = this;
			HiliteCollection = new ObservableCollection<Hilite>();
		}
		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (PushContext.Current.IsTileEnabled)
			{
				PushContext.Current.Connect(Dispatcher, c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher));
			}
			DefaultFetch();
		}

		private CurrentView _currentState;

		public CurrentView CurrentState
		{
			get { return _currentState; }
			set
			{
				_currentState = value;
				switch (value)
				{
					case CurrentView.Last:
						ToggleButton.Content = "Näytä kaikki";
						break;
					case CurrentView.All:
						ToggleButton.Content = "Näytä uudet";
						break;
				}
			}
		}

		private long _lastFetch;

		private void DefaultFetch()
		{
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
			FetchHilites(lastFetch);
			CurrentState = CurrentView.Last;
		}

		private bool _isBusy;

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

		public class Hilite
		{
			public string Channel { get; set; }
			public string Nick { get; set; }
			public string Message { get; set; }
			public string From { get { return "["+Timestamp.ToShortDateString()+" "+Timestamp.ToShortTimeString()+"] "+Nick + " @ " + Channel; } }
			private string _timestampString;

			public string TimestampString
			{
				get { return _timestampString; }
				set
				{
					_timestampString = value;
					try
					{
						Timestamp = DateTime.Parse(value);
					}
					catch (FormatException)
					{

					}
				}
			}

			public DateTime Timestamp { get; set; }
		}

		private ObservableCollection<Hilite> _hiliteCollection;
		public ObservableCollection<Hilite> HiliteCollection
		{
			get { return _hiliteCollection; }
			set
			{
				_hiliteCollection = value;
				NotifyPropertyChanged("HiliteCollection");
			}
		}

		private void ParseResult(string response)
		{
			try
			{
				var collection = new ObservableCollection<Hilite>();
				var result = JObject.Parse(response);
				IsolatedStorageSettings.ApplicationSettings["LastHiliteFetch"] = result["currentTimestamp"].ToString();
				var messages = JArray.Parse(result["messages"].ToString());
				foreach (var hilite in messages.Select(hiliteRow => JObject.Parse(hiliteRow.ToString())))
				{
					var hiliteObj = new Hilite
					                	{
					                		Channel = hilite["channel"].ToString(),
					                		Nick = hilite["nick"].ToString(),
					                		Message = hilite["message"].ToString(),
											TimestampString = hilite["timestamp"].ToString()
					                	};
					collection.Insert(0, hiliteObj);
				}
				HiliteCollection = collection;
			}
			catch (Exception e)
			{
				Dispatcher.BeginInvoke(() => MessageBox.Show("Virhe viestien noutamisessa: "+e));
			}
			IsBusy = false;
		}

		private void FetchHilites(long last = 0)
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
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/messages"), "POST", "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"] + "&guid=" + App.AppGuid + "&since=" + last);
		}

		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void ToggleButtonClick(object sender, RoutedEventArgs e)
		{
			if(CurrentState == CurrentView.Last)
			{
				FetchHilites();
				CurrentState = CurrentView.All;
			}
			else
			{
				FetchHilites(_lastFetch);
				CurrentState = CurrentView.Last;
			}
		}

		public enum CurrentView
		{
			Last,All
		}
	}
}
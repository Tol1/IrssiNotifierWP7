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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Windows.Threading;
using IrssiNotifier.Pages;
using IrssiNotifier.PushNotificationContext;
using Microsoft.Phone.Controls;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Microsoft.Phone.Shell;

namespace IrssiNotifier.Views
{
	public partial class SettingsView : INotifyPropertyChanged
	{
		public SettingsView(SettingsPage fromPage)
		{
			DataContext = this;
			InitializeComponent();//TODO uloskirjautuminen
			FromPage = fromPage;
		}

		public SettingsPage FromPage { get; private set; }

		public string UserId
		{
			get { return IsolatedStorageSettings.ApplicationSettings["userID"].ToString(); }
		}

		public bool IsToastEnabled
		{
			get { return PushContext.Current.IsToastEnabled; }
			set
			{
				if (PushContext.Current.IsToastEnabled != value)
				{
					PushContext.Current.IsToastEnabled = value;
					UpdateSettings("toast", value, Dispatcher);
					NotifyPropertyChanged("IsToastEnabled");
				}
			}
		}

		public bool IsTileEnabled
		{
			get { return PushContext.Current.IsTileEnabled; }
			set
			{
				if (PushContext.Current.IsTileEnabled != value)
				{
					PushContext.Current.IsTileEnabled = value;
					NotifyPropertyChanged("IsTileEnabled");
					UpdateSettings("tile", value, Dispatcher, () => PinTile(value));
				}
				
			}
		}

		public bool IsRawEnabled
		{
			get { return PushContext.Current.IsRawEnabled; }
			set
			{
				if (PushContext.Current.IsRawEnabled != value)
				{
					PushContext.Current.IsRawEnabled = value;
					UpdateSettings("raw", value, Dispatcher);
					NotifyPropertyChanged("IsRawEnabled");
				}
			}
		}

		public bool IsPushEnabled
		{
			get { return PushContext.Current.IsPushEnabled; }
			set
			{
				if (PushContext.Current.IsConnected != value)
				{
					if (value)
					{
						PushContext.Current.Connect(Dispatcher, c => RegisterChannelUri(c.ChannelUri, Dispatcher));
					}
					else
					{
						PushContext.Current.Disconnect();
					}
					NotifyPropertyChanged("IsPushEnabled");
				}
			}
		}

		public void NotifyPropertyChanged(string property){
			if(PropertyChanged != null){
				PropertyChanged(this,new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;


		public static void RegisterChannelUri(Uri channelUri, Dispatcher dispatcher)
		{
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				if (args.Error != null) {
					dispatcher.BeginInvoke(() => MessageBox.Show(args.Result));
					return;
				}
				var result = JObject.Parse(args.Result);
				if (bool.Parse(result["success"].ToString()))
				{
					var dirty = false;
					var toastStatus = bool.Parse(result["toastStatus"].ToString());
					var tileStatus = bool.Parse(result["tileStatus"].ToString());
					if(tileStatus && PushContext.Current.IsTileEnabled)
					{
						var hiliteTile = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.ToString() == App.Hilitepageurl);
						if (hiliteTile == null && PushContext.Current.IsPushEnabled)
						{
							dispatcher.BeginInvoke(() =>
							                       	{
							                       		MessageBox.Show("Livetiili poistettu. Poistetaan livetiili-päivitykset käytöstä.");
							                       		PushContext.Current.IsTileEnabled = false;
														UpdateSettings("tile", false, dispatcher);
							                       	});
						}
					}
					else if (tileStatus != PushContext.Current.IsTileEnabled)
					{
						PushContext.Current.IsTileEnabled = tileStatus;
						dirty = true;
					}
					if (toastStatus != PushContext.Current.IsToastEnabled)
					{
						PushContext.Current.IsToastEnabled = toastStatus;
						dirty = true;
					}
					if(dirty)
					{
						dispatcher.BeginInvoke( () =>
							MessageBox.Show("Web-sovellus on muuttanut notifikaatioasetuksia aiemmin sattuneen virheen vuoksi. " +
							                "Tarkista asetukset asetusnäkymässä."));
					}
				}
				else
				{
					dispatcher.BeginInvoke(() => MessageBox.Show(result["errorMessage"].ToString()));
				}
				ClearTileCount(dispatcher);

			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/update"), "POST", "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"] + "&guid=" + App.AppGuid + "&newUrl=" + channelUri);
		}

		public static void ClearTileCount(Dispatcher dispatcher)
		{
			if (PushContext.Current.IsConnected && PushContext.Current.IsPushEnabled && PushContext.Current.IsTileEnabled)
				{
					UpdateSettings("clearcount", true, dispatcher, () =>
					                                               	{
					                                               		foreach (var tile in ShellTile.ActiveTiles)
					                                               		{
					                                               			tile.Update(new StandardTileData { Count = 0 });
					                                               		}
					                                               	});

				}
		}

		private static void UpdateSettings(string param, bool enabled, Dispatcher dispatcher, Action callback = null)
		{
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				if (args.Error != null)
				{
					dispatcher.BeginInvoke(() => MessageBox.Show(args.Result));
					return;
				}
				var result = JObject.Parse(args.Result);
				if (!bool.Parse(result["success"].ToString()))
				{
					dispatcher.BeginInvoke(() => MessageBox.Show(result["errorMessage"].ToString()));
				}
				else
				{
					if (callback != null)
					{
						callback();
					}
				}
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/settings"), "POST", "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"] + "&guid=" + App.AppGuid + "&"+param+"=" + enabled);
		}

		private static void PinTile(bool value)
		{
			var hiliteTile = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.ToString() == App.Hilitepageurl);
			if (value && hiliteTile == null)
			{
				var answer = MessageBox.Show("Käyttääksesi livetiilitoimintoa sinun on kiinnitettävä tiili aloitusnäyttöön. Hyväksytkö tiilen lisäyksen?", "Vahvista", MessageBoxButton.OKCancel);
				if (answer == MessageBoxResult.OK)
				{
					var newTileData = new StandardTileData
					{
						BackgroundImage = new Uri("/Images/Tile.png", UriKind.Relative),
						Count = 0
					};
					ShellTile.Create(new Uri(App.Hilitepageurl, UriKind.Relative), newTileData);
				}
			}
			else if (!value && hiliteTile != null)
			{
				/*var answer = MessageBox.Show("Poistetaanko myös tiili?", "Vahvista", MessageBoxButton.OKCancel);
				if (answer == MessageBoxResult.OK)
				{
					hiliteTile.Delete();
				}*/
			}
		}

		private void LogoutClick(object sender, RoutedEventArgs e)
		{
			var answer = MessageBox.Show("Oletko varma että haluat kirjautua ulos?", "Vahvista uloskirjautuminen",
			                             MessageBoxButton.OKCancel);
			if(answer == MessageBoxResult.OK)
			{
				var browser = new WebBrowser();
				browser.Navigated += (o, args) =>
				                     	{
				                     		IsPushEnabled = false;
				                     		IsTileEnabled = false;
				                     		IsToastEnabled = false;
				                     		IsolatedStorageSettings.ApplicationSettings.Remove("userID");
				                     		while (FromPage.NavigationService.CanGoBack)
				                     		{
				                     			FromPage.NavigationService.RemoveBackEntry();
				                     		}
				                     		PhoneApplicationService.Current.State["logout"] = true;
				                     		FromPage.NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
				                     	};
				browser.NavigateToString(App.Baseaddress+"client/logout");
			}
		}
	}
}

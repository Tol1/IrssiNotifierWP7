using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using IrssiNotifier.Interfaces;
using IrssiNotifier.Pages;
using IrssiNotifier.PushNotificationContext;
using IrssiNotifier.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace IrssiNotifier.Views
{
	public partial class SettingsView : INotifyPropertyChanged
	{
		private static SettingsView _instance;

		public static SettingsView GetInstance()
		{
			return _instance ?? (_instance = new SettingsView());
		}

		public SettingsView()
		{
			DataContext = this;
			InitializeComponent();
		}

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
					if (value && !IsolatedStorageSettings.ApplicationSettings.Contains("UserAllowedToast"))
					{
						var answer = MessageBox.Show(AppResources.ToastNotificationPermissionText,
						                             AppResources.ToastNotificationPermissionTitle,
						                             MessageBoxButton.OKCancel);
						if (answer == MessageBoxResult.OK)
						{
							IsolatedStorageSettings.ApplicationSettings["UserAllowedToast"] = true;
						}
						else
						{
							NotifyPropertyChanged("IsToastEnabled");
							return;
						}
					}
					PushContext.Current.IsToastEnabled = value;
					UpdateSettings("toast", value, Dispatcher);
					NotifyPropertyChanged("IsToastEnabled");
					NotifyPropertyChanged("IntervalBrush");
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
					PinTile(value);
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
					PushContext.Current.IsPushEnabled = value;
					NotifyPropertyChanged("IsPushEnabled");
					NotifyPropertyChanged("IsSettingsEnabled");
					NotifyPropertyChanged("IntervalBrush");
				}
			}
		}

		private bool _isBusy;

		public bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				_isBusy = value;
				NotifyPropertyChanged("IsBusy");
				NotifyPropertyChanged("IsSettingsEnabled");
				NotifyPropertyChanged("IntervalBrush");
			}
		}

		public bool IsSettingsEnabled
		{
			get { return !IsBusy && IsPushEnabled; }
		}

		public Brush IntervalBrush
		{
			get { return (IsSettingsEnabled && IsToastEnabled) ? (Brush)Application.Current.Resources["PhoneForegroundBrush"] : (Brush)Application.Current.Resources["PhoneDisabledBrush"]; }
		}

		public int ToastInterval
		{
			get { return GetOrCreate("Settings.ToastInterval", 15); }
			set
			{
				if (GetOrCreate("Settings.ToastInterval", 15) != value)
				{
					SetOrCreate("Settings.ToastInterval", value);
					UpdateSettings("toastinterval", value, Dispatcher);
					NotifyPropertyChanged("ToastInterval");
				}
			}
		}

		public void NotifyPropertyChanged(string property){
			if(PropertyChanged != null){
				PropertyChanged(this,new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RegisterChannelUri(Uri channelUri, Dispatcher dispatcher)
		{
			IsBusy = true;
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				if (args.Error != null)
				{
					var exception = args.Error as WebException;
					if (exception != null && exception.Response is HttpWebResponse)
					{
						var response = exception.Response as HttpWebResponse;
						if (response.StatusCode == HttpStatusCode.NotFound)
						{
							dispatcher.BeginInvoke(() => MessageBox.Show(AppResources.ConnectionErrorText, AppResources.ConnectionErrorTitle, MessageBoxButton.OK));
							var page = App.GetCurrentPage() as ViewContainerPage;
							if (page != null)
							{
								page.View = new ConnectionProblemView();
								page.ApplicationBar = null;
								while (page.NavigationService.CanGoBack)
								{
									page.NavigationService.RemoveBackEntry();
								}
								return;
							}
						}
					}
					dispatcher.BeginInvoke(() => MessageBox.Show(args.Error.Message, AppResources.ErrorTitle, MessageBoxButton.OK));
					return;
				}
				var result = JObject.Parse(args.Result);
				if (bool.Parse(result["success"].ToString()))
				{
					var toastStatus = bool.Parse(result["toastStatus"].ToString());
					var tileStatus = bool.Parse(result["tileStatus"].ToString());
					if(tileStatus && IsTileEnabled)
					{
						var hiliteTile = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.ToString() == App.Hilitepageurl);
						if (hiliteTile == null && IsPushEnabled)
						{
							dispatcher.BeginInvoke(() =>
							                       	{
							                       		MessageBox.Show(AppResources.LiveTileRemovedText);
							                       		IsTileEnabled = false;
							                       	});
						}
					}
					IsTileEnabled = tileStatus;
					IsToastEnabled = toastStatus;
					SetOrCreate("Settings.ToastInterval", int.Parse(result["toastInterval"].ToString()));
					NotifyPropertyChanged("ToastInterval");
					if(bool.Parse(result["errorStatus"].ToString()))
					{
						dispatcher.BeginInvoke( () =>
							MessageBox.Show(AppResources.BackendErrorOccurredText));
					}
				}
				else
				{
					if (result["exceptionType"].ToString() == "UserNotFoundException" || result["exceptionType"].ToString() == "InvalidGUIDException")
					{
						dispatcher.BeginInvoke(
							() =>
								{
									switch (result["exceptionType"].ToString())
									{
										case "UserNotFoundException":
											MessageBox.Show(AppResources.ErrorUserNotFound);
											break;
										case "InvalidGUIDException":
											MessageBox.Show(
												AppResources.ErrorGuidNotFound);
											break;
									}
									PushContext.Current.Disconnect();
									PushContext.Current.IsPushEnabled = false;
									PushContext.Current.IsTileEnabled = false;
									PushContext.Current.IsToastEnabled = false;
									IsolatedStorageSettings.ApplicationSettings.Remove("userID");
									App.AppGuid = Guid.NewGuid().ToString();
									IsolatedStorageSettings.ApplicationSettings["GUID"] = App.AppGuid;
									var currentPage = App.GetCurrentPage();
									while (currentPage.NavigationService.CanGoBack)
									{
										currentPage.NavigationService.RemoveBackEntry();
									}
									PhoneApplicationService.Current.State["logout"] = true;
									currentPage.NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
								});
						return;
					}
					else
					{
						dispatcher.BeginInvoke(() => MessageBox.Show(result["errorMessage"].ToString()));
					}
				}
				ClearTileCount();
				IsBusy = false;
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/update"), "POST",
			                            "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"] + "&guid=" +
			                            App.AppGuid + "&newUrl=" + channelUri + "&version=" + App.Version);
		}

		private static void ClearTileCount()
		{
			foreach (var tile in ShellTile.ActiveTiles)
			{
				tile.Update(new StandardTileData {Count = 0});
			}
		}

		public void ClearTileCount(Dispatcher dispatcher)
		{
			if (PushContext.Current.IsConnected && IsPushEnabled && IsTileEnabled)
			{
				UpdateSettings("clearcount", true, dispatcher, ClearTileCount);

			}
		}

		private void UpdateSettings(string param, object value, Dispatcher dispatcher, Action callback = null)
		{
			IsBusy = true;
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
				IsBusy = false;
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/settings"), "POST",
			                            "apiToken=" + IsolatedStorageSettings.ApplicationSettings["userID"] + "&guid=" +
			                            App.AppGuid + "&" + param + "=" + value + "&version=" + App.Version);
		}

		private void PinTile(bool value)
		{
			var hiliteTile = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.ToString() == App.Hilitepageurl);
			if (value && hiliteTile == null)
			{
				var answer = MessageBox.Show(AppResources.PinLiveTileText, AppResources.PinLiveTileTitle, MessageBoxButton.OKCancel);
				if (answer == MessageBoxResult.OK)
				{
					UpdateSettings("tile", true, Dispatcher, () =>
					                                         	{
					                                         		var newTileData = new StandardTileData
					                                         		                  	{
					                                         		                  		BackgroundImage =
					                                         		                  			new Uri("/Images/Tile.png", UriKind.Relative),
					                                         		                  		Count = 0
					                                         		                  	};
					                                         		ShellTile.Create(new Uri(App.Hilitepageurl, UriKind.Relative),
					                                         		                 newTileData);
					                                         	});
				}
				else
				{
					PushContext.Current.IsTileEnabled = false;
					NotifyPropertyChanged("IsTileEnabled");
				}
			}
			else
			{
				UpdateSettings("tile", value, Dispatcher);
			}
			/*else if (!value && hiliteTile != null)
			{
				UpdateSettings("tile", false, Dispatcher);
				var answer = MessageBox.Show("Poistetaanko myös tiili?", "Vahvista", MessageBoxButton.OKCancel);
				if (answer == MessageBoxResult.OK)
				{
					hiliteTile.Delete();
				}
			}*/
		}

		private void LogoutClick(object sender, RoutedEventArgs e)
		{
			var answer = MessageBox.Show(AppResources.ConfirmLogoutText, AppResources.ConfirmLogoutTitle,
			                             MessageBoxButton.OKCancel);
			if(answer == MessageBoxResult.OK)
			{
				var settingsPage = App.GetCurrentPage() as ViewContainerPage;
				IsBusy = true;
				if (settingsPage != null)
				{
					settingsPage.View = new WebBrowser { IsScriptEnabled = true, IsEnabled = false };
					var browser = (WebBrowser) settingsPage.View;
					browser.Navigated += (o, args) =>
					                     	{
					                     		if (args.Uri.ToString().EndsWith("client/logout/logoutsuccess"))
					                     		{
					                     			IsPushEnabled = false;
					                     			IsTileEnabled = false;
					                     			IsToastEnabled = false;
					                     			IsolatedStorageSettings.ApplicationSettings.Remove("userID");
					                     			App.AppGuid = Guid.NewGuid().ToString();
					                     			IsolatedStorageSettings.ApplicationSettings["GUID"] = App.AppGuid;
					                     			while (settingsPage.NavigationService.CanGoBack)
					                     			{
					                     				settingsPage.NavigationService.RemoveBackEntry();
					                     			}
					                     			IsBusy = false;
					                     			PhoneApplicationService.Current.State["logout"] = true;
					                     			settingsPage.NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
					                     		}
					                     	};
					browser.Navigate(new Uri(App.Baseaddress + "client/logout"));
				}
			}
		}

		private void IntervalTimeOnTap(object sender, GestureEventArgs e)
		{
			if (!IsBusy && IsPushEnabled && IsToastEnabled)
			{
				var settingsPage = App.GetCurrentPage() as IViewContainerPage;
				if (settingsPage != null)
				{
					settingsPage.View = new ToastIntervalView();
				}
			}
		}

		private void SetOrCreate<T>(string key, T value)
		{
			IsolatedStorageSettings.ApplicationSettings[key] = value;
		}

		private T GetOrCreate<T>(string key, T defaultValue = default(T))
		{
			T value;
			if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(key, out value))
			{
				return value;
			}
			IsolatedStorageSettings.ApplicationSettings[key] = defaultValue;
			return defaultValue;
		}
	}
}

using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IrssiNotifier.Interfaces;
using IrssiNotifier.PushNotificationContext;
using IrssiNotifier.Resources;
using IrssiNotifier.Utils;
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
					UpdateSettings(Settings.Toast, value, success =>
					                               	{
					                               		if (success)
					                               		{
					                               			PushContext.Current.IsToastEnabled = value;
					                               		}
					                               		NotifyPropertyChanged("IsToastEnabled");
					                               		NotifyPropertyChanged("IntervalBrush");
					                               	});

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
					UpdateSettings(Settings.Raw, value, success =>
					                             	{
					                             		if (success)
					                             		{
					                             			PushContext.Current.IsRawEnabled = value;
					                             		}
					                             		NotifyPropertyChanged("IsRawEnabled");
					                             	});
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
					IsBusy = true;
					if (value)
					{
						PushContext.Current.Connect(Dispatcher, c => RegisterChannelUri(c.ChannelUri, () =>
						                                                                              {
						                                                                              	IsBusy = false;
                                                                                                        PushContext.Current.IsPushEnabled = true;
                                                                                                        NotifyPropertyChanged("IsPushEnabled");
                                                                                                        NotifyPropertyChanged("IsSettingsEnabled");
                                                                                                        NotifyPropertyChanged("IntervalBrush");
						                                                                              }));
					}
					else
					{
						IsBusy = false;
						PushContext.Current.IsPushEnabled = false;
						PushContext.Current.Disconnect();
					}
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
					UpdateSettings(Settings.ToastInterval, value, success =>
					                                                   	{
					                                                   		if (success)
					                                                   		{
					                                                   			SetOrCreate("Settings.ToastInterval", value);
					                                                   		}
					                                                   		NotifyPropertyChanged("ToastInterval");
					                                                   	});
				}
			}
		}

		public void NotifyPropertyChanged(string property){
			if(PropertyChanged != null){
				PropertyChanged(this,new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RegisterChannelUri(Uri channelUri, Action callback)
		{
			IsBusy = true;
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				if (args.Error != null)
				{
					var errorTitleText = AppResources.ErrorTitle;
					var errorMessage = args.Error.Message;
					var errorMessage2 = "";
					var exception = args.Error as WebException;
					if (exception != null && exception.Response is HttpWebResponse)
					{
						var response = exception.Response as HttpWebResponse;
						if (response.StatusCode == HttpStatusCode.NotFound)
						{
							errorTitleText = AppResources.ConnectionErrorTitle;
							errorMessage = AppResources.ConnectionErrorText;
							errorMessage2 = AppResources.ConnectionErrorText2;
						}
					}
					Dispatcher.BeginInvoke(() => MessageBox.Show(errorMessage, errorTitleText, MessageBoxButton.OK));
					var page = App.GetCurrentPage() as ViewContainerPage;
					if (page != null)
					{
						page.View = new ErrorView(errorTitleText, errorMessage, errorMessage2);
						page.ApplicationBar = null;
						while (page.NavigationService.CanGoBack)
						{
							page.NavigationService.RemoveBackEntry();
						}
					}
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
							Dispatcher.BeginInvoke(() =>
																			{
																				MessageBox.Show(AppResources.LiveTileRemovedText, AppResources.ErrorTitle, MessageBoxButton.OK);
																				IsTileEnabled = tileStatus = false;
																			});
						}
					}

					SkipUpdateBackend = true;
					IsTileEnabled = tileStatus;
					IsToastEnabled = toastStatus;
					SkipUpdateBackend = false;

					SetOrCreate("Settings.ToastInterval", int.Parse(result["toastInterval"].ToString()));
					NotifyPropertyChanged("ToastInterval");
					if(bool.Parse(result["errorStatus"].ToString()))
					{
						Dispatcher.BeginInvoke( () =>
							MessageBox.Show(AppResources.BackendErrorOccurredText, AppResources.ErrorTitle, MessageBoxButton.OK));
					}
				}
				else
				{
					if (result["exceptionType"].ToString() == "UserNotFoundException" || result["exceptionType"].ToString() == "InvalidGUIDException")
					{
						Dispatcher.BeginInvoke(
							() =>
								{
									switch (result["exceptionType"].ToString())
									{
										case "UserNotFoundException":
											MessageBox.Show(AppResources.ErrorUserNotFound, AppResources.ErrorTitle, MessageBoxButton.OK);
											break;
										case "InvalidGUIDException":
											MessageBox.Show(
												AppResources.ErrorGuidNotFound, AppResources.ErrorTitle, MessageBoxButton.OK);
											break;
									}
									PushContext.Current.IsPushEnabled = false;
									PushContext.Current.IsTileEnabled = false;
									PushContext.Current.IsToastEnabled = false;
									PushContext.Current.Disconnect();
									IsolatedStorageSettings.ApplicationSettings.Remove("userID");
									App.AppGuid = Guid.NewGuid().ToString();
									IsolatedStorageSettings.ApplicationSettings["GUID"] = App.AppGuid;
									var currentPage = App.GetCurrentPage();
									while (currentPage.NavigationService.CanGoBack)
									{
										currentPage.NavigationService.RemoveBackEntry();
									}
									PhoneApplicationService.Current.State["logout"] = true;
									currentPage.NavigationService.Navigate(new Uri("/Pages/MainPage.xaml?error="+DateTime.Now.Ticks, UriKind.Relative));
								});
					}
					else if(result["exceptionType"].ToString() == "OldVersionException")
					{
						Dispatcher.BeginInvoke(() =>
						                       	{
						                       		MessageBox.Show(AppResources.ErrorOldVersionDialogText, AppResources.ErrorOldVersionTitle,
						                       		                MessageBoxButton.OK);
						                       		var currentPage = App.GetCurrentPage() as IViewContainerPage;
						                       		if (currentPage != null)
						                       		{
						                       			currentPage.View = new ErrorView(AppResources.ErrorOldVersionTitle,
						                       			                                 AppResources.ErrorOldVersionText, null);
						                       		}
						                       	});
					}
					else
					{
						Dispatcher.BeginInvoke(() =>
						                       	{
						                       		MessageBox.Show(result["errorMessage"].ToString(), AppResources.ErrorTitle,
						                       		                MessageBoxButton.OK);
						                       		var currentPage = App.GetCurrentPage() as IViewContainerPage;
													if(currentPage != null)
													{
														currentPage.View = new ErrorView(AppResources.ErrorTitle, result["errorMessage"].ToString(), null);
													}
						                       	});
					}
					return;
				}
				ClearLocalTileCount();
				if (callback != null) callback();
				IsBusy = false;
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/update"), "POST",
			                            "apiToken=" + UserId + "&guid=" +
			                            App.AppGuid + "&newUrl=" + channelUri + "&version=" + App.Version+"&wp8=" + App.IsTargetedVersion);
		}

		private bool SkipUpdateBackend { get; set; }

		private static void ClearLocalTileCount(bool success = true)
		{
			if (success)
			{
				foreach (var tile in ShellTile.ActiveTiles)
				{
					tile.Update(new StandardTileData {Count = 0});
				}
			}
		}

		public void ClearTileCount()
		{
			if (PushContext.Current.IsConnected && IsPushEnabled && IsTileEnabled)
			{
				UpdateSettings(Settings.ClearCount, true, ClearLocalTileCount);

			}
		}

		private void UpdateSettings(Settings param, object value, Action<bool> callback)
		{
			if(SkipUpdateBackend)
			{
				callback(true);
				return;
			}
			IsBusy = true;
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				if (args.Error != null)
				{
					var errorText = args.Error.Message;
					var errorTitle = AppResources.ErrorTitle;
					var exception = args.Error as WebException;
					if (exception != null && exception.Response is HttpWebResponse)
					{
						var response = exception.Response as HttpWebResponse;
						if (response.StatusCode == HttpStatusCode.NotFound)
						{
							errorText = AppResources.ConnectionErrorText;
							errorTitle = AppResources.ConnectionErrorTitle;
						}
					}
					Dispatcher.BeginInvoke(() => MessageBox.Show(errorText, errorTitle, MessageBoxButton.OK));
					callback(false);
				}
				else
				{
					var result = JObject.Parse(args.Result);
					if (!bool.Parse(result["success"].ToString()))
					{
						Dispatcher.BeginInvoke(() => MessageBox.Show(result["errorMessage"].ToString(), AppResources.ErrorTitle, MessageBoxButton.OK));
						callback(false);
					}
					else
					{
						callback(true);
					}
				}
				IsBusy = false;
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.Baseaddress + "client/settings"), "POST",
			                            "apiToken=" + UserId + "&guid=" +
			                            App.AppGuid + "&" + param.ToString().ToLower() + "=" + value + "&version=" + App.Version);
		}

		private void PinTile(bool value)
		{
			var hiliteTile = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.ToString() == App.Hilitepageurl);
			if (value && hiliteTile == null)
			{
				var answer = MessageBox.Show(AppResources.PinLiveTileText, AppResources.PinLiveTileTitle, MessageBoxButton.OKCancel);
				if (answer == MessageBoxResult.OK)
				{
					UpdateSettings(Settings.Tile, true, success =>
					                             	{
					                             		if (success)
					                             		{
                                                            PushContext.Current.IsTileEnabled = true;
                                                            NotifyPropertyChanged("IsTileEnabled");
															if (App.IsTargetedVersion)
															{
																var tile = new Uri("/Images/Tile.png", UriKind.Relative);
																var wideTile = new Uri("/Images/Tile_Flip_Wide.png", UriKind.Relative);
																var tileData = ReflectionHelper.CreateFlipTileData(null, "Irssi Notifier", null,
																                                                   tile, tile, null, 0, null, wideTile,
																                                                   null);
																ReflectionHelper.Create(new Uri(App.Hilitepageurl, UriKind.Relative), tileData, true);
															}
															else
															{
																var newTileData = new StandardTileData
																                  	{
																                  		BackgroundImage =
																                  			new Uri("/Images/Tile.png", UriKind.Relative),
																                  		Count = 0
																                  	};
																ShellTile.Create(new Uri(App.Hilitepageurl, UriKind.Relative),
																                 newTileData);
															}
					                             		}
					                             		else
					                             		{
					                             			PushContext.Current.IsTileEnabled = false;
					                             			NotifyPropertyChanged("IsTileEnabled");
					                             		}
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
				UpdateSettings(Settings.Tile, value, success =>
				                              	{
				                              		if (success)
				                              		{
				                              			PushContext.Current.IsTileEnabled = value;
				                              		}
				                              		else
				                              		{
				                              			PushContext.Current.IsTileEnabled = !value;
				                              		}
				                              		NotifyPropertyChanged("IsTileEnabled");
				                              	});
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
					                     			HandleLogout(settingsPage);
					                     		}
												else
					                     		{
													ConfirmLogoutWithoutGoogle(settingsPage);
					                     		}
					                     	};
					browser.NavigationFailed += (o, args) => ConfirmLogoutWithoutGoogle(settingsPage);
					browser.Navigate(new Uri(App.Baseaddress + "client/logout"));
				}
			}
		}

		private void ConfirmLogoutWithoutGoogle(ViewContainerPage settingsPage)
		{
			var continueAnswer = MessageBox.Show(AppResources.ErrorLogoutFailed,
			                                     AppResources.ErrorTitle,
			                                     MessageBoxButton.OKCancel);
			if (continueAnswer == MessageBoxResult.OK)
			{
				HandleLogout(settingsPage);
			}
			else
			{
				IsBusy = false;
				settingsPage.View = GetInstance();
			}
		}

		private void HandleLogout(Page settingsPage)
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

		public void Connect(Action callback, LoadingView loadingView = null)
		{
			if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
			{
				PushContext.Current.Connect(Dispatcher, c =>
				                                        {
				                                        	if (loadingView != null)
				                                        	{
				                                        		loadingView.Text = AppResources.LoadingText;
				                                        	}
				                                        	RegisterChannelUri(c.ChannelUri, callback);
				                                        });
			}
			else
			{
				callback();
			}
		}

		private static void SetOrCreate<T>(string key, T value)
		{
			IsolatedStorageSettings.ApplicationSettings[key] = value;
		}

		private static T GetOrCreate<T>(string key, T defaultValue = default(T))
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

	internal enum Settings
	{
		Toast,
		ToastInterval,
		Tile,
		ClearCount,
		Raw
	}
}

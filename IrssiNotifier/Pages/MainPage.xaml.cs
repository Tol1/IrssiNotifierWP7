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
using Microsoft.Phone.Notification;
using System.Text;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using WindowsPhone.Recipes.Push.Client;
using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
		
        private IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
			try
			{
				var pushContext = new PushContext(App.CHANNELNAME, App.SERVICENAME, App.AllowedDomains, Dispatcher);
			}
			catch (InvalidOperationException)
			{

			}
			if (!appSettings.Contains("userID"))
			{
				ShowInitialView();
			}
			else
			{
				ShowMainView();
			}
        }

		private void ShowInitialView()
		{
			contentBorder.Child = new InitialView();
			firstButton.Content = "Rekisteröidy";
			firstButton.Visibility = Visibility.Visible;
			firstButton.Click += (sender, args) =>
			{
				NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
			};
		}

		private void ShowMainView()
		{
			firstButton.Content = "Asetukset";
			firstButton.Visibility = Visibility.Visible;
			firstButton.Click += (sender, args) =>
			{
				NavigationService.Navigate(new Uri("/Pages/Settings.xaml", UriKind.Relative));
			};
			secondButton.Content = "Hilitet";
			secondButton.Visibility = Visibility.Visible;
			secondButton.Click += (sender, args) =>
			{
				NavigationService.Navigate(new Uri("/Pages/HilitePage.xaml", UriKind.Relative));
			};
			/*secondButton.Content = "Kirjaudu ulos";
			secondButton.Visibility = Visibility.Visible;
			secondButton.Click += (sender, args) =>
			{
				//TODO uloskirjautuminen
			};*/
			if (PushContext.Current.IsPushEnabled && !PushContext.Current.IsConnected)
			{
				PushContext.Current.Connect(c => SettingsView.RegisterChannelUri(c.ChannelUri, Dispatcher));
			}
		}

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
			
			if (PhoneApplicationService.Current.State.ContainsKey("registered"))
			{
				while (NavigationService.CanGoBack)
				{
					NavigationService.RemoveBackEntry();	//clear backstack
				}
				PhoneApplicationService.Current.State.Remove("registered");
			}
        }

        void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            StringBuilder message = new StringBuilder();
            string relativeUri = string.Empty;

            message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

                if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }
            }

            // Display a dialog of all the fields in the toast.
            Dispatcher.BeginInvoke(() => MessageBox.Show(message.ToString()));

        }
    }
}
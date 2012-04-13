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

namespace IrssiNotifier
{
    public partial class MainPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
			OpenChannel();
        }

		private void OpenChannel()
		{
			/// Holds the push channel that is created or found.
			HttpNotificationChannel pushChannel;

			InitializeComponent();

			// Try to find the push channel.
			pushChannel = HttpNotificationChannel.Find(App.CHANNELNAME);

			// If the channel was not found, then create a new connection to the push service.
			if (pushChannel == null)
			{
				pushChannel = new HttpNotificationChannel(App.CHANNELNAME);

				// Register for all the events before attempting to open the channel.
				pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
				pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

				// Register for this notification only if you need to receive the notifications while your application is running.
				pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

				pushChannel.Open();

				// Bind this new channel for toast events.
				pushChannel.BindToShellToast();
				pushChannel.BindToShellTile();
				NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
			}
			else
			{
				// The channel was already open, so just register for all the events.
				pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
				pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

				// Register for this notification only if you need to receive the notifications while your application is running.
				pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

				// Display the URI for testing purposes. Normally, the URI would be passed back to your web service at this point.
				System.Diagnostics.Debug.WriteLine(pushChannel.ChannelUri.ToString());
				MessageBox.Show(String.Format("Channel Uri is {0}",
					pushChannel.ChannelUri.ToString()));
				statustextbox.Text = "Jee";
				if (!appSettings.Contains("NotificationChannelUri") || pushChannel.ChannelUri.ToString() != appSettings["NotificationChannelUri"].ToString())
				{
					appSettings["NotificationChannelUri"] = pushChannel.ChannelUri.ToString();
					NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
				}
			}
		}

        private void LogoutClick(object sender, EventArgs e)
        {
			var pushChannel = HttpNotificationChannel.Find(App.CHANNELNAME);
			if (pushChannel != null)
			{
				pushChannel.UnbindToShellTile();
				pushChannel.UnbindToShellToast();
			}
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				MessageBox.Show("Vastaanotto keskeytetty");
			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.BASEADDRESS + "client/settings"), "POST", "apiToken=" + appSettings["userID"].ToString() + "&guid=" + App.AppGuid + "&enable=false");
        }

		private void LoginClick(object sender, EventArgs e)
		{
			OpenChannel();
		}

        void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
			Dispatcher.BeginInvoke(() =>
			{
				// Display the new URI for testing purposes.   Normally, the URI would be passed back to your web service at this point.
				System.Diagnostics.Debug.WriteLine(e.ChannelUri.ToString());
				MessageBox.Show(String.Format("Channel Uri is {0}",
					e.ChannelUri.ToString()));

			});
			if (appSettings.Contains("NotificationChannelUri") && appSettings.Contains("userID") && appSettings["NotificationChannelUri"].ToString() != e.ChannelUri.ToString())
			{
				var webclient = new WebClient();
				webclient.UploadStringCompleted += (sender1, args) =>
				{
					appSettings["NotificationChannelUri"] = e.ChannelUri.ToString();
				};
				webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
				webclient.UploadStringAsync(new Uri(App.BASEADDRESS + "client/update"), "POST", "apiToken=" + appSettings["userID"].ToString() + "&guid=" + App.AppGuid + "&newUrl=" + e.ChannelUri.ToString());
			}
			else
			{
				appSettings["NotificationChannelUri"] = e.ChannelUri.ToString();
			}
        }

        void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
                    e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData))
                    );
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

		private void RefreshClick(object sender, EventArgs e)
		{
			var webclient = new WebClient();
			webclient.UploadStringCompleted += (sender1, args) =>
			{
				MessageBox.Show(args.Result);

			};
			webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.BASEADDRESS + "client/update"), "POST", "apiToken=" + appSettings["userID"].ToString() + "&guid=" + App.AppGuid + "&newUrl=" + appSettings["NotificationChannelUri"]);
		}
    }
}
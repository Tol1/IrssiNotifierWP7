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
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace IrssiNotifier
{
    public partial class Registration : PhoneApplicationPage, INotifyPropertyChanged
    {
        public Registration()
        {
            InitializeComponent();
			DataContext = this;
            var webclient = new WebClient();
            webclient.UploadStringCompleted += (sender, args) =>
            {
                Console.WriteLine(sender);
                var result = args.Result;
                var parsed = JObject.Parse(result);
                UserId = (string) parsed["success"];

            };
            var cookies = PhoneApplicationService.Current.State["cookies"] as CookieCollection;
            var cookieHeader = "";
            foreach (Cookie cookie in cookies)
            {
                cookieHeader += cookie.Name + "=" + cookie.Value + "; ";
            }
            webclient.Headers["Cookie"] = cookieHeader;
            webclient.Headers["Content-type"] = "application/x-www-form-urlencoded";
			webclient.UploadStringAsync(new Uri(App.BASEADDRESS+"client/register"), "PushChannelURI=" + IsolatedStorageSettings.ApplicationSettings["NotificationChannelUri"].ToString());
        }

		private string _userId;
		public string UserId
		{
			get { return _userId; }
			set
			{
				_userId = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("UserId"));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
using System;
using Microsoft.Phone.Notification;

namespace IrssiNotifier.PushNotificationContext
{
    public class PushContextEventArgs : EventArgs
    {
        public HttpNotificationChannel NotificationChannel { get; private set; }

        internal PushContextEventArgs(HttpNotificationChannel channel)
        {
            NotificationChannel = channel;
        }
    }
}

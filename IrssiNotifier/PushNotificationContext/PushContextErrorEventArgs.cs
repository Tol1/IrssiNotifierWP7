using System;

namespace IrssiNotifier.PushNotificationContext
{
    public class PushContextErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public PushContextErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}

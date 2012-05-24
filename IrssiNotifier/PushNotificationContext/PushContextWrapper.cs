using System;
using System.ComponentModel;
using System.Diagnostics;

namespace IrssiNotifier.PushNotificationContext
{
	public class PushContextWrapper : INotifyPropertyChanged
	{
		public PushContextWrapper()
		{
			try
			{
				PushContext.Current.PropertyChanged += (sender, args) => NotifyPropertyChanged(args.PropertyName);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		public bool IsBusy
		{
			get { return PushContext.Current.IsBusy; }
			set
			{
				PushContext.Current.IsBusy = value;
				NotifyPropertyChanged("IsBusy");
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
	}
}

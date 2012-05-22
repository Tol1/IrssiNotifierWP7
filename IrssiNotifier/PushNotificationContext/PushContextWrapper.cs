using System.ComponentModel;

namespace IrssiNotifier.PushNotificationContext
{
	public class PushContextWrapper : INotifyPropertyChanged
	{
		public PushContextWrapper()
		{
			PushContext.Current.PropertyChanged += (sender, args) => NotifyPropertyChanged(args.PropertyName);
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

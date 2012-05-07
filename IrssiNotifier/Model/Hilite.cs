using System;
using System.ComponentModel;
using System.Windows;

namespace IrssiNotifier.Model
{
	public class Hilite : INotifyPropertyChanged
	{
		public long Id { get; set; }
		public string Channel { get; set; }
		public string Nick { get; set; }
		public string Message { get; set; }
		public string From { get { return "["+Timestamp.ToShortDateString()+" "+Timestamp.ToShortTimeString()+"] "+Nick + " @ " + Channel; } }
		private string _timestampString;

		public string TimestampString
		{
			get { return _timestampString; }
			set
			{
				_timestampString = value;
				try
				{
					Timestamp = DateTime.Parse(value);
				}
				catch (FormatException)
				{

				}
			}
		}

		public DateTime Timestamp { get; set; }

		private bool _isLast;

		public bool IsLast
		{
			get { return _isLast; }
			set
			{
				_isLast = value;
				NotifyPropertyChanged("IsLast");
				NotifyPropertyChanged("ButtonVisibility");
			}
		}

		public Visibility ButtonVisibility
		{
			get { return IsLast ? Visibility.Visible : Visibility.Collapsed; }
		}

		private void NotifyPropertyChanged(string property)
		{
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
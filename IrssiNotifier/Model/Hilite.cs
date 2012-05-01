using System;

namespace IrssiNotifier.Model
{
	public class Hilite
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
	}
}
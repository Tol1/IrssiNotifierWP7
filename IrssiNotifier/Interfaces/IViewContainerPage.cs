using System.ComponentModel;
using System.Windows;
using Microsoft.Phone.Controls;

namespace IrssiNotifier.Interfaces
{
	internal interface IViewContainerPage : INotifyPropertyChanged
	{
		UIElement View { get; set; }
	}

	public class ViewContainerPage : PhoneApplicationPage, IViewContainerPage
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private UIElement _view;

		public UIElement View
		{
			get { return _view; }
			set
			{
				_view = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("View"));
				}
			}
		}
	}
}

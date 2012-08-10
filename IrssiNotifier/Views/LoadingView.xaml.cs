using System.ComponentModel;
using IrssiNotifier.Resources;

namespace IrssiNotifier.Views
{
	public partial class LoadingView : INotifyPropertyChanged
	{
		public LoadingView()
		{
			InitializeComponent();
		}

		private string _text = AppResources.LoadingChannelText;

		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				OnPropertyChanged("Text");
			}
		}

		private void OnPropertyChanged(string property)
		{
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}

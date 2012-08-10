using System.IO.IsolatedStorage;
using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class HilitePage
	{
		public HilitePage()
		{
			InitializeComponent();
			DataContext = this;
			if (!IsolatedStorageSettings.ApplicationSettings.Contains("userID"))
			{
				View = new InitialView();
			}
			else
			{
				View = new LoadingView();
				SettingsView.GetInstance().Connect(() =>
				                                   	{
				                                   		var view = new HiliteView();
				                                   		ApplicationBar = view.ApplicationBar;
				                                   		View = view;
				                                   	}, View as LoadingView);
			}
		}
	}
}
using System.Windows;
using IrssiNotifier.Interfaces;
using IrssiNotifier.PushNotificationContext;

namespace IrssiNotifier.Views
{
	public partial class ErrorView
	{
		public ErrorView(string titleText, string messageText1, string messageText2)
		{
			TitleText = titleText;
			MessageText1 = messageText1;
			MessageText2 = messageText2;
			InitializeComponent();
			PushContext.Current.IsConnected = false;
		}

		public string TitleText { get; set; }
		public string MessageText1 { get; set; }
		public string MessageText2 { get; set; }

		private void RetryClick(object sender, RoutedEventArgs e)
		{
			var page = App.GetCurrentPage() as ViewContainerPage;
			if(page != null)
			{
				page.View = new LoadingView();
				SettingsView.GetInstance().Connect(() =>
				                                   	{
				                                   		var view = new HiliteView();
				                                   		page.View = view;
				                                   		page.ApplicationBar = view.ApplicationBar;
				                                   	}, page.View as LoadingView);

			}
		}
	}
}

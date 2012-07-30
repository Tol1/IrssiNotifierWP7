using System.Windows;
using IrssiNotifier.Interfaces;

namespace IrssiNotifier.Views
{
	public partial class ConnectionProblemView
	{
		public ConnectionProblemView()
		{
			InitializeComponent();
			PushNotificationContext.PushContext.Current.IsConnected = false;
		}

		private void RetryClick(object sender, RoutedEventArgs e)
		{
			var page = App.GetCurrentPage() as ViewContainerPage;
			if(page != null)
			{
				var view = new HiliteView();
				page.View = view;
				page.ApplicationBar = view.ApplicationBar;
			}
		}
	}
}

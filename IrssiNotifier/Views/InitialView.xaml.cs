
using System;
using System.Windows;
using IrssiNotifier.Resources;

namespace IrssiNotifier.Views
{
	public partial class InitialView
	{
		public InitialView()
		{
			InitializeComponent();
		}

		private void RegisterButtonClick(object sender, RoutedEventArgs e)
		{
			var answer = MessageBox.Show(AppResources.PrivacyStatementText, AppResources.PrivacyStatementTitle,
			                             MessageBoxButton.OKCancel);
			if(answer == MessageBoxResult.OK)
			{
				App.GetCurrentPage().NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
			}
		}
	}
}

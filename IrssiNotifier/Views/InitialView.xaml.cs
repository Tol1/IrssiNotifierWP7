
using System;
using IrssiNotifier.Pages;

namespace IrssiNotifier.Views
{
	public partial class InitialView
	{
		public InitialView(MainPage fromPage)
		{
			InitializeComponent();
			FromPage = fromPage;
		}

		public MainPage FromPage { get; private set; }

		private void RegisterButtonClick(object sender, System.Windows.RoutedEventArgs e)
		{
			FromPage.NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
		}
	}
}

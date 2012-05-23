
using System;
using System.Windows.Controls;

namespace IrssiNotifier.Views
{
	public partial class InitialView
	{
		public InitialView(Page fromPage)
		{
			InitializeComponent();
			FromPage = fromPage;
		}

		public Page FromPage { get; private set; }

		private void RegisterButtonClick(object sender, System.Windows.RoutedEventArgs e)
		{
			FromPage.NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
		}
	}
}

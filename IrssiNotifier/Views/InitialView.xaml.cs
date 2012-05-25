
using System;
using System.Windows;

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
			var answer = MessageBox.Show(
				"Sinut ohjataan Googlen kirjautumissivulle. Tälle sovellukselle ei välity tilitietosi eikä salasanasi. Google välittää tälle sovellukselle ainoastaan tiedon kirjautumisesta.",
				"Tietosuoja", MessageBoxButton.OKCancel);
			if(answer == MessageBoxResult.Yes)
			{
				App.GetCurrentPage().NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
			}
		}
	}
}

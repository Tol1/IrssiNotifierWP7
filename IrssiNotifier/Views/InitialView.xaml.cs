
using System;
using System.Windows;
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

		private void RegisterButtonClick(object sender, RoutedEventArgs e)
		{
			var answer = MessageBox.Show(
				"Sinut ohjataan Googlen kirjautumissivulle. Tälle sovellukselle ei välity tilitietosi eikä salasanasi. Google välittää tälle sovellukselle ainoastaan tiedon kirjautumisesta.",
				"Tietosuoja", MessageBoxButton.OKCancel);
			if(answer == MessageBoxResult.Yes)
			{
				FromPage.NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
			}
		}
	}
}

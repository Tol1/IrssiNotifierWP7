
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
				"Sinut ohjataan Googlen kirjautumissivulle. Google ei välitä tälle sovellukselle salasanaasi eikä muitakaan tilitietojasi. Kirjautumista käytetään ainoastaan käyttäjien yksilöintiin. Google välittää tälle sovellukselle ainoastaan anonyymejä yksilöintitietoja.",
				"Tietosuoja", MessageBoxButton.OKCancel);
			if(answer == MessageBoxResult.OK)
			{
				App.GetCurrentPage().NavigationService.Navigate(new Uri("/Pages/LoginPage.xaml", UriKind.Relative));
			}
		}
	}
}

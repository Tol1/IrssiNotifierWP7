using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class SettingsPage
	{
		public SettingsPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
		{
			contentBorder.Child = null;
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			contentBorder.Child = SettingsView.GetInstance();
		}
	}
}
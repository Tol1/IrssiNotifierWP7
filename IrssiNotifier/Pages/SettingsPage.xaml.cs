using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class SettingsPage
	{
		public SettingsPage()
		{
			InitializeComponent();
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
		{
			if(contentBorder.Child == null || contentBorder.Child is SettingsView)
			{
				base.OnBackKeyPress(e);
			}
			else
			{
				contentBorder.Child = SettingsView.GetInstance();
				e.Cancel = true;
			}
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
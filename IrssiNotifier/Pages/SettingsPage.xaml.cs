using System.ComponentModel;
using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class SettingsPage
	{
		public SettingsPage()
		{
			InitializeComponent();
		}

		protected override void OnBackKeyPress(CancelEventArgs e)
		{
			if(View == null || View is SettingsView)
			{
				base.OnBackKeyPress(e);
			}
			else
			{
				View = SettingsView.GetInstance();
				e.Cancel = true;
			}
		}

		protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
		{
			View = null;
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			View = SettingsView.GetInstance();
		}
	}
}
using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class SettingsPage
	{
		public SettingsPage()
		{
			InitializeComponent();
			contentBorder.Child = new SettingsView();
		}
	}
}
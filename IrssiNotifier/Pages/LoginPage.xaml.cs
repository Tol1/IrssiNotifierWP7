using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class LoginPage
	{
		public LoginPage()
		{
			InitializeComponent();
			contentBorder.Child = new LoginView();
		}
	}
}
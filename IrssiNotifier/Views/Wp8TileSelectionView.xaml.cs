using System.Windows;
using System.Windows.Media;
using IrssiNotifier.Interfaces;
using IrssiNotifier.Resources;

namespace IrssiNotifier.Views
{
	public partial class Wp8TileSelectionView
	{
		private readonly TileType _previousType;
		public Wp8TileSelectionView()
		{
			InitializeComponent();
			_previousType = SettingsView.GetInstance().TileType;
			Loaded += (sender, args) =>
			          	{
			          		if (SettingsView.GetLiveTile() != null)
			          		{
			          			var button = SettingsView.GetInstance().TileType == TileType.Iconic ? iconicButton : flipButton;
			          			button.BorderBrush = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
			          		}
			          	};
		}

		private void IconicButtonClick(object sender, RoutedEventArgs e)
		{
			DoTilePin(TileType.Iconic);
		}

		private void FlipButtonClick(object sender, RoutedEventArgs e)
		{
			DoTilePin(TileType.Flip);
		}

		private void DoTilePin(TileType type)
		{
			if (SettingsView.GetLiveTile() == null || type == _previousType || MessageBox.Show(AppResources.RePinLiveTileText, AppResources.RePinLiveTileTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				SettingsView.GetInstance().TileType = type;
				SettingsView.GetInstance().PinTile(true, _previousType);
				NavigateBack();
			}
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			NavigateBack();
		}

		private static void NavigateBack()
		{
			var settingsPage = App.GetCurrentPage() as IViewContainerPage;
			if (settingsPage != null)
			{
				settingsPage.View = SettingsView.GetInstance();
			}
		}
	}
}

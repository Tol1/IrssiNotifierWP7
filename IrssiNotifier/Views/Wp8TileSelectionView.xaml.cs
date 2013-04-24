using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using IrssiNotifier.Interfaces;

namespace IrssiNotifier.Views
{
	public partial class Wp8TileSelectionView : UserControl
	{
		public Wp8TileSelectionView()
		{
			InitializeComponent();
		}

		private void IconicButtonClick(object sender, RoutedEventArgs e)
		{
			SettingsView.GetInstance().TileType = TileType.Iconic;
			SettingsView.GetInstance().PinTile(true);
		}

		private void FlipButtonClick(object sender, RoutedEventArgs e)
		{
			SettingsView.GetInstance().TileType = TileType.Flip;
			SettingsView.GetInstance().PinTile(true);
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			var settingsPage = App.GetCurrentPage() as IViewContainerPage;
			if (settingsPage != null)
			{
				//SettingsView.GetInstance().NotifyPropertyChanged("IsTileEnabled");
				settingsPage.View = SettingsView.GetInstance();
			}
		}
	}
}

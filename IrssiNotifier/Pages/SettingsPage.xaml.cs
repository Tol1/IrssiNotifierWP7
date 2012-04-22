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
using IrssiNotifier.Views;

namespace IrssiNotifier.Pages
{
	public partial class SettingsPage
	{
		public SettingsPage()
		{
			InitializeComponent();
			contentBorder.Child = new SettingsView(this);
		}
	}
}
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

namespace IrssiNotifier.Views
{
	public partial class InitialView : UserControl
	{
		public InitialView()
		{
			InitializeComponent();
		}

		private DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
		{
			DependencyObject parent = startObject;

			while (parent != null)
			{
				if (type.IsInstanceOfType(parent))
				{
					break;
				}
				else
				{
					parent = VisualTreeHelper.GetParent(parent);
				}
			}
			return parent;
		}
	}
}

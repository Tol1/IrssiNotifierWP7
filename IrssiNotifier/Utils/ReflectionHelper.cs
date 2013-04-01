using System;
using System.Windows.Media;
using Microsoft.Phone.Shell;

namespace IrssiNotifier.Utils
{
	public class ReflectionHelper
	{
		public static void Create(Uri uri, ShellTileData tiledata, bool usewide)
		{
			var shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
			if (shellTileType != null)
			{
				var createmethod = shellTileType.GetMethod("Create", new[] {typeof (Uri), typeof (ShellTileData), typeof (bool)});
				createmethod.Invoke(null, new object[] {uri, tiledata, usewide});
			}
		}

		public static void SetProperty(object instance, string name, object value)
		{
			var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
			setMethod.Invoke(instance, new[] {value});
		}

		public static ShellTileData CreateIconicTileData(string title, Uri iconImageUri, Uri smallIconImageUri, 
			string text1, string text2, string text3, int? count = null, Color? backGroundColor = null)
		{
			var tileDataType = Type.GetType("Microsoft.Phone.Shell.IconicTileData, Microsoft.Phone");
			var iconicTileData = (ShellTileData)tileDataType.GetConstructor(new Type[] { }).Invoke(null);
			SetProperty(iconicTileData, "Title", title);
			SetProperty(iconicTileData, "IconImage", iconImageUri);
			SetProperty(iconicTileData, "SmallIconImage", smallIconImageUri);
			SetProperty(iconicTileData, "WideContent1", text1);
			SetProperty(iconicTileData, "WideContent2", text2);
			SetProperty(iconicTileData, "WideContent3", text3);
			if (count.HasValue)
			{
				SetProperty(iconicTileData, "Count", count);
			}
			if (backGroundColor.HasValue)
			{
				SetProperty(iconicTileData, "BackgroundColor", backGroundColor); //Color.FromArgb(255, 200, 10, 30)
			}
			return iconicTileData;
		}

		public static ShellTileData CreateFlipTileData(string title, string backTitle, string backContent, Uri smallBackgroundImageUri, Uri backgroundImageUri,
			Uri backBackgroundImageUri, int? count, string wideBackContent, Uri wideBackgroundImageUri, Uri wideBackBackgroundImageUri)
		{
			var flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
			var flipTileData = (ShellTileData)flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

			// Set the properties. 
			SetProperty(flipTileData, "Title", title);
			if(count.HasValue)
			{
				SetProperty(flipTileData, "Count", count);
			}
			SetProperty(flipTileData, "BackTitle", backTitle);
			SetProperty(flipTileData, "BackContent", backContent);
			SetProperty(flipTileData, "SmallBackgroundImage", smallBackgroundImageUri);
			SetProperty(flipTileData, "BackgroundImage", backgroundImageUri);
			SetProperty(flipTileData, "BackBackgroundImage", backBackgroundImageUri);
			SetProperty(flipTileData, "WideBackgroundImage", wideBackgroundImageUri);
			SetProperty(flipTileData, "WideBackBackgroundImage", wideBackBackgroundImageUri);
			SetProperty(flipTileData, "WideBackContent", wideBackContent);
			return flipTileData;
		}
	}
}
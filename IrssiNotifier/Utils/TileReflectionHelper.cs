using System;
using System.Windows.Media;
using Microsoft.Phone.Shell;

namespace IrssiNotifier.Utils
{
	public abstract class TileReflectionHelper
	{
		public abstract ShellTileData CreateTileData();
		public void Create(Uri uri, bool usewide)
		{
			var shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
			if (shellTileType != null)
			{
				var createmethod = shellTileType.GetMethod("Create", new[] {typeof (Uri), typeof (ShellTileData), typeof (bool)});
				createmethod.Invoke(null, new object[] {uri, CreateTileData(), usewide});
			}
		}

		public static void SetProperty(object instance, string name, object value)
		{
			var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
			setMethod.Invoke(instance, new[] {value});
		}
	}
	internal class FlipTileDataReflectionHelper : TileReflectionHelper
	{
		public string Title { get; set; }
		public int? Count { get; set; }
		public string BackContent { get; set; }
		public string BackTitle{ get; set; }
		public Uri SmallBackgroundImageUri{ get; set; }
		public Uri BackgroundImageUri{ get; set; }
		public Uri BackBackgroundImageUri{ get; set; }
		public string WideBackContent{ get; set; }
		public Uri WideBackgroundImageUri{ get; set; }
		public Uri WideBackBackgroundImageUri { get; set; }
		public override ShellTileData CreateTileData()
		{
			var flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
			if (flipTileDataType != null)
			{
				var constructorInfo = flipTileDataType.GetConstructor(new Type[] {});
				if (constructorInfo != null)
				{
					var flipTileData = (ShellTileData)constructorInfo.Invoke(null);

					SetProperty(flipTileData, "Title", Title);
					if (Count.HasValue)
					{
						SetProperty(flipTileData, "Count", Count);
					}
					SetProperty(flipTileData, "BackTitle", BackTitle);
					SetProperty(flipTileData, "BackContent", BackContent);
					SetProperty(flipTileData, "SmallBackgroundImage", SmallBackgroundImageUri);
					SetProperty(flipTileData, "BackgroundImage", BackgroundImageUri);
					SetProperty(flipTileData, "BackBackgroundImage", BackBackgroundImageUri);
					SetProperty(flipTileData, "WideBackgroundImage", WideBackgroundImageUri);
					SetProperty(flipTileData, "WideBackBackgroundImage", WideBackBackgroundImageUri);
					SetProperty(flipTileData, "WideBackContent", WideBackContent);
					return flipTileData;
				}
			}
			return null;
		}
	}
	internal class IconicTileDataReflectionHelper : TileReflectionHelper
	{
		public string Title { get; set; }
		public int? Count { get; set; }
		public Uri IconImageUri { get; set; }
		public Uri SmallIconImageUri { get; set; }
		public string WideContent1 { get; set; }
		public string WideContent2 { get; set; }
		public string WideContent3 { get; set; }
		public Color? BackGroundColor { get; set; }
		public override ShellTileData CreateTileData()
		{
			var tileDataType = Type.GetType("Microsoft.Phone.Shell.IconicTileData, Microsoft.Phone");
			if (tileDataType != null)
			{
				var constructorInfo = tileDataType.GetConstructor(new Type[] {});
				if (constructorInfo != null)
				{
					var iconicTileData = (ShellTileData)constructorInfo.Invoke(null);
					SetProperty(iconicTileData, "Title", Title);
					SetProperty(iconicTileData, "IconImage", IconImageUri);
					SetProperty(iconicTileData, "SmallIconImage", SmallIconImageUri);
					SetProperty(iconicTileData, "WideContent1", WideContent1);
					SetProperty(iconicTileData, "WideContent2", WideContent2);
					SetProperty(iconicTileData, "WideContent3", WideContent3);
					if (Count.HasValue)
					{
						SetProperty(iconicTileData, "Count", Count);
					}
					if (BackGroundColor.HasValue)
					{
						SetProperty(iconicTileData, "BackgroundColor", BackGroundColor); //Color.FromArgb(255, 200, 10, 30)
					}
					return iconicTileData;
				}
			}
			return null;
		}

		public static ShellTileData ClearIconicTileCount()
		{
			return new IconicTileDataReflectionHelper {Count = 0}.CreateTileData();
		}
	}
}
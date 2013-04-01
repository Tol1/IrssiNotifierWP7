using System;
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
	}
}
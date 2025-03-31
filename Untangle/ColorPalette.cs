using System.Windows.Media;
using System.Collections.Generic;

namespace Untangle
{
	public static class ColorPalette
	{
		public static List<Brush> Default = new List<Brush>()
		{
			new SolidColorBrush(Color.FromRgb(255, 174, 201)),
			new SolidColorBrush(Color.FromRgb(255, 0, 255)),
			new SolidColorBrush(Color.FromRgb(255, 0, 100)),
			new SolidColorBrush(Color.FromRgb(255, 0, 0)),
			new SolidColorBrush(Color.FromRgb(144, 0, 0)),
			new SolidColorBrush(Color.FromRgb(222, 113, 0)),
			new SolidColorBrush(Color.FromRgb(255, 128, 0)),
			new SolidColorBrush(Color.FromRgb(255, 191, 0)),
			new SolidColorBrush(Color.FromRgb(255, 255, 191)),
			new SolidColorBrush(Color.FromRgb(255, 255, 0)),
			new SolidColorBrush(Color.FromRgb(223, 191, 64)),
			new SolidColorBrush(Color.FromRgb(191, 255, 0)),
			new SolidColorBrush(Color.FromRgb(201, 255, 201)),
			new SolidColorBrush(Color.FromRgb(0, 255, 0)),
			new SolidColorBrush(Color.FromRgb(0, 161, 0)),
			new SolidColorBrush(Color.FromRgb(100, 160, 160)),
			new SolidColorBrush(Color.FromRgb(197, 222, 250)),
			new SolidColorBrush(Color.FromRgb(80, 156, 239)),
			new SolidColorBrush(Color.FromRgb(0, 0, 255)),
			new SolidColorBrush(Color.FromRgb(126, 0, 251)),
			new SolidColorBrush(Color.FromRgb(128, 128, 251)),
			new SolidColorBrush(Color.FromRgb(223,191, 255)),
			new SolidColorBrush(Color.FromRgb(255, 255, 255)),
			new SolidColorBrush(Color.FromRgb(191,191,191)),
			new SolidColorBrush(Color.FromRgb(128, 128, 128)),
			new SolidColorBrush(Color.FromRgb(0, 0, 0))
		};

		public static List<Brush> PrimaryColors = new List<Brush>()
		{
			new SolidColorBrush(Colors.Red),
			new SolidColorBrush(Colors.Green),
			new SolidColorBrush(Colors.Blue),
			new SolidColorBrush(Colors.Gold),
			new SolidColorBrush(Colors.Magenta),
			new SolidColorBrush(Colors.Cyan),
			new SolidColorBrush(Colors.Brown),
			new SolidColorBrush(Colors.Orange),
			new SolidColorBrush(Colors.Tan)
		};
	}
}

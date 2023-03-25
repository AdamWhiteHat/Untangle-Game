using System.Windows.Media;
using System.Collections.Generic;

namespace Untangle
{
	public static class ColorPalette
	{
		public static List<Brush> Default = new List<Brush>()
		{
			new SolidColorBrush(Color.FromRgb(253, 3, 1)),
			new SolidColorBrush(Color.FromRgb(252, 125, 6)),
			new SolidColorBrush(Color.FromRgb(252,223,18)),
			new SolidColorBrush(Color.FromRgb(181, 230, 29)),
			new SolidColorBrush(Color.FromRgb(31, 252, 119)),
			new SolidColorBrush(Color.FromRgb(0, 128, 0)),
			new SolidColorBrush(Color.FromRgb(0, 0, 255)),
			new SolidColorBrush(Color.FromRgb(0, 128, 255)),
			new SolidColorBrush(Color.FromRgb(0, 128, 128)),
			new SolidColorBrush(Color.FromRgb(153, 217, 234)),
			new SolidColorBrush(Color.FromRgb(0, 255, 255)),
			new SolidColorBrush(Color.FromRgb(126, 0, 251)),
			new SolidColorBrush(Color.FromRgb(255,36,209)),
			new SolidColorBrush(Color.FromRgb(255,151,168)),
			new SolidColorBrush(Color.FromRgb(165, 102, 48)),
			new SolidColorBrush(Color.FromRgb(0, 0, 0)),
			new SolidColorBrush(Color.FromRgb(128, 128, 128)),
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

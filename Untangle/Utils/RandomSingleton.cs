using System;

namespace Untangle
{
	public static class RandomSingleton
	{
		private static Random _random;

		static RandomSingleton()
		{
			_random = new Random();
			int temp = 0;
			int counter = 30;
			while (counter-- > 0)
			{
				temp = _random.Next();
			}
		}

		public static bool NextBool()
		{
			return (_random.Next(0, 2) == 1);
		}

		public static int Next(int maxValue)
		{
			return _random.Next(maxValue);
		}

		public static int Next(int minValue, int maxValue)
		{
			return _random.Next(minValue, maxValue);
		}
	}
}

/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

using System;
using System.Windows;
using Untangle.Generation;
using Untangle.Resources;

namespace Untangle.Core
{
	/// <summary>
	/// A view model class for a single game of Untangle.
	/// </summary>
	public class Game : ViewModelBase
	{
		/// <summary>
		/// The current game level's number.
		/// </summary>
		public int LevelNumber
		{
			get { return _levelNumber; }
			set
			{
				if (_levelNumber == value)
				{
					return;
				}
				_levelNumber = value;
				RaisePropertyChanged();
			}
		}
		private int _levelNumber;

		/// <summary>
		/// The current game level.
		/// </summary>
		public GameLevel Level
		{
			get { return _level; }
			set
			{
				if (_level == value)
				{
					return;
				}

				_level = value;
				RaisePropertyChanged();
			}
		}
		private GameLevel _level;

		private Game()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="Game"/> instance with the specified start level number
		/// and generates the start level.
		/// </summary>
		/// <param name="startLevelNumber">The number of the start level of the Untangle game.
		/// </param>
		public Game(int startLevelNumber)
		{
			LevelNumber = startLevelNumber;
			var levelGenerator = new LevelGenerator(1 + LevelNumber, 2 + LevelNumber, 4);

			Level = levelGenerator.GenerateLevel();
		}

		/// <summary>
		/// Initializes a new <see cref="Game"/> instance with the specified <see cref="Level"/>
		/// and <see cref="LevelNumber"/>.
		/// </summary>
		/// <param name="level">The current game level.</param>
		/// <param name="levelNumber">The current game level's number.</param>
		public Game(GameLevel level, int levelNumber)
		{
			Level = level;
			LevelNumber = levelNumber;
		}

		protected override Freezable CreateInstanceCore()
		{
			return new Game();
		}
	}
}
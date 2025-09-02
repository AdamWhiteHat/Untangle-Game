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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Untangle.Core.Achievements;
using Untangle.Generation;
using Untangle.Resources;
using Untangle.Saves;
using Untangle.Utils;

namespace Untangle.Core
{
	/// <summary>
	/// A view model class for the main UI of the Untangle game application.
	/// </summary>
	public class MainViewModel : ViewModelBase
	{

		#region Properties and Fields

		/// <summary>
		/// Size of the game board. For restricting randomizing vertex positions.
		/// </summary>
		public Size GameBoardSize
		{
			get
			{
				return _gameBoardSize;
			}
			set
			{
				if (value != _gameBoardSize)
				{
					_gameBoardSize = value;
					RaisePropertyChanged(nameof(GameBoardSize));

					Point center = new Point(_gameBoardSize.Width / 2, _gameBoardSize.Height / 2);
					GameBoardCenter = center;
				}
			}
		}
		private Size _gameBoardSize = Size.Empty;

		public Point GameBoardCenter
		{
			get
			{
				return _gameBoardCenter;
			}
			set
			{
				if (value != _gameBoardCenter)
				{
					_gameBoardCenter = value;
					RaisePropertyChanged(nameof(GameBoardCenter));
				}
			}
		}
		private Point _gameBoardCenter = new Point(0, 0);

		/// <summary>
		/// Indicates if the game state has changed since the last save.
		/// </summary>
		public bool IsDirty
		{
			get { return _isDirty; }
			private set
			{
				if (_isDirty != value)
				{
					_isDirty = value;
					RaisePropertyChanged();
				}
			}
		}
		private bool _isDirty;

		public bool IsEditing { get { return Game.IsEditing; } }

		/// <summary>
		/// The application's language manager.
		/// </summary>
		public LanguageManager LanguageManager
		{
			get { return _languageManager; }
		}
		private readonly LanguageManager _languageManager;

		/// <summary>
		/// The current game of Untangle loaded in the application.
		/// </summary>
		public GameState Game
		{
			get { return _gamestate; }
			set
			{
				if (_gamestate == value)
				{
					return;
				}
				if (_gamestate != null)
				{
					_gamestate.PropertyChanged -= Game_PropertyChanged;
					_gamestate.LevelSolved -= Level_LevelSolved;
				}

				_gamestate = value;
				if (_gamestate != null)
				{
					_gamestate.PropertyChanged += Game_PropertyChanged;
					_gamestate.LevelSolved += Level_LevelSolved;
				}
				RaisePropertyChanged(nameof(MainViewModel.Game));
				RaisePropertyChanged(nameof(MainViewModel.Title));
			}
		}
		private GameState _gamestate;

		/// <summary>
		/// The best score information for the currently loaded game board.
		/// </summary>
		public Score BestScore
		{
			get { return _bestScore; }
			set
			{
				if (value != _bestScore)
				{
					_bestScore = value;
					RaisePropertyChanged(nameof(MainViewModel.BestScore));
				}
			}
		}
		private Score _bestScore;

		public int HighestLevelSolved
		{
			get { return _highestLevelSolved; }
			set
			{
				if (value != _highestLevelSolved)
				{
					_highestLevelSolved = value;
					RaisePropertyChanged();
				}
			}
		}
		private int _highestLevelSolved;

		/// <summary>
		/// The application's current title text.
		/// </summary>
		public string Title
		{
			get
			{
				if (_title == null)
				{
					return string.Format(Resources.MainWindow.WindowTitleFormat, $"Level {Game.LevelNumber}");
				}
				return _title;
			}
			set
			{
				string temp = string.Format(Resources.MainWindow.WindowTitleFormat, value);
				if (_title != temp)
				{
					_title = temp;
					RaisePropertyChanged();
				}
			}
		}
		private string _title = null;

		public decimal ScaleZoom
		{
			get { return _scaleZoom; }
			set
			{
				if (_scaleZoom != value)
				{
					_scaleZoom = value;
					RaisePropertyChanged();
				}
			}
		}
		private decimal _scaleZoom = 1.00m;

		#endregion

		/// <summary>
		/// Initializes a new <see cref="MainViewModel"/> instance.
		/// </summary>
		public MainViewModel()
		{
			_languageManager = new LanguageManager();
			_languageManager.PropertyChanged += LanguageManager_PropertyChanged;
			_gamestate = new GameState();
			IsDirty = false;
		}

		public void SetBoardSize(Size size)
		{
			GameBoardSize = size;
		}



		private Size CalculateGameBoardSize()
		{
			double maxX = Game.Graph.Vertices.Max(v => v.X);
			double maxY = Game.Graph.Vertices.Max(v => v.Y);
			return new Size(maxX, maxY);
		}

		public void AutoSolve()
		{
			AutoSolver.Solve(Game.Graph, GameBoardSize);
		}

		#region New/Load/Save/Edit

		/// <summary>
		/// The initial first game to load after the application loads.
		/// </summary>
		public void InitializeGame(Size size)
		{
			SetBoardSize(size);
			Game = GameState.Create(size, 1);
		}

		/// <summary>
		/// Starts a new game of Untangle from scratch.
		/// </summary>
		/// <remarks>
		/// <para>The user will be prompted to save his current game progress, if needed.</para>
		/// </remarks>
		public void NewGame(Size gameBoardSize)
		{
			GameBoardSize = gameBoardSize;

			int levelNumber = HighestLevelSolved + 1;
			NewLevelParameters newLevelParametersWindows = new NewLevelParameters(1 + levelNumber, 2 + levelNumber, 4, 2);

			bool? dialogResult = newLevelParametersWindows.ShowDialog();
			if (dialogResult.HasValue && dialogResult.Value == true)
			{
				if (newLevelParametersWindows.IsGeneratedTypeSelected)
				{
					int columns = newLevelParametersWindows.Columns;
					int rows = newLevelParametersWindows.Rows;
					int minEdges = newLevelParametersWindows.MinEdges;
					int maxEdges = newLevelParametersWindows.MaxEdges;

					var levelGenerator = new LevelGenerator(rows, columns, maxEdges, minEdges);
					Game = levelGenerator.GenerateLevel(GameBoardSize);
					Game.LevelNumber = levelNumber;
				}
				else if (newLevelParametersWindows.IsGraphNameTypeSelected)
				{
					int graphIndex = newLevelParametersWindows.GraphIndex;

					Generation.Vertex[] vertices = CriticalNonplanarGraphs.GenerateLevel(graphIndex);

					Game = GameState.Create(GameBoardSize, vertices);
					Game.LevelNumber = levelNumber;
				}
				IsDirty = false;
			}
		}

		/// <summary>
		/// Loads a saved game of Untangle from a file chosen by the user.
		/// </summary>
		/// <remarks>
		/// <para>The user will be prompted to save his current game progress, if needed.</para>
		/// </remarks>
		public bool LoadGame(string fileName)
		{
			try
			{
				GameState game;
				if (!SaveHelper.LoadGame(fileName, out game))
				{
					return false;
				}
				Game = game;

				if (GameBoardSize == Size.Empty)
				{
					GameBoardSize = CalculateGameBoardSize();
				}

				IsDirty = false;

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, MessageCaptions.LoadGameError, MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return false;
		}

		/// <summary>
		/// Saves the current game of Untangle to a file chosen by the user.
		/// </summary>
		/// <param name="showSuccessMessage">Specifies whether a message should be displayed to the
		/// user when the game has been saved successfully.</param>
		/// <returns><see langword="true"/> if the game has been saved successfully.</returns>
		public bool SaveGame(string fileName, bool showSuccessMessage)
		{
			try
			{
				if (SaveHelper.SaveGame(Game, fileName))
				{
					if (showSuccessMessage)
					{
						MessageBox.Show(Messages.SaveGameSuccess, MessageCaptions.SaveGameSuccess);
					}
					IsDirty = false;
					return true;
				}
				;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, MessageCaptions.SaveGameError, MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return false;
		}

		#endregion

		#region Solved/Quit methods

		/// <summary>
		/// Handles the <see cref="GameState.LevelSolved"/> event of the current game level.
		/// </summary>
		private void Level_LevelSolved(object sender, EventArgs e)
		{
			HighestLevelSolved = Math.Max(HighestLevelSolved, Game.LevelNumber);

			MessageBoxResult mboxResult =
				MessageBox.Show(
					string.Format(Messages.LevelSolved, Game.LevelNumber, Game.MoveCount, Game.SecondsElapsed)
						+ Environment.NewLine
						+ "Begin next level?",
					Application.Current.MainWindow.Title,
					MessageBoxButton.YesNo);

			if (mboxResult == MessageBoxResult.Yes)
			{
				if (GameBoardSize == Size.Empty)
				{
					GameBoardSize = CalculateGameBoardSize();
				}

				IsDirty = false;
				NewGame(GameBoardSize);
			}
		}

		#endregion

		#region Mouse Events

		/// <summary>
		/// Handles the event of the mouse being moved over the game field.
		/// </summary>
		/// <param name="position">The new position of the mouse on the game field.</param>
		/// <param name="buttonPressed">Specifies whether the left mouse button is currently
		/// pressed.</param>
		public void HandleMouseMove(Point position, bool buttonPressed)
		{
			if (Game.IsDragging)
			{
				if (buttonPressed)
				{
					Game.ContinueDrag(position);
				}
				else
				{
					IsDirty = true;
					Game.FinishDrag();
				}
			}
		}

		/// <summary>
		/// Handles the event of the left mouse button being released over the game field.
		/// </summary>
		public void HandleMouseUp(Point position, bool keyModifier)
		{
			if (Game.IsDragging)
			{
				IsDirty = true;
				Game.FinishDrag();
			}
			else if (Game.IsEditing && keyModifier)
			{
				Game.Edit_CreateVertex(position);
			}
		}

		/// <summary>
		/// Handles the event of the mouse cursor entering the area of a vertex on the game field.
		/// </summary>
		/// <param name="vertex">The vertex whose area has been entered by the mouse cursor.
		/// </param>
		public void HandleVertexMouseEnter(Vertex vertex)
		{
			Game.SetVertexUnderMouse(vertex);
		}

		/// <summary>
		/// Handles the event of the mouse cursor leaving the area of a vertex on the game field.
		/// </summary>
		/// <param name="vertex">The vertex whose area has been left by the mouse cursor.</param>
		public void HandleVertexMouseLeave()
		{
			Game.SetVertexUnderMouse(null);
		}

		/// <summary>
		/// Handles the event of the left mouse button being pressed over a vertex on the game
		/// field.
		/// </summary>
		/// <param name="vertex">The vertex over which the left mouse button has been pressed.
		/// </param>
		public void HandleVertexMouseDown(Vertex vertex, MouseButton button, ModifierKeys keyModifier)
		{
			if (button == MouseButton.Left)
			{
				if (Game.IsEditing && keyModifier == ModifierKeys.Control)
				{
					Game.Edit_JoinVertexSelect(vertex);
				}
				else
				{
					Game.StartDrag(vertex);
				}
			}
			else if (button == MouseButton.Right)
			{
				if (Game.IsEditing && keyModifier == ModifierKeys.Alt)
				{
					Game.Edit_DeleteVertex(vertex);
				}
			}
		}

		#endregion

		#region Undo/Redo


		#endregion

		#region Raise Event Methods

		/// <summary>
		/// Handles the <see cref="ViewModelBase.PropertyChanged"/> event of the application's
		/// language manager.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The event's arguments.</param>
		/// <remarks>
		/// <para>If the <see cref="Core.LanguageManager.SelectedLanguage"/> property of the
		/// application's language manager has changed, the application's title should also be
		/// invalidated.</para>
		/// </remarks>
		private void LanguageManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == LanguageManager.SelectedLanguagePropertyName)
			{
				RaisePropertyChanged(nameof(MainViewModel.Title));
			}
		}

		/// <summary>
		/// Handles the <see cref="ViewModelBase.PropertyChanged"/> event of the current game of
		/// Untangle loaded in the application.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The event's arguments.</param>
		/// <remarks>
		/// <para>If the <see cref="Core.Game.LevelNumber"/> property of the current game of
		/// Untangle loaded in the application has changed, the application's title should also be
		/// invalidated.</para>
		/// </remarks>
		private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Game.LevelNumber))
			{
				RaisePropertyChanged(nameof(MainViewModel.Title));
			}
			RaisePropertyChanged(e);
		}

		#endregion

		protected override Freezable CreateInstanceCore()
		{
			return new MainViewModel();
		}

	}
}

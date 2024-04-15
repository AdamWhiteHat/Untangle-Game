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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using Untangle.Generation;
using Untangle.Resources;
using Untangle.Saves;
using System.Collections.Generic;

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
		internal Size _gameBoardSize = Size.Empty;

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

		public bool IsEditing { get { return Game.Level.IsEditing; } }

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
		public Game Game
		{
			get { return _game; }
			set
			{
				if (_game == value)
				{
					return;
				}
				if (_game != null)
				{
					_game.PropertyChanged -= Game_PropertyChanged;
					if (_game.Level != null)
					{
						_game.Level.LevelSolved -= Level_LevelSolved; ;
					}
				}

				_game = value;
				if (_game != null)
				{
					_game.PropertyChanged += Game_PropertyChanged;
					if (_game.Level != null)
					{
						_game.Level.LevelSolved += Level_LevelSolved; ;
					}
				}
				RaisePropertyChanged(nameof(MainViewModel.Game));
				RaisePropertyChanged(nameof(MainViewModel.Title));
			}
		}
		private Game _game;

		/// <summary>
		/// The application's current title text.
		/// </summary>
		public string Title
		{
			get
			{
				return string.Format(Resources.MainWindow.WindowTitleFormat, Game.LevelNumber);
			}
		}

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
			IsDirty = false;
			_languageManager = new LanguageManager();

			Game = new Game(1);

			_languageManager.PropertyChanged += LanguageManager_PropertyChanged;
		}

		public void SetBoardSize(Size size)
		{
			_gameBoardSize = size;
		}

		private Size CalculateGameBoardSize()
		{
			double maxX = Game.Level.GameGraph.Vertices.Max(v => v.X);
			double maxY = Game.Level.GameGraph.Vertices.Max(v => v.Y);
			return new Size(maxX, maxY);
		}

		public void AutoSolve()
		{
			AutoSolver.Solve(Game.Level.GameGraph, _gameBoardSize);
		}

		#region New/Load/Save/Edit

		/// <summary>
		/// The initial first game to load after the application loads.
		/// </summary>
		public void InitialGame()
		{
			var levelGenerator = new LevelGenerator(2, 3, 4, 2);
			var level = levelGenerator.GenerateLevel(_gameBoardSize);
			Game = new Game(level, 1);
		}

		/// <summary>
		/// Starts a new game of Untangle from scratch.
		/// </summary>
		/// <remarks>
		/// <para>The user will be prompted to save his current game progress, if needed.</para>
		/// </remarks>
		public void NewGame(Size gameBoardSize)
		{
			if (!ConfirmQuit())
			{
				return;
			}

			_gameBoardSize = gameBoardSize;

			NewLevelParameters newLevelParametersWindows = new NewLevelParameters(1 + Game.LevelNumber, 2 + Game.LevelNumber, 2, 4);

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
					var level = levelGenerator.GenerateLevel(_gameBoardSize);
					Game = new Game(level, Game.LevelNumber);
				}
				else if (newLevelParametersWindows.IsGraphNameTypeSelected)
				{
					int graphIndex = newLevelParametersWindows.GraphIndex;

					Generation.Vertex[] vertices = CriticalNonplanarGraphs.GenerateLevel(graphIndex);

					GameLevel gameLevel = GameLevel.Create(_gameBoardSize, vertices);
					Game = new Game(gameLevel, graphIndex);
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
			if (!ConfirmQuit())
			{
				return false;
			}

			Game.Level.ExitEditMode();

			try
			{
				Game game;
				if (!SaveHelper.LoadGame(fileName, out game))
				{
					return false;
				}

				Game = game;

				if (_gameBoardSize == Size.Empty)
				{
					_gameBoardSize = CalculateGameBoardSize();
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
				Game.Level.ExitEditMode();

				if (SaveHelper.SaveGame(Game, fileName))
				{
					if (showSuccessMessage)
					{
						MessageBox.Show(Messages.SaveGameSuccess, MessageCaptions.SaveGameSuccess);
					}
					IsDirty = false;
					return true;
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, MessageCaptions.SaveGameError, MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return false;
		}

		/// <summary>
		/// Toggles the level editor.
		/// </summary>
		public void ToggleLevelEditor()
		{
			if (!Game.Level.IsEditing)
			{
				if (!ConfirmQuit())
				{
					return;
				}
				Game.Level.EnterEditMode();
			}
			else
			{
				Game.Level.ExitEditMode();
			}
		}

		#endregion

		#region Solved/Quit methods

		/// <summary>
		/// Handles the <see cref="GameLevel.LevelSolved"/> event of the current game level.
		/// </summary>
		private void Level_LevelSolved(object sender, EventArgs e)
		{
			MessageBoxResult mboxResult =
				MessageBox.Show(
					string.Format(Messages.LevelSolved, Game.LevelNumber, Game.Level.MoveCount)
						+ Environment.NewLine
						+ "Begin next level?",
					Application.Current.MainWindow.Title,
					MessageBoxButton.YesNo);

			if (mboxResult == MessageBoxResult.Yes)
			{
				if (_gameBoardSize == Size.Empty)
				{
					_gameBoardSize = CalculateGameBoardSize();
				}

				IsDirty = false;
				Game.LevelNumber++;
				NewGame(_gameBoardSize);
			}
		}

		/// <summary>
		/// Displays a save game prompt when the user is about to lose his current game progress, if needed.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the current operationa should proceed, possibly losing game state.
		/// This happens either when there is no unsaved game state (IsDirty flag is false), or the user has indicated they do not care.
		/// <see langword="true"/> if the current operation should abort, preserving the current game state.
		/// </returns>
		/// <remarks>
		/// <para>The save game prompt is not needed if the user has not dragged any vertices since
		/// the game was started or last saved.</para>
		/// </remarks>
		public bool ConfirmQuit()
		{
			if (!IsDirty)
			{
				return true;
			}

			MessageBoxResult result =
				MessageBox.Show(
					Messages.SaveGamePrompt,
					MessageCaptions.SaveGamePrompt,
					MessageBoxButton.YesNo,
					MessageBoxImage.Warning);

			return (result == MessageBoxResult.Yes);
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
			if (Game.Level.IsDragging)
			{
				if (buttonPressed)
				{
					Game.Level.ContinueDrag(position);
				}
				else
				{
					IsDirty = true;
					Game.Level.FinishDrag();
				}
			}
		}

		/// <summary>
		/// Handles the event of the left mouse button being released over the game field.
		/// </summary>
		public void HandleMouseUp(Point position, bool keyModifier)
		{
			if (Game.Level.IsDragging)
			{
				IsDirty = true;
				Game.Level.FinishDrag();
			}
			else if (Game.Level.IsEditing && keyModifier)
			{
				Game.Level.Edit_CreateVertex(position);
			}
		}

		/// <summary>
		/// Handles the event of the mouse cursor entering the area of a vertex on the game field.
		/// </summary>
		/// <param name="vertex">The vertex whose area has been entered by the mouse cursor.
		/// </param>
		public void HandleVertexMouseEnter(Vertex vertex)
		{
			Game.Level.SetVertexUnderMouse(vertex);
		}

		/// <summary>
		/// Handles the event of the mouse cursor leaving the area of a vertex on the game field.
		/// </summary>
		/// <param name="vertex">The vertex whose area has been left by the mouse cursor.</param>
		public void HandleVertexMouseLeave()
		{
			Game.Level.SetVertexUnderMouse(null);
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
				if (Game.Level.IsEditing && keyModifier == ModifierKeys.Control)
				{
					Game.Level.Edit_JoinVertexSelect(vertex);
				}
				else
				{
					Game.Level.StartDrag(vertex);
				}
			}
			else if (button == MouseButton.Right)
			{
				if (Game.Level.IsEditing && keyModifier == ModifierKeys.Alt)
				{
					Game.Level.Edit_DeleteVertex(vertex);
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

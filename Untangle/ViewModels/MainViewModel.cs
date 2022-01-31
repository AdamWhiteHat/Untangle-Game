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

namespace Untangle.ViewModels
{
	/// <summary>
	/// A view model class for the main UI of the Untangle game application.
	/// </summary>
	public class MainViewModel : ViewModelBase
	{

		#region Properties and Fields

		/// <summary>
		/// Property name constant for the current game of Untangle loaded in the application.
		/// </summary>
		public const string GamePropertyName = "Game";

		/// <summary>
		/// Property name constant for the application's current title text.
		/// </summary>
		public const string TitlePropertyName = "Title";

		/// <summary>
		/// A command for displaying the About box of the application.
		/// </summary>
		public static ICommand AboutCommand = new RoutedCommand();

		/// <summary>
		/// A command for changing the selected language of the application.
		/// </summary>
		public static ICommand LanguageCommand = new RoutedCommand();

		/// <summary>
		/// Size of the game board. For restricting randomizing vertex positions.
		/// </summary>
		private Size _gameBoardSize = Size.Empty;

		/// <summary>
		/// Specifies whether a save game prompt should be displayed if the user is about to lose
		/// his current game progress.
		/// </summary>
		/// <remarks>
		/// <para>The save game prompt is not needed if the user has not dragged any vertices since
		/// the game was started or last saved.</para>
		/// </remarks>
		private bool _needSaveGamePrompt;

		public bool IsEditing { get; private set; } = false;

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
				OnPropertyChanged(GamePropertyName);
				OnPropertyChanged(TitlePropertyName);
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
					OnPropertyChanged();
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
			Game = new Game(1);
			_needSaveGamePrompt = false;
			IsEditing = false;

			_languageManager.PropertyChanged += LanguageManager_PropertyChanged;
		}

		public void SetBoardSize(Size size)
		{
			_gameBoardSize = size;
		}

		private Size CalculateGameBoardSize()
		{
			double maxX = Game.Level.GameGraph.Vertices.Max(v => v.Position.X);
			double maxY = Game.Level.GameGraph.Vertices.Max(v => v.Position.Y);
			return new Size(maxX, maxY);
		}

		#region New/Load/Save/Edit

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
				if (newLevelParametersWindows.GenerateIsSelected)
				{
					int columns = newLevelParametersWindows.Columns;
					int rows = newLevelParametersWindows.Rows;
					int minEdges = newLevelParametersWindows.MinEdges;
					int maxEdges = newLevelParametersWindows.MaxEdges;

					var levelGenerator = new LevelGenerator(rows, columns, maxEdges, minEdges);
					var level = levelGenerator.GenerateLevel(_gameBoardSize);
					Game = new Game(level, Game.LevelNumber);
				}
				else if (newLevelParametersWindows.GraphNameIsSelected)
				{
					int graphIndex = newLevelParametersWindows.GraphIndex;

					Generation.Vertex[] vertices = CriticalNonplanarGraphs.GenerateLevel(graphIndex);

					GameLevel gameLevel = GameLevel.Create(_gameBoardSize, vertices);
					Game = new Game(gameLevel, graphIndex);
				}
				_needSaveGamePrompt = false;
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
			ExitLevelEditor();

			if (!ConfirmQuit())
			{
				return false;
			}

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

				_needSaveGamePrompt = false;

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
				ExitLevelEditor();

				if (SaveHelper.SaveGame(Game, fileName))
				{
					if (showSuccessMessage)
					{
						MessageBox.Show(Messages.SaveGameSuccess, MessageCaptions.SaveGameSuccess);
					}
					_needSaveGamePrompt = false;
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
			if (!IsEditing)
			{
				if (!ConfirmQuit())
				{
					return;
				}

				Game.Level.EnterEditMode();
				IsEditing = true;
			}
			else
			{
				ExitLevelEditor();
			}
		}

		/// <summary>
		/// Exits the level editor.
		/// </summary>
		private void ExitLevelEditor()
		{
			if (IsEditing)
			{
				IsEditing = false;
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

				_needSaveGamePrompt = false;
				Game.LevelNumber++;
				NewGame(_gameBoardSize);
			}
		}

		/// <summary>
		/// Displays a save game prompt when the user is about to lose his current game progress,
		/// if needed.
		/// </summary>
		/// <returns><see langword="true"/> if the user has chosen to proceed with the operation
		/// after saving his current game or deliberately choosing not to save it.</returns>
		/// <remarks>
		/// <para>The save game prompt is not needed if the user has not dragged any vertices since
		/// the game was started or last saved.</para>
		/// </remarks>
		public bool ConfirmQuit()
		{
			if (!_needSaveGamePrompt)
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
				_needSaveGamePrompt = true;
				if (buttonPressed)
				{
					Game.Level.ContinueDrag(position);
				}
				else
				{
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
				Game.Level.FinishDrag();
			}
			else if (IsEditing && keyModifier)
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
		public void HandleVertexMouseLeave(Vertex vertex)
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
				if (IsEditing && keyModifier == ModifierKeys.Control)
				{
					Game.Level.Edit_JoinVertexSelect(vertex);
				}
				else if (keyModifier.HasFlag(ModifierKeys.Control) && keyModifier.HasFlag(ModifierKeys.Shift))
				{
					Game.Level.ResetVertexToStartingPosition(vertex);
				}
				else
				{
					Game.Level.StartDrag(vertex);
				}
			}
			else if (button == MouseButton.Right)
			{
				if (IsEditing && keyModifier == ModifierKeys.Alt)
				{
					Game.Level.Edit_DeleteVertex(vertex);
				}
			}
		}

		#endregion

		#region Raise Event Methods

		/// <summary>
		/// Handles the <see cref="ViewModelBase.PropertyChanged"/> event of the application's
		/// language manager.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The event's arguments.</param>
		/// <remarks>
		/// <para>If the <see cref="ViewModels.LanguageManager.SelectedLanguage"/> property of the
		/// application's language manager has changed, the application's title should also be
		/// invalidated.</para>
		/// </remarks>
		private void LanguageManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == LanguageManager.SelectedLanguagePropertyName)
			{
				OnPropertyChanged(TitlePropertyName);
			}
		}

		/// <summary>
		/// Handles the <see cref="ViewModelBase.PropertyChanged"/> event of the current game of
		/// Untangle loaded in the application.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The event's arguments.</param>
		/// <remarks>
		/// <para>If the <see cref="ViewModels.Game.LevelNumber"/> property of the current game of
		/// Untangle loaded in the application has changed, the application's title should also be
		/// invalidated.</para>
		/// </remarks>
		private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Game.LevelNumber))
			{
				OnPropertyChanged(TitlePropertyName);
			}
		}

		#endregion

	}
}

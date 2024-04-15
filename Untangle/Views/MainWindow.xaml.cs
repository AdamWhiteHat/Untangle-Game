/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Original Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 * 
 * Project forked and heavily modified by Adam White
 * https://github.com/AdamWhiteHat/Untangle-Game
 * 
 *   
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Untangle.Generation;
using Untangle.Core;
using XAMLMarkupExtensions.Base;
using Vertex = Untangle.Core.Vertex;
using System.Windows.Forms.PropertyGridInternal;
using System.Windows.Data;

namespace Untangle
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml: Main window of the Untangle game application.
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// A command for changing the selected language of the application.
		/// </summary>
		public static ICommand LanguageCommand = new RoutedCommand();

		/// <summary>
		/// A command for displaying the About box of the application.
		/// </summary>
		public static ICommand AboutCommand = new RoutedCommand();







		/// <summary>
		/// The main window's view model instance.
		/// </summary>
		public MainViewModel ViewModel
		{
			get { return _viewModel; }
		}

		private static MainViewModel _viewModel;
		private static string EnterLevelBuilder_MenuText = "Enter Level Editor";
		private static string ExitLevelBuilder_MenuText = "Exit Level Editor";

		/// <summary>
		/// Initializes a new <see cref="MainWindow" /> instance and obtains its view model from the
		/// <see cref="System.Windows.FrameworkElement.DataContext" /> of the window.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			_viewModel = (MainViewModel)DataContext;
			this.AllowsTransparency = true;

			textBlockOpacity.DataContext = this;
			imageIcon.DataContext = this;
			levelEditorInstructions.Visibility = Visibility.Hidden;

			ic_GameField.SizeChanged += Ic_GameField_SizeChanged;
			this.Loaded += MainWindow_Loaded;

			ic_GameField.ClipToBounds = true;
			ic_GameField.UseLayoutRounding = true;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			ViewModel.SetBoardSize(ic_GameField.RenderSize);
			ViewModel.InitialGame();
		}

		private void SetTitle(string title)
		{
			string newTitle = $"Untangled - {title}";
			this.Title = newTitle;
		}

		private void Ic_GameField_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ViewModel.SetBoardSize(ic_GameField.RenderSize);
		}

		public Vertex GetVertexFromId(int vertexId)
		{
			IEnumerable<Ellipse> ellipses = WPFHelper.ChildrenOfType<Ellipse>(ic_GameField);
			Ellipse match = ellipses.Where(e => ((Vertex)(e.DataContext)).Id == vertexId).FirstOrDefault();
			if (match == default(Ellipse))
			{
				throw new KeyNotFoundException();
			}

			Vertex result = match.DataContext as Vertex;
			return result;
		}

		#region Minimize/Maximize/Close

		private void btnMinimizeWindow_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void btnMaximizeWindow_Click(object sender, RoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
				this.ResizeMode = ResizeMode.CanResizeWithGrip;
				btnMaximizeWindow.Content = "1";
			}
			else if (this.WindowState == WindowState.Normal)
			{
				this.ResizeMode = ResizeMode.NoResize;
				this.WindowState = WindowState.Maximized;
				btnMaximizeWindow.Content = "2";
			}
		}

		private void btnCloseWindow_Click(object sender, RoutedEventArgs e)
		{
			ApplicationCommands.Close.Execute(null, null);
		}

		private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				if (this.WindowState == WindowState.Maximized)
				{
					return;
				}
				this.DragMove();
			}
		}

		#endregion

		#region Menu Click Commands: New/Load/Save/About/Exit

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the New game command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void NewGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.NewGame(ic_GameField.RenderSize);
			UpdateDebugOutput();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the Save game command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void SaveGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string fileName;
			if (!FileDialogHelper.PromptForFileToSave(out fileName))
			{
				return;
			}

			if (ViewModel.SaveGame(fileName, true))
			{
				SetTitle($"\"{fileName}\"");
			}

			UpdateDebugOutput();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the Load game command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void LoadGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string fileName;
			if (!FileDialogHelper.PromptForFileToLoad(out fileName))
			{
				return;
			}

			if (ViewModel.LoadGame(fileName))
			{
				SetTitle($"\"{fileName}\"");
			}

			UpdateDebugOutput();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the About command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var aboutBox = new AboutBox
			{
				Owner = this,
			};
			aboutBox.ShowDialog();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the Exit command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (ViewModel.IsEditing)
			{
				ViewModel.ToggleLevelEditor();
			}

			Close();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Window.Closing"/> event of the Main window.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="CancelEventArgs"/>  containing the event arguments.</param>
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = !ViewModel.ConfirmQuit();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the Language choice command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void LanguageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.LanguageManager.SelectLanguage((string)e.Parameter);
		}

		#endregion

		#region Mouse Events

		/// <summary>
		/// Handles the <see cref="System.Windows.UIElement.MouseDown"/> event of a vertex
		/// <see cref="System.Windows.Shapes.Ellipse"/> in the game field.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/>  containing the event arguments.</param>
		private void Vertex_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var ellipse = (Ellipse)sender;
			var vertex = ellipse.DataContext as Core.Vertex;

			if (vertex != null)
			{
				ModifierKeys keyModifier = Keyboard.Modifiers;

				if (e.ChangedButton == MouseButton.Left && keyModifier.HasFlag(ModifierKeys.Control) && keyModifier.HasFlag(ModifierKeys.Shift))
				{
					if (!vertex.AtStartPosition)
					{
						SendVertexToStartingPosition(vertex);
					}
				}
				else if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
				{
					ViewModel.HandleVertexMouseDown(vertex, e.ChangedButton, keyModifier);
				}
				else if (e.ChangedButton == MouseButton.Middle)
				{
					vertex.NextColor();
					vertex.State = Enums.VertexState.Normal;
				}
			}
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.UIElement.MouseMove"/> event of the Main window.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/>  containing the event arguments.</param>
		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			Point position = e.GetPosition(ic_GameField);
			position.Offset(-ic_GameField.ActualWidth * 0.5, -ic_GameField.ActualHeight * 0.5);
			ViewModel.HandleMouseMove(position, (e.LeftButton == MouseButtonState.Pressed));
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.UIElement.MouseUp"/> event of the Main window.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/>  containing the event arguments.</param>
		private void Window_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				Point position = e.GetPosition(ic_GameField);
				position.Offset(-ic_GameField.ActualWidth * 0.5, -ic_GameField.ActualHeight * 0.5);

				bool keyModifier = false;
				if (Keyboard.Modifiers == ModifierKeys.Alt)
				{
					keyModifier = true;
				}

				ViewModel.HandleMouseUp(position, keyModifier);
			}
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.UIElement.MouseEnter"/> event of a vertex
		/// <see cref="System.Windows.Shapes.Ellipse"/> in the game field.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/>  containing the event arguments.</param>
		private void Vertex_MouseEnter(object sender, MouseEventArgs e)
		{
			var ellipse = (Ellipse)sender;
			var vertex = ellipse.DataContext as Core.Vertex;
			if (vertex != null)
			{
				ViewModel.HandleVertexMouseEnter(vertex);
			}
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.UIElement.MouseLeave"/> event of a vertex
		/// <see cref="System.Windows.Shapes.Ellipse"/> in the game field.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/>  containing the event arguments.</param>
		private void Vertex_MouseLeave(object sender, MouseEventArgs e)
		{
			var ellipse = (Ellipse)sender;
			var vertex = ellipse.DataContext as Core.Vertex;
			if (vertex != null)
			{
				ViewModel.HandleVertexMouseLeave(vertex);
			}
		}

		#endregion

		#region Misc Input Events

		/// <summary>
		/// Handles zooming of the game field.
		/// </summary>
		private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				decimal value = e.Delta / 120;
				decimal scaledValue = Math.Abs(value * 0.05m);
				int sign = Math.Sign(value);
				if (sign == -1)
				{
					ViewModel.ScaleZoom -= scaledValue;
				}
				else if (sign == 1)
				{
					ViewModel.ScaleZoom += scaledValue;
				}
			}
		}

		/// <summary>
		/// Handles opacity adjustment keys.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (mi_Transparent.IsChecked)
			{
				if (e.Key == Key.Add)
				{
					this.Opacity += 0.05;
				}
				else if (e.Key == Key.Subtract)
				{
					this.Opacity -= 0.05;
				}
			}

			if (Keyboard.Modifiers == ModifierKeys.Alt)
			{
				if (e.SystemKey == Key.Enter)
				{
					ViewModel.Game.Level.ShrinkLongestEdge();
				}
			}
		}

		#endregion

		#region Undo/Redo

		/// <summary>
		/// Handles the Executed event of the command binding for the Undo command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;

			HistoricalMove doMove = ViewModel.Game.Level.MoveHistory.ElementAtOrDefault(ViewModel.Game.Level.MoveCount - 1);
			if (doMove == null)
			{
				return;
			}

			Vertex matchingVertex = GetVertexFromId(doMove.VertexId);
			if (matchingVertex == null)
			{
				return;
			}

			Point fromPoint = doMove.ToPosition;
			Point toPoint = doMove.FromPosition;

			ViewModel.Game.Level.MoveCount -= 1;

			AnimateVertexMovement(matchingVertex, fromPoint, toPoint);
		}

		/// <summary>
		///  Handles the Executed event of the command binding for the Redo command.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;

			HistoricalMove doMove = ViewModel.Game.Level.MoveHistory.ElementAtOrDefault(ViewModel.Game.Level.MoveCount + 1);
			if (doMove == null)
			{
				return;
			}

			Vertex matchingVertex = GetVertexFromId(doMove.VertexId);
			if (matchingVertex == null)
			{
				return;
			}

			ViewModel.Game.Level.MoveCount += 1;

			Point fromPoint = doMove.FromPosition;
			Point toPoint = doMove.ToPosition;

			AnimateVertexMovement(matchingVertex, fromPoint, toPoint);
		}

		public void SendVertexToStartingPosition(Vertex vertex)
		{
			if (vertex.AtStartPosition)
			{
				return;
			}

			Point fromPoint = vertex.GetPosition();
			Point toPoint = vertex.StartingPosition.Value;
			ViewModel.Game.Level.AddMoveToHistory(vertex.Id, fromPoint, toPoint);

			AnimateVertexMovement(vertex, fromPoint, toPoint);
		}

		private void AnimateVertexMovement(Vertex vertex, Point from, Point to)
		{
			DoubleAnimation xAnimation = new DoubleAnimation(from.X, to.X, ViewModelBase.Animation_Duration, FillBehavior.HoldEnd) { EasingFunction = ViewModelBase.Animation_EasingFunction, AutoReverse = false };
			DoubleAnimation yAnimation = new DoubleAnimation(from.Y, to.Y, ViewModelBase.Animation_Duration, FillBehavior.HoldEnd) { EasingFunction = ViewModelBase.Animation_EasingFunction, AutoReverse = false };

			xAnimation.Completed += (s, e) =>
			{
				//using (var d = Dispatcher.DisableProcessing())
				//{
					//this.Dispatcher.BeginInvoke(new Action(() => {

					vertex.SetValue(Vertex.XProperty, to.X);
					vertex.BeginAnimation(Vertex.XProperty, null);

					_viewModel.Game.Level.GameGraph.RecalculateIntersections(vertex);
					UpdateDebugOutput();
					_viewModel.Game.Level.GameSolvedCheck();

					//}), DispatcherPriority.Render);
				//}
			};

			yAnimation.Completed += (s, e) =>
			{
				//using (var d = Dispatcher.DisableProcessing())
				//{
					//this.Dispatcher.BeginInvoke(new Action(() => {

					vertex.SetValue(Vertex.YProperty, to.Y);
					vertex.BeginAnimation(Vertex.YProperty, null);

					_viewModel.Game.Level.GameGraph.RecalculateIntersections(vertex);
					UpdateDebugOutput();
					_viewModel.Game.Level.GameSolvedCheck();

					//}), DispatcherPriority.Render);
				//}
			};

			vertex.BeginAnimation(Vertex.XProperty, xAnimation);
			vertex.BeginAnimation(Vertex.YProperty, yAnimation);
		}

		#endregion

		#region Misc Menu Commands

		/// <summary>
		/// Toggles level editor
		/// </summary>
		private void MenuCommand_LevelBuilder_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.ToggleLevelEditor();

			if (ViewModel.IsEditing)
			{
				mi_LevelBuilder.Header = ExitLevelBuilder_MenuText;
				borderGameField.BorderBrush = Brushes.Red;
				levelEditorInstructions.Visibility = Visibility.Visible;
				mi_RandomizeVertices.Visibility = Visibility.Visible;
			}
			else
			{
				mi_LevelBuilder.Header = EnterLevelBuilder_MenuText;
				borderGameField.SetResourceReference(Border.BorderBrushProperty, "windowBorderColor");
				levelEditorInstructions.Visibility = Visibility.Hidden;
				mi_RandomizeVertices.Visibility = Visibility.Collapsed;
			}
		}

		#region Color Vertices

		private void ColorGraph_Click(object sender, RoutedEventArgs e)
		{
			int chromaticNumber = GameLevel.GetChromaticNumber(ViewModel.Game.Level.GameGraph.Vertices);
			MessageBox.Show($"The graph's chromatic number is: {chromaticNumber}", "Chromatic Number", MessageBoxButton.OK);
		}

		private void mi_ColorByStartPositions_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Game.Level.MarkVerticesInStartPosition();
		}

		#endregion

		#region Moving Vertices

		private void mi_AutoSolve_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AutoSolve();
			UpdateDebugOutput();
		}

		private void mi_SendVerticesToStartPositions_Click(object sender, RoutedEventArgs e)
		{
			foreach (Vertex vertex in ViewModel.Game.Level.GameGraph.Vertices)
			{
				SendVertexToStartingPosition(vertex);
			}
			UpdateDebugOutput();
		}

		private void mi_RandomizeVertices_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Game.Level.Edit_RandomizeVertices(ic_GameField.RenderSize);
			UpdateDebugOutput();
		}

		#endregion

		#region Settings Menu Commands

		/// <summary>
		/// Toggles window transparency.
		/// </summary>
		private void mi_Transparent_Click(object sender, RoutedEventArgs e)
		{
			if (mi_Transparent.IsChecked)
			{
				this.Opacity = 0.50;
			}
			else
			{
				this.Opacity = 1.0;
			}
		}

		private void mi_ShowGridLines_Click(object sender, RoutedEventArgs e)
		{
			if (mi_ShowGridLines.IsChecked)
			{

				gameboardGrid.Background = TryFindResource("SquareLattice") as Brush;
			}
			else
			{
				gameboardGrid.Background = Brushes.Transparent;
			}
		}

		#endregion

		#region Debug View

		private void mi_DebugView_Click(object sender, RoutedEventArgs e)
		{
			if (mi_DebugView.IsChecked)
			{
				ViewModel.Game.Level.PlayerMoved += UpdateDebugOutput;
				UpdateDebugOutput(null, EventArgs.Empty);
			}
			else
			{
				ViewModel.Game.Level.PlayerMoved -= UpdateDebugOutput;
				textBoxDebugInfo.Clear();
			}
		}

		private void UpdateDebugOutput(object sender, EventArgs e)
		{
			new Action(() => UpdateDebugOutput()).Invoke();
		}

		private void UpdateDebugOutput()
		{
			if (mi_DebugView.IsChecked)
			{
				StringBuilder output = new StringBuilder();

				output.AppendLine();
				output.AppendLine("Undo/Redo buffer:");
				output.AppendLine($"Position.Index: {ViewModel.Game.Level.MoveCount - 1}");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Level.MoveHistory.Select(mv => mv.ToString())));
				output.AppendLine();
				output.AppendLine("Vertices:");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Level.GameGraph.Vertices.Select(v => v.ToString())));
				output.AppendLine();
				output.AppendLine();
				output.AppendLine("Line Segments:");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Level.GameGraph.LineSegments.Select(v => v.ToString())));
				output.AppendLine();
				output.AppendLine();
				output.AppendLine("Intersections:");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Level.GameGraph.Intersections.Select(v => $"{v.Key}:" + Environment.NewLine + "\t" + $"{string.Join(Environment.NewLine + "\t", v.Value.Select(h => h.ToString()))}")));
				output.AppendLine();
				output.AppendLine();
				output.AppendLine("Starting Position(s):");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Level.GameGraph.Vertices.Select(v => $"({v.StartingPosition.Value.X}, {v.StartingPosition.Value.Y})")));
				output.AppendLine();

				textBoxDebugInfo.SetCurrentValue(TextBox.TextProperty, output.ToString());
			}
		}

		#endregion

		#endregion

	}
}
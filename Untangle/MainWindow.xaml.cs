﻿/*
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
using Untangle.ViewModels;

namespace Untangle
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml: Main window of the Untangle game application.
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// The main window's view model instance.
		/// </summary>
		public MainViewModel ViewModel
		{
			get { return _viewModel; }
		}

		private Storyboard Animation_StoryBoard;
		private int _throttleIntervalInMilliseconds = 100;
		private ThrottleDispatcher _undoRedoThrottle = new ThrottleDispatcher();

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
			Animation_StoryBoard = new Storyboard();
			_viewModel = (MainViewModel)DataContext;
			textBlockOpacity.DataContext = this;
			levelEditorInstructions.Visibility = Visibility.Hidden;

			ic_GameField.SizeChanged += Ic_GameField_SizeChanged;
		}

		private void SetTitle(string title)
		{
			string newTitle = $"Untangled - {title}";
			this.Title = newTitle;
			labelTitle.Content = newTitle;
		}

		private void Ic_GameField_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			_viewModel.SetBoardSize(ic_GameField.RenderSize);
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
					btnMaximizeWindow_Click(null, null);
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
			_viewModel.NewGame(ic_GameField.RenderSize);
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

			if (_viewModel.SaveGame(fileName, true))
			{
				SetTitle($"\"{fileName}\"");
			}
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

			if (_viewModel.LoadGame(fileName))
			{
				SetTitle($"\"{fileName}\"");
			}
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the About command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		//// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
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
			Close();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Window.Closing"/> event of the Main window.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="CancelEventArgs"/>  containing the event arguments.</param>
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = !_viewModel.ConfirmQuit();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Input.CommandBinding.Executed"/> event of the
		/// command binding for the Language choice command.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void LanguageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			_viewModel.LanguageManager.SelectLanguage((string)e.Parameter);
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
			var vertex = (ViewModels.Vertex)ellipse.DataContext;

			if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
			{
				_viewModel.HandleVertexMouseDown(vertex, e.ChangedButton, Keyboard.Modifiers);
			}
			else if (e.ChangedButton == MouseButton.Middle)
			{
				vertex.NextColor();
				vertex.State = Enums.VertexState.Normal;
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
			_viewModel.HandleMouseMove(position, (e.LeftButton == MouseButtonState.Pressed));
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

				_viewModel.HandleMouseUp(position, keyModifier);
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
			var vertex = (ViewModels.Vertex)ellipse.DataContext;
			_viewModel.HandleVertexMouseEnter(vertex);
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
			var vertex = ellipse.DataContext as ViewModels.Vertex;
			if (vertex != null)
			{
				_viewModel.HandleVertexMouseLeave(vertex);
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
					_viewModel.ScaleZoom -= scaledValue;
				}
				else if (sign == 1)
				{
					_viewModel.ScaleZoom += scaledValue;
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
					_viewModel.Game.Level.ShrinkLongestEdge();
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
			HistoricalMove doMove = _viewModel.Game.Level.MoveHistory.ElementAtOrDefault(_viewModel.Game.Level.MoveCount - 1);
			if (doMove == null)
			{
				return;
			}

			IEnumerable<Ellipse> ellipses = WPFHelper.ChildrenOfType<Ellipse>(ic_GameField);
			Ellipse match = ellipses.Where(e => ((Vertex)(e.DataContext)).Id == doMove.VertexId).FirstOrDefault();
			if (match == default(Ellipse))
			{
				return;
			}

			/*
			IEnumerable<Line> lines = WPFHelper.ChildrenOfType<Line>(ic_GameField);
			Vertex matchingVertex = match.DataContext as Vertex;
			if (matchingVertex == null)
			{
				return;
			}

			List<ViewModels.LineSegment> attachedLineSegments = matchingVertex.LineSegments.ToList();
			var attachedLines = lines.Where(line => attachedLineSegments.Contains(((ViewModels.LineSegment)line.DataContext))).ToList();

			string vertexId = $"({doMove.VertexId})";
			string lineLocation = "";
			if (attachedLines.Any())
			{
				lineLocation = $"({attachedLines[0].X1}, {attachedLines[0].Y1}) -> ({attachedLines[0].X2}, {attachedLines[0].Y2})";
			}

			Point localCoordinants = this.PointFromScreen(new Point(0,0));
			Point screenCoordinants = PointToScreen(new Point(0, 0));

			var layoutTransform = this.ic_GameField.LayoutTransform;
			var ancestorTransform = this.ic_GameField.TransformToAncestor(this);
			*/

			Point fromPoint = doMove.ToPosition;
			Point toPoint = doMove.FromPosition;

			DoubleAnimation xAnimation = new DoubleAnimation(fromPoint.X, toPoint.X, Animation_Duration) { EasingFunction = Animation_EasingFunction };
			DoubleAnimation yAnimation = new DoubleAnimation(fromPoint.Y, toPoint.Y, Animation_Duration) { EasingFunction = Animation_EasingFunction };

			//var matrices = attachedLines.Select(ln => $"({ln.X1},{ln.Y1}) -> ({ln.X2},{ln.Y2}) : {ln.RenderTransform.Value}");

			TranslateTransform translateTransform = (TranslateTransform)((TransformGroup)match.RenderTransform).Children[0];
			translateTransform.BeginAnimation(TranslateTransform.XProperty, xAnimation);
			translateTransform.BeginAnimation(TranslateTransform.YProperty, yAnimation);

			//foreach (Line line in attachedLines)
			//{
			//	TranslateTransform transform = (TranslateTransform)line.RenderTransform;
			//	transform.BeginAnimation(TranslateTransform.XProperty, xAnimation);
			//	transform.BeginAnimation(TranslateTransform.YProperty, yAnimation);
			//}

			_undoRedoThrottle.Throttle(_throttleIntervalInMilliseconds, parm => _viewModel.Game.Level.UndoMove());
		}

		private static Duration Animation_Duration = new Duration(TimeSpan.FromMilliseconds(600));
		private static EasingFunctionBase Animation_EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };

		/// <summary>
		///  Handles the Executed event of the command binding for the Redo command.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ExecutedRoutedEventArgs"/>  containing the event arguments.</param>
		private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			HistoricalMove doMove = _viewModel.Game.Level.MoveHistory.ElementAtOrDefault(_viewModel.Game.Level.MoveCount);
			if (doMove == null)
			{
				return;
			}

			IEnumerable<Ellipse> ellipses = WPFHelper.ChildrenOfType<Ellipse>(ic_GameField);
			Ellipse match = ellipses.Where(e => ((Vertex)(e.DataContext)).Id == doMove.VertexId).FirstOrDefault();
			if (match == default(Ellipse))
			{
				return;
			}

			Point fromPoint = doMove.FromPosition;
			Point toPoint = doMove.ToPosition;

			DoubleAnimation xAnimation = new DoubleAnimation(fromPoint.X, toPoint.X, Animation_Duration) { EasingFunction = Animation_EasingFunction };
			DoubleAnimation yAnimation = new DoubleAnimation(fromPoint.Y, toPoint.Y, Animation_Duration) { EasingFunction = Animation_EasingFunction };

			TranslateTransform translateTransform = (TranslateTransform)((TransformGroup)match.RenderTransform).Children[0];
			translateTransform.BeginAnimation(TranslateTransform.XProperty, xAnimation);
			translateTransform.BeginAnimation(TranslateTransform.YProperty, yAnimation);

			_undoRedoThrottle.Throttle(_throttleIntervalInMilliseconds, parm => _viewModel.Game.Level.RedoMove());
		}

		#endregion

		#region Misc Menu Commands

		/// <summary>
		/// Toggles level editor
		/// </summary>
		private void MenuCommand_LevelBuilder_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.ToggleLevelEditor();
			if (_viewModel.IsEditing)
			{
				mi_LevelBuilder.Header = ExitLevelBuilder_MenuText;
				borderGameField.BorderBrush = Brushes.Red;
				levelEditorInstructions.Visibility = Visibility.Visible;
				mi_RandomizeVertices.Visibility = Visibility.Visible;
			}
			else
			{
				mi_LevelBuilder.Header = EnterLevelBuilder_MenuText;
				borderGameField.BorderBrush = Brushes.Transparent;
				levelEditorInstructions.Visibility = Visibility.Hidden;
				mi_RandomizeVertices.Visibility = Visibility.Collapsed;
			}

		}

		#region Color Vertices

		private void ColorGraph_Click(object sender, RoutedEventArgs e)
		{
			int chromaticNumber = GameLevel.ColorGraph(_viewModel.Game.Level.GameGraph.Vertices);
			MessageBox.Show($"The graph's chromatic number is: {chromaticNumber}", "Chromatic Number", MessageBoxButton.OK);
		}

		private void mi_ColorByStartPositions_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.Game.Level.MarkVerticesInStartPosition();
		}

		#endregion

		#region Moving Vertices

		private void mi_AutoSolve_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.AutoSolve();
		}

		private void mi_AttemptMoveVerticesToStartPositions_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.Game.Level.AttemptSendVerticesToStartPositionWithoutCrossings();
		}

		private void mi_RandomizeVertices_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.Game.Level.Edit_RandomizeVertices(ic_GameField.RenderSize);
		}

		#endregion

		#endregion

		#region Settings Menu Commands

		/// <summary>
		/// Toggles window transparency.
		/// </summary>
		private void mi_Transparent_Click(object sender, RoutedEventArgs e)
		{
			if (mi_Transparent.IsChecked)
			{
				this.Opacity = 0.22;
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

		#region Debug View

		private void mi_DebugView_Click(object sender, RoutedEventArgs e)
		{
			if (mi_DebugView.IsChecked)
			{
				_viewModel.Game.Level.GameGraph.VertexCollectionChanged -= UpdateDebugOutput;
				_viewModel.Game.Level.GameGraph.LineSegmentCollectionChanged -= UpdateDebugOutput;
				_viewModel.Game.Level.GameGraph.IntersectionCollectionChanged -= UpdateDebugOutput;

				textBoxDebugInfo.Clear();
			}
			else
			{
				_viewModel.Game.Level.GameGraph.VertexCollectionChanged += UpdateDebugOutput;
				_viewModel.Game.Level.GameGraph.LineSegmentCollectionChanged += UpdateDebugOutput;
				_viewModel.Game.Level.GameGraph.IntersectionCollectionChanged += UpdateDebugOutput;

				UpdateDebugOutput(null, EventArgs.Empty);
			}
		}

		private void UpdateDebugOutput(object sender, EventArgs e)
		{
			new Action(() => UpdateDebugOutput()).Invoke();
		}

		private void UpdateDebugOutput()
		{
			textBoxDebugInfo.Clear();
			StringBuilder output = new StringBuilder();

			output.AppendLine("Vertices:");
			output.AppendLine(string.Join(Environment.NewLine, _viewModel.Game.Level.GameGraph.Vertices.Select(v => v.ToString())));

			output.AppendLine();
			output.AppendLine("Line Segments:");
			output.AppendLine(string.Join(Environment.NewLine, _viewModel.Game.Level.GameGraph.LineSegments.Select(v => v.ToString())));

			output.AppendLine();
			output.AppendLine("Intersections:");
			output.AppendLine(string.Join(Environment.NewLine, _viewModel.Game.Level.GameGraph.Intersections.Select(v => $"{v.Key}:" + Environment.NewLine + "\t" + $"{string.Join(Environment.NewLine + "\t", v.Value.Select(h => h.ToString()))}")));

			textBoxDebugInfo.SetCurrentValue(TextBox.TextProperty, output.ToString());
			//textBoxDebugInfo.InvalidateVisual();
		}

		#endregion

		#endregion

	}
}
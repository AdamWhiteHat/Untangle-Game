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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Untangle.Core;
using Untangle.Generation;
using Untangle.Resources;
using Untangle.Utils;
using Vertex = Untangle.Core.Vertex;

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
		private static readonly string EnterLevelBuilder_MenuText = "Enter Level Editor";
		private static readonly string ExitLevelBuilder_MenuText = "Exit Level Editor";

		/// <summary>
		/// Initializes a new <see cref="MainWindow" /> instance and obtains its view model from the
		/// <see cref="System.Windows.FrameworkElement.DataContext" /> of the window.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			this.AllowsTransparency = true;
			_viewModel = (MainViewModel)DataContext;

			imageIcon.DataContext = this;

			debugView.Width = 0;
			mi_DebugView.IsChecked = false;
			levelEditorInstructions.Visibility = Visibility.Hidden;

			ic_GameField.SizeChanged += Ic_GameField_SizeChanged;
			ic_GameField.ClipToBounds = false;
			ic_GameField.UseLayoutRounding = true;

			this.Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			double actualWidth = ic_GameField.ActualWidth;
			double actualHeight = ic_GameField.ActualHeight;
			var renderSize = ic_GameField.RenderSize;
			var desiredSize = ic_GameField.DesiredSize;

			var a = this.RenderSize;
			var b = this.DesiredSize;
			var ah = this.ActualHeight;
			var hw = this.ActualWidth;
			var tw = this.Width;
			var th = this.Height;

			_viewModel.SetBoardSize(ic_GameField.RenderSize);

			Style itemUndoContainerStyle = new Style(typeof(ListBoxItem));
			itemUndoContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
			itemUndoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(lbUndoRedoListItem_PreviewMouseLeftButtonDown)));
			itemUndoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(UndoRedoListItem_PreviewMouseLeftButtonUp)));
			itemUndoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent, new MouseEventHandler(UndoRedoListItem_MouseMove)));
			itemUndoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(lbUndoRedoListItem_Drop)));
			itemUndoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.KeyUpEvent, new KeyEventHandler(lbUndoRedoListItem_KeyUp)));
			lbUndoListBox.ItemContainerStyle = itemUndoContainerStyle;

			Style itemRedoContainerStyle = new Style(typeof(ListBoxItem));
			itemRedoContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
			itemRedoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(lbUndoRedoListItem_PreviewMouseLeftButtonDown)));
			itemRedoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(UndoRedoListItem_PreviewMouseLeftButtonUp)));
			itemRedoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent, new MouseEventHandler(UndoRedoListItem_MouseMove)));
			itemRedoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(lbUndoRedoListItem_Drop)));
			itemRedoContainerStyle.Setters.Add(new EventSetter(ListBoxItem.KeyUpEvent, new KeyEventHandler(lbUndoRedoListItem_KeyUp)));
			lbRedoListBox.ItemContainerStyle = itemRedoContainerStyle;

			ViewModel.InitializeGame(ic_GameField.RenderSize);
		}

		private void SetTitle(string title)
		{
			ViewModel.Title = title;
			this.Title = ViewModel.Title;
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
			if (!MainWindow.ConfirmQuit())
			{
				return;
			}

			ViewModel.NewGame(ic_GameField.RenderSize);
			ResetLevelBuilderUi();
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
				SetTitle(System.IO.Path.GetFileName(fileName));
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
			if (!ConfirmQuit())
			{
				return;
			}

			if (ViewModel.IsEditing)
			{
				ToggleLevelEditor();
			}


			string fileName;
			if (!FileDialogHelper.PromptForFileToLoad(out fileName))
			{
				return;
			}

			if (ViewModel.LoadGame(fileName))
			{
				SetTitle($"\"{System.IO.Path.GetFileName(fileName)}\"");
			}

			UpdateDebugOutput();
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
				ToggleLevelEditor();
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
			e.Cancel = !MainWindow.ConfirmQuit();
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
		/// Displays a save game prompt when the user is about to lose his current game progress, if needed.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the current operation  should proceed, possibly losing game state.
		/// This happens either when there is no unsaved game state (IsDirty flag is false), or the user has indicated they do not care.
		/// <see langword="true"/> if the current operation should abort, preserving the current game state.
		/// </returns>
		/// <remarks>
		/// <para>The save game prompt is not needed if the user has not dragged any vertices since
		/// the game was started or last saved.</para>
		/// </remarks>
		public static bool ConfirmQuit()
		{
			if (!_viewModel.IsDirty)
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

		/// <summary>
		/// Toggles the level editor.
		/// </summary>
		public void ToggleLevelEditor()
		{
			if (!ViewModel.Game.IsEditing)
			{
				if (!MainWindow.ConfirmQuit())
				{
					return;
				}
				ViewModel.Game.EnterEditMode();
			}
			else
			{
				ViewModel.Game.ExitEditMode();
			}
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
					SendVertexToStartingPosition(vertex);
				}
				else if (e.ChangedButton == MouseButton.Left && keyModifier.HasFlag(ModifierKeys.Alt) && keyModifier.HasFlag(ModifierKeys.Shift))
				{
					SendVertexToSolvedPosition(vertex);
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
				ViewModel.HandleVertexMouseLeave();
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
					ViewModel.Game.ShrinkLongestEdge();
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

			if (!ViewModel.Game.UndoStack.Any())
			{
				return;
			}

			HistoricalMove doMove = ViewModel.Game.UndoStack.Pop();  // ViewModel.Game.MoveHistory.ElementAtOrDefault(ViewModel.Game.MoveCount - 1);
			ViewModel.Game.RedoStack.Push(doMove);
			if (doMove == null)
			{
				return;
			}

			Vertex matchingVertex = doMove.Vertex;
			if (matchingVertex == null)
			{
				return;
			}

			Point fromPoint = doMove.ToPosition;
			Point toPoint = doMove.FromPosition;

			ViewModel.Game.MoveCount -= 1;

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

			if (!ViewModel.Game.RedoStack.Any())
			{
				return;
			}

			HistoricalMove doMove = ViewModel.Game.RedoStack.Pop();//ViewModel.Game.MoveHistory.ElementAtOrDefault(ViewModel.Game.MoveCount + 1);
			ViewModel.Game.UndoStack.Push(doMove);
			if (doMove == null)
			{
				return;
			}

			Vertex matchingVertex = doMove.Vertex;
			if (matchingVertex == null)
			{
				return;
			}

			ViewModel.Game.MoveCount += 1;

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
			ViewModel.Game.AddMoveToHistory(vertex, fromPoint, toPoint);

			AnimateVertexMovement(vertex, fromPoint, toPoint);
		}

		public void SendVertexToSolvedPosition(Vertex vertex)
		{
			if (!vertex.SolvedPosition.HasValue)
			{
				return;
			}

			if (vertex.AtSolvedPosition)
			{
				return;
			}

			Point fromPoint = vertex.GetPosition();
			Point toPoint = vertex.SolvedPosition.Value;
			ViewModel.Game.AddMoveToHistory(vertex, fromPoint, toPoint);

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

				_viewModel.Game.Graph.RecalculateIntersections(vertex);
				UpdateDebugOutput();
				_viewModel.Game.GameSolvedCheck();

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

				_viewModel.Game.Graph.RecalculateIntersections(vertex);
				UpdateDebugOutput();
				_viewModel.Game.GameSolvedCheck();

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
			ToggleLevelEditor();

			if (ViewModel.IsEditing)
			{
				mi_LevelBuilder.Header = ExitLevelBuilder_MenuText;
				borderGameField.BorderBrush = Brushes.Red;
				levelEditorInstructions.Visibility = Visibility.Visible;
				mi_RandomizeVerticesLocation.Visibility = Visibility.Visible;
				mi_RandomizeVerticesColors.Visibility = Visibility.Visible;
				mi_ReScaleVertices.Visibility = Visibility.Visible;
				mi_RecenterAllVertices.Visibility = Visibility.Visible;
			}
			else
			{
				ResetLevelBuilderUi();
			}
		}

		private void ResetLevelBuilderUi()
		{
			mi_LevelBuilder.Header = EnterLevelBuilder_MenuText;
			borderGameField.SetResourceReference(Border.BorderBrushProperty, "windowBorderColor");
			levelEditorInstructions.Visibility = Visibility.Hidden;
			mi_RandomizeVerticesLocation.Visibility = Visibility.Collapsed;
			mi_RandomizeVerticesColors.Visibility = Visibility.Collapsed;
			mi_ReScaleVertices.Visibility = Visibility.Collapsed;
			mi_RecenterAllVertices.Visibility = Visibility.Collapsed;
		}

		#region Color Vertices

		private void ColorGraph_Click(object sender, RoutedEventArgs e)
		{
			int chromaticNumber = GameState.GetChromaticNumber(ViewModel.Game.Graph.Vertices);
			MessageBox.Show($"The graph's chromatic number is: {chromaticNumber}", "Chromatic Number", MessageBoxButton.OK);
		}

		private void mi_ColorByStartPositions_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Game.MarkVerticesInStartPosition();
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
			SendVerticesToStartPositions();
		}

		private void SendVerticesToStartPositions()
		{
			foreach (Vertex vertex in ViewModel.Game.Graph.Vertices)
			{
				SendVertexToStartingPosition(vertex);
			}
			UpdateDebugOutput();
		}

		private void mi_SetVerticesSolvePositions_Click(object sender, RoutedEventArgs e)
		{
			foreach (Vertex vertex in ViewModel.Game.Graph.Vertices)
			{
				vertex.SolvedPosition = vertex.GetPosition();
			}

			mi_DrawSolveLines.Visibility = Visibility.Visible;
		}

		private void mi_DrawSolveLines_Click(object sender, RoutedEventArgs e)
		{
			if (ViewModel.Game.Graph.Vertices.Any(v => v.SolvedPosition == null))
			{
				return;
			}

			if (ViewModel.Game.Graph.Vertices.Any(v => !v.AtStartPosition))
			{
				SendVerticesToStartPositions();
			}

			if (ViewModel.Game.Graph.NonInteractiveLines.Any())
			{
				ViewModel.Game.Graph.NonInteractiveLines.Clear();
			}

			RandomizeVerticesColors();

			foreach (Vertex vertex in ViewModel.Game.Graph.Vertices)
			{
				NonInteractiveLine newLine = new NonInteractiveLine(vertex);
				//newLine.X1 = vertex.StartingPosition.Value.X;
				//newLine.Y1 = vertex.StartingPosition.Value.Y;
				//newLine.X2 = vertex.SolvedPosition.Value.X;
				//newLine.Y2 = vertex.SolvedPosition.Value.Y;
				//newLine.Stroke = Brushes.Orange;
				//newLine.StrokeEndLineCap = PenLineCap.Triangle;
				//newLine.StrokeStartLineCap = PenLineCap.Triangle;
				//newLine.StrokeDashOffset = 1;
				//newLine.StrokeThickness = 0.5;
				//newLine.Opacity = 0.75;
				//newLine.DataContext = ViewModel;
				ViewModel.Game.Graph.NonInteractiveLines.Add(newLine);
			}

			int i = 0;
		}

		private void mi_RandomizeVerticesLocation_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Game.Edit_RandomizeVerticesLocations(ic_GameField.RenderSize);
			UpdateDebugOutput();
		}

		private void mi_RandomizeVerticesColors_Click(object sender, RoutedEventArgs e)
		{
			RandomizeVerticesColors();
		}

		private void RandomizeVerticesColors()
		{
			int max = ColorPalette.Default.Count - 1;
			int counter = 0;
			foreach (Vertex vertex in ViewModel.Game.Graph.Vertices)
			{
				vertex.SetColor(ColorPalette.Default[(counter % max)]);
				counter++;
			}
		}

		private void mi_ReScaleVertices_Click(object sender, RoutedEventArgs e)
		{
			double scale = (double)ViewModel.ScaleZoom;
			ViewModel.Game.Edit_RescaleAllVertices(scale);
		}

		private void mi_RecenterAllVertices_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Game.Edit_RecenterVertices(ic_GameField.RenderSize);
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
				debugView.Width = 175;
				ViewModel.Game.PlayerMoved += UpdateDebugOutput;
				UpdateDebugOutput(null, EventArgs.Empty);
			}
			else
			{
				debugView.Width = 0;
				ViewModel.Game.PlayerMoved -= UpdateDebugOutput;
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

				/*
				StringBuilder output = new StringBuilder();

				output.AppendLine();
				output.AppendLine("Undo/Redo buffer:");
				output.AppendLine($"Position.Index: {ViewModel.Game.MoveCount - 1}");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.MoveHistory.Select(mv => mv.ToString())));
				output.AppendLine();
				output.AppendLine("Vertices:");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Graph.Vertices.Select(v => v.ToString())));
				output.AppendLine();
				output.AppendLine();
				output.AppendLine("Line Segments:");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Graph.LineSegments.Select(v => v.ToString())));
				output.AppendLine();
				output.AppendLine();
				output.AppendLine("Intersections:");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Graph.Intersections.Select(v => $"{v.Key}:" + Environment.NewLine + "\t" + $"{string.Join(Environment.NewLine + "\t", v.Value.Select(h => h.ToString()))}")));
				output.AppendLine();
				output.AppendLine();
				output.AppendLine("Starting Position(s):");
				output.AppendLine(string.Join(Environment.NewLine, ViewModel.Game.Graph.Vertices.Select(v => $"({v.StartingPosition.Value.X}, {v.StartingPosition.Value.Y})")));
				output.AppendLine();

				textBoxDebugInfo.SetCurrentValue(TextBox.TextProperty, output.ToString());
				*/
			}
		}

		private void lbUndoRedoListItem_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				ListBoxItem sourceItem = (ListBoxItem)sender;
				ListBox sourceControl = sourceItem.GetParentOfType<ListBox>() as ListBox;

				ObservableStack<HistoricalMove> sourceStack = GetBoundStackFromListbox(sourceControl);

				IList toRemove = sourceControl.SelectedItems;
				int index = 0;
				while (index < toRemove.Count)
				{
					HistoricalMove item = toRemove[index] as HistoricalMove;
					item.DeleteSelf();
					sourceStack.Remove(item);
				}
			}
			else if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (e.Key == Key.A)
				{
					ListBoxItem sourceItem = (ListBoxItem)sender;
					ListBox sourceControl = sourceItem.GetParentOfType<ListBox>() as ListBox;
					sourceControl.SelectAll();
				}
			}
		}


		#endregion

		#endregion

		private ListBoxItem draggedItem = null;

		private void lbUndoRedoListItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (sender is ListBoxItem)
			{
				ListBoxItem sourceItem = sender as ListBoxItem;

				draggedItem = sourceItem;

				//DragDrop.DoDragDrop(sourceItem, sourceItem, System.Windows.DragDropEffects.Move);
				//sourceItem.IsSelected = true;
				//e.Handled = true;
			}
		}

		private void UndoRedoListItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (draggedItem != null)
			{
				draggedItem = null;
			}
		}

		private void UndoRedoListItem_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (draggedItem != null)
				{
					DragDrop.DoDragDrop(draggedItem, draggedItem, System.Windows.DragDropEffects.Move);
					draggedItem = null;
				}
			}
		}

		private ObservableStack<HistoricalMove> GetBoundStackFromListbox(ListBox parentListbox)
		{
			BindingExpression bindingExpression = parentListbox.GetBindingExpression(ListBox.ItemsSourceProperty);
			return bindingExpression.GetBoundPropertyValue<ObservableStack<HistoricalMove>>();
		}

		private void lbUndoRedoListItem_Drop(object sender, System.Windows.DragEventArgs e)
		{
			ListBoxItem sourceItem = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
			ListBoxItem targetItem = sender as ListBoxItem;


			ListBox sourceControl = sourceItem.GetParentOfType<ListBox>() as ListBox;
			ListBox targetControl = targetItem.GetParentOfType<ListBox>() as ListBox;

			if (sourceControl == null || targetControl == null)
			{
				return;
			}

			ObservableStack<HistoricalMove> sourceStack = GetBoundStackFromListbox(sourceControl);
			ObservableStack<HistoricalMove> targetStack = GetBoundStackFromListbox(targetControl);

			if (sourceStack == null || targetStack == null)
			{
				return;
			}

			HistoricalMove souceMove = sourceItem.DataContext as HistoricalMove;
			HistoricalMove targetMove = targetItem.DataContext as HistoricalMove;

			int fromIndex = sourceControl.Items.IndexOf(souceMove);
			int toIndex = targetControl.Items.IndexOf(targetMove);


			if (sourceControl != targetControl)
			{
				sourceStack.RemoveAt(fromIndex);
				targetStack.Insert(toIndex + 1, souceMove);
			}
			else
			{
				if (fromIndex < toIndex)
				{
					targetStack.Insert(toIndex + 1, souceMove);
					sourceStack.RemoveAt(fromIndex);
				}
				else
				{
					int remIdx = fromIndex + 1;
					if (targetStack.Count + 1 > remIdx)
					{
						targetStack.Insert(toIndex, souceMove);
						sourceStack.RemoveAt(remIdx);
					}
				}
			}
		}
	}
}
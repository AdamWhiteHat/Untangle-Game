﻿/*
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
using System.Collections.Generic;
using System.Collections.Immutable;
using Untangle.Enums;
using System.Windows.Media;
using Untangle.Generation;
using System.Windows.Media.Animation;
using XAMLMarkupExtensions.Base;
using System.Windows.Threading;
using Microsoft.Msagl.Core.DataStructures;

namespace Untangle.ViewModels
{
	/// <summary>
	/// A view model class for a single game level in a game of Untangle.
	/// </summary>
	public class GameLevel : ViewModelBase
	{

		#region Properties and Fields

		/// <summary>
		/// Occurs when the user has successfully solved the level by removing all intersections
		/// between line segments.
		/// </summary>
		public event EventHandler LevelSolved;

		/// <summary>
		/// The vertex which is currently being dragged by the user, if any.
		/// </summary>
		private Vertex _draggedVertex;

		/// <summary>
		/// The dragged vertex start position
		/// </summary>
		private Point _dragStartPosition;

		/// <summary>
		/// The dragged vertex end position
		/// </summary>
		private Point _dragEndPosition;

		/// <summary>
		/// Join two vertices with an edge temp variables
		/// </summary>
		private Vertex _selectedVertexPair_First = null;
		private Vertex _selectedVertexPair_Second = null;

		/// <summary>
		/// The vertex which is currently under the mouse cursor, if any.
		/// </summary>
		private Vertex _vertexUnderMouse;

		/// <summary>
		/// Indicates whether this instance is in level editing mode or not.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is in edit mode; otherwise, <c>false</c>.
		/// </value>
		/// 
		public bool IsEditing
		{
			get
			{
				return _isEditing;
			}
			set
			{
				if (_isEditing != value)
				{
					_isEditing = value;
					OnPropertyChanged();
				}
			}
		}
		private bool _isEditing = false;

		/// <summary>
		/// Specifies whether a vertex is currently being dragged by the user.
		/// </summary>
		public bool IsDragging
		{
			get { return (_draggedVertex != null); }
		}

		/// <summary>
		/// Indicates whether there is a vertex joining operation in progress.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is join operation in progress; otherwise, <c>false</c>.
		/// </value>
		private bool IsJoinOperationInProgress
		{
			get { return (IsEditing && !IsDragging && _selectedVertexPair_First != null); }
		}

		internal void ClearAllTempVars()
		{
			ClearJoinOperation();
			ClearDragOperation();
			ClearUndoRedoHistory();
		}

		private void ClearJoinOperation()
		{
			_selectedVertexPair_First = null;
			_selectedVertexPair_Second = null;
		}

		private void ClearDragOperation()
		{
			_draggedVertex = null;
			_dragStartPosition = new Point();
			_dragEndPosition = new Point();
		}

		private void ClearUndoRedoHistory()
		{
			MoveCount = 0;
			MoveHistory = new List<HistoricalMove>();
		}

		/// <summary>
		/// An enumeration of all vertices and line segments in the game level.
		/// </summary>
		public IEnumerable<object> GameObjects
		{
			get
			{
				var objects = new List<object>(GameGraph.Vertices);
				objects.AddRange(GameGraph.LineSegments);
				return objects;
			}
		}

		/// <summary>
		/// Gets or sets the game graph.
		/// </summary>
		/// <value>The game 
		public Graph GameGraph
		{
			get
			{
				return _gameGraph;
			}
			set
			{
				if (_gameGraph != value)
				{
					_gameGraph = value;
					OnPropertyChanged();
				}
			}
		}
		private Graph _gameGraph;

		/// <summary>
		/// The number of moves made in the game so far.
		/// </summary>
		public int MoveCount
		{
			get { return _moveCount; }
			set
			{
				if (_moveCount == value)
				{
					return;
				}

				_moveCount = value;
				OnPropertyChanged();
			}
		}
		private int _moveCount;

		/// <summary>
		/// Gets the move history. Act as an undo/redo buffer
		/// </summary>
		public List<HistoricalMove> MoveHistory { get; private set; }

		#endregion

		private GameLevel()
		{
			_draggedVertex = null;
			_dragStartPosition = new Point();
			_dragEndPosition = new Point();
			ClearJoinOperation();

			_moveCount = 0;
			MoveHistory = new List<HistoricalMove>();
		}

		public GameLevel(Graph gameGraph)
			: this()
		{
			GameGraph = gameGraph;

			gameGraph.VertexCollectionChanged += GameGraph_VerticesCollectionChanged;
			gameGraph.LineSegmentCollectionChanged += GameGraph_LineSegmentsCollectionChanged;
			gameGraph.IntersectionCollectionChanged += GameGraph_IntersectionCollectionChanged;
		}

		private void GameGraph_VerticesCollectionChanged(object sender, EventArgs e)
		{
			OnPropertyChanged("GameObjects");
		}

		private void GameGraph_LineSegmentsCollectionChanged(object sender, EventArgs e)
		{
			OnPropertyChanged("GameObjects");
		}

		private void GameGraph_IntersectionCollectionChanged(object sender, EventArgs e)
		{
			OnPropertyChanged("GameObjects");
			GameSolvedCheck();
		}

		/// <summary>
		/// Creates a new <see cref="GameLevel"/> instance from an enumeration of vertices loaded
		/// from a saved game file.
		/// </summary>
		/// <param name="savedVertices">An enumeration of vertices loaded from the saved game file.
		/// </param>
		/// <returns>The created game level instance.</returns>
		/// <remarks>
		/// <para>The loaded vertices' positions are preserved during the game level's
		/// initialization.</para>
		/// </remarks>
		public static GameLevel Create(string UID, IEnumerable<Untangle.ViewModels.Vertex> savedVertices)
		{
			var vertexMappings = new Dictionary<int, Vertex>();
			var lineSegments = new List<LineSegment>();

			bool hasColorsSpecified = !(savedVertices.All(v => v.ColorIndex == 0));
			foreach (Untangle.ViewModels.Vertex savedVertex in savedVertices)
			{
				var vertex = new Vertex(savedVertex.X, savedVertex.Y);
				vertex.Id = savedVertex.Id;

				if (hasColorsSpecified)
				{
					vertex.SetColor(savedVertex.Color);
				}
				else
				{
					vertex.ColorIndex = vertex.Id;
				}

				vertexMappings[savedVertex.Id] = vertex;
				foreach (int connectedVertexId in savedVertex.ConnectedVertexIds)
				{
					if (!vertexMappings.ContainsKey(connectedVertexId))
					{
						// The connected vertex has not been mapped to a vertex view model yet;
						// skip this line segment for now, it will be added when the other vertex
						// is enumerated
						continue;
					}

					Vertex otherVertex = vertexMappings[connectedVertexId];
					LineSegment lineSegment = Vertex.AddConnection(vertex, otherVertex);
					lineSegments.Add(lineSegment);
				}
			}

			Graph graph = new Graph(UID, vertexMappings.Values, lineSegments);
			graph.CalculateAllIntersections();

			return new GameLevel(graph);
		}

		public static int ColorTheGraph(IEnumerable<Vertex> graph)
		{
			return GetChromaticNumber(graph);
		}




		/// <summary>
		/// DSatur graph coloring algorithm.
		/// Let the "degree of saturation" of a vertex be the number of different colors being used by its neighbors.
		/// The algorithm operates as follows:
		///		1) Let v be the uncolored vertex in the graph with the highest degree of saturation. 
		///		   In cases of ties, choose the vertex among the uncolored vertices with the largest degree (LineSegmentCount).
		///		2) Assign v to the lowest color label not being used by any of its neighbors.
		///		3) If all vertices have been colored, then end; otherwise return to Step 1.
		///		
		/// As a result of step 1), vertices seen to be the most "constrained" are colored first.
		/// 
		/// </summary>
		/// <param name="graph">The graph of vertices.</param>
		/// <returns>The chromatic number of the graph.</returns>
		private static int GetChromaticNumber(IEnumerable<Vertex> graph)
		{
			foreach (Vertex blankMe in graph)
			{
				blankMe.SetColor(Brushes.Black);
			}

			Stack<Brush> unusedColors = new Stack<Brush>(ColorPalette.PrimaryColors.AsEnumerable().Reverse());
			List<Brush> usedColors = new List<Brush>() { unusedColors.Pop() };

			int colorCount = 1;
			while (true)
			{
				Vertex nextVertex = GetUncoloredVertexWithHighestSaturation(graph);
				if (nextVertex == null)
				{
					break;
				}

				bool success = false;
				while (nextVertex.Color == Brushes.Black)
				{
					foreach (Brush testColor in usedColors)
					{
						if (CanColor(nextVertex, testColor))
						{
							nextVertex.SetColor(testColor);
							success = true;
							break;
						}
					}

					if (success)
					{
						break;
					}

					usedColors.Add(unusedColors.Pop());
					colorCount++;
				}
			}

			return colorCount;
		}

		/// <summary>
		/// Get the uncolored vertex in the supplied graph with the highest degree of saturation.
		/// That is: Order by number of different colors being used by its neighbors, highest to lowest (descending).
		/// In cases of ties, choose the vertex among the uncolored vertices with the largest degree (LineSegments), descending.
		/// Return the vertex on the top.
		/// </summary>
		/// <param name="graph">The graph to choose from.</param>
		/// <returns>The next uncolored vertex to color, or null if there is none.</returns>
		private static Vertex GetUncoloredVertexWithHighestSaturation(IEnumerable<Vertex> graph)
		{
			// Return the uncolored vertex in the supplied graph [...]
			List<Vertex> uncoloredVertices = graph.Where(vertex => vertex.Color == Brushes.Black).ToList();

			List<Vertex> orderedVertices = uncoloredVertices
											.OrderByDescending
											// ... with the highest degree of saturation.
											(vertex =>
												// The count of distinct colors of ConnectedVertices that have color (not black).
												vertex.ConnectedVertices
														.Select(neighbor => neighbor.Color)
														.Where(color => color != Brushes.Black)
														.Distinct()
														.Count()
											)
											// In cases of ties, choose the vertex with the largest degree.
											.ThenByDescending(vert => vert.LineSegmentCount)
											// Finally, use lexicographic ordering (lowest first),
											//		to break any ties and make the ordering deterministic.
											.ThenBy(v => v.Name)
											.ToList();

			if (orderedVertices.Any())
			{
				return orderedVertices.First();
			}
			return null;
		}

		/// <summary>
		/// Can the vertex be assigned this color during graph coloring?
		/// This move is legal (return true) if none of its connected vertices have that color.
		/// </summary>
		/// <param name="testVertex">Vertex to check.</param>
		/// <param name="testColor">Color to check for.</param>
		/// <returns>True if none of the specified vertex's connected vertices are the test color.</returns>
		private static bool CanColor(Vertex testVertex, Brush testColor)
		{
			return !testVertex.ConnectedVertices
								.Select(v => v.Color)
								.Contains(testColor);
		}







		/// <summary>
		/// Creates a simple circular <see cref="GameLevel"/> instance from an enumeration of vertices
		/// generated by a <see cref="Generation.LevelGenerator"/> instance.
		/// </summary>
		/// <param name="generatedVertices">An enumeration of vertices generated by the game level
		/// generator.</param>
		/// <returns>The created game level instance.</returns>
		public static GameLevel Create(IEnumerable<Generation.Vertex> generatedVertices)
		{
			return Internal_Create(null, generatedVertices);
		}

		/// <summary>
		/// Creates a new <see cref="GameLevel"/> instance from an enumeration of vertices
		/// generated by a <see cref="Generation.LevelGenerator"/> instance.
		/// </summary>
		/// <param name="gameboardSize">The size of the gameboard to restrict layout of the vertices.</param>
		/// <param name="generatedVertices">An enumeration of vertices generated by the game level
		/// generator.</param>
		/// <returns>The created game level instance.</returns>
		public static GameLevel Create(System.Windows.Size gameboardSize, IEnumerable<Generation.Vertex> generatedVertices)
		{
			return Internal_Create(gameboardSize, generatedVertices);
		}

		private static GameLevel Internal_Create(System.Windows.Size? gameboardSize, IEnumerable<Generation.Vertex> generatedVertices)
		{
			var vertexMappings = new Dictionary<Generation.Vertex, Vertex>();
			var lineSegments = new List<LineSegment>();

			int currentId = 0;
			foreach (Generation.Vertex generatedVertex in generatedVertices)
			{
				var vertex = new Vertex();
				vertex.Id = currentId++;
				vertex.ColorIndex = vertex.Id;
				vertexMappings[generatedVertex] = vertex;
				foreach (Generation.Vertex connectedGeneratedVertex in generatedVertex.ConnectedVertices)
				{
					if (!vertexMappings.ContainsKey(connectedGeneratedVertex))
					{
						// The connected vertex has not been mapped to a vertex view model yet;
						// skip this line segment for now, it will be added when the other vertex
						// is enumerated
						continue;
					}

					Vertex otherVertex = vertexMappings[connectedGeneratedVertex];
					LineSegment lineSegment = Vertex.AddConnection(vertex, otherVertex);
					lineSegments.Add(lineSegment);
				}
			}

			Vertex[] vertices = vertexMappings.Values.ToArray();

			Graph graph = new Graph(vertices, lineSegments);
			GameLevel result = new GameLevel(graph);

			int intersectionCount = 0;
			while (intersectionCount == 0)
			{
				if (gameboardSize != null && gameboardSize.HasValue)
				{
					intersectionCount = GraphLayout.SelectRandomLayout(graph, gameboardSize.Value);
				}
				else
				{
					intersectionCount = GraphLayout.Circle(graph);
				}
			}

			return result;
		}

		#region Edit Mode

		/// <summary>
		/// Places the instance into level editing mode.
		/// </summary>
		public void EnterEditMode()
		{
			IsEditing = true;
			ClearJoinOperation();
		}

		/// <summary>
		/// Ends level editing mode for this instance.
		/// </summary>
		public void ExitEditMode()
		{
			IsEditing = false;

			ClearAllTempVars();

			GameGraph = new Graph(GameGraph.Vertices, GameGraph.LineSegments);
			GameGraph.CalculateAllIntersections();
		}

		public void Edit_RandomizeVertices(System.Windows.Size size)
		{
			GraphLayout.SelectRandomLayout(GameGraph, size);
		}

		/// <summary>
		/// Creates a new vertex when in level editing mode.
		/// </summary>
		/// <param name="position">The position.</param>
		public void Edit_CreateVertex(Point position)
		{
			if (IsJoinOperationInProgress)
			{
				ClearJoinOperation();
			}

			GameGraph.AddVertex(position);
		}

		/// <summary>
		/// Allows you to join two vertices with an edge using the GUI when in level editing mode.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		public void Edit_JoinVertexSelect(Vertex vertex)
		{
			if (!IsEditing || IsDragging)
			{
				return;
			}

			if (_selectedVertexPair_Second == null)
			{
				if (_selectedVertexPair_First == null)
				{
					_selectedVertexPair_First = vertex;
					ChangeVertexState(_selectedVertexPair_First, VertexState.ConnectedToHighlighted);
				}
				else if (_selectedVertexPair_First.Id == vertex.Id)
				{
					ClearJoinOperation();
					return;
				}
				else
				{
					_selectedVertexPair_Second = vertex;
					ChangeVertexState(_selectedVertexPair_Second, VertexState.ConnectedToHighlighted);
				}
			}

			if (_selectedVertexPair_First != null && _selectedVertexPair_Second != null)
			{
				if (_selectedVertexPair_First.Id != _selectedVertexPair_Second.Id)
				{
					if (_selectedVertexPair_First.ConnectedVertexIds.Contains(_selectedVertexPair_Second.Id))
					{
						GameGraph.RemoveEdge(_selectedVertexPair_First, _selectedVertexPair_Second);
					}
					else
					{
						GameGraph.AddEdge(_selectedVertexPair_First, _selectedVertexPair_Second);
					}
				}

				ClearJoinOperation();
			}
		}

		/// <summary>
		/// Removes a vertex when in level editing mode.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		public void Edit_DeleteVertex(Vertex vertex)
		{
			if (IsEditing && !IsDragging)
			{
				ClearJoinOperation();
				GameGraph.RemoveVertex(vertex);
			}
		}

		#endregion

		#region Click and Drag

		/// <summary>
		/// Sets the vertex which is currently under the mouse cursor.
		/// </summary>
		/// <param name="vertex">The vertex which is currently under the mouse cursor.</param>
		public void SetVertexUnderMouse(Vertex vertex)
		{
			if (_vertexUnderMouse == vertex)
			{
				return;
			}

			if (_vertexUnderMouse != null && !IsDragging /*&& _vertexUnderMouse.State == VertexState.UnderMouse**/)
			{
				ChangeVertexState(_vertexUnderMouse, VertexState.Normal);
			}

			_vertexUnderMouse = vertex;

			if (_vertexUnderMouse != null && !IsDragging)
			{
				ChangeVertexState(_vertexUnderMouse, VertexState.UnderMouse);
			}
		}

		/// <summary>
		/// Initiates the dragging of a specified vertex requested by the user.
		/// </summary>
		/// <param name="draggedVertex">The vertex which should be dragged.</param>
		public void StartDrag(Vertex draggedVertex)
		{
			_draggedVertex = draggedVertex;
			_draggedVertex.State = VertexState.Dragged;
			_dragStartPosition = _draggedVertex.GetPosition();
		}

		/// <summary>
		/// Moves the vertex which is currently being dragged, to another position on the game
		/// field.
		/// </summary>
		/// <param name="position">The new position of the dragged vertex.</param>
		public void ContinueDrag(Point position)
		{
			_draggedVertex.SetPosition(position);
		}

		/// <summary>
		/// Completes the dragging of the vertex which is currently being dragged, and
		/// recalculates all current and potential intersections which might have been affected by
		/// the dragging.
		/// </summary>
		public void FinishDrag()
		{
			if (IsDragging)
			{
				_dragEndPosition = _draggedVertex.GetPosition();
				ChangeVertexState(_draggedVertex, VertexState.Normal);
				GameGraph.RecalculateIntersections(_draggedVertex);

				AddMoveToHistory(_draggedVertex.Id, _dragStartPosition, _dragEndPosition);

				_draggedVertex = null;
			}

			if (_vertexUnderMouse != null)
			{
				ChangeVertexState(_vertexUnderMouse, VertexState.UnderMouse);
			}

			_priorShrunkSegments.Clear();

			GameSolvedCheck();
		}

		#endregion

		#region Sending Vertices To Start Position

		public void MarkVerticesInStartPosition()
		{
			if (GameGraph == null || GameGraph.Vertices == null || !GameGraph.Vertices.Any())
			{
				return;
			}

			foreach (Vertex vertex in GameGraph.Vertices)
			{
				if (vertex.StartingPosition.HasValue)
				{
					if (vertex.AtStartPosition)
					{
						vertex.SetColor(Brushes.LightGray);
					}
					else
					{
						vertex.SetColor(Brushes.Black);
					}
				}

			}
		}

		public void AttemptresetVerticesToStartPositionWithoutCrossings()
		{
			if (GameGraph == null || GameGraph.Vertices == null || !GameGraph.Vertices.Any())
			{
				return;
			}

			foreach (Vertex vertex in GameGraph.Vertices)
			{
				AttemptResetVertexToStartingPosition(vertex);
			}
		}

		public void ResetVerticesToStartPosition()
		{
			if (GameGraph == null || GameGraph.Vertices == null || !GameGraph.Vertices.Any())
			{
				return;
			}

			foreach (Vertex vertex in GameGraph.Vertices)
			{
				ResetVertexToStartingPosition(vertex);
			}
		}

		public void ResetVertexToStartingPosition(Vertex vertex)
		{
			if (!vertex.StartingPosition.HasValue || vertex.AtStartPosition)
			{
				return;
			}

			AddMoveToHistory(vertex.Id, vertex.GetPosition(), vertex.StartingPosition.Value);


			Point fromPoint = vertex.GetPosition();
			Point toPoint = vertex.StartingPosition.Value;

			DoubleAnimation xAnimation = new DoubleAnimation(fromPoint.X, toPoint.X,
				Animation_Duration, FillBehavior.Stop)
			{ EasingFunction = Animation_EasingFunction, AutoReverse = false };
			DoubleAnimation yAnimation = new DoubleAnimation(fromPoint.Y, toPoint.Y,
				Animation_Duration, FillBehavior.Stop)
			{ EasingFunction = Animation_EasingFunction, AutoReverse = false };



			yAnimation.Completed += (s, e) =>
			{
				using (var d = Dispatcher.DisableProcessing())
				{
					this.Dispatcher.BeginInvoke(new Action(
					() =>
					{
						vertex.SetValueSync(Vertex.XProperty, toPoint.X);
						vertex.SetValueSync(Vertex.YProperty, toPoint.Y);
					}
					), DispatcherPriority.Render);
				}
			};

			vertex.BeginAnimation(Vertex.XProperty, xAnimation);
			vertex.BeginAnimation(Vertex.YProperty, yAnimation);
		}

		public void AttemptResetVertexToStartingPosition(Vertex vertex)
		{
			if (!vertex.StartingPosition.HasValue || vertex.AtStartPosition)
			{
				return;
			}

			Point fromPoint = vertex.GetPosition();
			Point toPoint = vertex.StartingPosition.Value;

			DoubleAnimation xAnimation = new DoubleAnimation(fromPoint.X, toPoint.X,
				Animation_Duration, FillBehavior.Stop)
			{ EasingFunction = Animation_EasingFunction, AutoReverse = false };
			DoubleAnimation yAnimation = new DoubleAnimation(fromPoint.Y, toPoint.Y,
				Animation_Duration, FillBehavior.Stop)
			{ EasingFunction = Animation_EasingFunction, AutoReverse = false };

			int intersectionCountBefore = GameGraph.IntersectionCount;

			yAnimation.Completed += (s, e) =>
			{
				using (var d = Dispatcher.DisableProcessing())
				{
					this.Dispatcher.BeginInvoke(new Action(
					() =>
					{
						GameGraph.CalculateAllIntersections();

						int intersectionCountAfter = GameGraph.IntersectionCount;
						if (intersectionCountAfter <= intersectionCountBefore)
						{
							vertex.SetValueSync(Vertex.XProperty, toPoint.X);
							vertex.SetValueSync(Vertex.YProperty, toPoint.Y);
						}
						else
						{
							foreach (var linesegment in vertex.LineSegments)
							{
								linesegment.State = LineSegmentState.Intersected;
							}

							DoubleAnimation xReverseAnimation = new DoubleAnimation(toPoint.X, fromPoint.X,
				Animation_Duration, FillBehavior.Stop)
							{ EasingFunction = Animation_EasingFunction, AutoReverse = false };
							DoubleAnimation yReverseAnimation = new DoubleAnimation(toPoint.Y, fromPoint.Y,
								Animation_Duration, FillBehavior.Stop)
							{ EasingFunction = Animation_EasingFunction, AutoReverse = false };

							yReverseAnimation.Completed += (s, e) =>
							{
								vertex.SetValueSync(Vertex.XProperty, fromPoint.X);
								vertex.SetValueSync(Vertex.YProperty, fromPoint.Y);
								GameGraph.CalculateAllIntersections();
							};

							vertex.BeginAnimation(Vertex.XProperty, xReverseAnimation, HandoffBehavior.Compose);
							vertex.BeginAnimation(Vertex.YProperty, yReverseAnimation, HandoffBehavior.Compose);
						}
					}
					), DispatcherPriority.Render);
				}
			};

			vertex.BeginAnimation(Vertex.XProperty, xAnimation);
			vertex.BeginAnimation(Vertex.YProperty, yAnimation);
		}

		#endregion

		#region Undo/Redo

		private void AddMoveToHistory(int vertexID, Point from, Point to)
		{
			MoveCount = MoveCount + 1;
			HistoricalMove move = new HistoricalMove(GameGraph.UID, MoveCount, vertexID, from, to);

			if (MoveHistory.Count >= MoveCount)
			{
				MoveHistory = MoveHistory.Take(MoveCount - 1).ToList();
			}
			MoveHistory.Add(move);
		}


		/// <summary>
		/// Undoes the last move from the undo buffer
		/// </summary>
		public void UndoMove()
		{
			if (MoveCount == 0)
			{
				return;
			}

			HistoricalMove doMove = MoveHistory.ElementAtOrDefault(MoveCount - 1);
			if (doMove == default(HistoricalMove))
			{
				return;
			}
			if (doMove.GameUID != GameGraph.UID)
			{
				throw new Exception("Attempting to apply move history to a different game!");
			}

			Vertex moveVertex = GameGraph.Vertices.FirstOrDefault(v => v.Id == doMove.VertexId);
			if (moveVertex == default(Vertex))
			{
				throw new Exception($"No matching VertexID '{doMove.VertexId}' found for historical move #{doMove.MoveNumber}?");
			}

			MoveCount = MoveCount - 1;

			moveVertex.SetValueSync(Vertex.XProperty, doMove.FromPosition.X);
			moveVertex.SetValueSync(Vertex.YProperty, doMove.FromPosition.Y);

			GameGraph.RecalculateIntersections(moveVertex);
			GameSolvedCheck();
		}

		#endregion

		/// <summary>
		/// Check if the game is solved, and if it is, raises the <see cref="LevelSolved"/> event.
		/// </summary>
		public bool GameSolvedCheck()
		{
			if (!IsEditing)
			{
				if (GameGraph.IntersectionCount == 0)
				{
					OnLevelSolved();
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Raises the <see cref="LevelSolved"/> event.
		/// </summary>
		private void OnLevelSolved()
		{
			if (LevelSolved != null)
			{
				LevelSolved(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Changes the current state of a vertex and possibly the states of vertices which are
		/// directly connected to it and line segments which are attached to it.
		/// </summary>
		/// <param name="vertex">The vertex whose state should be changed.</param>
		/// <param name="newState">The new state of the vertex.</param>
		private void ChangeVertexState(Vertex vertex, VertexState newState)
		{
			VertexState oldState = vertex.State;
			vertex.State = newState;

			if (_selectedVertexPair_First != null && vertex.Id == _selectedVertexPair_First.Id)
			{
				return;
			}
			else if (newState == VertexState.Dragged || newState == VertexState.UnderMouse)
			{
				if (oldState == VertexState.Normal)
				//(oldState != VertexState.Dragged && oldState != VertexState.UnderMouse)
				{
					// The vertex was neither under the mouse, nor was it being dragged by the
					// user, but now either of those events has occurred
					foreach (Vertex connectedVertex in vertex.ConnectedVertices)
					{
						connectedVertex.State = VertexState.ConnectedToHighlighted;
					}
					foreach (LineSegment lineSegment in vertex.LineSegments)
					{
						lineSegment.State = LineSegmentState.Highlighted;
					}
				}
			}
			else if (newState == VertexState.Normal)
			{
				// The vertex was under the mouse or was being dragged by the user, but
				// that is no longer the case
				foreach (Vertex connectedVertex in vertex.ConnectedVertices)
				{
					connectedVertex.State = VertexState.Normal;
				}
				foreach (LineSegment lineSegment in vertex.LineSegments)
				{
					lineSegment.State = (GameGraph.Intersections[lineSegment].Count > 0
											? LineSegmentState.Intersected
											: LineSegmentState.Normal);
				}
			}
		}


		private List<LineSegment> _priorShrunkSegments = new List<LineSegment>();

		public void ShrinkLongestEdge()
		{
			int tries = 10;
			while (tries-- > 0)
			{
				if (TryShrinkLongestEdge())
				{
					break;
				}
			}
		}

		private bool TryShrinkLongestEdge()
		{
			if (GameSolvedCheck())
			{
				return true;
			}

			IEnumerable<KeyValuePair<LineSegment, HashSet<LineSegment>>> candidates = new List<KeyValuePair<LineSegment, HashSet<LineSegment>>>();
			while (true)
			{
				candidates = GameGraph.Intersections
								.Where(kvp => kvp.Value.Count > 0)
								.Where(kvp => !_priorShrunkSegments.Contains(kvp.Key));

				if (candidates.Any())
				{
					break;
				}
				else
				{
					if (_priorShrunkSegments.Any())
					{
						_priorShrunkSegments.Clear();
					}
					else
					{
						return false;
					}
				}
			}

			KeyValuePair<LineSegment, HashSet<LineSegment>> longestIntersection =
				candidates
				.OrderBy(kvp => kvp.Value.Count)
				.ThenByDescending(kvp => kvp.Key.Length)
				.First();

			LineSegment currentSegment = longestIntersection.Key;
			_priorShrunkSegments.Add(currentSegment);

			List<LineSegment> intersectingSegments = longestIntersection.Value.ToList();

			double vertex1Min = double.MaxValue;
			Point vertex1Intersection = currentSegment.Vertex2.GetPosition();

			double vertex2Min = double.MaxValue;
			Point vertex2Intersection = currentSegment.Vertex1.GetPosition();

			foreach (LineSegment intersecting in intersectingSegments)
			{
				Point intersection = CalculationHelper.GetIntersectionPoint(currentSegment.Point1, currentSegment.Point2, intersecting.Point1, intersecting.Point2);

				if (intersection.X == double.NaN)
				{
					continue;
				}

				double distance1 = LineSegment.Distance(currentSegment.Point1, intersection);
				double distance2 = LineSegment.Distance(currentSegment.Point2, intersection);

				if (distance1 < vertex1Min)
				{
					vertex1Min = distance1;
					vertex1Intersection = intersection;
				}
				if (distance2 < vertex2Min)
				{
					vertex2Min = distance2;
					vertex2Intersection = intersection;
				}
			}

			Vertex closest;
			Vertex farthest;
			Point farthestIntersection;

			if (vertex1Min <= vertex2Min)
			{
				closest = currentSegment.Vertex1;
				farthest = currentSegment.Vertex2;
				farthestIntersection = vertex2Intersection;
			}
			else
			{
				closest = currentSegment.Vertex2;
				farthest = currentSegment.Vertex1;
				farthestIntersection = vertex1Intersection;
			}

			var newPosition = LineSegment.MidPoint(farthest.GetPosition(), farthestIntersection);

			bool result = MoveAndCheckWithRollback(closest, currentSegment, newPosition);

			if (result)
			{
				Point pos = closest.GetPosition();
				//Rect box = new Rect(pos, new Vector(, )
			}

			return result;
		}

		private bool MoveAndCheckWithRollback(Vertex vertexToMove, LineSegment lineSegment, Point newPosition)
		{
			int oldIntersectionCount = GameGraph.IntersectionCount;
			Point oldPosition = vertexToMove.GetPosition();

			vertexToMove.SetPosition(newPosition);
			GameGraph.CalculateAllIntersections();
			int newIntersectionCount = GameGraph.IntersectionCount;

			if (newIntersectionCount > oldIntersectionCount)
			{
				// Roll back
				vertexToMove.SetPosition(oldPosition);
				GameGraph.CalculateAllIntersections();
				return false;
			}
			return true;
		}

		protected override Freezable CreateInstanceCore()
		{
			return new GameLevel();
		}








	}
}
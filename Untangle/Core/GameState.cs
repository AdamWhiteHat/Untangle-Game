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
using System.Collections.Generic;
using System.Collections.Immutable;
using Untangle.Enums;
using System.Windows.Media;
using Untangle.Generation;
using Untangle.Utils;
using System.Windows.Media.Animation;
using XAMLMarkupExtensions.Base;
using System.Windows.Threading;
using Microsoft.Msagl.Core.DataStructures;
using System.Collections.ObjectModel;
using Microsoft.Msagl.Core.Geometry.Curves;
using System.Windows.Forms;
using System.Timers;
using System.Threading;

namespace Untangle.Core
{
	/// <summary>
	/// A view model class for a single game level in a game of Untangle.
	/// </summary>
	public class GameState : ViewModelBase
	{
		#region Properties and Fields

		/// <summary>
		/// Occurs when the user has successfully solved the level by removing all intersections
		/// between line segments.
		/// </summary>
		public event EventHandler LevelSolved;

		/// <summary>
		/// Occurs when the player has moved a vertex, ie made a move in the game.
		/// </summary>
		public event EventHandler PlayerMoved;

		/// <summary>
		/// The undo move stack
		/// </summary>
		public ObservableStack<HistoricalMove> UndoStack { get; private set; }

		/// <summary>
		/// The redo move stack
		/// </summary>
		public ObservableStack<HistoricalMove> RedoStack { get; private set; }

		/// <summary>
		/// An enumeration of all vertices and line segments in the game level.
		/// </summary>
		public IEnumerable<object> GameObjects
		{
			get
			{
				List<object> result = new List<object>(Graph.Vertices);
				result.AddRange(Graph.LineSegments);
				result.AddRange(Graph.NonInteractiveLines);
				return result;
			}
		}

		/// <summary>
		/// Specifies whether a vertex is currently being dragged by the user.
		/// </summary>
		public bool IsDragging
		{
			get { return (_draggedVertex != null); }
		}

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
					RaisePropertyChanged();
				}
			}
		}
		private bool _isEditing = false;

		/// <summary>
		/// Gets or sets the game graph.
		/// </summary>
		/// <value>The game 
		public GameGraph Graph
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
					RaisePropertyChanged();
				}
			}
		}
		private GameGraph _gameGraph;

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
				RaisePropertyChanged();
			}
		}
		private int _moveCount;

		/// <summary>
		/// A counter of the number of seconds that have elapsed since the game was last solved.
		/// </summary>
		public int SecondsElapsed
		{
			get { return _secondsElapsed; }
			set
			{
				if (_secondsElapsed == value)
				{
					return;
				}

				_secondsElapsed = value;
				RaisePropertyChanged(nameof(SecondsElapsed));
			}
		}
		private int _secondsElapsed;

		private System.Windows.Forms.Timer SecondsElapsedTimer = null;

		public bool HasGameStarted
		{
			get { return _hasGameStarted; }
			set
			{
				if (_hasGameStarted == value)
				{
					return;
				}

				_hasGameStarted = value;
				RaisePropertyChanged();
			}
		}
		private bool _hasGameStarted = false;

		/// <summary>
		/// A counter of the number of times this game board has been solved.
		/// </summary>
		public int SolveCount
		{
			get { return _solveCount; }
			set
			{
				if (_solveCount == value)
				{
					return;
				}

				_solveCount = value;
				RaisePropertyChanged();
			}
		}
		private int _solveCount;

		/// <summary>
		/// The current game level's number, which serves to represent a game's approximate difficulty level.
		/// Many distinct games can have the same LevelNumber.
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

		#region Private

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
		/// Indicates whether there is a vertex joining operation in progress.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is join operation in progress; otherwise, <c>false</c>.
		/// </value>
		private bool IsJoinOperationInProgress
		{
			get { return (IsEditing && !IsDragging && _selectedVertexPair_First != null); }
		}

		#endregion

		#endregion

		#region Create & Constructors

		internal GameState()
		{
			_isEditing = false;
			_levelNumber = 1;
			_gameGraph = new GameGraph();

			MoveCount = 0;
			SecondsElapsed = 0;
			HasGameStarted = false;

			if (SecondsElapsedTimer != null)
			{
				SecondsElapsedTimer.Tick -= SecondsElapsedTimer_Tick;
				SecondsElapsedTimer.Stop();
				SecondsElapsedTimer.Dispose();
			}
			SecondsElapsedTimer = new System.Windows.Forms.Timer();
			SecondsElapsedTimer.Enabled = false;
			SecondsElapsedTimer.Interval = 1000;
			SecondsElapsedTimer.Tick += SecondsElapsedTimer_Tick;

			ClearAllTempVars();
			ClearUndoRedoHistory();
		}

		private void SecondsElapsedTimer_Tick(object sender, EventArgs e)
		{
			SecondsElapsed += 1;
		}

		private GameState(GameGraph gameGraph)
			: this()
		{
			Graph = gameGraph;

			UndoStack = new ObservableStack<HistoricalMove>();
			RedoStack = new ObservableStack<HistoricalMove>();

			gameGraph.VertexCollectionChanged += GameGraph_VerticesCollectionChanged;
			gameGraph.LineSegmentCollectionChanged += GameGraph_LineSegmentsCollectionChanged;
			gameGraph.IntersectionCollectionChanged += GameGraph_IntersectionCollectionChanged;
		}

		public static GameState Create(System.Windows.Size gameboardSize, int levelNumber)
		{
			var levelGenerator = new LevelGenerator(1 + levelNumber, 2 + levelNumber, 4, 2);
			return levelGenerator.GenerateLevel(gameboardSize);
		}

		/// <summary>
		/// Creates a new <see cref="GameState"/> instance from an enumeration of vertices loaded
		/// from a saved game file.
		/// </summary>
		/// <param name="savedVertices">An enumeration of vertices loaded from the saved game file.
		/// </param>
		/// <returns>The created game level instance.</returns>
		/// <remarks>
		/// <para>The loaded vertices' positions are preserved during the game level's
		/// initialization.</para>
		/// </remarks>
		public static GameState Create(string UID, IEnumerable<Untangle.Core.Vertex> savedVertices)
		{
			var vertexMappings = new Dictionary<int, Vertex>();
			var lineSegments = new List<LineSegment>();

			bool hasColorsSpecified = !(savedVertices.All(v => v.ColorIndex == 0));
			foreach (Untangle.Core.Vertex savedVertex in savedVertices)
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

			GameGraph graph = new GameGraph(UID, vertexMappings.Values, lineSegments);
			graph.CalculateAllIntersections();

			return new GameState(graph);
		}

		/// <summary>
		/// Creates a simple circular <see cref="GameState"/> instance from an enumeration of vertices
		/// generated by a <see cref="Generation.LevelGenerator"/> instance.
		/// </summary>
		/// <param name="generatedVertices">An enumeration of vertices generated by the game level
		/// generator.</param>
		/// <returns>The created game level instance.</returns>
		public static GameState Create(IEnumerable<Generation.Vertex> generatedVertices)
		{
			return Internal_Create(null, generatedVertices);
		}

		/// <summary>
		/// Creates a new <see cref="GameState"/> instance from an enumeration of vertices
		/// generated by a <see cref="Generation.LevelGenerator"/> instance.
		/// </summary>
		/// <param name="gameboardSize">The size of the gameboard to restrict layout of the vertices.</param>
		/// <param name="generatedVertices">An enumeration of vertices generated by the game level
		/// generator.</param>
		/// <returns>The created game level instance.</returns>
		public static GameState Create(System.Windows.Size gameboardSize, IEnumerable<Generation.Vertex> generatedVertices)
		{
			return Internal_Create(gameboardSize, generatedVertices);
		}

		private static GameState Internal_Create(System.Windows.Size? gameboardSize, IEnumerable<Generation.Vertex> generatedVertices)
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

			GameGraph graph = new GameGraph(vertices, lineSegments);
			GameState result = new GameState(graph);

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

		#endregion

		#region Edit Mode

		/// <summary>
		/// Places the instance into level editing mode.
		/// </summary>
		public void EnterEditMode()
		{
			if (!IsEditing)
			{
				IsEditing = true;
				ClearAllTempVars();
			}
		}

		/// <summary>
		/// Ends level editing mode for this instance.
		/// </summary>
		public void ExitEditMode()
		{
			if (IsEditing)
			{
				IsEditing = false;

				ClearAllTempVars();

				Graph = new GameGraph(Graph.Vertices, Graph.LineSegments);
				Graph.CalculateAllIntersections();
			}
		}

		public void Edit_RandomizeVerticesLocations(System.Windows.Size size)
		{
			GraphLayout.SelectRandomLayout(Graph, size);
		}

		public void Edit_RescaleAllVertices(double scale)
		{
			GraphLayout.ReScaleAllVertices(Graph, scale);
		}

		public void Edit_RecenterVertices(System.Windows.Size size)
		{
			GraphLayout.RecenterVertices(Graph, size);
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

			Graph.AddVertex(position);
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
						Graph.RemoveEdge(_selectedVertexPair_First, _selectedVertexPair_Second);
					}
					else
					{
						Graph.AddEdge(_selectedVertexPair_First, _selectedVertexPair_Second);
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
			if (!IsEditing || IsDragging)
			{
				return;
			}

			ClearJoinOperation();
			Graph.RemoveVertex(vertex);
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
				AddMoveToHistory(_draggedVertex, _dragStartPosition, _dragEndPosition);

				Graph.RecalculateIntersections(_draggedVertex);

				_draggedVertex = null;
				if (!HasGameStarted)
				{
					HasGameStarted = true;
					SecondsElapsedTimer.Start();
				}
				RaisedPlayerMoved();
			}

			if (_vertexUnderMouse != null)
			{
				ChangeVertexState(_vertexUnderMouse, VertexState.UnderMouse);
			}

			_priorShrunkSegments.Clear();
		}

		#endregion

		#region High-level Graph Manipulation Functions

		#region Sending Vertices To Start Position

		public void MarkVerticesInStartPosition()
		{
			if (Graph == null || Graph.Vertices == null || !Graph.Vertices.Any())
			{
				return;
			}

			foreach (Vertex vertex in Graph.Vertices)
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

		#endregion

		#region Shrink Edges

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

		private List<LineSegment> _priorShrunkSegments = new List<LineSegment>();

		private bool TryShrinkLongestEdge()
		{
			if (GameSolvedCheck())
			{
				return true;
			}

			IEnumerable<KeyValuePair<LineSegment, HashSet<LineSegment>>> candidates = new List<KeyValuePair<LineSegment, HashSet<LineSegment>>>();
			while (true)
			{
				candidates = Graph.Intersections
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

				if (double.IsNaN(intersection.X))
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
			int oldIntersectionCount = Graph.IntersectionCount;
			Point oldPosition = vertexToMove.GetPosition();

			vertexToMove.SetPosition(newPosition);
			Graph.CalculateAllIntersections();
			int newIntersectionCount = Graph.IntersectionCount;

			if (newIntersectionCount > oldIntersectionCount)
			{
				// Roll back
				vertexToMove.SetPosition(oldPosition);
				Graph.CalculateAllIntersections();
				return false;
			}
			return true;
		}

		#endregion

		#region Chromatic Number

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
		public static int GetChromaticNumber(IEnumerable<Vertex> graph)
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




		#endregion
		#endregion
		#region Undo/Redo

		public void AddMoveToHistory(Vertex vertex, Point from, Point to)
		{
			MoveCount++;

			HistoricalMove currentMove = new HistoricalMove(Graph.UID, vertex, from, to);
			UndoStack.Push(currentMove);

			var toDelete = RedoStack.Where(mv => mv.Vertex == vertex).ToList();

			if (toDelete.Any())
			{
				int count = toDelete.Count;
				int index = -1;

				while (++index < count)
				{
					RedoStack.Remove(toDelete[index]);
				}

				foreach (var move in toDelete)
				{
					move.DeleteSelf();
				}
			}

			HistoricalMove lastMove = currentMove.Vertex.HistoryStack.Peek();

			if (lastMove != null)
			{
				currentMove.PreviousMove = lastMove;
				lastMove.NextMove = currentMove;
			}

			currentMove.Vertex.HistoryStack.Push(currentMove);

			//if (MoveHistory.Count >= MoveCount)
			//{
			//	MoveHistory = MoveHistory.Take(MoveCount - 1).ToList();
			//}
			//MoveHistory.Add(move);
		}


		#endregion

		#region Internal Management/Methods

		#region Event Raising Methods

		/// <summary>
		/// Raises the <see cref="LevelSolved"/> event.
		/// </summary>
		private void RaiseLevelSolved()
		{
			if (LevelSolved != null)
			{
				LevelSolved(this, EventArgs.Empty);
			}
		}

		private void RaisedPlayerMoved()
		{
			if (PlayerMoved != null)
			{
				PlayerMoved(this, EventArgs.Empty);
			}
		}

		private void GameGraph_VerticesCollectionChanged(object sender, EventArgs e)
		{
			RaisePropertyChanged(nameof(GameObjects));
		}

		private void GameGraph_LineSegmentsCollectionChanged(object sender, EventArgs e)
		{
			RaisePropertyChanged(nameof(GameObjects));
		}

		private void GameGraph_IntersectionCollectionChanged(object sender, EventArgs e)
		{
			RaisePropertyChanged(nameof(GameObjects));
			GameSolvedCheck();
		}

		#endregion

		#region Internal Clear Function

		internal void ClearAllTempVars()
		{
			ClearJoinOperation();
			ClearDragOperation();
		}

		private void ClearJoinOperation()
		{
			_selectedVertexPair_First = null;
			_selectedVertexPair_Second = null;
		}

		private void ClearDragOperation()
		{
			_draggedVertex = null;
			_vertexUnderMouse = null;
			_dragStartPosition = new Point();
			_dragEndPosition = new Point();
		}

		private void ClearUndoRedoHistory()
		{
			MoveCount = 0;
			if (UndoStack == null)
			{
				UndoStack = new ObservableStack<HistoricalMove>();
			}
			if (RedoStack == null)
			{
				RedoStack = new ObservableStack<HistoricalMove>();
			}

			UndoStack.Clear();
			RedoStack.Clear();
			//MoveHistory = new List<HistoricalMove>();
		}

		#endregion

		/// <summary>
		/// Check if the game is solved, and if it is, raises the <see cref="LevelSolved"/> event.
		/// </summary>
		public bool GameSolvedCheck()
		{
			if (!IsEditing)
			{
				if (Graph.IntersectionCount == 0)
				{
					SecondsElapsedTimer.Stop();
					RaiseLevelSolved();
					return true;
				}
			}
			return false;
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
					lineSegment.State = (Graph.Intersections[lineSegment].Count > 0
											? LineSegmentState.Intersected
											: LineSegmentState.Normal);
				}
			}
		}

		#endregion

		protected override Freezable CreateInstanceCore()
		{
			return new GameState();
		}

	}
}
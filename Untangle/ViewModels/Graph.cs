using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Untangle.Enums;
using System.ComponentModel;
using Untangle.Generation;

namespace Untangle.ViewModels
{
	public class Graph : ViewModelBase
	{
		public event EventHandler VertexCollectionChanged;

		public event EventHandler LineSegmentCollectionChanged;

		public event EventHandler IntersectionCollectionChanged;

		/// <summary>
		/// A unique ID value generated for each new game.
		/// Primarily used when applying automated moves to a level
		/// to prevent applying moves to the wrong game board.
		/// </summary>
		public string UID { get; private set; }

		/// <summary>
		/// The number of vertices in the game level.
		/// </summary>
		public int VertexCount
		{
			get { return Vertices.Length; }
		}

		/// <summary>
		/// The vertices in the game level.
		/// </summary>
		public Vertex[] Vertices
		{
			get
			{
				if (_vertices == null)
				{
					_vertices = _verticesList.ToArray();
				}
				return _vertices;
			}
		}
		private Vertex[] _vertices = null;

		private ObservableCollection<Vertex> _verticesList;

		/// <summary>
		/// The line segments in the game level.
		/// </summary>
		public LineSegment[] LineSegments
		{
			get
			{
				if (_lineSegments == null)
				{
					_lineSegments = _lineSegmentsList.ToArray();
				}
				return _lineSegments;
			}
		}
		private LineSegment[] _lineSegments = null;

		private ObservableCollection<LineSegment> _lineSegmentsList;

		/// <summary>
		/// The number of intersections remaining in the game level.
		/// </summary>
		public int IntersectionCount
		{
			get { return _intersectionCount; }
			set
			{
				if (_intersectionCount == value)
				{
					return;
				}

				_intersectionCount = value;
				OnPropertyChanged();
			}
		}
		private int _intersectionCount;

		/// <summary>
		/// A dictionary containing a set of intersecting line segments for each line segment
		/// in the game level.
		/// </summary>
		public ReadOnlyDictionary<LineSegment, HashSet<LineSegment>> Intersections
		{
			get { return new ReadOnlyDictionary<LineSegment, HashSet<LineSegment>>(_intersections); }
		}
		private Dictionary<LineSegment, HashSet<LineSegment>> _intersections;

























		public Graph(IEnumerable<Vertex> vertices, IEnumerable<LineSegment> lineSegments)
			: this(GenerateNewUID(), vertices, lineSegments)
		{
		}

		public Graph(string uid, IEnumerable<Vertex> vertices, IEnumerable<LineSegment> lineSegments)
		{
			UID = uid;
			_verticesList = new ObservableCollection<Vertex>(vertices);
			_lineSegmentsList = new ObservableCollection<LineSegment>(lineSegments);

			_verticesList.CollectionChanged += CollectionChanged_Vertices;
			_lineSegmentsList.CollectionChanged += CollectionChanged_LineSegments;

			_intersections = new Dictionary<LineSegment, HashSet<LineSegment>>();
			foreach (LineSegment lineSegment in _lineSegmentsList)
			{
				_intersections[lineSegment] = new HashSet<LineSegment>();
			}
			IntersectionCount = 0;

			CalculateAllIntersections();
		}
















		/// <summary>
		/// Handles the CollectionChanged event for the Vertices property.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void CollectionChanged_Vertices(object sender, NotifyCollectionChangedEventArgs e)
		{
			_vertices = null;
			OnVertexCollectionChanged();
			OnPropertyChanged("VertexCount");
		}

		/// <summary>
		/// Handles the CollectionChanged for the LineSegments property.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void CollectionChanged_LineSegments(object sender, NotifyCollectionChangedEventArgs e)
		{
			_lineSegments = null;
			OnLineSegmentCollectionChanged();
		}

		protected void OnVertexCollectionChanged()
		{
			if (VertexCollectionChanged != null)
			{
				VertexCollectionChanged(this, EventArgs.Empty);
			}
		}

		protected void OnLineSegmentCollectionChanged()
		{
			if (LineSegmentCollectionChanged != null)
			{
				LineSegmentCollectionChanged(this, EventArgs.Empty);
			}
		}

		protected void OnIntersectionCollectionChanged()
		{
			if (IntersectionCollectionChanged != null)
			{
				IntersectionCollectionChanged(this, EventArgs.Empty);
			}
		}

		private static string GenerateNewUID()
		{
			return System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName());
		}
























		/// <summary>
		/// Adds a new, totally disconnected, vertex to the graph.
		/// </summary>
		/// <param name="location">The location.</param>
		public Vertex AddVertex(Point location)
		{
			Vertex vertex = new Vertex(location.X, location.Y);

			int newId = 0;

			if (_verticesList.Any())
			{
				newId = _verticesList.Select(v => v.Id).Max() + 1;
			}

			vertex.Id = newId;
			_verticesList.Add(vertex);
			return vertex;
		}

		/// <summary>
		/// Removes a vertex from the graph and any of its connections.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		public bool RemoveVertex(Vertex vertex)
		{
			var vertices = vertex.ConnectedVertices;

			foreach (Vertex otherVertex in vertices)
			{
				RemoveLineSegment(vertex, otherVertex);
			}

			bool result = _verticesList.Remove(vertex);

			CalculateAllIntersections();
			return result;
		}

		/// <summary>
		/// Adds an edge/connection between two vertices.
		/// </summary>
		/// <param name="vertex1">The vertex1.</param>
		/// <param name="vertex2">The vertex2.</param>
		public void AddEdge(Vertex vertex1, Vertex vertex2)
		{
			LineSegment lineSegment = Vertex.AddConnection(vertex1, vertex2);
			_lineSegmentsList.Add(lineSegment);
			_intersections.Add(lineSegment, new HashSet<LineSegment>());

			vertex1.State = VertexState.Normal;
			vertex2.State = VertexState.Normal;

			CalculateAllIntersections();
		}

		/// <summary>
		/// Removes an edge/connection between two vertices.
		/// </summary>
		/// <param name="vertex1">The vertex1.</param>
		/// <param name="vertex2">The vertex2.</param>
		public void RemoveEdge(Vertex vertex1, Vertex vertex2)
		{
			RemoveLineSegment(vertex1, vertex2);
			CalculateAllIntersections();
		}

		private void RemoveLineSegment(Vertex vertex1, Vertex vertex2)
		{
			LineSegment lineSegment = Vertex.RemoveConnection(vertex1, vertex2);

			var hashSet = _intersections[lineSegment];
			IntersectionCount -= hashSet.Count;
			hashSet.Clear();

			_lineSegmentsList.Remove(lineSegment);
		}

		/// <summary>
		/// Adds an intersection between two line segments.
		/// </summary>
		/// <param name="lineSegment1">The first line segment.</param>
		/// <param name="lineSegment2">The second line segment.</param>
		/// <remarks>
		/// <para>Each of the two line segments is added to the other's set of intersecting line
		/// segments and the number of intersections in the game level is increased by 1.</para>
		/// </remarks>
		private void AddIntersection(LineSegment lineSegment1, LineSegment lineSegment2)
		{
			_intersections[lineSegment1].Add(lineSegment2);
			_intersections[lineSegment2].Add(lineSegment1);
			IntersectionCount++;

			lineSegment1.State = LineSegmentState.Intersected;
			lineSegment2.State = LineSegmentState.Intersected;
		}

		/// <summary>
		/// Removes an intersection between two line segments.
		/// </summary>
		/// <param name="lineSegment1">The first line segment.</param>
		/// <param name="lineSegment2">The second line segment.</param>
		/// <remarks>
		/// <para>Each of the two line segments is removed from the other's set of intersecting
		/// line segments and the number of intersections in the game level is decreased by 1.
		/// </para>
		/// </remarks>
		private void RemoveIntersection(LineSegment lineSegment1, LineSegment lineSegment2)
		{
			HashSet<LineSegment> intersectingSegments1 = _intersections[lineSegment1];
			HashSet<LineSegment> intersectingSegments2 = _intersections[lineSegment2];

			intersectingSegments1.Remove(lineSegment2);
			intersectingSegments2.Remove(lineSegment1);
			IntersectionCount--;

			lineSegment1.State = (intersectingSegments1.Count > 0
									? LineSegmentState.Intersected
									: LineSegmentState.Normal);

			lineSegment2.State = (intersectingSegments2.Count > 0
									? LineSegmentState.Intersected
									: LineSegmentState.Normal);
		}

		/// <summary>
		/// Cleans up the set of intersecting line segments for each line segment in the game level
		/// and resets the number of intersections in the game level to 0.
		/// </summary>
		private void ClearIntersections()
		{
			foreach (LineSegment lineSegment in _lineSegmentsList)
			{
				_intersections[lineSegment].Clear();
				lineSegment.State = LineSegmentState.Normal;
			}
			IntersectionCount = 0;
		}

		/// <summary>
		/// Identifies any changes in the intersections between line segments attached to a
		/// specific vertex and any line segments in the game level, after that vertex has been
		/// dragged to a new position.
		/// </summary>
		/// <param name="vertex">The vertex which has been dragged to a new position.</param>
		/// <remarks>
		/// <para>The a single vertex has been dragged to a new position, only the intersections
		/// of the line segments attached to it might have changed, so it is unnecessary to
		/// recalculate all intersections in the game level.</para>
		/// </remarks>
		public void RecalculateIntersectionsForVertex(Vertex vertex)
		{
			bool intersectionChanged = false;
			foreach (LineSegment lineSegment in vertex.LineSegments)
			{
				HashSet<LineSegment> intersectingSegments = _intersections[lineSegment];
				foreach (LineSegment otherSegment in _lineSegmentsList)
				{
					if (otherSegment == lineSegment)
					{
						continue;
					}
					
					if (CalculationHelper.CheckLinesIntersect(lineSegment, otherSegment))
					{
						if (!intersectingSegments.Contains(otherSegment))
						{
							AddIntersection(lineSegment, otherSegment);
							intersectionChanged = true;
						}
					}
					else if (intersectingSegments.Contains(otherSegment))
					{
						RemoveIntersection(lineSegment, otherSegment);
						intersectionChanged = true;
					}
				}
			}
			if (intersectionChanged)
			{
				OnIntersectionCollectionChanged();
			}
		}

		/// <summary>
		/// Traverses all line segments in the game level and identifies all intersections between
		/// them.
		/// </summary>
		public void CalculateAllIntersections()
		{
			ClearIntersections();
			foreach (LineSegment lineSegment in _lineSegmentsList)
			{
				HashSet<LineSegment> intersectingSegments = _intersections[lineSegment];
				foreach (LineSegment otherSegment in _lineSegmentsList)
				{
					if (otherSegment == lineSegment || intersectingSegments.Contains(otherSegment))
					{
						continue;
					}

					if (CalculationHelper.CheckLinesIntersect(lineSegment, otherSegment))
					{
						AddIntersection(lineSegment, otherSegment);
					}
				}

				lineSegment.State = (intersectingSegments.Count > 0 ? LineSegmentState.Intersected : LineSegmentState.Normal);
			}
			OnIntersectionCollectionChanged();
		}


	}
}

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
using System.Xml.Serialization;
using System.Collections.Generic;
using Untangle.Enums;
using System.Windows.Media;
using System.Numerics;

namespace Untangle.ViewModels
{
	/// <summary>
	/// A view model class for a single vertex in a game level.
	/// </summary>
	public class Vertex : ViewModelBase
	{
		/// <summary>
		/// Property name constant for the current state of the vertex.
		/// </summary>
		public const string StatePropertyName = "State";

		/// <summary>
		/// Property name constant for the Z index of the vertex.
		/// </summary>
		public const string ZIndexPropertyName = "ZIndex";

		/// <summary>
		/// The unique identifier of the vertex in the saved game level.
		/// </summary>
		[XmlAttribute]
		public int Id { get; set; }

		/// <summary>
		/// The size of the vertex on the game field.
		/// </summary>
		[XmlIgnore]
		public double Size
		{
			get { return 30; }
		}

		/// <summary>
		/// The X coordinate of the vertex on the game field.
		/// </summary>
		[XmlIgnore]
		public double X
		{
			get { return _position.X; }
		}

		[XmlAttribute("X")]
		public string XString
		{
			get { return X.ToString("####0.######"); }
			set
			{
				double x = 0;
				if (Double.TryParse(value, out x))
				{
					_position.X = x;
				}
			}
		}

		/// <summary>
		/// The Y coordinate of the vertex on the game field.
		/// </summary>
		[XmlIgnore]
		public double Y
		{
			get { return _position.Y; }
		}

		[XmlAttribute("Y")]
		public string YString
		{
			get { return Y.ToString("####0.######"); }
			set
			{
				double y = 0;
				if (Double.TryParse(value, out y))
				{
					_position.Y = y;
				}
			}
		}

		/// <summary>
		/// The Z index of the vertex.
		/// </summary>
		/// <remarks>
		/// <para>Z indices of game objects are used to pull/push them to the front/back of the
		/// display surface, possibly obscuring other overlapping objects. Dragged and highlighted
		/// vertices are deliberately pulled to the front of the drawing surface so that they do
		/// not get obscured by other vertices.</para>
		/// </remarks>
		[XmlIgnore]
		public int ZIndex
		{
			get
			{
				switch (State)
				{
					case VertexState.ConnectedToHighlighted:
						return 1;
					case VertexState.Dragged:
					case VertexState.UnderMouse:
						return 2;
					default:
						return 0;
				}
			}
		}

		/// <summary>
		/// The current state of the vertex.
		/// </summary>
		[XmlIgnore]
		public VertexState State
		{
			get { return _state; }
			set
			{
				if (_state == value)
				{
					return;
				}

				_state = value;
				OnPropertyChanged(StatePropertyName);
				OnPropertyChanged(ZIndexPropertyName);
			}
		}

		[XmlIgnore]
		public Brush Color
		{
			get { return _color; }
			set
			{
				if (_color == value)
				{
					return;
				}

				_color = value;
				OnPropertyChanged();
			}
		}
		private Brush _color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));

		[XmlAttribute("ColorId")]
		public int ColorIndex
		{
			get { return _colorIndex; }
			set
			{
				int newIndex = value % ColorPalette.Default.Count;
				if (_colorIndex == newIndex)
				{
					return;
				}

				_colorIndex = newIndex;
				OnPropertyChanged();
				Color = ColorPalette.Default[_colorIndex];
			}
		}
		private int _colorIndex = 0;

		/// <summary>
		/// An enumeration of all vertices which are directly connected to the vertex.
		/// </summary>
		[XmlIgnore]
		public IEnumerable<Vertex> ConnectedVertices
		{
			get { return _lineSegmentsMap.Keys; }
		}

		/// <summary>
		/// An array containing the unique identifiers of all vertices which are directly connected
		/// to the current vertex.
		/// </summary>
		[XmlElement("ConnectedVertexId")]
		public int[] ConnectedVertexIds
		{
			get
			{
				if (_connectedVertexIds == null)
				{
					_connectedVertexIds = ConnectedVertices.Select(v => v.Id).ToArray();
				}
				return _connectedVertexIds;
			}
			set { _connectedVertexIds = value; }
		}
		private int[] _connectedVertexIds = null;

		/// <summary>
		/// An enumeration of all line segments which are attached to the vertex.
		/// </summary>
		[XmlIgnore]
		public IEnumerable<LineSegment> LineSegments
		{
			get { return _lineSegmentsMap.Values; }
		}

		/// <summary>
		/// The number of line segments which are attached to the vertex.
		/// </summary>
		[XmlIgnore]
		public int LineSegmentCount
		{
			get { return _lineSegmentsMap.Count; }
		}

		/// <summary>
		/// A dictionary of vertices directly connected to the vertex and the line segments
		/// connecting them.
		/// </summary>
		private readonly Dictionary<Vertex, LineSegment> _lineSegmentsMap;

		/// <summary>
		/// The position of the vertex on the game field.
		/// </summary>
		public Point Position
		{
			get { return _position; }
			set
			{
				if (!_position.Equals(value))
				{
					_position = value;
					OnPropertyChanged(nameof(X));
					OnPropertyChanged(nameof(Y));
					OnPropertyChanged(nameof(Position));
				}
			}
		}
		private Point _position;

		public Point? StartingPosition { get; set; }

		public string Name { get { return $"Vertex_{Id}"; } }

		/// <summary>
		/// The current state of the vertex.
		/// </summary>
		private VertexState _state;

		/// <summary>
		/// Initializes a new <see cref="Vertex"/> instance with no specific position on the game
		/// field.
		/// </summary>
		public Vertex()
			: this(0.0, 0.0)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="Vertex"/> instance with the specified position on the game
		/// field.
		/// </summary>
		/// <param name="x">The X coordinate of the vertex on the game field.</param>
		/// <param name="y">The Y coordinate of the vertex on the game field.</param>
		public Vertex(double x, double y)
			: this(new Point(x, y))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vertex"/> class.
		/// </summary>
		/// <param name="location">The location point.</param>
		public Vertex(Point location)
		{
			_lineSegmentsMap = new Dictionary<Vertex, LineSegment>();
			_state = VertexState.Normal;
			_position = location;
			if (!(location.X == 0 && location.Y == 0))
			{
				StartingPosition = _position;
			}
		}

		/// <summary>
		/// Connects the vertex to another vertex with a line segment and returns it.
		/// </summary>
		/// <param name="second">The vertex which should be connected to the current one.
		/// </param>
		/// <returns>The created line segment between the two vertices.</returns>
		/// <exception cref="InvalidOperationException">
		/// An attempt is made to connect the vertex to itself.
		/// 
		/// -or-
		/// A line segment between the two vertices already exists.
		/// </exception>
		public static LineSegment AddConnection(Vertex first, Vertex second)
		{
			if (first == second)
			{
				throw new InvalidOperationException("A vertex cannot be connected to itself.");
			}

			if (first._lineSegmentsMap.ContainsKey(second))
			{
				throw new InvalidOperationException("A line segment between the two vertices already exists.");
			}

			var lineSegment = new LineSegment(first, second);
			first._lineSegmentsMap[second] = lineSegment;
			second._lineSegmentsMap[first] = lineSegment;

			first._connectedVertexIds = null;
			second._connectedVertexIds = null;

			return lineSegment;
		}

		public static LineSegment RemoveConnection(Vertex first, Vertex second)
		{
			if (first == second)
			{
				throw new InvalidOperationException("A vertex cannot be connected to itself.");
			}

			if (!first._lineSegmentsMap.ContainsKey(second))
			{
				throw new InvalidOperationException("A line segment between the two vertices doesn't exist.");
			}

			LineSegment lineSegment1 = first._lineSegmentsMap[second];
			LineSegment lineSegment2 = second._lineSegmentsMap[first];

			if (lineSegment1 != lineSegment2)
			{
				throw new Exception("WTF?");
			}

			first._lineSegmentsMap.Remove(second);
			second._lineSegmentsMap.Remove(first);

			first._connectedVertexIds = null;
			second._connectedVertexIds = null;

			return lineSegment1;
		}

		/// <summary>
		/// Changes the position of the vertex on the game field.
		/// </summary>
		/// <param name="position">The new position of the vertex.</param>
		public void SetPosition(Point position)
		{
			_position = position;
			OnPropertyChanged("X");
			OnPropertyChanged("Y");
		}

		public void NextColor()
		{
			_colorIndex = (_colorIndex + 1) % ColorPalette.Default.Count;
			Color = ColorPalette.Default[_colorIndex];
		}

		public void SetColor(Brush color)
		{
			Color = color;
			int index = ColorPalette.Default.IndexOf(color);
			if (index != -1)
			{
				_colorIndex = index;
			}
		}

		public override string ToString()
		{
			return $"#{Id}: {string.Join(" , ", LineSegments.Select(ls => ls.ToString()))}";
		}
	}
}
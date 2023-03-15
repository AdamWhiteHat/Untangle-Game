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
using System.Windows.Shapes;
//using Microsoft.Msagl.Core.Geometry.Curves;
//using Microsoft.Msagl.Drawing;

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
		public int Id
		{
			get { return _id; }
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}
		private int _id;

		/// <summary>
		/// The size of the vertex on the game field.
		/// </summary>		
		[XmlIgnore]
		public double Size
		{
			get { return 30; }
		}

		/// <summary>Identifies the <see cref="P:Untangle.ViewModels.Vertex.X" /> dependency property.</summary>
		public static readonly DependencyProperty XProperty = DependencyProperty.Register(
		  "X",
		  typeof(double),
		  typeof(Vertex),
		  new FrameworkPropertyMetadata(new PropertyChangedCallback(OnXPropertyChanged))
		// { AffectsRender = true }
		);

		/// <summary>Identifies the <see cref="P:Untangle.ViewModels.Vertex.Y" /> dependency property.</summary>
		public static readonly DependencyProperty YProperty = DependencyProperty.Register(
		  "Y",
		  typeof(double),
		  typeof(Vertex),
		  new FrameworkPropertyMetadata(new PropertyChangedCallback(OnYPropertyChanged))
		// { AffectsRender = true }
		);


		/// <summary>
		/// The X coordinate of the vertex on the game field.
		/// </summary>
		[XmlIgnore]
		public double X
		{
			get { return (double)GetValue(XProperty); }
			set
			{
				double oldValue = (double)GetValue(XProperty);
				if (Math.Round(oldValue, 4) != Math.Round(value, 4))
				{
					SetValue(XProperty, value);
					OnPropertyChanged(nameof(X));
				}
			}
		}

		/// <summary>
		/// The Y coordinate of the vertex on the game field.
		/// </summary>
		[XmlIgnore]
		public double Y
		{
			get { return (double)GetValue(YProperty); }
			set
			{
				double oldValue = (double)GetValue(YProperty);
				if (Math.Round(oldValue, 4) != Math.Round(value, 4))
				{
					SetValue(YProperty, value);
					OnPropertyChanged(nameof(Y));
				}
			}
		}

		[XmlAttribute("X")]
		public string XString
		{
			get { return X.ToString("####0.####"); }
			//set
			//{
			//	double x = 0;
			//	if (Double.TryParse(value, out x))
			//	{
			//		_position.X = x;
			//		OnPropertyChanged(nameof(X));

			//	}
			//}
		}

		[XmlAttribute("Y")]
		public string YString
		{
			get { return Y.ToString("####0.####"); }
			//set
			//{
			//	double y = 0;
			//	if (Double.TryParse(value, out y))
			//	{
			//		_position.Y = y;
			//		OnPropertyChanged(nameof(Y));
			//	}
			//}
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
		private VertexState _state;

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

		/*
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
		*/

		public Point? StartingPosition { get; set; }

		public bool AtStartPosition { get { return (StartingPosition.HasValue) ? (X == StartingPosition.Value.X && Y == StartingPosition.Value.Y) : false; } }

		public string Name { get { return $"Vertex_{Id}"; } }

		/// <summary>
		/// Initializes a new <see cref="Vertex"/> instance with no specific position on the game
		/// field.
		/// </summary>
		public Vertex()
			: this(0.0, 0.0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vertex"/> class.
		/// </summary>
		/// <param name="location">The location point.</param>
		public Vertex(Point location)
			: this(location.X, location.Y)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="Vertex"/> instance with the specified position on the game
		/// field.
		/// </summary>
		/// <param name="x">The X coordinate of the vertex on the game field.</param>
		/// <param name="y">The Y coordinate of the vertex on the game field.</param>
		public Vertex(double x, double y)
		{
			_lineSegmentsMap = new Dictionary<Vertex, LineSegment>();
			_state = VertexState.Normal;
			X = x;
			Y = y;
			StartingPosition = new Point(X, Y);
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
			X = position.X;
			Y = position.Y;
		}

		/// <summary>
		/// Returns a Point object with this vertex's position.
		/// </summary>
		/// <returns>A Point object with this vertex's position.</returns>
		public Point GetPosition()
		{
			return new Point(X, Y);
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

		private static void OnXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Vertex v = (Vertex)d;
			v.X = (double)e.NewValue;
		}

		private static void OnYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Vertex v = (Vertex)d;
			v.Y = (double)e.NewValue;
		}


		protected override Freezable CreateInstanceCore()
		{
			return new Vertex();
		}

		public override string ToString()
		{
			return $"#{Id} => ({Math.Round(X, 2)}, {Math.Round(Y, 2)}): {string.Join(" , ", LineSegments.Select(ls => ls.ToString()))}";
		}
	}
}
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
using System.Collections.Generic;
using System.Windows;
using Untangle.Enums;

namespace Untangle.Core
{
	/// <summary>
	/// A view model class for a single line segment in a game level.
	/// </summary>
	public class LineSegment
	{
		/// <summary>
		/// The vertex at the first endpoint of the line segment.
		/// </summary>
		private readonly Vertex _vertex1;

		/// <summary>
		/// The vertex at the second endpoint of the line segment.
		/// </summary>
		private readonly Vertex _vertex2;

		/// <summary>
		/// The current state of the line segment.
		/// </summary>
		private LineSegmentState _state;

		/// <summary>
		/// The vertex at the first endpoint of the line segment.
		/// </summary>
		public Vertex Vertex1
		{
			get { return _vertex1; }
		}

		/// <summary>
		/// The vertex at the second endpoint of the line segment.
		/// </summary>
		public Vertex Vertex2
		{
			get { return _vertex2; }
		}

		/// <summary>
		/// The current state of the line segment.
		/// </summary>
		public LineSegmentState State
		{
			get { return _state; }
			set
			{
				if (_state == value)
				{
					return;
				}

				_state = value;
			}
		}

		/// <summary>
		/// The Z index of the line segment.
		/// </summary>
		/// <remarks>
		/// <para>Z indices of game objects are used to pull/push them to the front/back of the
		/// display surface, possibly obscuring other overlapping objects. Line segments are
		/// deliberately pushed to the back of the drawing surface so that they do not get drawn
		/// over vertices.</para>
		/// </remarks>
		public int ZIndex
		{
			get
			{
				switch (State)
				{
					case LineSegmentState.Intersected:
						return -9;
					case LineSegmentState.Highlighted:
						return -8;
					default:
						return -10;
				}
			}
		}

		/// <summary>
		/// The position of the first endpoint of the line segment.
		/// </summary>
		public Point Point1
		{
			get { return new Point(_vertex1.X, _vertex1.Y); }
		}

		/// <summary>
		/// The position of the second endpoint of the line segment.
		/// </summary>
		public Point Point2
		{
			get { return new Point(_vertex2.X, _vertex2.Y); }
		}

		public double Length
		{
			get { return Distance(Point1, Point2); }
		}

		private LineSegment()
		{ }

		/// <summary>
		/// Initializes a new <see cref="LineSegment"/> instance with the specified two endpoint
		/// vertices.
		/// </summary>
		/// <param name="vertex1">The vertex at the first endpoint of the line segment.</param>
		/// <param name="vertex2">The vertex at the second endpoint of the line segment.</param>
		public LineSegment(Vertex vertex1, Vertex vertex2)
		{
			_vertex1 = vertex1;
			_vertex2 = vertex2;
		}

		public void Shrink()
		{
			var mid = MidPoint(Point1, Point2);

			Point newVertex1 = MidPoint(Point1, mid);
			Point newVertex2 = MidPoint(mid, Point2);

			Vertex1.SetPosition(newVertex1);
			Vertex2.SetPosition(newVertex2);
		}

		public static double Distance(Point a, Point b)
		{
			return Math.Sqrt((Math.Pow((b.X - a.X), 2) + Math.Pow((b.Y - a.Y), 2)));
		}

		public static Point MidPoint(Point a, Point b)
		{
			return new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
		}

		public override string ToString()
		{
			return $"{Vertex1.Id}<->{Vertex2.Id}";
		}
	}
}
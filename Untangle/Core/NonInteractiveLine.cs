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
using System.Windows.Media;
using System.Windows.Shapes;
using Untangle.Enums;

namespace Untangle.Core
{
	/// <summary>
	/// A view model class for a single line segment in a game level.
	/// </summary>
	public class NonInteractiveLine
	{
		/// <summary>
		/// The vertex at the first endpoint of the line segment.
		/// </summary>
		public Vertex SourceVertex
		{
			get { return _vertex; }
		}
		private readonly Vertex _vertex;

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
				return -10;
			}
		}

		public double X1
		{
			get { return _vertex.StartingPosition.Value.X; }
		}

		public double Y1
		{
			get { return _vertex.StartingPosition.Value.Y; }
		}

		public double X2
		{
			get { return _vertex.SolvedPosition.Value.X; }
		}

		public double Y2
		{
			get { return _vertex.SolvedPosition.Value.Y; }
		}

		public Brush LineColor
		{
			get { return _vertex.Color; }
		}

		/// <summary>
		/// The position of the first endpoint of the line segment.
		/// </summary>
		public Point PointFrom
		{
			get { return new Point(_vertex.StartingPosition.Value.X, _vertex.StartingPosition.Value.Y); }
		}

		/// <summary>
		/// The position of the second endpoint of the line segment.
		/// </summary>
		public Point PointTo
		{
			get { return new Point(_vertex.SolvedPosition.Value.X, _vertex.SolvedPosition.Value.Y); }
		}

		private NonInteractiveLine()
		{ }

		/// <summary>
		/// Initializes a new <see cref="LineSegment"/> instance with the specified two endpoint
		/// vertices.
		/// </summary>
		/// <param name="vertex1">The vertex at the first endpoint of the line segment.</param>
		/// <param name="vertex2">The vertex at the second endpoint of the line segment.</param>
		public NonInteractiveLine(Vertex vertex)
		{
			_vertex = vertex;
		}

		public override string ToString()
		{
			return $"{_vertex.Id}";
		}
	}
}
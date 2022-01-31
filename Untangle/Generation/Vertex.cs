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

namespace Untangle.Generation
{
	/// <summary>
	/// Represents a single vertex of the level graph. Keeps a list of directly connected vertices.
	/// </summary>
	public class Vertex
	{
		/// <summary>
		/// The row of the vertex in the level matrix.
		/// </summary>
		private readonly int _row;

		/// <summary>
		///  The column of the vertex in the level matrix.
		/// </summary>
		private readonly int _column;

		/// <summary>
		/// A hash set of all vertices which are directly connected to the vertex.
		/// </summary>
		private readonly HashSet<Vertex> _connectedVertices;

		/// <summary>
		/// The row of the vertex in the level matrix.
		/// </summary>
		public int Row
		{
			get { return _row; }
		}

		/// <summary>
		///  The column of the vertex in the level matrix.
		/// </summary>
		public int Column
		{
			get { return _column; }
		}

		/// <summary>
		/// An enumeration of all vertices which are directly connected to the vertex.
		/// </summary>
		public IEnumerable<Vertex> ConnectedVertices
		{
			get { return _connectedVertices; }
		}

		/// <summary>
		/// The number of vertices which are directly connected to the vertex.
		/// </summary>
		public int ConnectedVertexCount
		{
			get { return _connectedVertices.Count; }
		}

		/// <summary>
		/// Initializes a new <see cref="Vertex"/> instance with the specified values for
		/// <see cref="Row"/> and <see cref="Column"/>.
		/// </summary>
		/// <param name="row">The row of the vertex in the level matrix.</param>
		/// <param name="column">The column of the vertex in the level matrix.</param>
		public Vertex(int row, int column)
		{
			_row = row;
			_column = column;
			_connectedVertices = new HashSet<Vertex>();
		}




		/// <summary>
		/// Checks if the vertex is directly connected to another vertex.
		/// </summary>
		/// <param name="otherVertex">The other vertex which should be checked.</param>
		/// <returns><see langword="true"/> if the specified vertex is directly connected to the
		/// current one.</returns>
		public bool IsConnectedToVertex(Vertex otherVertex)
		{
			return _connectedVertices.Contains(otherVertex);
		}

		/// <summary>
		/// Adds an edge between the vertex and another vertex.
		/// </summary>
		/// <param name="otherVertex">The vertex which should be connected to the current one.
		/// </param>
		/// <exception cref="InvalidOperationException">
		/// An attempt is made to connect the vertex to itself.
		/// 
		/// -or-
		/// An edge between the two vertices already exists.
		/// </exception>
		/// <remarks>
		/// <para>The method adds the specified vertex to the list of directly connected vertices
		/// of the current vertex and vice versa.</para>
		/// </remarks>
		public void ConnectToVertex(Vertex otherVertex)
		{
			if (this == otherVertex)
			{
				throw new InvalidOperationException("A vertex cannot be connected to itself.");
			}

			if (IsConnectedToVertex(otherVertex))
			{
				throw new InvalidOperationException("An edge between the two vertices already exists.");
			}

			_connectedVertices.Add(otherVertex);
			otherVertex._connectedVertices.Add(this);
		}

		/// <summary>
		/// Removes the edge between the vertex and another vertex.
		/// </summary>
		/// <param name="otherVertex">The vertex which should be disconnected from the current one.
		/// </param>
		/// <exception cref="InvalidOperationException">
		/// An attempt is made to disconnect the vertex from itself.
		/// 
		/// -or-
		/// No edge between the two vertices exists.
		/// </exception>
		/// <remarks>
		/// <para>The method removes the specified vertex from the list of directly connected
		/// vertices of the current vertex and vice versa.</para>
		/// </remarks>
		public void DisconnectFromVertex(Vertex otherVertex)
		{
			if (this == otherVertex)
			{
				throw new InvalidOperationException("A vertex cannot be connected to itself.");
			}

			if (!IsConnectedToVertex(otherVertex))
			{
				throw new InvalidOperationException("No edge between the two vertices exists.");
			}

			_connectedVertices.Remove(otherVertex);
			otherVertex._connectedVertices.Remove(this);
		}

		/// <summary>
		/// Returns a useful for debugging string representation of the vertex.
		/// </summary>
		/// <returns>A string representation of the vertex.</returns>
		public override string ToString()
		{
			return $"({Row},{Column}) Vertex.Count: {ConnectedVertexCount}";
		}
	}
}
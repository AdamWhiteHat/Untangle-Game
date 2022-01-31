/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

using System.Collections.Generic;

namespace Untangle.Generation
{
	/// <summary>
	/// Represents a weakly connected portion of a connected subgraph of the entire level graph.
	/// </summary>
	/// <remarks>
	/// <para>A weakly connected portion of a connected subgraph is only connected to the rest of
	/// the vertices in the connected subgraph through a single vertex.</para>
	/// </remarks>
	public class WeaklyConnectedSubgraph
	{
		/// <summary>
		/// The anchor vertex of the weakly connected portion of the connected subgraph.
		/// </summary>
		/// <remarks>
		/// <para>The anchor vertex is the only vertex from the strongly connected portion of the
		/// connected subgraph which connects it to the weakly connected portion.</para>
		/// </remarks>
		private readonly Vertex _anchorVertex;

		/// <summary>
		/// A hash set of all vertices which belong to the weakly connected portion of the
		/// connected subgraph.
		/// </summary>
		private readonly HashSet<Vertex> _connectedVertices;

		/// <summary>
		/// The anchor vertex of the weakly connected portion of the connected subgraph.
		/// </summary>
		/// <remarks>
		/// <para>The anchor vertex is the only vertex from the strongly connected portion of the
		/// connected subgraph which connects it to the weakly connected portion.</para>
		/// </remarks>
		public Vertex AnchorVertex
		{
			get { return _anchorVertex; }
		}

		/// <summary>
		/// An enumeration of all vertices which belong to the weakly connected portion of the
		/// connected subgraph.
		/// </summary>
		public IEnumerable<Vertex> ConnectedVertices
		{
			get { return _connectedVertices; }
		}

		/// <summary>
		/// Initializes a new <see cref="WeaklyConnectedSubgraph"/> instance with the specified
		/// anchor vertex.
		/// </summary>
		/// <param name="anchorVertex">The anchor vertex of the weakly connected portion of the
		/// connected subgraph.</param>
		/// <remarks>
		/// <para>The anchor vertex is the only vertex from the strongly connected portion of the
		/// connected subgraph which connects it to the weakly connected portion.</para>
		/// </remarks>
		public WeaklyConnectedSubgraph(Vertex anchorVertex)
		{
			_anchorVertex = anchorVertex;
			_connectedVertices = new HashSet<Vertex>();
		}

		/// <summary>
		/// Checks if a vertex belongs to the weakly connected portion of the connected subgraph.
		/// </summary>
		/// <param name="vertex">The vertex which should be checked.</param>
		/// <returns><see langword="true"/> if the vertex belongs to the weakly connected portion
		/// of the connected subgraph.
		/// </returns>
		public bool ContainsVertex(Vertex vertex)
		{
			return _connectedVertices.Contains(vertex);
		}

		/// <summary>
		/// Adds a vertex to the list of vertices in the weakly connected portion of the connected
		/// subgraph.
		/// </summary>
		/// <param name="vertex">The vertex which should be added to the list of vertices.</param>
		public void AddConnectedVertex(Vertex vertex)
		{
			_connectedVertices.Add(vertex);
		}
	}
}
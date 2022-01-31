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
using System.Linq;

namespace Untangle.Generation
{
	/// <summary>
	/// Represents a connected subgraph of the entire level graph.
	/// </summary>
	public class ConnectedSubgraph
	{
		/// <summary>
		/// The main vertex of the connected subgraph.
		/// </summary>
		/// <remarks>
		/// <para>The main vertex of a connected subgraph is used to identify all other vertices in
		/// it and is the only vertex that is certain to remain in the connected subgraph when
		/// edges are removed.</para>
		/// </remarks>
		private readonly Vertex _mainVertex;

		/// <summary>
		/// A hash set of all vertices which belong to the connected subgraph.
		/// </summary>
		private readonly HashSet<Vertex> _connectedVertices;

		/// <summary>
		/// A dictionary of containing the weakly connected portions of the connected subgraph
		/// which each vertex belonging to the connected subgraph belongs to, or
		/// <see langword="null"/> in case the vertex belongs to the strongly connected portion of
		/// the connected subgraph.
		/// </summary>
		/// <remarks>
		/// <para>A weakly connected portion of a connected subgraph is only connected to the rest
		/// of the vertices in the connected subgraph through a single vertex.</para>
		/// </remarks>
		private readonly Dictionary<Vertex, WeaklyConnectedSubgraph> _weaklyConnectedSubgraphs;

		/// <summary>
		/// An enumeration of all vertices which belong to the connected subgraph.
		/// </summary>
		public IEnumerable<Vertex> ConnectedVertices
		{
			get { return _connectedVertices; }
		}

		/// <summary>
		/// An enumeration of all vertices which belong to the strongly connected portion of the
		/// connected subgraph.
		/// </summary>
		/// <remarks>
		/// <para>The strongly connected portion of a connected subgraph is the largest subgraph
		/// containing the main vertex that cannot be split into a pair of subgraphs which are only
		/// connected through a single vertex.</para>
		/// </remarks>
		public IEnumerable<Vertex> StronglyConnectedVertices
		{
			get
			{
				return _weaklyConnectedSubgraphs
					.Where(kv => kv.Value == null)
					.Select(kv => kv.Key);
			}
		}

		/// <summary>
		/// Signifies whether the whole connected subgraph is strongly connected.
		/// </summary>
		/// <remarks>
		/// <para>A connected subgraph is considered to be strongly connected if it cannot be split
		/// into a pair of subgraphs which are only connected through a single vertex.</para>
		/// </remarks>
		public bool IsStronglyConnected
		{
			get
			{
				return _weaklyConnectedSubgraphs.All(kv => kv.Value == null);
			}
		}

		/// <summary>
		/// The number of vertices which belong to the connected subgraph.
		/// </summary>
		public int ConnectedVertexCount
		{
			get { return _connectedVertices.Count; }
		}

		/// <summary>
		/// Initializes a new <see cref="ConnectedSubgraph"/> instance with the specified main
		/// vertex.
		/// </summary>
		/// <param name="mainVertex">The main vertex of the connected subgraph.</param>
		/// <remarks>
		/// <para>The main vertex of a connected subgraph is used to identify all other vertices in
		/// it and is the only vertex that is certain to remain in the connected subgraph when
		/// edges are removed.</para>
		/// </remarks>
		public ConnectedSubgraph(Vertex mainVertex)
		{
			_mainVertex = mainVertex;
			_connectedVertices = new HashSet<Vertex> { _mainVertex };
			_weaklyConnectedSubgraphs = new Dictionary<Vertex, WeaklyConnectedSubgraph>
			{
                // The main vertex of the connected subgraph belongs to its strongly connected
                // portion by definition
                { _mainVertex, null },
			};

			AddOtherConnectedVertices(_mainVertex);
		}

		/// <summary>
		/// Checks if a vertex belongs to the connected subgraph.
		/// </summary>
		/// <param name="vertex">The vertex which should be checked.</param>
		/// <returns><see langword="true"/> if the vertex belongs to the connected subgraph.
		/// </returns>
		public bool ContainsVertex(Vertex vertex)
		{
			return _connectedVertices.Contains(vertex);
		}

		/// <summary>
		/// Checks if a vertex belongs to the strongly connected portion of the connected subgraph.
		/// </summary>
		/// <param name="vertex">The vertex which should be checked.</param>
		/// <returns><see langword="true"/> if the vertex belongs to the strongly connected portion
		/// of the connected subgraph.</returns>
		public bool IsVertexStronglyConnected(Vertex vertex)
		{
			return (_weaklyConnectedSubgraphs[vertex] == null);
		}

		/// <summary>
		/// Checks if adding an edge between two vertices in the connected subgraph will expand
		/// the strongly connected portion of the connected subgraph.
		/// </summary>
		/// <param name="vertex1">The first vertex which might be connected.</param>
		/// <param name="vertex2">The second vertex which might be connected.</param>
		/// <returns>see langword="true"/> if adding an edge between the two vertices will expand
		/// the strongly connected portion of the connected subgraph.</returns>
		public bool CheckWillConnectStrongly(Vertex vertex1, Vertex vertex2)
		{
			WeaklyConnectedSubgraph weaklyConnectedSubgraph1 = _weaklyConnectedSubgraphs[vertex1];
			WeaklyConnectedSubgraph weaklyConnectedSubgraph2 = _weaklyConnectedSubgraphs[vertex2];

			// If both vertices belong to the same weakly connected portion of the connected
			// subgraph or to the strongly connected portion of the connected subgraph, adding the
			// edge will not expand the strongly connected portion
			if (weaklyConnectedSubgraph1 == weaklyConnectedSubgraph2)
			{
				return false;
			}

			// If the first vertex belongs to the strongly connected portion of the connected
			// subgraph, adding the edge will expand the strongly connected portion when the first
			// vertex is the main vertex of the connected subgraph or differs from the anchor
			// vertex of the weakly connected portion of the connected subgraph which contains the
			// second vertex
			if (weaklyConnectedSubgraph1 == null)
			{
				return (vertex1 == _mainVertex || vertex1 != weaklyConnectedSubgraph2.AnchorVertex);
			}

			// If the second vertex belongs to the strongly connected portion of the connected
			// subgraph, adding the edge will expand the strongly connected portion when the second
			// vertex is the main vertex of the connected subgraph or differs from the anchor
			// vertex of the weakly connected portion of the connected subgraph which contains the
			// first vertex
			if (weaklyConnectedSubgraph2 == null)
			{
				return (vertex2 == _mainVertex || vertex1 != weaklyConnectedSubgraph1.AnchorVertex);
			}

			// If both vertices belong to different weakly connected portions of the connected
			// subgraph, adding the edge will integrate both weakly connected portions back into
			// the strongly connected portion
			return true;
		}

		/// <summary>
		/// Refreshes the list of vertices and weakly connected subgraphs in the connected
		/// subgraph.
		/// </summary>
		public void RecalculateConnectedVertices()
		{
			_connectedVertices.Clear();
			_weaklyConnectedSubgraphs.Clear();

			_connectedVertices.Add(_mainVertex);
			_weaklyConnectedSubgraphs.Add(_mainVertex, null);

			AddOtherConnectedVertices(_mainVertex);
		}

		/// <summary>
		/// Adds a vertex to the list of vertices in the connected subgraph and then recursively
		/// adds all vertices directly connected to it. The vertex is added to the list because it
		/// is directly connected to a base vertex which is already in the list of vertices.
		/// </summary>
		/// <param name="vertex">The vertex which should be added to the list of vertices.</param>
		/// <param name="baseVertex">The base vertex which the specified vertex is directly
		/// connected to.</param>
		/// <remarks>
		/// <para>If the specified vertex is not yet in the list of vertices, it is be added to a
		/// suitable weakly connected portion of the connected subgraph.</para>
		/// <para>If the specified vertex is already in the list of vertices, the method checks if
		/// they belong to one or two weakly connected portions of the connected subgraph and
		/// integrates them back into the strongly connected portion of the connected subgraph, if
		/// eligible.</para>
		/// </remarks>
		private void AddConnectedVertex(Vertex vertex, Vertex baseVertex)
		{
			if (_connectedVertices.Contains(vertex))
			{
				if (!IsVertexStronglyConnected(vertex))
				{
					AdjustWeaklyConnectedSubgraphs(vertex, baseVertex);
				}
				else if (!IsVertexStronglyConnected(baseVertex))
				{
					AdjustWeaklyConnectedSubgraphs(baseVertex, vertex);
				}
			}
			else
			{
				_connectedVertices.Add(vertex);

				WeaklyConnectedSubgraph weaklyConnectedSubgraph =
					(IsVertexStronglyConnected(baseVertex)
						? new WeaklyConnectedSubgraph(baseVertex)
						: _weaklyConnectedSubgraphs[baseVertex]);
				weaklyConnectedSubgraph.AddConnectedVertex(vertex);
				_weaklyConnectedSubgraphs.Add(vertex, weaklyConnectedSubgraph);

				AddOtherConnectedVertices(vertex);
			}
		}

		/// <summary>
		/// Adds all vertices which are directly connected to a specific base vertex to the list of
		/// vertices in the connected subgraph. The base vertex itself must have already been added
		/// to the list of vertices in the connected subgraph.
		/// </summary>
		/// <param name="baseVertex">The base vertex which is already in the list of vertices.
		/// </param>
		private void AddOtherConnectedVertices(Vertex baseVertex)
		{
			foreach (Vertex directlyConnectedVertex in baseVertex.ConnectedVertices)
			{
				AddConnectedVertex(directlyConnectedVertex, baseVertex);
			}
		}

		/// <summary>
		/// Adjusts the weakly connected portions of the connected subgraph which two vertices
		/// might belong to immediately after adding an edge between them, possibly reintegrating
		/// them back into the strongly connected portion of the connected subgraph.
		/// </summary>
		/// <param name="vertex1">The first vertex which might belong to a weakly connected portion
		/// of the connected subgraph.</param>
		/// <param name="vertex2">The second vertex which might belong to a weakly connected
		/// portion of the connected subgraph.</param>
		private void AdjustWeaklyConnectedSubgraphs(Vertex vertex1, Vertex vertex2)
		{
			WeaklyConnectedSubgraph weaklyConnectedSubgraph1 = _weaklyConnectedSubgraphs[vertex1];
			WeaklyConnectedSubgraph weaklyConnectedSubgraph2 = _weaklyConnectedSubgraphs[vertex2];

			// If both vertices belong to the same weakly connected portion of the connected
			// subgraph or to the strongly connected portion of the connected subgraph nothing
			// needs to be recalculated
			if (weaklyConnectedSubgraph1 == weaklyConnectedSubgraph2)
			{
				return;
			}

			if (weaklyConnectedSubgraph1 == null)
			{
				// If the first vertex belongs to the strongly connected portion of the connected
				// subgraph and the second vertex belongs to a weakly connected portion, only
				// integrate the weakly connected portion back into the strongly connected portion
				// if the first vertex is the main vertex of the connected subgraph or differs from
				// the anchor vertex of the weakly connected portion
				if (vertex1 == _mainVertex || vertex1 != weaklyConnectedSubgraph2.AnchorVertex)
					IntegrateWeaklyConnectedSubgraph(weaklyConnectedSubgraph2);
			}
			else if (weaklyConnectedSubgraph2 == null)
			{
				// If the second vertex belongs to the strongly connected portion of the connected
				// subgraph and the first vertex belongs to a weakly connected portion, only
				// integrate the weakly connected portion back into the strongly connected portion
				// if the second vertex is the main vertex of the connected subgraph or differs
				// from the anchor vertex of the weakly connected portion
				if (vertex2 == _mainVertex || vertex1 != weaklyConnectedSubgraph1.AnchorVertex)
				{
					IntegrateWeaklyConnectedSubgraph(weaklyConnectedSubgraph1);
				}
			}
			else
			{
				// If both vertices belong to different weakly connected portions of the connected
				// subgraph, integrate both of them back into the strongly connected portion
				IntegrateWeaklyConnectedSubgraph(weaklyConnectedSubgraph1);
				IntegrateWeaklyConnectedSubgraph(weaklyConnectedSubgraph2);
			}
		}

		/// <summary>
		/// Marks all vertices belonging to a weakly connected portion of the connected subgraph
		/// as belonging to its strongly connected portion, thus removing the weakly connected
		/// portion.
		/// </summary>
		/// <param name="weaklyConnectedSubgraph">The weakly connected portion of the connected
		/// subgraph which should be integrated back into the strongly connected portion.</param>
		private void IntegrateWeaklyConnectedSubgraph(WeaklyConnectedSubgraph weaklyConnectedSubgraph)
		{
			foreach (Vertex vertex in weaklyConnectedSubgraph.ConnectedVertices)
			{
				_weaklyConnectedSubgraphs[vertex] = null;
			}
		}
	}
}
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
using System.Collections.Generic;
using Untangle.ViewModels;
using System.Windows;
using static Untangle.ViewModels.GameLevel;
using System.Drawing;
using Microsoft.Msagl.GraphmapsWithMesh;

namespace Untangle.Generation
{
	/// <summary>
	/// A class that implements an algorithm for the generation of game levels based on a matrix of
	/// vertices that are only connected to some of their horizontally, vertically or diagonally
	/// adjacent vertices in the matrix.
	/// </summary>
	public class LevelGenerator
	{
		/// <summary>
		/// The number of rows in the level matrix.
		/// </summary>
		private readonly int _rows;

		/// <summary>
		/// The number of columns in the level matrix.
		/// </summary>
		private readonly int _columns;

		/// <summary>
		/// The maximum number of edges entering a single vertex in the level graph.
		/// </summary>
		private readonly int _maxEdges;

		/// <summary>
		/// The minimum number of edges entering a single vertex in the level graph.
		/// Can never be less than 2
		/// </summary>
		private readonly int _minEdges = 2;

		/// <summary>
		/// The level matrix: a two-dimensional rectangular matrix of vertices.
		/// </summary>
		private readonly Vertex[,] _levelMatrix;

		/// <summary>
		/// A dictionary containing the connected subgraph which each vertex belongs to.
		/// </summary>
		private readonly Dictionary<Vertex, ConnectedSubgraph> _connectedSubgraphs;

		/// <summary>
		/// A list of vertices that still need more edges entering them.
		/// </summary>
		private readonly List<Vertex> _availableVertices;

		/// <summary>
		/// Initializes a new <see cref="LevelGenerator"/> instance for a level with the specified
		/// number of rows and columns in its matrix and the specified maximum number of edges
		/// entering a single vertex.
		/// </summary>
		/// <param name="rows">The number of rows in the level matrix.</param>
		/// <param name="columns">The number of columns in the level matrix.</param>
		/// <param name="maxEdges">The maximum number of edges entering a single vertex in
		/// the level graph.</param>
		public LevelGenerator(int rows, int columns, int maxEdges, int minEdges = 2)
		{
			if (minEdges > maxEdges)
			{
				throw new Exception("Minimum edges parameter cannot be greater than Maximum edges parameter.");
			}
			if (minEdges < 2)
			{
				throw new Exception("Parameter minimum edges must be at least two.");
			}

			_rows = rows;
			_columns = columns;
			_maxEdges = maxEdges;
			_minEdges = minEdges;
			_levelMatrix = new Vertex[_rows, _columns];
			_connectedSubgraphs = new Dictionary<Generation.Vertex, ConnectedSubgraph>();
			_availableVertices = new List<Vertex>();
		}

		/// <summary>
		/// Generates level zero.
		/// </summary>
		/// <returns>The generated level.</returns>
		public GameLevel GenerateLevel()
		{
			Initialize();

			GameLevel result = GameLevel.Create(_connectedSubgraphs.Keys);
			int intersectionCount = GraphLayout.Circle(result.GameGraph);
			result.GameGraph.IntersectionCount = intersectionCount;
			return result;
		}

		/// <summary>
		/// Generates a level which fits the constraints of the level generator.
		/// </summary>
		/// <param name="gameboardSize">The size of the game window.
		/// The Level generator will generate all the tame assets within these bounds.</param>
		/// <returns>The generated level.</returns>
		public GameLevel GenerateLevel(System.Windows.Size gameboardSize)
		{
			Initialize();

			GameLevel result = null;
			if (!_connectedSubgraphs.Any())
			{
				result = GenerateLevel();
			}
			else
			{
				result = GameLevel.Create(gameboardSize, _connectedSubgraphs.Keys);

				int intersectionCount = GraphLayout.SelectRandomLayout(result.GameGraph, gameboardSize);
				result.GameGraph.IntersectionCount = intersectionCount;
			}

			return result;
		}

		private void Initialize()
		{
			CleanUp();
			CreateVertices();
			while (CheckEdgesNeeded())
			{
				if (_availableVertices.Count == 0)
				{
					bool done =
						_connectedSubgraphs
						.Values
						.ToArray()
						.All(v => v.ConnectedVertexCount >= _maxEdges);

					if (done)
					{
						return;
					}
				}

				AddEdge();
			}
		}

		/// <summary>
		/// Cleans up the internal collections of the level generator in preparation for the
		/// generation of a new level.
		/// </summary>
		private void CleanUp()
		{
			_connectedSubgraphs.Clear();
			_availableVertices.Clear();
		}

		/// <summary>
		/// Creates the vertices in the level graph and fills the level matrix and connected
		/// subgraph dictionary with them.
		/// </summary>
		private void CreateVertices()
		{
			for (int row = 0; row < _rows; row++)
			{
				for (int column = 0; column < _columns; column++)
				{
					var vertex = new Vertex(row, column);
					_levelMatrix[row, column] = vertex;
					_connectedSubgraphs[vertex] = new ConnectedSubgraph(vertex);
					_availableVertices.Add(vertex);
				}
			}
		}

		/// <summary>
		/// Checks if additional edges still need to be added to the level graph to fits the
		/// generation constraints.
		/// </summary>
		/// <returns><see langword="true"/> if the level graph does not fit the generation
		/// constraints.</returns>
		/// <remarks>
		/// <para>The level generation constraints are the following:
		/// <list>
		/// <item>There must be no vertices with less than two edges entering them.</item>
		/// <item>The entire level graph must be connected.</item>
		/// <item>There must be no vertices which can support another edge to an adjacent vertex.
		/// </item>
		/// </list>
		/// </para>
		/// </remarks>
		private bool CheckEdgesNeeded()
		{
			if (!_connectedSubgraphs.Any())
			{
				return false;
			}

			if (_availableVertices.Count > 0)
			{
				return true;
			}

			// If the whole level graph is not strongly connected, more edges will be needed
			ConnectedSubgraph firstConnectedSubgraph = _connectedSubgraphs.Values.First();
			if (firstConnectedSubgraph.ConnectedVertexCount < _connectedSubgraphs.Count
				|| !firstConnectedSubgraph.IsStronglyConnected)
			{
				return true;
			}

			// If there is a vertex with less than _minEdges edges entering it, more edges will be needed
			if (_connectedSubgraphs.Keys.Any(v => v.ConnectedVertexCount < _minEdges))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if an edge can be added between two adjacent vertices without resulting in an
		/// intersection.
		/// </summary>
		/// <param name="vertex1">The first vertex.</param>
		/// <param name="vertex2">The second vertex.</param>
		/// <returns><see langword="true"/> if the vertices are not directly connected and an edge
		/// can be added between them without resulting in an intersection.</returns>
		private bool CheckCanConnectVertices(Vertex vertex1, Vertex vertex2)
		{
			// If the two vertices are already connected, they should not be connected again
			if (vertex1.IsConnectedToVertex(vertex2))
			{
				return false;
			}

			// If the two vertices are diagonally adjacent in the level matrix, check if there is
			// an edge between their opposite vertices that will result in an intersection
			if (vertex1.Row != vertex2.Row && vertex1.Column != vertex2.Column)
			{
				Vertex crossVertex1 = _levelMatrix[vertex1.Row, vertex2.Column];
				Vertex crossVertex2 = _levelMatrix[vertex2.Row, vertex1.Column];
				if (crossVertex1.IsConnectedToVertex(crossVertex2))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Obtains a list of all vertices adjacent to a vertex.
		/// </summary>
		/// <param name="vertex">The vertex whose adjacent vertices should be obtained.</param>
		/// <returns>A list of adjacent vertices.</returns>
		/// <remarks>
		/// <para>Adjacent vertices are all vertices that are horizontally, vertically or
		/// diagonally adjacent to the specified vertex in the level matrix. All edges in the level
		/// graph are between adjacent vertices, so as to make the generation algorithm simpler.
		/// </para>
		/// </remarks>
		private List<Vertex> GetAdjacentVertices(Vertex vertex)
		{
			var adjacentVertices = new List<Vertex>();
			if (vertex.Row > 0)
			{
				if (vertex.Column > 0)
				{
					adjacentVertices.Add(_levelMatrix[vertex.Row - 1, vertex.Column - 1]);
				}
				adjacentVertices.Add(_levelMatrix[vertex.Row - 1, vertex.Column]);
				if (vertex.Column < _columns - 1)
				{
					adjacentVertices.Add(_levelMatrix[vertex.Row - 1, vertex.Column + 1]);
				}
			}
			if (vertex.Column > 0)
			{
				adjacentVertices.Add(_levelMatrix[vertex.Row, vertex.Column - 1]);
			}
			if (vertex.Column < _columns - 1)
			{
				adjacentVertices.Add(_levelMatrix[vertex.Row, vertex.Column + 1]);
			}
			if (vertex.Row < _rows - 1)
			{
				if (vertex.Column > 0)
				{
					adjacentVertices.Add(_levelMatrix[vertex.Row + 1, vertex.Column - 1]);
				}
				adjacentVertices.Add(_levelMatrix[vertex.Row + 1, vertex.Column]);
				if (vertex.Column < _columns - 1)
				{
					adjacentVertices.Add(_levelMatrix[vertex.Row + 1, vertex.Column + 1]);
				}
			}
			return adjacentVertices;
		}

		/// <summary>
		/// Adds an edge to the level graph.
		/// </summary>
		/// <exception cref="Exception">
		/// An invalid level matrix state is reached and the algorithm cannot recover.
		/// </exception>
		/// <remarks>
		/// <para>If no edge can be added to the level graph without violating the maximum edges
		/// entering a single vertex and without resulting in an intersection, a suitable edge is
		/// chosen and removed from the level graph and another one is added in the freed spot.
		/// </para>
		/// </remarks>
		private void AddEdge()
		{
			// If there are still vertices able to support another edge, choose one at random and
			// add an edge entering it
			while (_availableVertices.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(_availableVertices.Count);
				Vertex vertex = _availableVertices[vertexIndex];
				if (AddEdgeEnteringVertex(vertex))
				{
					return;
				}
				else
				{
					_availableVertices.Remove(vertex);
				}
			}

			// If no vertices are able to support another edge, check if the whole level graph is
			// connected
			ConnectedSubgraph firstConnectedSubgraph = _connectedSubgraphs.Values.First();
			if (firstConnectedSubgraph.ConnectedVertexCount < _connectedSubgraphs.Count)
			{
				// If the whole level graph is not connected, find a suitable vertex from the first
				// connected subgraph which can be connected to another connected subgraph
				List<Vertex> vertices = firstConnectedSubgraph.ConnectedVertices.ToList();
				while (vertices.Count > 0)
				{
					int vertexIndex = RandomSingleton.Next(vertices.Count);
					Vertex vertex = vertices[vertexIndex];
					if (AddExternalEdgeEnteringVertex(firstConnectedSubgraph, vertex))
					{
						return;
					}
					else
					{
						vertices.RemoveAt(vertexIndex);
					}
				}

				throw new Exception("Invalid level matrix state reached: failed to find a vertex from the first connected subgraph that can be connected to any other connected subgraph.");
			}
			else if (!firstConnectedSubgraph.IsStronglyConnected)
			{
				// If the whole level graph is connected but there is a weakly connected portion,
				// find a suitable vertex from the strongly connected portion which can be
				// connected to a vertex from a weakly connected portion
				List<Vertex> vertices = firstConnectedSubgraph.StronglyConnectedVertices.ToList();
				while (vertices.Count > 0)
				{
					int vertexIndex = RandomSingleton.Next(vertices.Count);
					Vertex vertex = vertices[vertexIndex];
					if (AddStronglyConnectingEdgeEnteringVertex(firstConnectedSubgraph, vertex))
					{
						return;
					}
					else
					{
						vertices.RemoveAt(vertexIndex);
					}
				}

				throw new Exception("Invalid level matrix state reached: failed to find a vertex from the strongly connected subgraph that can be connected to any weakly connected subgraph.");
			}
			else
			{
				// If the whole level graph is connected, attempt to find an isolated vertex and
				// add an edge entering it
				List<Vertex> vertices = _connectedSubgraphs.Keys.Where(v => v.ConnectedVertexCount < _minEdges).ToList();
				if (vertices.Count > 0)
				{
					int vertexIndex = RandomSingleton.Next(vertices.Count);
					Vertex vertex = vertices[vertexIndex];
					if (!AddSecondaryEdgeEnteringVertex(vertex))
					{
						throw new Exception("Invalid level matrix state reached: failed to find a possible edge from an isolated vertex.");
					}
				}
			}
		}

		/// <summary>
		/// Attempts to add an edge entering a specific vertex.
		/// </summary>
		/// <param name="vertex">The vertex which should be entered by the edge.</param>
		/// <returns><see langword="true"/> if the edge was successfully added.</returns>
		private bool AddEdgeEnteringVertex(Vertex vertex)
		{
			// If the maximum number of edges entering the vertex has been reached, give up
			if (vertex.ConnectedVertexCount == _maxEdges)
			{
				return false;
			}

			// Attempt to find an adjacent vertex which can support another edge
			Vertex otherVertex = null;
			List<Vertex> adjacentVertices = GetAdjacentVertices(vertex);
			while (adjacentVertices.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(adjacentVertices.Count);
				Vertex adjacentVertex = adjacentVertices[vertexIndex];
				if (adjacentVertex.ConnectedVertexCount < _maxEdges
					&& CheckCanConnectVertices(vertex, adjacentVertex))
				{
					otherVertex = adjacentVertex;
					break;
				}
				adjacentVertices.RemoveAt(vertexIndex);
			}

			// If no suitable other vertex was found, give up
			if (otherVertex == null)
			{
				return false;
			}

			// Add the edge to the level graph
			AddEdgeBetweenVertices(vertex, otherVertex);
			return true;
		}

		/// <summary>
		/// Attempts to remove an edge entering a specific vertex in a specific connected subgraph
		/// and replace it with another edge towards a different connected subgraph, thus merging
		/// the two connected subgraphs.
		/// </summary>
		/// <param name="connectedSubgraph">The connected subgraph which should be merged with
		/// another one.</param>
		/// <param name="vertex">The vertex from the strongly connected portion of the connected
		/// subgraph.</param>
		/// <returns><see langword="true"/> if the edge was successfully added.</returns>
		private bool AddExternalEdgeEnteringVertex(ConnectedSubgraph connectedSubgraph, Vertex vertex)
		{
			// Attempt to find another vertex from a different connected subgraph that can be
			// connected to the specified vertex; if an edge prevents the two vertices from being
			// connected, remove it
			Vertex otherVertex = null;
			List<Vertex> adjacentVertices = GetAdjacentVertices(vertex);
			while (adjacentVertices.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(adjacentVertices.Count);
				Vertex adjacentVertex = adjacentVertices[vertexIndex];
				if (!connectedSubgraph.ContainsVertex(adjacentVertex))
				{
					if (!CheckCanConnectVertices(vertex, adjacentVertex))
					{
						// If the two vertices cannot be connected without causing an intersection,
						// remove the other edge
						Vertex crossVertex1 = _levelMatrix[vertex.Row, adjacentVertex.Column];
						Vertex crossVertex2 = _levelMatrix[adjacentVertex.Row, vertex.Column];
						RemoveEdgeBetweenVertices(crossVertex1, crossVertex2);
					}
					// Check if the other vertex has the maximum number of edges entering it and if
					// so, remove a random edge entering it
					if (adjacentVertex.ConnectedVertexCount == _maxEdges)
					{
						RemoveEdgeEnteringVertex(adjacentVertex);
					}
					otherVertex = adjacentVertex;
					break;
				}
				adjacentVertices.RemoveAt(vertexIndex);
			}

			// If no suitable other vertex was found, give up
			if (otherVertex == null)
			{
				return false;
			}
			// Check if the specified vertex has the maximum number of edges entering it and if so,
			// remove a random edge entering it
			if (vertex.ConnectedVertexCount == _maxEdges)
			{
				RemoveEdgeEnteringVertex(vertex);
			}

			// Add the edge to the level graph
			AddEdgeBetweenVertices(vertex, otherVertex);
			return true;
		}

		/// <summary>
		/// Attempts to remove an edge entering a specific vertex from the strongly connected
		/// portion of a specific connected subgraph and replace it with another edge towards an
		/// edge from a weakly connected portion of the connected subgraph, thus expanding the
		/// strongly connected portion.
		/// </summary>
		/// <param name="connectedSubgraph">The connected subgraph which contains a weakly
		/// connected portion.</param>
		/// <param name="vertex">The vertex from the strongly connected portion of the connected
		/// subgraph.</param>
		/// <returns><see langword="true"/> if the edge was successfully added.</returns>
		private bool AddStronglyConnectingEdgeEnteringVertex(ConnectedSubgraph connectedSubgraph, Vertex vertex)
		{
			Vertex otherVertex = null;
			List<Vertex> adjacentVertices = GetAdjacentVertices(vertex);
			while (adjacentVertices.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(adjacentVertices.Count);
				Vertex adjacentVertex = adjacentVertices[vertexIndex];
				if (!vertex.IsConnectedToVertex(adjacentVertex)
					&& connectedSubgraph.ContainsVertex(adjacentVertex)
					&& !connectedSubgraph.IsVertexStronglyConnected(adjacentVertex))
				{
					if (connectedSubgraph.CheckWillConnectStrongly(vertex, adjacentVertex))
					{
						if (!CheckCanConnectVertices(vertex, adjacentVertex))
						{
							// If the two vertices cannot be connected without causing an
							// intersection, remove the other edge
							Vertex crossVertex1 = _levelMatrix[vertex.Row, adjacentVertex.Column];
							Vertex crossVertex2 = _levelMatrix[adjacentVertex.Row, vertex.Column];
							RemoveEdgeBetweenVertices(crossVertex1, crossVertex2);
						}

						// Check if the other vertex has the maximum number of edges entering it
						// and if so, remove a random edge entering it
						if (adjacentVertex.ConnectedVertexCount == _maxEdges)
						{
							RemoveEdgeEnteringVertex(adjacentVertex);
						}
						otherVertex = adjacentVertex;
						break;
					}
				}
				adjacentVertices.RemoveAt(vertexIndex);
			}

			// If no suitable other vertex was found, give up
			if (otherVertex == null)
			{
				return false;
			}

			// Check if the specified vertex has the maximum number of edges entering it and if so,
			// remove a random edge entering it
			if (vertex.ConnectedVertexCount == _maxEdges)
			{
				RemoveEdgeEnteringVertex(vertex);
			}

			// Add the edge to the level graph
			AddEdgeBetweenVertices(vertex, otherVertex);
			return true;
		}

		/// <summary>
		/// Attempts to add an edge entering an isolated vertex in order to meet the minimum
		/// requirement of two edges entering it.
		/// </summary>
		/// <param name="vertex">The isolated vertex which needs another edge entering it.</param>
		/// <returns><see langword="true"/> if the edge was successfully added.</returns>
		private bool AddSecondaryEdgeEnteringVertex(Vertex vertex)
		{
			// Attempt to find an adjacent vertex that can be connected to the specified vertex; if
			// an edge prevents the two vertices from being connected, remove it
			Vertex otherVertex = null;
			List<Vertex> adjacentVertices = GetAdjacentVertices(vertex);
			while (adjacentVertices.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(adjacentVertices.Count);
				Vertex adjacentVertex = adjacentVertices[vertexIndex];
				if (!vertex.IsConnectedToVertex(adjacentVertex))
				{
					if (!CheckCanConnectVertices(vertex, adjacentVertex))
					{
						// If the two vertices cannot be connected without causing an intersection,
						// remove the other edge
						Vertex crossVertex1 = _levelMatrix[vertex.Row, adjacentVertex.Column];
						Vertex crossVertex2 = _levelMatrix[adjacentVertex.Row, vertex.Column];
						RemoveEdgeBetweenVertices(crossVertex1, crossVertex2);
					}

					// Check if the other vertex has the maximum number of edges entering it and if
					// so, remove a random edge entering it
					if (adjacentVertex.ConnectedVertexCount == _maxEdges)
					{
						RemoveEdgeEnteringVertex(adjacentVertex);
					}
					otherVertex = adjacentVertex;
					break;
				}
				adjacentVertices.RemoveAt(vertexIndex);
			}

			// If no suitable other vertex was found, give up
			if (otherVertex == null)
			{
				return false;
			}

			// Add the edge to the level graph
			AddEdgeBetweenVertices(vertex, otherVertex);
			return true;
		}

		/// <summary>
		/// Removes a random edge entering a specific vertex.
		/// </summary>
		/// <param name="vertex">The vertex that should be disconnected from a random other vertex.
		/// </param>
		private void RemoveEdgeEnteringVertex(Vertex vertex)
		{
			List<Vertex> connectedVertices = vertex.ConnectedVertices.ToList();
			int vertexIndex = RandomSingleton.Next(connectedVertices.Count);
			Vertex vertexToDisconnect = connectedVertices[vertexIndex];

			RemoveEdgeBetweenVertices(vertex, vertexToDisconnect);
		}

		/// <summary>
		/// Adds a vertex to the list of vertices which need more edges entering them.
		/// </summary>
		/// <param name="vertex">The vertex which should be added to the list.</param>
		/// <remarks>
		/// <para>If the vertex is already in the list of vertices which need more edges entering
		/// them, the method does nothing.</para>
		/// </remarks>
		private void AddAvailableVertex(Vertex vertex)
		{
			if (_availableVertices.Contains(vertex))
			{
				_availableVertices.Add(vertex);
			}
		}

		/// <summary>
		/// Adds an edge connecting two vertices and recalculates their respective connected
		/// subgraphs if needed.
		/// </summary>
		/// <param name="vertex1">The first vertex.</param>
		/// <param name="vertex2">The second vertex.</param>
		private void AddEdgeBetweenVertices(Vertex vertex1, Vertex vertex2)
		{
			// Connect vertices
			vertex1.ConnectToVertex(vertex2);

			// If the two vertices were not part of the same connected subgraph until now,
			// recalculate the new connected subgraph and assign it to all vertices that belong to
			// it
			ConnectedSubgraph mainGraph = _connectedSubgraphs[vertex1];

			if (mainGraph != _connectedSubgraphs[vertex2])
			{
				mainGraph.RecalculateConnectedVertices();
				foreach (Vertex connectedVertex in mainGraph.ConnectedVertices)
				{
					_connectedSubgraphs[connectedVertex] = mainGraph;
				}
			}
			else if (mainGraph.CheckWillConnectStrongly(vertex1, vertex2))
			{
				mainGraph.RecalculateConnectedVertices();
			}
		}

		/// <summary>
		/// Removes an edge connecting two vertices and recalculates their respective connected
		/// subgraphs.
		/// </summary>
		/// <param name="vertex1">The first vertex.</param>
		/// <param name="vertex2">The second vertex.</param>
		private void RemoveEdgeBetweenVertices(Vertex vertex1, Vertex vertex2)
		{
			// Disconnect vertices
			vertex1.DisconnectFromVertex(vertex2);

			// Add the vertices to the list of vertices needing more edges entering them
			AddAvailableVertex(vertex1);
			AddAvailableVertex(vertex2);

			// If the vertices are diagonally adjacent in the level matrix, removing the edge
			// between them opens up space for an edge between the opposite vertices, so add those
			// to the list of vertices needing more edges entering them too
			if (vertex1.Row != vertex2.Row && vertex1.Column != vertex2.Column)
			{
				AddAvailableVertex(_levelMatrix[vertex1.Row, vertex2.Column]);
				AddAvailableVertex(_levelMatrix[vertex2.Row, vertex1.Column]);
			}

			// Adjust connected subgraphs for both vertices
			AdjustConnectedSubgraphs(vertex1, true);
			AdjustConnectedSubgraphs(vertex2, false);
		}

		/// <summary>
		/// Adjusts the connected subgraph assignment for a specific vertex.
		/// </summary>
		/// <param name="vertex">The vertex whose connected subgraph should be adjusted.
		/// </param>
		/// <param name="recalculateCurrent">Specifies whether the vertex's currently assigned
		/// connected subgraph should be recalculated before adjusting the assignment.</param>
		private void AdjustConnectedSubgraphs(Vertex vertex, bool recalculateCurrent)
		{
			ConnectedSubgraph currentGraph = _connectedSubgraphs[vertex];

			// Recalculate the currently assigned connected subgraph if needed
			if (recalculateCurrent)
				currentGraph.RecalculateConnectedVertices();

			// If the specified vertex does not belong to its currently assigned connected
			// subgraph, create a new connected subgraph object using the specified vertex as a
			// main vertex for it and then assign it to all vertices that belong to it
			if (!currentGraph.ContainsVertex(vertex))
			{
				var newGraph = new ConnectedSubgraph(vertex);
				foreach (Vertex connectedVertex in newGraph.ConnectedVertices)
				{
					_connectedSubgraphs[connectedVertex] = newGraph;
				}
			}
		}
	}
}
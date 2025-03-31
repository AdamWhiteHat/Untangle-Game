using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Untangle.Core;
using Untangle.Utils;

namespace Untangle.Generation
{
	public static class CriticalNonplanarGraphs
	{
		/// <summary>
		/// Generates a level from a random Critical Non-planar Graph with 1 random vertex removed.
		/// </summary>
		/// <returns>A ready-to-play GameLevel</returns>
		public static Vertex[] GenerateLevel()
		{
			int randomIndex = RandomSingleton.Next(Graphs.Count);
			return GenerateLevel(randomIndex);
		}

		public static Vertex[] GenerateLevel(string levelName)
		{
			if (!Graphs.ContainsKey(levelName))
			{
				throw new Exception($"Level name does not exist: {levelName}. Possible level names are: {string.Join(", ", Graphs.Keys)}");
			}
			var kvp = Graphs.FirstOrDefault(kvp => kvp.Key == levelName);

			int indexOf = Graphs.ToList().IndexOf(kvp);
			return GenerateLevel(indexOf);
		}

		/// <summary>
		/// Generates a level from the specified Critical Non-planar Graph with 1 random vertex removed.
		/// </summary>
		/// <returns>A ready-to-play GameLevel</returns>
		public static Vertex[] GenerateLevel(int graphIndex)
		{
			int[][] graph = Graphs.Values.ElementAt(graphIndex);

			List<int> distinctVertexIds = graph.SelectMany(arr => arr).Distinct().OrderBy(n => n).ToList();
			int maxVertexId = distinctVertexIds.Max();

			Dictionary<int, Vertex> vertexDict = new Dictionary<int, Vertex>();

			foreach (int id in distinctVertexIds)
			{
				var vertex = new Vertex(0, 0);
				vertexDict.Add(id, vertex);
			}

			foreach (int[] node in graph)
			{
				int id1 = node[0];
				int id2 = node[1];

				Vertex vertex1 = vertexDict[id1];
				Vertex vertex2 = vertexDict[id2];

				vertex1.ConnectToVertex(vertex2);
			}

			vertexDict = RemoveRandomVertex(vertexDict);

			return vertexDict.Values.ToArray();
		}

		private static Dictionary<int, Vertex> RemoveRandomVertex(Dictionary<int, Vertex> graph)
		{
			int removeIndex = RandomSingleton.Next(graph.Count); // Pick at random a vertex to remove
			var kvp = graph.ToList()[removeIndex];
			int removeKey = kvp.Key;
			Vertex removeValue = kvp.Value;

			List<Vertex> connectedVertices = removeValue.ConnectedVertices.ToList();
			foreach (Vertex otherVertex in connectedVertices)
			{
				removeValue.DisconnectFromVertex(otherVertex);
			}

			graph.Remove(removeKey);

			return graph;
		}

		public static Dictionary<string, int[][]> Graphs =
			new Dictionary<string, int[][]>()
			{
				/*
				{
					"{6,114}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 6 },
					}
				},

				{
					"{6,149}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 6 },
						new int[] { 5, 6 },
					}
				},
				*/
				{
					"{6,150}",
					new int[][]
					{
						new[] { 1, 3 },
						new[] { 1, 4 },
						new[] { 1, 5 },
						new[] { 1, 6 },
						new[] { 2, 3 },
						new[] { 2, 4 },
						new[] { 2, 5 },
						new[] { 2, 6 },
						new[] { 3, 5 },
						new[] { 3, 6 },
						new[] { 5, 6 },
					}
				},
				/*
				{
					"{7,733}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
					}
				},

				{
					"{7,734}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,735}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,737}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,738}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,740}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,748}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,749}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,758}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 7 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,763}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,764}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,836}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 5 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,837}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,838}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 5 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,839}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 7 },
					}
				},

				{
					"{7,840}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,841}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,842}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,843}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,856}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,858}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,875}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 5 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,885}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,957}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,961}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,962}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,963}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
					}
				},

				{
					"{7,964}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,965}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,1008}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,1009}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,1011}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{7,1013}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},
				*/
				{
					"{7,1015}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},
				/*
				{
					"{7,1017}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 5, 7 },
					}
				},

				{
					"{7,1018}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
					}
				},

				{
					"{7,1019}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
					}
				},
				
				{
					"{7,1020}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},
				*/
			
				{
					"{8,8186}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 5 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
					}
				},
				/*
				{
					"{8,10982}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 7 },
						new int[] { 5, 8 },
						new int[] { 6, 8 },
					}
				},
				*/
				/*
				{
					"{9,172352}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 9 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 2, 9 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 8 },
						new int[] { 5, 9 },
					}
				},

				{
					"{9,176255}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 3, 6 },
						new int[] { 3, 8 },
						new int[] { 3, 9 },
						new int[] { 4, 8 },
						new int[] { 4, 9 },
						new int[] { 5, 8 },
						new int[] { 5, 9 },
						new int[] { 6, 7 },
					}
				},
				*/
				{
					"{9,181483}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 8 },
						new int[] { 4, 9 },
						new int[] { 5, 8 },
						new int[] { 5, 9 },
						new int[] { 6, 9 },
					}
				},
				/*
				{
					"{BiggestLittlePolygon,6}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 3 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 6 },
						new int[] { 4, 5 },
						new int[] { 5, 6 },
					}
				},
				*/
				{
					"{BiggestLittlePolygon,8}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 8 },
						new int[] { 2, 3 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 4 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 8 },
						new int[] { 5, 6 },
						new int[] { 6, 7 },
						new int[] { 7, 8 },
					}
				},
				/*
				{
					"{Circulant,{7,{1,2}}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 7 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
					}
				},

				{
					"{Circulant,{9,{1,2}}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 9 },
						new int[] { 2, 3 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 3, 4 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 8 },
						new int[] { 4, 9 },
						new int[] { 5, 6 },
						new int[] { 5, 9 },
						new int[] { 6, 7 },
						new int[] { 7, 8 },
						new int[] { 8, 9 },
					}
				},

				{
					"{Circulant,{11,{1,2}}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 1, 11 },
						new int[] { 2, 3 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 4 },
						new int[] { 3, 8 },
						new int[] { 3, 9 },
						new int[] { 4, 5 },
						new int[] { 4, 9 },
						new int[] { 4, 10 },
						new int[] { 5, 6 },
						new int[] { 5, 10 },
						new int[] { 5, 11 },
						new int[] { 6, 7 },
						new int[] { 6, 11 },
						new int[] { 7, 8 },
						new int[] { 8, 9 },
						new int[] { 9, 10 },
						new int[] { 10, 11 },
					}
				},

				{
					"{Circulant,{13,{1,2}}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 1, 13 },
						new int[] { 2, 3 },
						new int[] { 2, 8 },
						new int[] { 2, 9 },
						new int[] { 3, 4 },
						new int[] { 3, 9 },
						new int[] { 3, 10 },
						new int[] { 4, 5 },
						new int[] { 4, 10 },
						new int[] { 4, 11 },
						new int[] { 5, 6 },
						new int[] { 5, 11 },
						new int[] { 5, 12 },
						new int[] { 6, 7 },
						new int[] { 6, 12 },
						new int[] { 6, 13 },
						new int[] { 7, 8 },
						new int[] { 7, 13 },
						new int[] { 8, 9 },
						new int[] { 9, 10 },
						new int[] { 10, 11 },
						new int[] { 11, 12 },
						new int[] { 12, 13 },
					}
				},

				{
					"{Circulant,{15,{1,2}}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 14 },
						new int[] { 1, 15 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 15 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
						new int[] { 7, 9 },
						new int[] { 8, 9 },
						new int[] { 8, 10 },
						new int[] { 9, 10 },
						new int[] { 9, 11 },
						new int[] { 10, 11 },
						new int[] { 10, 12 },
						new int[] { 11, 12 },
						new int[] { 11, 13 },
						new int[] { 12, 13 },
						new int[] { 12, 14 },
						new int[] { 13, 14 },
						new int[] { 13, 15 },
						new int[] { 14, 15 },
					}
				},

				{
					"{Circulant,{17,{1,2}}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 16 },
						new int[] { 1, 17 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 17 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
						new int[] { 7, 9 },
						new int[] { 8, 9 },
						new int[] { 8, 10 },
						new int[] { 9, 10 },
						new int[] { 9, 11 },
						new int[] { 10, 11 },
						new int[] { 10, 12 },
						new int[] { 11, 12 },
						new int[] { 11, 13 },
						new int[] { 12, 13 },
						new int[] { 12, 14 },
						new int[] { 13, 14 },
						new int[] { 13, 15 },
						new int[] { 14, 15 },
						new int[] { 14, 16 },
						new int[] { 15, 16 },
						new int[] { 15, 17 },
						new int[] { 16, 17 },
					}
				},
				*/
				{
					"{Circulant,{19,{1,2}}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 18 },
						new int[] { 1, 19 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 19 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
						new int[] { 7, 9 },
						new int[] { 8, 9 },
						new int[] { 8, 10 },
						new int[] { 9, 10 },
						new int[] { 9, 11 },
						new int[] { 10, 11 },
						new int[] { 10, 12 },
						new int[] { 11, 12 },
						new int[] { 11, 13 },
						new int[] { 12, 13 },
						new int[] { 12, 14 },
						new int[] { 13, 14 },
						new int[] { 13, 15 },
						new int[] { 14, 15 },
						new int[] { 14, 16 },
						new int[] { 15, 16 },
						new int[] { 15, 17 },
						new int[] { 16, 17 },
						new int[] { 16, 18 },
						new int[] { 17, 18 },
						new int[] { 17, 19 },
						new int[] { 18, 19 },
					}
				},

				{
					"{CompleteKPartite,{1,1,1,3}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
					}
				},

				{
					"{CompleteTripartite,{1,2,3}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
					}
				},
				/*
				{
					"{Cubic,{8,3}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 5 },
						new int[] { 1, 8 },
						new int[] { 2, 3 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 8 },
						new int[] { 6, 7 },
						new int[] { 7, 8 },
					}
				},
				*/
				{
					"{Cubic,{10,9}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 2, 3 },
						new int[] { 2, 7 },
						new int[] { 3, 6 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 9 },
						new int[] { 5, 8 },
						new int[] { 6, 9 },
						new int[] { 6, 10 },
						new int[] { 7, 8 },
						new int[] { 7, 10 },
						new int[] { 9, 10 },
					}
				},
				/*
				{
					"{Cubic,{10,10}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 5 },
						new int[] { 2, 3 },
						new int[] { 2, 7 },
						new int[] { 3, 6 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 5, 9 },
						new int[] { 6, 8 },
						new int[] { 7, 10 },
						new int[] { 8, 9 },
						new int[] { 8, 10 },
						new int[] { 9, 10 },
					}
				},

				{
					"{Cubic,{12,52}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 2, 3 },
						new int[] { 2, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 7 },
						new int[] { 5, 9 },
						new int[] { 6, 10 },
						new int[] { 6, 11 },
						new int[] { 7, 10 },
						new int[] { 8, 9 },
						new int[] { 8, 12 },
						new int[] { 9, 12 },
						new int[] { 10, 11 },
						new int[] { 11, 12 },
					}
				},

				{
					"{Cubic,{12,62}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 2, 3 },
						new int[] { 2, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 9 },
						new int[] { 5, 10 },
						new int[] { 6, 11 },
						new int[] { 6, 12 },
						new int[] { 7, 9 },
						new int[] { 7, 11 },
						new int[] { 8, 10 },
						new int[] { 8, 12 },
						new int[] { 9, 11 },
						new int[] { 10, 12 },
					}
				},

				{
					"{Cubic,{14,367}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 2, 3 },
						new int[] { 2, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 9 },
						new int[] { 5, 10 },
						new int[] { 6, 11 },
						new int[] { 6, 12 },
						new int[] { 7, 8 },
						new int[] { 7, 9 },
						new int[] { 8, 13 },
						new int[] { 9, 11 },
						new int[] { 10, 13 },
						new int[] { 10, 14 },
						new int[] { 11, 12 },
						new int[] { 12, 14 },
						new int[] { 13, 14 },
					}
				},
		
				{
					"{Cubic,{14,370}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 2, 3 },
						new int[] { 2, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 9 },
						new int[] { 5, 10 },
						new int[] { 6, 11 },
						new int[] { 6, 12 },
						new int[] { 7, 8 },
						new int[] { 7, 13 },
						new int[] { 8, 14 },
						new int[] { 9, 10 },
						new int[] { 9, 13 },
						new int[] { 10, 14 },
						new int[] { 11, 12 },
						new int[] { 11, 13 },
						new int[] { 12, 14 },
					}
				},
				*/
				{
					"{CubicTransitive,23}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 2, 5 },
						new int[] { 2, 9 },
						new int[] { 2, 10 },
						new int[] { 3, 6 },
						new int[] { 3, 11 },
						new int[] { 3, 12 },
						new int[] { 4, 13 },
						new int[] { 4, 14 },
						new int[] { 5, 15 },
						new int[] { 5, 16 },
						new int[] { 6, 17 },
						new int[] { 6, 18 },
						new int[] { 7, 8 },
						new int[] { 7, 18 },
						new int[] { 8, 9 },
						new int[] { 9, 10 },
						new int[] { 10, 11 },
						new int[] { 11, 12 },
						new int[] { 12, 13 },
						new int[] { 13, 14 },
						new int[] { 14, 15 },
						new int[] { 15, 16 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
					}
				},
				/*
				{
					"{EdgeTransitive,{15,3}}",
					new int[][]
					{
						new int[] { 1, 10 },
						new int[] { 1, 11 },
						new int[] { 2, 12 },
						new int[] { 2, 13 },
						new int[] { 3, 12 },
						new int[] { 3, 14 },
						new int[] { 4, 13 },
						new int[] { 4, 15 },
						new int[] { 5, 14 },
						new int[] { 5, 15 },
						new int[] { 6, 10 },
						new int[] { 6, 12 },
						new int[] { 7, 10 },
						new int[] { 7, 15 },
						new int[] { 8, 11 },
						new int[] { 8, 13 },
						new int[] { 9, 11 },
						new int[] { 9, 14 },
					}
				},
				*/
				{
					"{EdgeTransitive,{15,4}}",
					new int[][]
					{
						new int[] { 1, 11 },
						new int[] { 1, 12 },
						new int[] { 2, 13 },
						new int[] { 2, 14 },
						new int[] { 3, 13 },
						new int[] { 3, 15 },
						new int[] { 4, 14 },
						new int[] { 4, 15 },
						new int[] { 5, 11 },
						new int[] { 5, 13 },
						new int[] { 6, 12 },
						new int[] { 6, 13 },
						new int[] { 7, 12 },
						new int[] { 7, 14 },
						new int[] { 8, 12 },
						new int[] { 8, 15 },
						new int[] { 9, 11 },
						new int[] { 9, 14 },
						new int[] { 10, 11 },
						new int[] { 10, 15 },
					}
				},

				{
					"{GeneralizedQuadrangle,{2,1}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 4 },
						new int[] { 3, 8 },
						new int[] { 3, 9 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 6 },
						new int[] { 5, 8 },
						new int[] { 6, 7 },
						new int[] { 6, 9 },
						new int[] { 7, 9 },
						new int[] { 8, 9 },
					}
				},
				/*
				{
					"{HamiltonLaceable,{8,3}}",
					new int[][]
					{
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 8 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
					}
				},
				*/
				{
					"{HamiltonLaceable,{8,7}}",
					new int[][]
					{
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 8 },
						new int[] { 3, 5 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
					}
				},
				/*
				{
					"{Harary,{3,7}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 7 },
						new int[] { 2, 3 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 5, 6 },
						new int[] { 6, 7 },
					}
				},
				*/
				{
					"{Harary,{3,9}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 9 },
						new int[] { 2, 3 },
						new int[] { 2, 7 },
						new int[] { 3, 4 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 9 },
						new int[] { 5, 6 },
						new int[] { 6, 7 },
						new int[] { 7, 8 },
						new int[] { 8, 9 },
					}
				},

				{
					"{MoebiusLadder,5}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 6 },
						new int[] { 1, 10 },
						new int[] { 2, 3 },
						new int[] { 2, 7 },
						new int[] { 3, 4 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 9 },
						new int[] { 5, 6 },
						new int[] { 5, 10 },
						new int[] { 6, 7 },
						new int[] { 7, 8 },
						new int[] { 8, 9 },
						new int[] { 9, 10 },
					}
				},
		/*
				{
					"{MoebiusLadder,6}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 7 },
						new int[] { 1, 12 },
						new int[] { 2, 3 },
						new int[] { 2, 8 },
						new int[] { 3, 4 },
						new int[] { 3, 9 },
						new int[] { 4, 5 },
						new int[] { 4, 10 },
						new int[] { 5, 6 },
						new int[] { 5, 11 },
						new int[] { 6, 7 },
						new int[] { 6, 12 },
						new int[] { 7, 8 },
						new int[] { 8, 9 },
						new int[] { 9, 10 },
						new int[] { 10, 11 },
						new int[] { 11, 12 },
					}
				},

				{
					"{MoebiusLadder,7}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 8 },
						new int[] { 1, 14 },
						new int[] { 2, 3 },
						new int[] { 2, 9 },
						new int[] { 3, 4 },
						new int[] { 3, 10 },
						new int[] { 4, 5 },
						new int[] { 4, 11 },
						new int[] { 5, 6 },
						new int[] { 5, 12 },
						new int[] { 6, 7 },
						new int[] { 6, 13 },
						new int[] { 7, 8 },
						new int[] { 7, 14 },
						new int[] { 8, 9 },
						new int[] { 9, 10 },
						new int[] { 10, 11 },
						new int[] { 11, 12 },
						new int[] { 12, 13 },
						new int[] { 13, 14 },
					}
				},

				{
					"{MoebiusLadder,8}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 9 },
						new int[] { 1, 16 },
						new int[] { 2, 3 },
						new int[] { 2, 10 },
						new int[] { 3, 4 },
						new int[] { 3, 11 },
						new int[] { 4, 5 },
						new int[] { 4, 12 },
						new int[] { 5, 6 },
						new int[] { 5, 13 },
						new int[] { 6, 7 },
						new int[] { 6, 14 },
						new int[] { 7, 8 },
						new int[] { 7, 15 },
						new int[] { 8, 9 },
						new int[] { 8, 16 },
						new int[] { 9, 10 },
						new int[] { 10, 11 },
						new int[] { 11, 12 },
						new int[] { 12, 13 },
						new int[] { 13, 14 },
						new int[] { 14, 15 },
						new int[] { 15, 16 },
					}
				},

				{
					"{MoebiusLadder,9}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 10 },
						new int[] { 1, 18 },
						new int[] { 2, 3 },
						new int[] { 2, 11 },
						new int[] { 3, 4 },
						new int[] { 3, 12 },
						new int[] { 4, 5 },
						new int[] { 4, 13 },
						new int[] { 5, 6 },
						new int[] { 5, 14 },
						new int[] { 6, 7 },
						new int[] { 6, 15 },
						new int[] { 7, 8 },
						new int[] { 7, 16 },
						new int[] { 8, 9 },
						new int[] { 8, 17 },
						new int[] { 9, 10 },
						new int[] { 9, 18 },
						new int[] { 10, 11 },
						new int[] { 11, 12 },
						new int[] { 12, 13 },
						new int[] { 13, 14 },
						new int[] { 14, 15 },
						new int[] { 15, 16 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
					}
				},

				{
					"{MoebiusLadder,10}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 11 },
						new int[] { 1, 20 },
						new int[] { 2, 3 },
						new int[] { 2, 12 },
						new int[] { 3, 4 },
						new int[] { 3, 13 },
						new int[] { 4, 5 },
						new int[] { 4, 14 },
						new int[] { 5, 6 },
						new int[] { 5, 15 },
						new int[] { 6, 7 },
						new int[] { 6, 16 },
						new int[] { 7, 8 },
						new int[] { 7, 17 },
						new int[] { 8, 9 },
						new int[] { 8, 18 },
						new int[] { 9, 10 },
						new int[] { 9, 19 },
						new int[] { 10, 11 },
						new int[] { 10, 20 },
						new int[] { 11, 12 },
						new int[] { 12, 13 },
						new int[] { 13, 14 },
						new int[] { 14, 15 },
						new int[] { 15, 16 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
					}
				},

				{
					"{MoebiusLadder,11}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 12 },
						new int[] { 1, 22 },
						new int[] { 2, 3 },
						new int[] { 2, 13 },
						new int[] { 3, 4 },
						new int[] { 3, 14 },
						new int[] { 4, 5 },
						new int[] { 4, 15 },
						new int[] { 5, 6 },
						new int[] { 5, 16 },
						new int[] { 6, 7 },
						new int[] { 6, 17 },
						new int[] { 7, 8 },
						new int[] { 7, 18 },
						new int[] { 8, 9 },
						new int[] { 8, 19 },
						new int[] { 9, 10 },
						new int[] { 9, 20 },
						new int[] { 10, 11 },
						new int[] { 10, 21 },
						new int[] { 11, 12 },
						new int[] { 11, 22 },
						new int[] { 12, 13 },
						new int[] { 13, 14 },
						new int[] { 14, 15 },
						new int[] { 15, 16 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
					}
				},

				{
					"{MoebiusLadder,12}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 13 },
						new int[] { 1, 24 },
						new int[] { 2, 3 },
						new int[] { 2, 14 },
						new int[] { 3, 4 },
						new int[] { 3, 15 },
						new int[] { 4, 5 },
						new int[] { 4, 16 },
						new int[] { 5, 6 },
						new int[] { 5, 17 },
						new int[] { 6, 7 },
						new int[] { 6, 18 },
						new int[] { 7, 8 },
						new int[] { 7, 19 },
						new int[] { 8, 9 },
						new int[] { 8, 20 },
						new int[] { 9, 10 },
						new int[] { 9, 21 },
						new int[] { 10, 11 },
						new int[] { 10, 22 },
						new int[] { 11, 12 },
						new int[] { 11, 23 },
						new int[] { 12, 13 },
						new int[] { 12, 24 },
						new int[] { 13, 14 },
						new int[] { 14, 15 },
						new int[] { 15, 16 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
					}
				},

				{
					"{MoebiusLadder,13}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 14 },
						new int[] { 1, 26 },
						new int[] { 2, 3 },
						new int[] { 2, 15 },
						new int[] { 3, 4 },
						new int[] { 3, 16 },
						new int[] { 4, 5 },
						new int[] { 4, 17 },
						new int[] { 5, 6 },
						new int[] { 5, 18 },
						new int[] { 6, 7 },
						new int[] { 6, 19 },
						new int[] { 7, 8 },
						new int[] { 7, 20 },
						new int[] { 8, 9 },
						new int[] { 8, 21 },
						new int[] { 9, 10 },
						new int[] { 9, 22 },
						new int[] { 10, 11 },
						new int[] { 10, 23 },
						new int[] { 11, 12 },
						new int[] { 11, 24 },
						new int[] { 12, 13 },
						new int[] { 12, 25 },
						new int[] { 13, 14 },
						new int[] { 13, 26 },
						new int[] { 14, 15 },
						new int[] { 15, 16 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
					}
				},

				{
					"{MoebiusLadder,14}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 15 },
						new int[] { 1, 28 },
						new int[] { 2, 3 },
						new int[] { 2, 16 },
						new int[] { 3, 4 },
						new int[] { 3, 17 },
						new int[] { 4, 5 },
						new int[] { 4, 18 },
						new int[] { 5, 6 },
						new int[] { 5, 19 },
						new int[] { 6, 7 },
						new int[] { 6, 20 },
						new int[] { 7, 8 },
						new int[] { 7, 21 },
						new int[] { 8, 9 },
						new int[] { 8, 22 },
						new int[] { 9, 10 },
						new int[] { 9, 23 },
						new int[] { 10, 11 },
						new int[] { 10, 24 },
						new int[] { 11, 12 },
						new int[] { 11, 25 },
						new int[] { 12, 13 },
						new int[] { 12, 26 },
						new int[] { 13, 14 },
						new int[] { 13, 27 },
						new int[] { 14, 15 },
						new int[] { 14, 28 },
						new int[] { 15, 16 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
					}
				},

				{
					"{MoebiusLadder,15}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 16 },
						new int[] { 1, 30 },
						new int[] { 2, 3 },
						new int[] { 2, 17 },
						new int[] { 3, 4 },
						new int[] { 3, 18 },
						new int[] { 4, 5 },
						new int[] { 4, 19 },
						new int[] { 5, 6 },
						new int[] { 5, 20 },
						new int[] { 6, 7 },
						new int[] { 6, 21 },
						new int[] { 7, 8 },
						new int[] { 7, 22 },
						new int[] { 8, 9 },
						new int[] { 8, 23 },
						new int[] { 9, 10 },
						new int[] { 9, 24 },
						new int[] { 10, 11 },
						new int[] { 10, 25 },
						new int[] { 11, 12 },
						new int[] { 11, 26 },
						new int[] { 12, 13 },
						new int[] { 12, 27 },
						new int[] { 13, 14 },
						new int[] { 13, 28 },
						new int[] { 14, 15 },
						new int[] { 14, 29 },
						new int[] { 15, 16 },
						new int[] { 15, 30 },
						new int[] { 16, 17 },
						new int[] { 17, 18 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
					}
				},

				{
					"{MoebiusLadder,16}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 17 },
						new int[] { 1, 32 },
						new int[] { 2, 3 },
						new int[] { 2, 18 },
						new int[] { 3, 4 },
						new int[] { 3, 19 },
						new int[] { 4, 5 },
						new int[] { 4, 20 },
						new int[] { 5, 6 },
						new int[] { 5, 21 },
						new int[] { 6, 7 },
						new int[] { 6, 22 },
						new int[] { 7, 8 },
						new int[] { 7, 23 },
						new int[] { 8, 9 },
						new int[] { 8, 24 },
						new int[] { 9, 10 },
						new int[] { 9, 25 },
						new int[] { 10, 11 },
						new int[] { 10, 26 },
						new int[] { 11, 12 },
						new int[] { 11, 27 },
						new int[] { 12, 13 },
						new int[] { 12, 28 },
						new int[] { 13, 14 },
						new int[] { 13, 29 },
						new int[] { 14, 15 },
						new int[] { 14, 30 },
						new int[] { 15, 16 },
						new int[] { 15, 31 },
						new int[] { 16, 17 },
						new int[] { 16, 32 },
						new int[] { 17, 18 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
					}
				},

				{
					"{MoebiusLadder,17}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 18 },
						new int[] { 1, 34 },
						new int[] { 2, 3 },
						new int[] { 2, 19 },
						new int[] { 3, 4 },
						new int[] { 3, 20 },
						new int[] { 4, 5 },
						new int[] { 4, 21 },
						new int[] { 5, 6 },
						new int[] { 5, 22 },
						new int[] { 6, 7 },
						new int[] { 6, 23 },
						new int[] { 7, 8 },
						new int[] { 7, 24 },
						new int[] { 8, 9 },
						new int[] { 8, 25 },
						new int[] { 9, 10 },
						new int[] { 9, 26 },
						new int[] { 10, 11 },
						new int[] { 10, 27 },
						new int[] { 11, 12 },
						new int[] { 11, 28 },
						new int[] { 12, 13 },
						new int[] { 12, 29 },
						new int[] { 13, 14 },
						new int[] { 13, 30 },
						new int[] { 14, 15 },
						new int[] { 14, 31 },
						new int[] { 15, 16 },
						new int[] { 15, 32 },
						new int[] { 16, 17 },
						new int[] { 16, 33 },
						new int[] { 17, 18 },
						new int[] { 17, 34 },
						new int[] { 18, 19 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
					}
				},

				{
					"{MoebiusLadder,18}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 19 },
						new int[] { 1, 36 },
						new int[] { 2, 3 },
						new int[] { 2, 20 },
						new int[] { 3, 4 },
						new int[] { 3, 21 },
						new int[] { 4, 5 },
						new int[] { 4, 22 },
						new int[] { 5, 6 },
						new int[] { 5, 23 },
						new int[] { 6, 7 },
						new int[] { 6, 24 },
						new int[] { 7, 8 },
						new int[] { 7, 25 },
						new int[] { 8, 9 },
						new int[] { 8, 26 },
						new int[] { 9, 10 },
						new int[] { 9, 27 },
						new int[] { 10, 11 },
						new int[] { 10, 28 },
						new int[] { 11, 12 },
						new int[] { 11, 29 },
						new int[] { 12, 13 },
						new int[] { 12, 30 },
						new int[] { 13, 14 },
						new int[] { 13, 31 },
						new int[] { 14, 15 },
						new int[] { 14, 32 },
						new int[] { 15, 16 },
						new int[] { 15, 33 },
						new int[] { 16, 17 },
						new int[] { 16, 34 },
						new int[] { 17, 18 },
						new int[] { 17, 35 },
						new int[] { 18, 19 },
						new int[] { 18, 36 },
						new int[] { 19, 20 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
					}
				},

				{
					"{MoebiusLadder,19}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 20 },
						new int[] { 1, 38 },
						new int[] { 2, 3 },
						new int[] { 2, 21 },
						new int[] { 3, 4 },
						new int[] { 3, 22 },
						new int[] { 4, 5 },
						new int[] { 4, 23 },
						new int[] { 5, 6 },
						new int[] { 5, 24 },
						new int[] { 6, 7 },
						new int[] { 6, 25 },
						new int[] { 7, 8 },
						new int[] { 7, 26 },
						new int[] { 8, 9 },
						new int[] { 8, 27 },
						new int[] { 9, 10 },
						new int[] { 9, 28 },
						new int[] { 10, 11 },
						new int[] { 10, 29 },
						new int[] { 11, 12 },
						new int[] { 11, 30 },
						new int[] { 12, 13 },
						new int[] { 12, 31 },
						new int[] { 13, 14 },
						new int[] { 13, 32 },
						new int[] { 14, 15 },
						new int[] { 14, 33 },
						new int[] { 15, 16 },
						new int[] { 15, 34 },
						new int[] { 16, 17 },
						new int[] { 16, 35 },
						new int[] { 17, 18 },
						new int[] { 17, 36 },
						new int[] { 18, 19 },
						new int[] { 18, 37 },
						new int[] { 19, 20 },
						new int[] { 19, 38 },
						new int[] { 20, 21 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
						new int[] { 36, 37 },
						new int[] { 37, 38 },
					}
				},

				{
					"{MoebiusLadder,20}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 21 },
						new int[] { 1, 40 },
						new int[] { 2, 3 },
						new int[] { 2, 22 },
						new int[] { 3, 4 },
						new int[] { 3, 23 },
						new int[] { 4, 5 },
						new int[] { 4, 24 },
						new int[] { 5, 6 },
						new int[] { 5, 25 },
						new int[] { 6, 7 },
						new int[] { 6, 26 },
						new int[] { 7, 8 },
						new int[] { 7, 27 },
						new int[] { 8, 9 },
						new int[] { 8, 28 },
						new int[] { 9, 10 },
						new int[] { 9, 29 },
						new int[] { 10, 11 },
						new int[] { 10, 30 },
						new int[] { 11, 12 },
						new int[] { 11, 31 },
						new int[] { 12, 13 },
						new int[] { 12, 32 },
						new int[] { 13, 14 },
						new int[] { 13, 33 },
						new int[] { 14, 15 },
						new int[] { 14, 34 },
						new int[] { 15, 16 },
						new int[] { 15, 35 },
						new int[] { 16, 17 },
						new int[] { 16, 36 },
						new int[] { 17, 18 },
						new int[] { 17, 37 },
						new int[] { 18, 19 },
						new int[] { 18, 38 },
						new int[] { 19, 20 },
						new int[] { 19, 39 },
						new int[] { 20, 21 },
						new int[] { 20, 40 },
						new int[] { 21, 22 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
						new int[] { 36, 37 },
						new int[] { 37, 38 },
						new int[] { 38, 39 },
						new int[] { 39, 40 },
					}
				},

				{
					"{MoebiusLadder,21}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 22 },
						new int[] { 1, 42 },
						new int[] { 2, 3 },
						new int[] { 2, 23 },
						new int[] { 3, 4 },
						new int[] { 3, 24 },
						new int[] { 4, 5 },
						new int[] { 4, 25 },
						new int[] { 5, 6 },
						new int[] { 5, 26 },
						new int[] { 6, 7 },
						new int[] { 6, 27 },
						new int[] { 7, 8 },
						new int[] { 7, 28 },
						new int[] { 8, 9 },
						new int[] { 8, 29 },
						new int[] { 9, 10 },
						new int[] { 9, 30 },
						new int[] { 10, 11 },
						new int[] { 10, 31 },
						new int[] { 11, 12 },
						new int[] { 11, 32 },
						new int[] { 12, 13 },
						new int[] { 12, 33 },
						new int[] { 13, 14 },
						new int[] { 13, 34 },
						new int[] { 14, 15 },
						new int[] { 14, 35 },
						new int[] { 15, 16 },
						new int[] { 15, 36 },
						new int[] { 16, 17 },
						new int[] { 16, 37 },
						new int[] { 17, 18 },
						new int[] { 17, 38 },
						new int[] { 18, 19 },
						new int[] { 18, 39 },
						new int[] { 19, 20 },
						new int[] { 19, 40 },
						new int[] { 20, 21 },
						new int[] { 20, 41 },
						new int[] { 21, 22 },
						new int[] { 21, 42 },
						new int[] { 22, 23 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
						new int[] { 36, 37 },
						new int[] { 37, 38 },
						new int[] { 38, 39 },
						new int[] { 39, 40 },
						new int[] { 40, 41 },
						new int[] { 41, 42 },
					}
				},

				{
					"{MoebiusLadder,22}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 23 },
						new int[] { 1, 44 },
						new int[] { 2, 3 },
						new int[] { 2, 24 },
						new int[] { 3, 4 },
						new int[] { 3, 25 },
						new int[] { 4, 5 },
						new int[] { 4, 26 },
						new int[] { 5, 6 },
						new int[] { 5, 27 },
						new int[] { 6, 7 },
						new int[] { 6, 28 },
						new int[] { 7, 8 },
						new int[] { 7, 29 },
						new int[] { 8, 9 },
						new int[] { 8, 30 },
						new int[] { 9, 10 },
						new int[] { 9, 31 },
						new int[] { 10, 11 },
						new int[] { 10, 32 },
						new int[] { 11, 12 },
						new int[] { 11, 33 },
						new int[] { 12, 13 },
						new int[] { 12, 34 },
						new int[] { 13, 14 },
						new int[] { 13, 35 },
						new int[] { 14, 15 },
						new int[] { 14, 36 },
						new int[] { 15, 16 },
						new int[] { 15, 37 },
						new int[] { 16, 17 },
						new int[] { 16, 38 },
						new int[] { 17, 18 },
						new int[] { 17, 39 },
						new int[] { 18, 19 },
						new int[] { 18, 40 },
						new int[] { 19, 20 },
						new int[] { 19, 41 },
						new int[] { 20, 21 },
						new int[] { 20, 42 },
						new int[] { 21, 22 },
						new int[] { 21, 43 },
						new int[] { 22, 23 },
						new int[] { 22, 44 },
						new int[] { 23, 24 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
						new int[] { 36, 37 },
						new int[] { 37, 38 },
						new int[] { 38, 39 },
						new int[] { 39, 40 },
						new int[] { 40, 41 },
						new int[] { 41, 42 },
						new int[] { 42, 43 },
						new int[] { 43, 44 },
					}
				},

				{
					"{MoebiusLadder,23}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 24 },
						new int[] { 1, 46 },
						new int[] { 2, 3 },
						new int[] { 2, 25 },
						new int[] { 3, 4 },
						new int[] { 3, 26 },
						new int[] { 4, 5 },
						new int[] { 4, 27 },
						new int[] { 5, 6 },
						new int[] { 5, 28 },
						new int[] { 6, 7 },
						new int[] { 6, 29 },
						new int[] { 7, 8 },
						new int[] { 7, 30 },
						new int[] { 8, 9 },
						new int[] { 8, 31 },
						new int[] { 9, 10 },
						new int[] { 9, 32 },
						new int[] { 10, 11 },
						new int[] { 10, 33 },
						new int[] { 11, 12 },
						new int[] { 11, 34 },
						new int[] { 12, 13 },
						new int[] { 12, 35 },
						new int[] { 13, 14 },
						new int[] { 13, 36 },
						new int[] { 14, 15 },
						new int[] { 14, 37 },
						new int[] { 15, 16 },
						new int[] { 15, 38 },
						new int[] { 16, 17 },
						new int[] { 16, 39 },
						new int[] { 17, 18 },
						new int[] { 17, 40 },
						new int[] { 18, 19 },
						new int[] { 18, 41 },
						new int[] { 19, 20 },
						new int[] { 19, 42 },
						new int[] { 20, 21 },
						new int[] { 20, 43 },
						new int[] { 21, 22 },
						new int[] { 21, 44 },
						new int[] { 22, 23 },
						new int[] { 22, 45 },
						new int[] { 23, 24 },
						new int[] { 23, 46 },
						new int[] { 24, 25 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
						new int[] { 36, 37 },
						new int[] { 37, 38 },
						new int[] { 38, 39 },
						new int[] { 39, 40 },
						new int[] { 40, 41 },
						new int[] { 41, 42 },
						new int[] { 42, 43 },
						new int[] { 43, 44 },
						new int[] { 44, 45 },
						new int[] { 45, 46 },
					}
				},

				{
					"{MoebiusLadder,24}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 25 },
						new int[] { 1, 48 },
						new int[] { 2, 3 },
						new int[] { 2, 26 },
						new int[] { 3, 4 },
						new int[] { 3, 27 },
						new int[] { 4, 5 },
						new int[] { 4, 28 },
						new int[] { 5, 6 },
						new int[] { 5, 29 },
						new int[] { 6, 7 },
						new int[] { 6, 30 },
						new int[] { 7, 8 },
						new int[] { 7, 31 },
						new int[] { 8, 9 },
						new int[] { 8, 32 },
						new int[] { 9, 10 },
						new int[] { 9, 33 },
						new int[] { 10, 11 },
						new int[] { 10, 34 },
						new int[] { 11, 12 },
						new int[] { 11, 35 },
						new int[] { 12, 13 },
						new int[] { 12, 36 },
						new int[] { 13, 14 },
						new int[] { 13, 37 },
						new int[] { 14, 15 },
						new int[] { 14, 38 },
						new int[] { 15, 16 },
						new int[] { 15, 39 },
						new int[] { 16, 17 },
						new int[] { 16, 40 },
						new int[] { 17, 18 },
						new int[] { 17, 41 },
						new int[] { 18, 19 },
						new int[] { 18, 42 },
						new int[] { 19, 20 },
						new int[] { 19, 43 },
						new int[] { 20, 21 },
						new int[] { 20, 44 },
						new int[] { 21, 22 },
						new int[] { 21, 45 },
						new int[] { 22, 23 },
						new int[] { 22, 46 },
						new int[] { 23, 24 },
						new int[] { 23, 47 },
						new int[] { 24, 25 },
						new int[] { 24, 48 },
						new int[] { 25, 26 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
						new int[] { 36, 37 },
						new int[] { 37, 38 },
						new int[] { 38, 39 },
						new int[] { 39, 40 },
						new int[] { 40, 41 },
						new int[] { 41, 42 },
						new int[] { 42, 43 },
						new int[] { 43, 44 },
						new int[] { 44, 45 },
						new int[] { 45, 46 },
						new int[] { 46, 47 },
						new int[] { 47, 48 },
					}
				},
				*/
				{
					"{MoebiusLadder,25}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 26 },
						new int[] { 1, 50 },
						new int[] { 2, 3 },
						new int[] { 2, 27 },
						new int[] { 3, 4 },
						new int[] { 3, 28 },
						new int[] { 4, 5 },
						new int[] { 4, 29 },
						new int[] { 5, 6 },
						new int[] { 5, 30 },
						new int[] { 6, 7 },
						new int[] { 6, 31 },
						new int[] { 7, 8 },
						new int[] { 7, 32 },
						new int[] { 8, 9 },
						new int[] { 8, 33 },
						new int[] { 9, 10 },
						new int[] { 9, 34 },
						new int[] { 10, 11 },
						new int[] { 10, 35 },
						new int[] { 11, 12 },
						new int[] { 11, 36 },
						new int[] { 12, 13 },
						new int[] { 12, 37 },
						new int[] { 13, 14 },
						new int[] { 13, 38 },
						new int[] { 14, 15 },
						new int[] { 14, 39 },
						new int[] { 15, 16 },
						new int[] { 15, 40 },
						new int[] { 16, 17 },
						new int[] { 16, 41 },
						new int[] { 17, 18 },
						new int[] { 17, 42 },
						new int[] { 18, 19 },
						new int[] { 18, 43 },
						new int[] { 19, 20 },
						new int[] { 19, 44 },
						new int[] { 20, 21 },
						new int[] { 20, 45 },
						new int[] { 21, 22 },
						new int[] { 21, 46 },
						new int[] { 22, 23 },
						new int[] { 22, 47 },
						new int[] { 23, 24 },
						new int[] { 23, 48 },
						new int[] { 24, 25 },
						new int[] { 24, 49 },
						new int[] { 25, 26 },
						new int[] { 25, 50 },
						new int[] { 26, 27 },
						new int[] { 27, 28 },
						new int[] { 28, 29 },
						new int[] { 29, 30 },
						new int[] { 30, 31 },
						new int[] { 31, 32 },
						new int[] { 32, 33 },
						new int[] { 33, 34 },
						new int[] { 34, 35 },
						new int[] { 35, 36 },
						new int[] { 36, 37 },
						new int[] { 37, 38 },
						new int[] { 38, 39 },
						new int[] { 39, 40 },
						new int[] { 40, 41 },
						new int[] { 41, 42 },
						new int[] { 42, 43 },
						new int[] { 43, 44 },
						new int[] { 44, 45 },
						new int[] { 45, 46 },
						new int[] { 46, 47 },
						new int[] { 47, 48 },
						new int[] { 48, 49 },
						new int[] { 49, 50 },
					}
				},
				/*
				{
					"PentatopeGraph",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 4, 5 },
					}
				},
				*/
				{
					"{QuasiCubic,{9,19}}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 9 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 2, 9 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 8 },
						new int[] { 5, 9 },
						new int[] { 6, 9 },
					}
				},
				/*
				{
					"{QuasiCubic,{9,22}}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 2, 9 },
						new int[] { 3, 6 },
						new int[] { 3, 8 },
						new int[] { 3, 9 },
						new int[] { 4, 8 },
						new int[] { 4, 9 },
						new int[] { 5, 8 },
						new int[] { 5, 9 },
						new int[] { 6, 7 },
					}
				},
			
				{
					"{QuasiCubic,{9,23}}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 2, 5 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 6 },
						new int[] { 3, 8 },
						new int[] { 3, 9 },
						new int[] { 4, 8 },
						new int[] { 4, 9 },
						new int[] { 5, 7 },
						new int[] { 5, 9 },
						new int[] { 6, 9 },
					}
				},

				{
					"{Queen,{2,3}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 5, 6 },
					}
				},
	*/
				{
					"{Rook,{2,4}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 5, 8 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
					}
				},

				{
					"{SelfComplementary,{9,14}}",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 6 },
						new int[] { 1, 8 },
						new int[] { 1, 9 },
						new int[] { 2, 5 },
						new int[] { 2, 8 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 3, 9 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 4, 9 },
						new int[] { 5, 9 },
						new int[] { 6, 8 },
						new int[] { 6, 9 },
						new int[] { 7, 8 },
						new int[] { 7, 9 },
					}
				},

				{
					"{StackedBook,{3,3}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 5, 8 },
						new int[] { 5, 9 },
						new int[] { 6, 10 },
						new int[] { 7, 11 },
						new int[] { 8, 12 },
						new int[] { 9, 10 },
						new int[] { 9, 11 },
						new int[] { 9, 12 },
					}
				},

				{
					"{Turan,{8,5}}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 7 },
						new int[] { 5, 8 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
					}
				},
				/*
				{
					"{Turan,{8,6}}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 5, 8 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
					}
				},
				*/
				{
					"{Turan,{8,7}}",
					new int[][]
					{
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 1, 8 },
						new int[] { 2, 3 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 2, 7 },
						new int[] { 2, 8 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 3, 8 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 4, 7 },
						new int[] { 4, 8 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 5, 8 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
					}
				},

				{
					"{UnitDistance,{12,2}}",
					new int[][]
					{
						new int[] { 1, 6 },
						new int[] { 1, 7 },
						new int[] { 1, 10 },
						new int[] { 1, 12 },
						new int[] { 2, 5 },
						new int[] { 2, 8 },
						new int[] { 2, 9 },
						new int[] { 2, 11 },
						new int[] { 3, 4 },
						new int[] { 3, 6 },
						new int[] { 3, 9 },
						new int[] { 4, 5 },
						new int[] { 4, 10 },
						new int[] { 5, 10 },
						new int[] { 5, 11 },
						new int[] { 6, 9 },
						new int[] { 6, 12 },
						new int[] { 7, 8 },
						new int[] { 7, 10 },
						new int[] { 8, 9 },
						new int[] { 11, 12 },
					}
				},
				/*
				{
					"{UnitDistanceForbidden,{9,14,6}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 2, 6 },
						new int[] { 2, 8 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 9 },
						new int[] { 5, 6 },
						new int[] { 6, 7 },
						new int[] { 7, 8 },
						new int[] { 8, 9 },
					}
				},

				{
					"{UnitDistanceForbidden,{9,14,7}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 2, 8 },
						new int[] { 3, 6 },
						new int[] { 3, 7 },
						new int[] { 4, 6 },
						new int[] { 4, 9 },
						new int[] { 5, 7 },
						new int[] { 5, 8 },
						new int[] { 5, 9 },
						new int[] { 7, 8 },
						new int[] { 8, 9 },
					}
				},
		*/
				{
					"{UnitDistanceForbidden,{9,14,16}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 2, 7 },
						new int[] { 3, 5 },
						new int[] { 3, 9 },
						new int[] { 4, 8 },
						new int[] { 4, 9 },
						new int[] { 5, 6 },
						new int[] { 5, 7 },
						new int[] { 6, 7 },
						new int[] { 6, 8 },
						new int[] { 7, 8 },
						new int[] { 7, 9 },
					}
				},
		/*
				{
					"{UnitDistanceForbidden,{9,14,19}}",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 3 },
						new int[] { 1, 4 },
						new int[] { 2, 4 },
						new int[] { 2, 9 },
						new int[] { 3, 5 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 6 },
						new int[] { 4, 8 },
						new int[] { 5, 6 },
						new int[] { 6, 9 },
						new int[] { 7, 8 },
						new int[] { 7, 9 },
					}
				},
		*/
				{
					"UtilityGraph",
					new int[][]
					{
						new int[] { 1, 4 },
						new int[] { 1, 5 },
						new int[] { 1, 6 },
						new int[] { 2, 4 },
						new int[] { 2, 5 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 5 },
						new int[] { 3, 6 },
					}
				},

				{
					"WagnerGraph",
					new int[][]
					{
						new int[] { 1, 2 },
						new int[] { 1, 5 },
						new int[] { 1, 8 },
						new int[] { 2, 3 },
						new int[] { 2, 6 },
						new int[] { 3, 4 },
						new int[] { 3, 7 },
						new int[] { 4, 5 },
						new int[] { 4, 8 },
						new int[] { 5, 6 },
						new int[] { 6, 7 },
						new int[] { 7, 8 },
					}
				}
		};
	}
}

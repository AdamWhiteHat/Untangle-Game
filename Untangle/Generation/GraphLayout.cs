using Microsoft.Msagl.GraphmapsWithMesh;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Untangle.ViewModels;

using PointF = System.Drawing.PointF;

namespace Untangle.Generation
{
	public enum GraphLayoutTypes
	{
		Random,
		Circle,
		Lattice,
		LatticeCrystal,
		Hexagonal,
		CriticalNonplanarGraphs
	}

	public static class GraphLayout
	{
		public static int SelectRandomLayout(Graph graph, System.Windows.Size size)
		{
			int arrangement = RandomSingleton.Next(5);
			int intersectionCount = 0;
			while (intersectionCount == 0)
			{
				if (arrangement == 0)
				{
					intersectionCount = GraphLayout.Circle(graph);
				}
				else if (arrangement == 1)
				{
					intersectionCount = GraphLayout.RandomPoints(graph, size);
				}
				else if (arrangement == 2)
				{
					intersectionCount = GraphLayout.Lattice(graph, size, (int)size.Height / 101, (int)size.Width / 101);
				}
				else if (arrangement == 3)
				{
					intersectionCount = QuaziCrystal(graph, size, Math.Min(graph.VertexCount, (int)size.Height / 101), Math.Min(graph.VertexCount, (int)size.Width / 101));
				}
				else if (arrangement == 4)
				{
					intersectionCount = Hexagonal(graph, size);
				}
			}

			return intersectionCount;
		}

		public static int ChooseLayout(Graph graph, System.Windows.Size size, GraphLayoutTypes layout)
		{
			int intersectionCount = 0;
			while (intersectionCount == 0)
			{
				switch (layout)
				{
					case GraphLayoutTypes.Circle:
						intersectionCount = GraphLayout.Circle(graph);
						break;
					case GraphLayoutTypes.Lattice:
						intersectionCount = GraphLayout.Lattice(graph, size, (int)size.Height / 101, (int)size.Width / 101);
						break;
					case GraphLayoutTypes.LatticeCrystal:
						intersectionCount = QuaziCrystal(graph, size, (int)size.Height / 101, (int)size.Width / 101);
						break;
					case GraphLayoutTypes.Hexagonal:
						intersectionCount = Hexagonal(graph, size);
						break;
					case GraphLayoutTypes.Random:
					default:
						intersectionCount = GraphLayout.RandomPoints(graph, size);
						break;
				}
			}

			return intersectionCount;
		}

		private static int _peturbMagnitude = 6;
		private static System.Windows.Point PreturbPosition(System.Windows.Point In)
		{
			int xOffset = RandomSingleton.Next(1, _peturbMagnitude) * (RandomSingleton.NextBool() ? -1 : 1);
			int yOffset = RandomSingleton.Next(1, _peturbMagnitude) * (RandomSingleton.NextBool() ? -1 : 1);

			return new System.Windows.Point(In.X + xOffset, In.Y + yOffset);
		}


		/// <summary>
		/// Resets the positions of all vertices in the game level, arranging them in a circle in
		/// random order.
		/// </summary>
		public static int Circle(Graph graph)
		{
			int vertexCount = graph.VertexCount;
			List<ViewModels.Vertex> verticesToScramble = graph.Vertices.ToList();
			int i = 0;
			while (verticesToScramble.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(verticesToScramble.Count);

				ViewModels.Vertex vertex = verticesToScramble[vertexIndex];
				double angle = Math.PI * 2 * i / vertexCount;
				var position = new System.Windows.Point(Math.Cos(angle) * 300.0, -Math.Sin(angle) * 300.0);

				vertex.SetPosition(position);
				vertex.StartingPosition = position;

				verticesToScramble.RemoveAt(vertexIndex);
				i++;
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		/// <summary>
		/// Resets the positions of all vertices in the game level, arranging them randomly.
		/// </summary>
		public static int RandomPoints(Graph graph, System.Windows.Size size)
		{
			int vertexCount = graph.VertexCount;
			List<ViewModels.Vertex> verticesToScramble = graph.Vertices.ToList();
			int i = 0;
			while (verticesToScramble.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(verticesToScramble.Count);
				ViewModels.Vertex vertex = verticesToScramble[vertexIndex];

				int width = (int)(size.Width / 2);
				int height = (int)(size.Height / 2);

				int x = RandomSingleton.Next(-width, width);
				int y = RandomSingleton.Next(-height, height);
				var position = new System.Windows.Point(x, y);

				vertex.SetPosition(position);
				vertex.StartingPosition = position;

				verticesToScramble.RemoveAt(vertexIndex);
				i++;
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		/// <summary>
		/// Resets the positions of all vertices in the game level, 
		/// arranging them randomly into a regular square lattice.
		/// </summary>
		public static int Lattice(Graph graph, System.Windows.Size size, int rows, int columns)
		{
			var lattice = GetLatticeRowsAndColumns(graph, size, rows, columns);

			int[] rowVectors = lattice.Rows;
			int[] columnVectors = lattice.Columns;

			List<(int, int)> latticePoints = new List<(int, int)>();

			int colIndex = 0;
			int rowIndex = 0;
			while (colIndex < columnVectors.Length)
			{
				rowIndex = 0;
				while (rowIndex < rowVectors.Length)
				{
					latticePoints.Add((columnVectors[colIndex], rowVectors[rowIndex]));
					rowIndex++;
				}
				colIndex++;
			}

			List<ViewModels.Vertex> verticesToScramble = graph.Vertices.ToList();
			int i = 0;
			while (verticesToScramble.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(verticesToScramble.Count);
				ViewModels.Vertex vertex = verticesToScramble[vertexIndex];

				int latticeIndex = RandomSingleton.Next(latticePoints.Count);
				(int, int) point = latticePoints[latticeIndex];
				latticePoints.RemoveAt(latticeIndex);

				int x = point.Item1;
				int y = point.Item2;

				var position = PreturbPosition(new System.Windows.Point(x, y));
				vertex.SetPosition(position);
				vertex.StartingPosition = position;

				verticesToScramble.RemoveAt(vertexIndex);
				i++;
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		private static (int[] Rows, int[] Columns) GetLatticeRowsAndColumns(Graph graph, System.Windows.Size size, int rows, int columns)
		{
			int width = (int)(size.Width - (size.Width / 10));
			int height = (int)(size.Height - (size.Height / 10));


			int vertexCount = graph.VertexCount;
			bool addRows = (columns >= rows);

			while (rows * columns <= vertexCount)
			{
				if (addRows)
				{
					rows++;
					addRows = false;
				}
				else
				{
					columns++;
					addRows = true;
				}
			}

			var columnBasis = (int)(width / columns);
			var rowBasis = (int)(height / rows);

			int widthStart = -(int)(width / 2);
			int heightStart = -(int)(height / 2);

			int multiplier = 0;
			int[] columnVectors = Enumerable.Repeat(widthStart, columns + 1).Select(n => n + (columnBasis * multiplier++)).ToArray();

			multiplier = 0;

			int[] rowVectors = Enumerable.Repeat(heightStart, rows + 1).Select(n => n + (rowBasis * multiplier++)).ToArray();

			return (Rows: rowVectors, Columns: columnVectors);
		}


		/// <summary>
		/// Resets the positions of all vertices in the game level, 
		/// arranging them randomly into lattice points based on wave equations.
		/// </summary>
		public static int QuaziCrystal(Graph graph, System.Windows.Size size, int rows, int columns)
		{
			var lattice = GetLatticeRowsAndColumns(graph, size, rows, columns);

			int[] rowVectors = lattice.Rows;
			int[] columnVectors = lattice.Columns;

			System.Drawing.Size latticeSize = new System.Drawing.Size(columns, rows);

			bool lastChance = false;
			float scale = 8.0f;
			int symmetries = 7;
			int availableSpots = 0;
			int step = -1;
			List<Tuple<bool, System.Drawing.Point>> positions = new List<Tuple<bool, System.Drawing.Point>>();
		beginLoop:
			do
			{
				step++;
				positions = QuasicrystalCalc(scale, symmetries, latticeSize, step);
				availableSpots = positions.Where(tup => tup.Item1).Count();
			}
			while (
				availableSpots < graph.VertexCount
				&&
				step < 9
			);

			if (step == 9)
			{
				if (!lastChance)
				{
					if ((positions.Count - availableSpots) >= graph.VertexCount)
					{
						positions = positions.Select(tup => new Tuple<bool, System.Drawing.Point>(!tup.Item1, tup.Item2)).ToList();
					}
					else
					{
						lastChance = true;
						symmetries = 2;
						step = -1;
						goto beginLoop;
					}
				}
				else
				{
					throw new Exception("Couldn't find enough lattice points to place all the vertices.");
				}
			}

			var selected = positions.Where(tup => tup.Item1).Select(tup => tup.Item2).OrderByDescending(pt => pt.X + pt.Y).ToList();

			Stack<System.Drawing.Point> stack = new Stack<System.Drawing.Point>(selected);

			List<ViewModels.Vertex> verticesToScramble = graph.Vertices.ToList();
			int i = 0;
			while (verticesToScramble.Count > 0)
			{
				int vertexIndex = RandomSingleton.Next(verticesToScramble.Count);
				ViewModels.Vertex vertex = verticesToScramble[vertexIndex];

				var latticePoint = stack.Pop();

				int x = columnVectors[latticePoint.X];
				int y = rowVectors[latticePoint.Y];

				var position = PreturbPosition(new System.Windows.Point(x, y));
				vertex.SetPosition(position);
				vertex.StartingPosition = position;

				verticesToScramble.RemoveAt(vertexIndex);
				i++;
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		private static List<Tuple<bool, System.Drawing.Point>> QuasicrystalCalc(float scale, int symmetries, System.Drawing.Size size, int step)
		{
			float angle = (float)(Math.PI / symmetries);

			IEnumerable<float> rangeTheta = Enumerable.Range(0, symmetries).Select(i => angle * i);

			List<Tuple<float, float>> precalculatedTheta = rangeTheta.Select(t => new Tuple<float, float>((float)Math.Sin(t), ((float)Math.Cos(t)))).ToList();

			IEnumerable<int> rangeX = Enumerable.Range(0, size.Width);
			List<int> rangeY = Enumerable.Range(0, size.Height).ToList();

			List<Tuple<bool, System.Drawing.Point>> result =
				rangeX.SelectMany(x =>
					rangeY.Select(y =>
						new Tuple<bool, System.Drawing.Point>(
							ThresholdFunction(precalculatedTheta.Select(theta => WaveFunction(scale, theta.Item1, theta.Item2, x, y, step)).Sum()) == 1,
							new System.Drawing.Point(x, y)
						)
					)
				).ToList();

			return result;
		}

		private static int ThresholdFunction(float w)
		{
			// (1 + Tanh[ 10 * (w - 0.5) ] ) / 2
			return (int)Math.Round(((1.0f + Math.Tanh(10.0f * (w - 0.5f))) / 2.0f));
		}

		private static float WaveFunction(float scale, float sinTheta, float cosTheta, float x, float y, float step)
		{
			float sum = (float)((y * sinTheta) + Math.Cos(x * cosTheta) + step) * scale;
			return (float)Math.Sin(sum);
		}

		/// <summary>
		///  Resets the positions of all vertices in the game level, arranging them in a Hexagonal lattice.
		/// </summary>
		public static int Hexagonal(Graph graph, System.Windows.Size size)
		{
			int hexagonsRequired = graph.VertexCount / 3;

			int rows = (int)Math.Floor(Math.Sqrt(hexagonsRequired));

			int columns = hexagonsRequired / rows;

			float height = (float)(size.Height / (rows + 1));

			List<PointF> hexPoints = GetHexLatticePoints(rows, columns, height);

			float maxWidth = hexPoints.Select(pt => pt.X).Max();
			float maxHeight = hexPoints.Select(pt => pt.Y).Max();

			var vertexPool = graph.Vertices.ToList();

			float xAdjustment = (float)maxWidth / 2;
			float yAdjustment = (float)maxHeight / 2;

			foreach (PointF point in hexPoints)
			{
				if (!vertexPool.Any())
				{
					break;
				}
				int nextIndex = RandomSingleton.Next(0, vertexPool.Count);
				ViewModels.Vertex vertex = vertexPool[nextIndex];

				var position = PreturbPosition(new System.Windows.Point(point.X - xAdjustment, point.Y - yAdjustment));
				vertex.SetPosition(position);
				vertex.StartingPosition = position;

				vertexPool.Remove(vertex);
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		// Define other methods and classes here
		private static List<PointF> GetHexLatticePoints(int rows, int columns, float height)
		{
			List<PointF> results = new List<PointF>();

			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < columns; col++) // Draw the row.
				{
					PointF[] hexagon = GetHexagonPoints(height, row, col); // Get the points for the row's next hexagon.
					results.AddRange(hexagon);
				}
			}
			return results.Distinct(new PointFEqualityComparer()).ToList();
		}

		// Return the points that define the indicated hexagon.
		private static PointF[] GetHexagonPoints(float height, float row, float col)
		{
			float width = HexWidth(height);
			float y = height / 2; // Start with the leftmost corner of the upper left hexagon.
			float x = 0;

			x += col * (width * 0.75f); // Move over for the column number.
			y += row * height; // Move down the required number of rows.        
			if (col % 2 == 1) // If the column is odd, move down half a hex more.
			{
				y += height / 2;
			}

			return new PointF[] // Generate the points.
			{
			new PointF(x, y),
			new PointF(x + width * 0.25f, y - height / 2),
			new PointF(x + width * 0.75f, y - height / 2),
			new PointF(x + width, y),
			new PointF(x + width * 0.75f, y + height / 2),
			new PointF(x + width * 0.25f, y + height / 2),
			};
		}

		// Return the width of a hexagon.
		private static float HexWidth(float height)
		{
			return (float)(4 * (height / 2 / Math.Sqrt(3)));
		}

		private class PointFEqualityComparer : IEqualityComparer<PointF>
		{
			bool IEqualityComparer<PointF>.Equals(PointF x, PointF y)
			{
				return (x.X == y.X && x.Y == y.Y) ? true : false;
			}

			int IEqualityComparer<PointF>.GetHashCode(PointF obj)
			{
				return new Tuple<float, float>(obj.X, obj.Y).GetHashCode();
			}
		}
	}
}

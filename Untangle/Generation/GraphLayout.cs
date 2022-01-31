using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Untangle.ViewModels;

using PointF = System.Drawing.PointF;

namespace Untangle.Generation
{
	public static class GraphLayout
	{
		public static int SelectRandomLayout(Graph graph, System.Windows.Size size)
		{
			int arrangement = RandomSingleton.Next(7);

			int intersectionCount = 0;
			while (intersectionCount == 0)
			{
				if (arrangement == 0 || arrangement == 1)
				{
					intersectionCount = GraphLayout.Circle(graph);
				}
				else if (arrangement == 2)
				{
					intersectionCount = GraphLayout.RandomPoints(graph, size);
				}
				else if (arrangement == 3 || arrangement == 4)
				{
					intersectionCount = GraphLayout.Lattice(graph, size, (int)size.Height / 101, (int)size.Width / 101);
				}
				else if (arrangement == 5 || arrangement == 6)
				{
					intersectionCount = Hexagonal(graph, size);
				}
			}

			return intersectionCount;
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
				vertex.Position = position;

				verticesToScramble.RemoveAt(vertexIndex);
				i++;
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		/// <summary>
		/// Resets the positions of all vertices in the game level, arranging them in a circle in
		/// random order.
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
				vertex.Position = position;

				verticesToScramble.RemoveAt(vertexIndex);
				i++;
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		/// <summary>
		/// Resets the positions of all vertices in the game level, arranging them in a circle in
		/// random order.
		/// </summary>
		public static int Lattice(Graph graph, System.Windows.Size size, int rows, int columns)
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
			var columnVectors = Enumerable.Repeat(widthStart, columns + 1).Select(n => n + (columnBasis * multiplier++)).ToArray();
			multiplier = 0;
			var rowVectors = Enumerable.Repeat(heightStart, rows + 1).Select(n => n + (rowBasis * multiplier++)).ToArray();

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

				var position = new System.Windows.Point(x, y);
				vertex.Position = position;

				verticesToScramble.RemoveAt(vertexIndex);
				i++;
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
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
				ViewModels.Vertex selected = vertexPool[nextIndex];
				vertexPool.Remove(selected);
				selected.SetPosition(new System.Windows.Point(point.X - xAdjustment, point.Y - yAdjustment));
			}

			graph.CalculateAllIntersections();
			return graph.IntersectionCount;
		}

		// Define other methods and classes here
		public static List<PointF> GetHexLatticePoints(int rows, int columns, float height)
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
		public static PointF[] GetHexagonPoints(float height, float row, float col)
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
		public static float HexWidth(float height)
		{
			return (float)(4 * (height / 2 / Math.Sqrt(3)));
		}

		public class PointFEqualityComparer : IEqualityComparer<PointF>
		{
			bool IEqualityComparer<PointF>.Equals(PointF x, PointF y)
			{
				if (x == null)
				{
					return (y == null) ? true : false;
				}
				return (x.X == y.X && x.Y == y.Y) ? true : false;
			}

			int IEqualityComparer<PointF>.GetHashCode(PointF obj)
			{
				return new Tuple<float, float>(obj.X, obj.Y).GetHashCode();
			}
		}
	}
}

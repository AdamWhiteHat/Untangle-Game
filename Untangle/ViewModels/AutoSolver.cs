using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Msagl;
using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Prototype.Ranking;
using Microsoft.Msagl.Prototype.Phylo;

namespace Untangle.ViewModels
{
	public static class AutoSolver
	{
		public static void Solve(Untangle.ViewModels.Graph graph, System.Windows.Size gameBoardSize)
		{
			Dictionary<Vertex, Microsoft.Msagl.Core.Layout.Node> vertexNodeDictionary = new Dictionary<Vertex, Microsoft.Msagl.Core.Layout.Node>();
			GeometryGraph layoutGraph = new GeometryGraph();

			Microsoft.Msagl.Core.Geometry.Rectangle beforeBounds = layoutGraph.BoundingBox;

			foreach (var lineSegment in graph.LineSegments)
			{
				Microsoft.Msagl.Core.Layout.Node source = new Microsoft.Msagl.Core.Layout.Node();
				if (vertexNodeDictionary.ContainsKey(lineSegment.Vertex1))
				{
					source = vertexNodeDictionary[lineSegment.Vertex1];
				}
				else
				{
					source = AddNodeFromVertex(layoutGraph, lineSegment.Vertex1);
					vertexNodeDictionary.Add(lineSegment.Vertex1, source);
				}

				Microsoft.Msagl.Core.Layout.Node target = new Microsoft.Msagl.Core.Layout.Node();
				if (vertexNodeDictionary.ContainsKey(lineSegment.Vertex2))
				{
					target = vertexNodeDictionary[lineSegment.Vertex2];
				}
				else
				{
					target = AddNodeFromVertex(layoutGraph, lineSegment.Vertex2);
					vertexNodeDictionary.Add(lineSegment.Vertex2, target);
				}

				Microsoft.Msagl.Core.Layout.Edge edge = new Microsoft.Msagl.Core.Layout.Edge(source, target);
				layoutGraph.Edges.Add(edge);
			}

			double aspectRatio = gameBoardSize.Width / gameBoardSize.Height;

			/*
			LayeredLayout layeredLayout = new LayeredLayout(
				 layoutGraph,
				 new SugiyamaLayoutSettings() { AspectRatio = aspectRatio }
			);
			layeredLayout.Run();
			*/

			MdsGraphLayout mdsLayout = new MdsGraphLayout(
				   new MdsLayoutSettings()
				   {
					   PackingAspectRatio = aspectRatio,
					   RemoveOverlaps = true,
					    
				   },
				   layoutGraph
			);
			mdsLayout.Run();

			var centers = layoutGraph.Nodes.Select(n => n.Center).ToList();

			double minX = centers.Select(c => c.X).Min();
			double maxX = centers.Select(c => c.X).Max();
			double minY = centers.Select(c => c.Y).Min();
			double maxY = centers.Select(c => c.Y).Max();

			double widthSubtract = (maxX - minX) / 2;
			double heightSubtract = (maxY - minY) / 2;

			foreach (var kvp in vertexNodeDictionary)
			{
				Microsoft.Msagl.Core.Layout.Node node = layoutGraph.FindNodeByUserData(kvp.Value.UserData);
				System.Windows.Point point = new System.Windows.Point((node.Center.X - minX) - widthSubtract, (node.Center.Y - minY) - heightSubtract);
				kvp.Key.SetPosition(point);
			}

			graph.CalculateAllIntersections();
		}

		private static Microsoft.Msagl.Core.Layout.Node AddNodeFromVertex(GeometryGraph layoutGraph, Vertex vertex)
		{
			Microsoft.Msagl.Drawing.Node sourceNode = new Microsoft.Msagl.Drawing.Node(vertex.Id.ToString());

			Microsoft.Msagl.Core.Geometry.Point location = new Microsoft.Msagl.Core.Geometry.Point(0, 0); // (vertex.X, vertex.Y);
			ICurve curve = CurveFactory.CreateEllipse(vertex.Size, vertex.Size, location);

			Microsoft.Msagl.Core.Layout.Node result = new Microsoft.Msagl.Core.Layout.Node(curve, sourceNode);
			layoutGraph.Nodes.Add(result);
			return result;
		}


		private static LayoutAlgorithmSettings GetSugiyamaLayoutSettings()
		{
			LayoutAlgorithmSettings result = new SugiyamaLayoutSettings();
			return result;
		}

		private static LayoutAlgorithmSettings GetMdsLayoutSettings()
		{
			LayoutAlgorithmSettings result = new MdsLayoutSettings();
			return result;
		}

		private static LayoutAlgorithmSettings GetRankingLayoutSettings()
		{
			LayoutAlgorithmSettings result = new RankingLayoutSettings();
			return result;
		}

		private static LayoutAlgorithmSettings GetFastIncrementalLayoutSettings()
		{
			LayoutAlgorithmSettings result = new FastIncrementalLayoutSettings();
			return result;
		}
	}
}

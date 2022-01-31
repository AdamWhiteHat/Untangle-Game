using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Untangle.ViewModels
{
	public class HistoricalMove
	{
		/// <summary>
		/// The unique ID of the game this move belongs to.
		/// Prevents replaying moves to the wrong game board.
		/// No sense in trying to apply moves to a different game, it wouldn't work.
		/// </summary>
		public string GameUID { get; private set; }

		/// <summary>
		/// The move number. 
		/// Tracks the order of the moves. 
		/// The first move is 1, second move is 2, and so on.
		/// Used to ensure the moves are replayed in the correct order.
		/// </summary>
		public int MoveNumber { get; private set; }

		/// <summary>
		/// The unique ID of the vertex that was moved during this move.
		/// </summary>
		public int VertexId { get; private set; }

		/// <summary>
		/// The moved vertex's starting position.
		/// </summary>
		public Point FromPosition { get; private set; }

		/// <summary>
		/// The moved vertex's final position.
		/// </summary>
		public Point ToPosition { get; private set; }

		public string VertexName { get { return $"Vertex_{VertexId}"; } }

		public HistoricalMove(string gameUID, int moveNumber, int vertexId, Point from, Point to)
		{
			GameUID = gameUID;
			MoveNumber = moveNumber;
			VertexId = vertexId;
			FromPosition = from;
			ToPosition = to;
		}

		public override string ToString()
		{
			return $"#{MoveNumber}: Move VertexID {VertexId} from ({FromPosition.X}, {FromPosition.Y}) to ({ToPosition.X}, {ToPosition.Y}).";
		}
	}
}

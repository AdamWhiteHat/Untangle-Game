using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Untangle.Core
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
		/// The vertex that was moved during this move.
		/// </summary>
		public Vertex Vertex { get; private set; }

		/// <summary>
		/// The moved vertex's starting position.
		/// </summary>
		public Point FromPosition { get; private set; }

		/// <summary>
		/// The moved vertex's final position.
		/// </summary>
		public Point ToPosition { get; private set; }

		/// <summary>
		/// The previous move in the undo/redo chain. This property aids deletion of items from the middle of the sequence.
		/// </summary>
		public HistoricalMove PreviousMove { get; set; } = null;

		/// <summary>
		///  The next move in the undo/redo chain. This property aids deletion of items from the middle of the sequence.
		/// </summary>
		public HistoricalMove NextMove { get; set; } = null;

		public HistoricalMove(string gameUID, Vertex vertex, Point from, Point to)
		{
			GameUID = gameUID;
			Vertex = vertex;
			FromPosition = from;
			ToPosition = to;
		}

		public void DeleteSelf()
		{
			//                              My.To -> NextMove.From
			//                   My.From -> My.To
			//PreviousMove.To -> My.From

			if (PreviousMove != null)
			{
				PreviousMove.ToPosition = this.ToPosition;
				if (NextMove != null)
				{
					PreviousMove.NextMove = NextMove;

				}
			}
			if (NextMove != null)
			{
				NextMove.FromPosition = this.FromPosition;
				if (PreviousMove != null)
				{
					NextMove.PreviousMove = PreviousMove;
				}
			}


			Vertex.HistoryStack.Remove(this);
		}

		public override string ToString()
		{
			return $"V{Vertex.Id}: ({Math.Round(FromPosition.X, 2)}, {Math.Round(FromPosition.Y, 2)}) -> ({Math.Round(ToPosition.X, 2)}, {Math.Round(ToPosition.Y, 2)})";
		}
	}
}

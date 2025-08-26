using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Untangle.Core.Achievements
{
	public class Score : IEquatable<Score>
	{
		/// <summary>
		/// The count of moves it took to solve a game.
		/// </summary>
		[XmlElement]
		public int MoveCount { get; set; }

		/// <summary>
		/// The time it took, in seconds, to make the associated game's graph planar.
		/// </summary>
		[XmlElement]
		public int SolveTime { get; set; }

		/// <summary>
		/// A flag that indicates if any of the serializable values have changed since the last save or load from disk
		/// </summary>
		[XmlIgnore]
		public bool IsDirty { get; set; }

		public Score()
		{
		}

		public bool Equals(Score other)
		{
			if(this.MoveCount != other.MoveCount) { return false; }
			if(this.SolveTime != other.SolveTime) { return false; }
			return true;
		}
	}
}

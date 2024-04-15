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
using System.Xml.Serialization;

namespace Untangle.Saves
{
	/// <summary>
	/// Stores information about an saved game of Untangle.
	/// </summary>
	[XmlRoot]
	public class SavedGame
	{
		/// <summary>
		/// The saved game's version number.
		/// </summary>
		/// <remarks>
		/// <para>A saved game's version number can be used to ensure compatibility with saved
		/// games created by earlier versions of the game.</para>
		/// </remarks>
		[XmlAttribute]
		public int Version { get; set; }

		/// <summary>
		/// The saved game's creation date and time.
		/// </summary>
		[XmlAttribute]
		public DateTime CreationDate { get; set; }

		/// <summary>
		/// The level number of the saved game's current level.
		/// </summary>
		[XmlElement]
		public int LevelNumber { get; set; }

		/// <summary>
		/// The number of vertices in the saved game's current level.
		/// </summary>
		[XmlElement]
		public int VertexCount { get; set; }

		/// <summary>
		/// The number of intersections remaining in the saved game's current level.
		/// </summary>
		[XmlElement]
		public int IntersectionCount { get; set; }

		/// <summary>
		/// The number of moves made in current saved game.
		/// </summary>
		[XmlElement]
		public int MoveCount { get; set; }

		/// <summary>
		/// The number of moves made in current saved game.
		/// </summary>
		[XmlElement]
		public string UID { get; set; }

		/// <summary>
		/// An array containing all vertices in the saved game's current level.
		/// </summary>
		[XmlElement("Vertex")]
		public Untangle.Core.Vertex[] Vertices { get; set; }
	}
}
/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

namespace Untangle.Enums
{
	/// <summary>
	/// Possible states for a vertex in a game level.
	/// </summary>
	public enum VertexState
	{
		/// <summary>
		/// The vertex is in normal state.
		/// </summary>
		Normal = 0,

		/// <summary>
		/// The vertex is highlighted, because one of the vertices directly connected to it is
		/// under the mouse cursor or is being currently dragged by the user.
		/// </summary>
		ConnectedToHighlighted = 1,

		/// <summary>
		/// The vertex is being currently dragged by the user.
		/// </summary>
		Dragged = 2,

		/// <summary>
		/// The mouse is currently over the vertex, highlighting it.
		/// </summary>
		UnderMouse = 3,
	}
}
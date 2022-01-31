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
	/// Possible states for a line segment in a game level.
	/// </summary>
	public enum LineSegmentState
	{
		/// <summary>
		/// The line segment is in normal state.
		/// </summary>
		Normal = 0,

		/// <summary>
		/// The line segment intersects at least one other line segment.
		/// </summary>
		Intersected = 1,

		/// <summary>
		/// The line segment is highlighted, because it is attached to the vertex which is under
		/// the mouse cursor or is being currently dragged by the user.
		/// </summary>
		Highlighted = 2,
	}
}
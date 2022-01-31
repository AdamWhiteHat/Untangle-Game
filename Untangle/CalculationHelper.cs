/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

using System.Windows;
using Untangle.ViewModels;

namespace Untangle
{
	/// <summary>
	/// A helper class that provides methods for identifying intersections between line segments.
	/// </summary>
	public static class CalculationHelper
	{
		/// <summary>
		/// Checks if two line segments intersect.
		/// </summary>
		/// <param name="segment1">The first segment.</param>
		/// <param name="segment2">The second segment.</param>
		/// <returns></returns>
		public static bool CheckLinesIntersect(LineSegment segment1, LineSegment segment2)
		{
			return CheckLinesIntersect(segment1.Point1, segment1.Point2, segment2.Point1, segment2.Point2);
		}

		/// <summary>
		/// Checks if two line segments intersect.
		/// </summary>
		/// <param name="endpointA1">The first line segment's first endpoint.</param>
		/// <param name="endpointA2">The first line segment's second endpoint.</param>
		/// <param name="endpointB1">The second line segment's first endpoint.</param>
		/// <param name="endpointB2">The second line segment's second endpoint.</param>
		/// <returns><see langword="true"/> if the two line segments intersect.</returns>
		/// <remarks>
		/// <para>Two line segments are not considered to intersect if they share the same
		/// endpoint.</para>
		/// </remarks>
		public static bool CheckLinesIntersect(Point endpointA1, Point endpointA2, Point endpointB1, Point endpointB2)
		{
			double orientedArea1 = CalculateTriangleOrientedArea(endpointA1, endpointA2, endpointB1);
			double orientedArea2 = CalculateTriangleOrientedArea(endpointA1, endpointA2, endpointB2);

			if (orientedArea1 * orientedArea2 >= 0.0)
			{
				return false;
			}

			double orientedArea3 = CalculateTriangleOrientedArea(endpointB1, endpointB2, endpointA1);
			double orientedArea4 = CalculateTriangleOrientedArea(endpointB1, endpointB2, endpointA2);

			return (orientedArea3 * orientedArea4 < 0.0);
		}

		public static Point GetIntersectionPoint(Point endpointA1, Point endpointA2, Point endpointB1, Point endpointB2)
		{
			// Line AB represented as a1x + b1y = c1 
			double aY = endpointA2.Y - endpointA1.Y;
			double aX = endpointA1.X - endpointA2.X;
			double cA = aY * (endpointA1.X) + aX * (endpointA1.Y);

			// Line CD represented as a2x + b2y = c2 
			double bY = endpointB2.Y - endpointB1.Y;
			double bX = endpointB1.X - endpointB2.X;
			double cB = bY * (endpointB1.X) + bX * (endpointB1.Y);

			double determinant = aY * bX - bY * aX;

			if (determinant == 0)
			{
				return new Point(double.NaN, double.NaN); // The lines are parallel. This is simplified by returning (NaN, NaN) 
			}
			else
			{
				double x = (bX * cA - aX * cB) / determinant;
				double y = (aY * cB - bY * cA) / determinant;
				return new Point(x, y);
			}
		}

		/// <summary>
		/// Calculates the oriented area of a triangle defined by its three vertices.
		/// </summary>
		/// <param name="vertex1">The triangle's first vertex.</param>
		/// <param name="vertex2">The triangle's second vertex.</param>
		/// <param name="vertex3">The triangle's third vertex.</param>
		/// <returns>The triangle's oriented area.</returns>
		private static double CalculateTriangleOrientedArea(Point vertex1, Point vertex2, Point vertex3)
		{
			double x1 = vertex2.X - vertex1.X;
			double y1 = vertex2.Y - vertex1.Y;
			double x2 = vertex3.X - vertex1.X;
			double y2 = vertex3.Y - vertex1.Y;

			return (x1 * y2 - x2 * y1) / 2.0;
		}
	}
}
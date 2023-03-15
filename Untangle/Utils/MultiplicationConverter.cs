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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Untangle
{
	/// <summary>
	/// A value converter class which converts its input to <see cref="double"/> and then
	/// multiplies it by a fixed multiplier provided in the formatter's parameter.
	/// </summary>
	[ValueConversion(typeof(double), typeof(double))]
	public class MultiplicationConverter : MarkupExtension, IValueConverter
	{
		#region IValueConverter Members

		/// <summary>
		/// Converts an object to <see cref="double"/> and the multiplies it by a specific
		/// multiplier.
		/// </summary>
		/// <param name="value">The object that should be converted to <see cref="double"/> and
		/// multiplied.</param>
		/// <param name="targetType">The target type of the conversion; ignored.</param>
		/// <param name="parameter">The fixed multiplier that should be used in the provided
		/// value's multiplication.</param>
		/// <param name="culture">The culture info that should be used for the conversion; ignored.
		/// </param>
		/// <returns>The result of the multiplication of the converted to <see cref="double"/>
		/// <paramref name="value"/> and <paramref name="parameter"/>.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (System.Convert.ToDouble(value, CultureInfo.InvariantCulture)
					* System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Non-implemented backwards conversion method.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// Returns the type instance itself in order to permit
		/// the class from being used as a markup extension in XAML.
		/// </summary>
		/// <param name="serviceProvider">A service provider helper that can provide services for
		/// the markup extension; ignored.</param>
		/// <returns>The instance.</returns>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
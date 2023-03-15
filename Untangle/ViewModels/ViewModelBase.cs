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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
//using System.Runtime.CompilerServices;

namespace Untangle.ViewModels
{
	/// <summary>
	/// A base class for all view model classes, implementing <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	public abstract class ViewModelBase : Animatable, INotifyPropertyChanged
	{
		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		internal static Duration Animation_Duration = new Duration(TimeSpan.FromMilliseconds(400));
		internal static EasingFunctionBase Animation_EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };
		//new SineEase() { EasingMode = EasingMode.EaseOut };

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event with the specified
		/// <see cref="PropertyChangedEventArgs.PropertyName"/>.
		/// </summary>
		/// <param name="propertyName">The name of the property whose value has changed.</param>
		protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
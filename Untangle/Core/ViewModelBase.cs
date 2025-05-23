﻿/*
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

namespace Untangle.Core
{
	/// <summary>
	/// A base class for all view model classes, implementing <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	public abstract class ViewModelBase : Animatable, INotifyPropertyChanged
	{
		/// <summary>
		/// Property Changed event. Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event with the specified
		/// <see cref="PropertyChangedEventArgs.PropertyName"/>.
		/// </summary>
		/// <param name="propertyName">The name of the property whose value has changed.</param>
		protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		public void RaisePropertyChanged(PropertyChangedEventArgs evenArgs)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, evenArgs);
			}
		}

		internal static Duration Animation_Duration = new Duration(TimeSpan.FromMilliseconds(400));
		internal static EasingFunctionBase Animation_EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };
	}
}
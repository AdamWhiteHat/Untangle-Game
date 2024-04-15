/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

using System.Globalization;
using System.Windows;
using Untangle.Core;

namespace Untangle.Resources
{
	/// <summary>
	/// A view model class for a language item in the language menu.
	/// </summary>
	public class LanguageItem : ViewModelBase
	{
		/// <summary>
		/// Property name constant for the property specifying whether this is the currently
		/// selected language of the application.
		/// </summary>
		public const string IsSelectedPropertyName = "IsSelected";

		/// <summary>
		/// The culture info of the culture associated with this language.
		/// </summary>
		private readonly CultureInfo _culture;

		/// <summary>
		/// Specifies whether this is the currently selected language of the application.
		/// </summary>
		private bool _isSelected;

		/// <summary>
		/// The culture info of the culture associated with this language.
		/// </summary>
		public CultureInfo Culture
		{
			get { return _culture; }
		}

		/// <summary>
		/// The culture name in the format "languagecode2-country/regioncode2" of the culture
		/// associated with this language.
		/// </summary>
		public string CultureName
		{
			get { return _culture.Name; }
		}

		/// <summary>
		/// The friendly name of the culture associated with this language.
		/// </summary>
		public string FriendlyName
		{
			get { return _culture.NativeName; }
		}

		/// <summary>
		/// Specifies whether this is the currently selected language of the application.
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (_isSelected == value)
					return;

				_isSelected = value;
				RaisePropertyChanged(IsSelectedPropertyName);
			}
		}

		/// <summary>
		/// Initializes a new <see cref="LanguageItem"/> instance for the language corresponding to
		/// the specified culture.
		/// </summary>
		/// <param name="culture">The culture info of the culture associated with the language.
		/// </param>
		public LanguageItem(CultureInfo culture)
		{
			_culture = culture;
		}


		protected override Freezable CreateInstanceCore()
		{
			return new LanguageItem(CultureInfo.InvariantCulture);
		}
	}
}
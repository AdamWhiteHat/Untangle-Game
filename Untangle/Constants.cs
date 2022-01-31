/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

namespace Untangle
{
	/// <summary>
	/// Global application constants.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// The current version of Untangle.
		/// </summary>
		public const string Version = "1.1.0";

		/// <summary>
		/// The path of the registry key in HKEY_CURRENT_USER which contains Untangle settings.
		/// </summary>
		public const string RegistryKeyName = @"Software\Untangle";

		/// <summary>
		/// The name of the registry value which contains the current language culture name
		/// setting.
		/// </summary>
		public const string LanguageValueName = "Language";

		/// <summary>
		/// The culture name of the default language.
		/// </summary>
		public const string DefaultCultureName = "en-US";
	}
}
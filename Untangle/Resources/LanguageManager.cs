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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Untangle.ViewModels;
using WPFLocalizeExtension.Engine;

namespace Untangle.Resources
{
	/// <summary>
	/// A view model class for the language management logic of the application.
	/// </summary>
	public class LanguageManager : ViewModelBase
	{
		/// <summary>
		/// Property name constant for the currently selected language of the application.
		/// </summary>
		public const string SelectedLanguagePropertyName = "SelectedLanguage";

		/// <summary>
		/// A collection of all languages supported by the application.
		/// </summary>
		private readonly ReadOnlyCollection<LanguageItem> _languages;

		/// <summary>
		/// The currently selected language of the application.
		/// </summary>
		private LanguageItem _selectedLanguage;

		/// <summary>
		/// An enumeration of all languages supported by the application.
		/// </summary>
		public IEnumerable<LanguageItem> Languages
		{
			get { return _languages; }
		}

		/// <summary>
		/// The currently selected language of the application.
		/// </summary>
		public LanguageItem SelectedLanguage
		{
			get { return _selectedLanguage; }
			private set
			{
				if (_selectedLanguage == value)
					return;
				if (value == null)
					throw new ArgumentNullException("value");

				if (_selectedLanguage != null)
					_selectedLanguage.IsSelected = false;
				_selectedLanguage = value;
				_selectedLanguage.IsSelected = true;

				// Dynamically update the localized strings in the UI
				LocalizeDictionary.Instance.Culture = _selectedLanguage.Culture;
				// Store the language settings
				SaveLanguageSettings();

				OnPropertyChanged(SelectedLanguagePropertyName);
			}
		}

		/// <summary>
		/// Initializes a new <see cref="LanguageManager"/> instance and loads the currently
		/// supported languages as well as the previously selected language.
		/// </summary>
		public LanguageManager()
		{
			LocalizeDictionary.Instance.SetCurrentThreadCulture = true;

			_languages = LoadAvailableLanguages();

			LoadLanguageSettings();
		}

		/// <summary>
		/// Changes the currently selected language to the language corresponding to the culture
		/// with the specified culture name.
		/// </summary>
		/// <param name="cultureName">The culture name of the culture associated with the newly
		/// selected language.</param>
		public void SelectLanguage(string cultureName)
		{
			LanguageItem language = FindLanguage(cultureName);
			if (language != null)
				SelectedLanguage = language;
		}

		/// <summary>
		/// Looks up all available localization assemblies and returns a collection of all
		/// languages supported by the application.
		/// </summary>
		/// <returns>A read-only collection containing all supported languages.</returns>
		/// <remarks>
		/// <para>The default language, specified by <see cref="Constants.DefaultCultureName"/>
		/// is always supported, as its resources are embedded in the main assembly of the
		/// application.</para>
		/// </remarks>
		private ReadOnlyCollection<LanguageItem> LoadAvailableLanguages()
		{
			var languageCultures = new List<CultureInfo>();

			// Add the default language to the list of supported languages
			languageCultures.Add(CultureInfo.GetCultureInfo(Constants.DefaultCultureName));

			try
			{
				string executable = Assembly.GetEntryAssembly().Location;
				string executableDirectory = Path.GetDirectoryName(executable);
				string resourceFileName = string.Format("{0}.resources.dll", Path.GetFileNameWithoutExtension(executable));

				// Enumerate potential resource directories
				IEnumerable<string> localizationDirectories = Directory.EnumerateDirectories(
					Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
					"??-??",
					SearchOption.TopDirectoryOnly);
				foreach (string localizationDirectory in localizationDirectories)
				{
					if (File.Exists(Path.Combine(localizationDirectory, resourceFileName)))
					{
						try
						{
							// Add language to the list of supported languages
							string cultureName = Path.GetFileName(localizationDirectory);
							languageCultures.Add(CultureInfo.GetCultureInfo(cultureName));
						}
						catch (CultureNotFoundException)
						{
							// If no culture corresponds to this resource assembly, ignore it
						}
					}
				}
			}
			catch
			{
				// If the list of potential resource directories cannot be enumerated for any
				// reason, silence the exception and continue without languages
			}

			return new ReadOnlyCollection<LanguageItem>(
				languageCultures
					.Select(ci => new LanguageItem(ci))
					.ToList());
		}

		/// <summary>
		/// Loads the previously stored language settings from the Windows registry.
		/// </summary>
		/// <remarks>
		/// <para>Currently, the only setting stored is the selected language of the application.
		/// </para>
		/// </remarks>
		private void LoadLanguageSettings()
		{
			string cultureName = null;

			// Attempt to find the application's registry key
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(Constants.RegistryKeyName);
			if (registryKey != null)
			{
				using (registryKey)
				{
					// Get the previously selected language's culture name
					cultureName = (string)registryKey.GetValue(Constants.LanguageValueName);
				}
			}

			// Attempt to set the selected language of the application to the previously selected
			// one, if it is supported
			_selectedLanguage = FindLanguage(cultureName) ?? _languages[0];
			_selectedLanguage.IsSelected = true;
			LocalizeDictionary.Instance.Culture = _selectedLanguage.Culture;
		}

		/// <summary>
		/// Attempts to find a supported language by the culture name of the culture associated
		/// with it.
		/// </summary>
		/// <param name="cultureName">The culture name of the culture associated with the language.
		/// </param>
		/// <returns>The requested language, if it is supported; <see langword="null"/>, in case
		/// the requested language is not supported by the application.</returns>
		private LanguageItem FindLanguage(string cultureName)
		{
			return _languages.FirstOrDefault(li => li.CultureName == cultureName);
		}

		/// <summary>
		/// Stores the language settings of the application in the Windows registry.
		/// </summary>
		/// <remarks>
		/// <para>Currently, the only setting stored is the selected language of the application.
		/// </para>
		/// </remarks>
		private void SaveLanguageSettings()
		{
			// Attempt to create the application's registry key, or open it for writing in case it
			// already exists
			RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(Constants.RegistryKeyName);
			if (registryKey == null)
				return;

			using (registryKey)
			{
				// Store the selected language's culture name
				registryKey.SetValue(
					Constants.LanguageValueName,
					SelectedLanguage.Culture.Name);
			}
		}
	}
}
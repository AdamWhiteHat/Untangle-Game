using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Untangle.Resources;

namespace Untangle
{

	public static class FileDialogHelper
	{
		/// <summary>
		/// The directory of the saved game file which was most recently saved or loaded.
		/// </summary>
		private static string _lastSavedGamePath;

		/// <summary>
		/// The default extension for saved game files.
		/// </summary>
		private const string SavedGameExtension = ".usg";

		/// <summary>
		/// Initializes <see cref="_lastSavedGamePath"/> with a default value of "&lt;User
		/// Documents Folder&gt;\My Games\Untangle" and creates that folder if it does not
		/// exist yet.
		/// </summary>
		static FileDialogHelper()
		{
			_lastSavedGamePath = string.Format(@"{0}\My Games\Untangle", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

			// Create default saved game directory if needed
			if (!Directory.Exists(_lastSavedGamePath))
			{
				Directory.CreateDirectory(_lastSavedGamePath);
			}
		}

		/// <summary>
		/// Prompts the user for a file which a game should be saved to.
		/// </summary>
		/// <param name="fileName">The full file name chosen by the user.</param>
		/// <returns><see langword="true"/> if the user has chosen a file which the game should be
		/// saved to.</returns>
		public static bool PromptForFileToSave(out string fileName)
		{
			var dialog = new SaveFileDialog
			{
				Title = DialogStrings.SaveGameDialogTitle,
				Filter = DialogStrings.SavedGameFilesFilter,
				DefaultExt = SavedGameExtension,
				InitialDirectory = _lastSavedGamePath,
				OverwritePrompt = true,
			};

			if (dialog.ShowDialog() == true)
			{
				fileName = dialog.FileName;
				_lastSavedGamePath = Path.GetDirectoryName(fileName);
				return true;
			}
			else
			{
				fileName = null;
				return false;
			}
		}

		/// <summary>
		/// Prompts the user for a saved game file which could be loaded.
		/// </summary>
		/// <param name="fileName">The full file name chosen by the user.</param>
		/// <returns><see langword="true"/> if the user has chosen a saved game file which should
		/// be loaded.</returns>
		public static bool PromptForFileToLoad(out string fileName)
		{
			var dialog = new OpenFileDialog
			{
				Title = DialogStrings.LoadGameDialogTitle,
				Filter = DialogStrings.SavedGameFilesFilter,
				DefaultExt = SavedGameExtension,
				InitialDirectory = _lastSavedGamePath,
				CheckFileExists = true,
			};

			if (dialog.ShowDialog() == true)
			{
				fileName = dialog.FileName;
				_lastSavedGamePath = Path.GetDirectoryName(fileName);
				return true;
			}
			else
			{
				fileName = null;
				return false;
			}
		}
	}
}

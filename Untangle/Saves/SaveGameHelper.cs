/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Untangle.Core;
using Untangle.Resources;

namespace Untangle.Saves
{
	/// <summary>
	/// A helper class that provides methods for saving and loading of games.
	/// </summary>
	public static class SaveHelper
	{
		/// <summary>
		/// The game's current version number.
		/// </summary>
		private const float CurrentVersion = 4.0f;

		/// <summary>
		/// The XML element name of the element containing the validation hash of a saved game.
		/// </summary>
		private const string HashElementName = "Hash";
		private const string HashExcludeAttributeName = "ExcludeFromHash";


		private static string[] HashBypassValues = new string[]
		{
			 "DEBUG"
		};

		/// <summary>
		/// Saves a game to a file chosen by the user.
		/// </summary>
		/// <param name="game">The game to be saved.</param>
		/// <returns><see langword="true"/> if the user has chosen to save the game and the saved
		/// game file has been created successfully.</returns>
		public static bool SaveGame(GameState game, string fileName)
		{
			Vertex[] vertices = game.Graph.Vertices;

			foreach (Vertex node in vertices)
			{
				node.StartingPosition = new System.Windows.Point(node.X, node.Y);
			}

			// Create saved game objects
			var savedGame = new SavedGame
			{
				UID = game.Graph.UID,
				Version = CurrentVersion,
				CreationDate = DateTime.Now,
				LevelNumber = game.LevelNumber,
				VertexCount = game.Graph.VertexCount,
				IntersectionCount = game.Graph.IntersectionCount,
				//Vertices = savedVertices.Values.ToArray(),
				Vertices = vertices.ToArray(),
				MoveCount = game.MoveCount
			};

			// Save the saved game object to the specified file
			SaveToFile(savedGame, fileName);

			return true;
		}

		/// <summary>
		/// Loads a previously saved game from a file chosen by the user.
		/// </summary>
		/// <param name="game">The game which has been loaded from the chosen file.</param>
		/// <returns><see langword="true"/> if the user has chosen a valid saved game file to load
		/// and the game has been loaded into <paramref name="game"/> successfully.</returns>
		/// <exception cref="Exception">
		/// The chosen saved game file is damaged or does not contain a valid Untangle saved game.
		/// 
		/// -or-
		/// The chosen saved game file was created by a newer version of the game.
		/// </exception>
		public static bool LoadGame(string fileName, out GameState game)
		{
			// Load the saved game object from the specified file
			SavedGame savedGame = LoadFromFile(fileName);

			// Verify saved game version
			if (savedGame.Version != CurrentVersion)
			{
				throw new Exception(ExceptionMessages.SavedGameVersionNotSupported);
			}

			// Verify that game level vertices are saved
			if (savedGame.Vertices == null)
			{
				throw new Exception(ExceptionMessages.DamagedSavedGame);
			}

			// Create the game and return it
			game = GameState.Create(savedGame.UID, savedGame.Vertices);
			return true;
		}

		/// <summary>
		/// Writes a <see cref="SavedGame"/> object to a specific file.
		/// </summary>
		/// <param name="savedGame">An object storing the information about the saved game.</param>
		/// <param name="fileName">The name of the file which the saved game should be written to.
		/// </param>
		private static void SaveToFile(SavedGame savedGame, string fileName)
		{
			// Get raw saved game XML document
			XDocument savedGameXml;
			using (var stream = new MemoryStream())
			{
				XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
				ns.Add("", "");

				var xmlSerializer = new XmlSerializer(typeof(SavedGame));

				xmlSerializer.Serialize(stream, savedGame, ns);
				stream.Position = 0;

				savedGameXml = XDocument.Load(stream, LoadOptions.None);
			}

			// Compute hash on the raw saved game XML document and append it to the document

			XElement hashElement = ConstructHashElement(GetSavedGameHash(savedGameXml));
			savedGameXml.Root.Add(hashElement);

			savedGameXml.Save(fileName, SaveOptions.None);
		}

		/// <summary>
		/// Reads a <see cref="SavedGame"/> object from a specific file.
		/// </summary>
		/// <param name="fileName">The name of the file which the saved game should be read from.
		/// </param>
		/// <returns>An object storing the information about the previously saved game.</returns>
		/// <exception cref="Exception">
		/// The chosen saved game file is damaged or does not contain a valid Untangle saved game.
		/// </exception>
		private static SavedGame LoadFromFile(string fileName)
		{
			// Read and decompress the saved game XML document from the specified file
			XDocument savedGameXml;
			try
			{
				savedGameXml = XDocument.Load(fileName, LoadOptions.None);
			}
			catch (Exception ex)
			{
				throw new Exception(ExceptionMessages.DamagedSavedGame, ex);
			}

			// Find the hash element in the saved game XML document and remove it from it
			XElement hashElement = savedGameXml.Root.Element(HashElementName);
			if (hashElement == null)
			{
				throw new Exception(ExceptionMessages.DamagedSavedGame);
			}
			string savedHash = hashElement.Value;

			if (!HashBypassValues.Contains(savedHash))
			{
				// Calculate the hash of the raw saved game XML document and ensure it matches the saved hash
				string loadedHash = GetSavedGameHash(savedGameXml);
				if (savedHash != loadedHash)
				{
					throw new Exception(ExceptionMessages.DamagedSavedGame);
				}
			}

			// De-serialize the saved game and return it
			SavedGame result = null;
			try
			{
				using (XmlReader reader = savedGameXml.CreateReader())
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(SavedGame));
					result = (SavedGame)xmlSerializer.Deserialize(reader);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ExceptionMessages.DamagedSavedGame, ex);
			}

			return result;
		}

		/// <summary>
		/// Computes the validation hash of a saved game.
		/// </summary>
		/// <param name="savedGameXml">The raw saved game XML document.</param>
		/// <returns>The base-64 encoded SHA1 hash of the raw saved game XML document.</returns>
		private static string GetSavedGameHash(XDocument savedGameXml)
		{
			// First, create a deep clone of the document so we can modify it without modifying the original)
			XDocument clone = XDocument.Load(savedGameXml.CreateReader());

			// Select all elements that have the 'exclude from hashing' attribute.
			// Examples of elements that should be excluded from hashing include the element that holds the hashing result.
			IEnumerable<object> matches = ((IEnumerable<object>)clone.XPathEvaluate($"//*[@{HashExcludeAttributeName}='true']"));

			if (matches.Any())
			{
				IEnumerable<XElement> matchingElements = matches.Cast<XElement>();
				// And remove them from the document (so they dont participate in the hash)
				foreach (XElement element in matchingElements)
				{
					element.Remove();
				}
			}

			// Old way of removing hash element
			//XElement hashElement = clone.Root.Element(HashElementName);
			//if (hashElement != null)
			//{
			//	hashElement.Remove();
			//}

			// Now, hash the remainder of the document.
			return GetStringHash(clone.ToString(SaveOptions.DisableFormatting));
		}

		private static XElement ConstructHashElement(string hashValue)
		{
			XElement result = new XElement(HashElementName, hashValue);
			// Add an attribute that excludes the element from participating in the hash.
			// For SHA256, it is an open problem as to how to construct a hash which includes a copy of itself in the hashing data.
			// And finding one via brute force is computationally infeasible.
			// Look, we are just trying to write a game here, not solve unanswered questions in mathematics and cryptography.
			result.SetAttributeValue(HashExcludeAttributeName, "true");
			return result;
		}

		private static string GetStringHash(string text)
		{
			string result = string.Empty;
			using (SHA256 sha = SHA256.Create())
			{
				byte[] savedGameBytes = Encoding.UTF8.GetBytes(text);
				byte[] hash = sha.ComputeHash(savedGameBytes);
				result = Convert.ToBase64String(hash, Base64FormattingOptions.None);
			}
			return result;
		}

		/// <summary>
		/// If its possible to automatically upgrade a save file from one version to the next higher version
		/// using a series of string search and replacements, moving data from one data structure to another, or by whatever means,
		/// go ahead and add a clause to handle that here, ensuring to set the version converted to and returning true at the end of your clause.
		/// Note: Its required only to support upgrading from one version to the next higher version.
		/// Support for upgrading from one version to another (higher) arbitrary version can be done automatically by
		/// calling this method repeatedly until the desired version is achieved or the method returns false.
		/// Only return true if an upgrade was performed successfully and to completion.
		/// Return false if no upgrade path is possible or save file is already at the latest version.
		/// Throw an exception if an upgrade was attempted or began, but an error was encountered 
		/// </summary>
		/// <param name="fileText">The save file contents as a string. Upgrading will modify this value.</param>
		/// <param name="fromVersion">The version of the file who's contents are being passed in.
		/// The upgrade process that is selected is based on this value.</param>
		/// <param name="toVersion"></param>		
		/// <returns>True if an upgrade was possible and the upgrade was performed.</returns>
		private static bool AutomaticSaveFileUpgrade(ref string fileText, float fromVersion, out float toVersion)
		{
			if (string.IsNullOrWhiteSpace(fileText))
			{
				toVersion = NotSupportedVersion;
				return false;
			}
			else if (fromVersion < 3f)
			{
				toVersion = NotSupportedVersion;
				return false;
			}
			else if (fromVersion == 3f)
			{
				fileText = fileText.Replace("<Hash>", "<Hash ExcludeFromHash=\"true\">");
				fileText = fileText.Replace("<SavedGame Version=\"3\" ", "<SavedGame Version=\"4.0\" ");
				toVersion = 4.0f;
				return true;
			}
			else
			{
				toVersion = NotSupportedVersion;
				return false;
			}
		}
		private const float NotSupportedVersion = float.NaN;
	}
}
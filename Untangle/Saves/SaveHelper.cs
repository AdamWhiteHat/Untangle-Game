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
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO.Pipes;
using System.Xml.Linq;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography;
using Untangle.Resources;
using Untangle.Core;

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
		private const int CurrentVersion = 3;

		/// <summary>
		/// The XML element name of the element containing the validation hash of a saved game.
		/// </summary>
		private const string HashElementName = "Hash";


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
		public static bool SaveGame(Game game, string fileName)
		{
			Vertex[] vertices = game.Level.GameGraph.Vertices;

			foreach (Vertex node in vertices)
			{
				node.StartingPosition = new System.Windows.Point(node.X, node.Y);
			}

			// Create saved game objects
			var savedGame = new SavedGame
			{
				UID = game.Level.GameGraph.UID,
				Version = CurrentVersion,
				CreationDate = DateTime.Now,
				LevelNumber = game.LevelNumber,
				VertexCount = game.Level.GameGraph.VertexCount,
				IntersectionCount = game.Level.GameGraph.IntersectionCount,
				//Vertices = savedVertices.Values.ToArray(),
				Vertices = vertices.ToArray(),
				MoveCount = game.Level.MoveCount
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
		public static bool LoadGame(string fileName, out Game game)
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
			GameLevel gameLevel = GameLevel.Create(savedGame.UID, savedGame.Vertices);
			game = new Game(gameLevel, savedGame.LevelNumber);
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
			string base64Hash = GetSavedGameHash(savedGameXml);
			var hashElement = new XElement(HashElementName, base64Hash);
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
			hashElement.Remove();

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
				using (var stream = new MemoryStream())
				{
					savedGameXml.Save(stream, SaveOptions.DisableFormatting);
					stream.Position = 0;
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(SavedGame));
					result = (SavedGame)xmlSerializer.Deserialize(stream);
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
			return GetStringHash(savedGameXml.ToString(SaveOptions.DisableFormatting));
		}

		private static string GetStringHash(string text)
		{
			byte[] savedGameBytes = Encoding.UTF8.GetBytes(text);
			using (SHA256 sha = SHA256.Create())
			{
				byte[] hash = sha.ComputeHash(savedGameBytes);
				return Convert.ToBase64String(hash, Base64FormattingOptions.None);
			}
		}
	}
}
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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Untangle.Utils
{
	/// <summary>
	/// A WPF behavior class used to process resource strings containing hyperlink tags which must
	/// be displayed inside a <see cref="TextBlock"/>.
	/// </summary>
	public static class HyperlinkTextBehavior
	{
		/// <summary>
		/// A regular expression that captures hyperlink tags.
		/// </summary>
		private static readonly Regex HyperlinkRegex =
			new Regex(
				@"(
                    # Capture arbitrary text
                    (?<preLinkText>.*?)
                    # Capture hyperlink markup
                    \[a\s+href='(?<linkUri>[^']+)'\](?<linkText>.*?)\[/a\]
                )*
                # Capture arbitrary text
                (?<postLinkText>.*)",
				RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

		/// <summary>
		/// Retrieves the value of the <see cref="FormattedTextProperty"/> dependency property of
		/// the specified text block.
		/// </summary>
		/// <param name="obj">The text block whose property's value should be obtained.</param>
		/// <returns>The value of the property.</returns>
		public static string GetFormattedText(TextBlock obj)
		{
			return (string)obj.GetValue(FormattedTextProperty);
		}

		/// <summary>
		/// Updates the value of the <see cref="FormattedTextProperty"/> dependency property of the
		/// specified text block.
		/// </summary>
		/// <param name="obj">The text block whose property's value should be updated.</param>
		/// <param name="value">The new value of the property.</param>
		public static void SetFormattedText(TextBlock obj, string value)
		{
			obj.SetValue(FormattedTextProperty, value);
		}

		/// <summary>
		/// An attached dependency property which automatically converts a string containing
		/// hyperlink tags into a proper list of inlines in the <see cref="TextBlock"/> that it is
		/// attached to.
		/// </summary>
		public static readonly DependencyProperty FormattedTextProperty =
			DependencyProperty.RegisterAttached(
				"FormattedText",
				typeof(string),
				typeof(HyperlinkTextBehavior),
				new UIPropertyMetadata("", FormattedTextChanged));

		private static void FormattedTextChanged(
			DependencyObject sender,
			DependencyPropertyChangedEventArgs e)
		{
			var textBlock = sender as TextBlock;
			if (textBlock == null)
			{
				throw new ArgumentException(
					"The property HyperlinkTextBehavior.FormattedText can only be attached to a TextBlock object.",
					"sender");
			}

			textBlock.Inlines.Clear();

			string value = e.NewValue as string;

			// Find all hyperlink tags
			Match match = HyperlinkRegex.Match(value);

			for (int i = 0; i < match.Groups[1].Captures.Count; i++)
			{
				// For each hyperlink tag, first put a Run inline containing the text immediately
				// prior to the hyperlink...
				string preLinkText = match.Groups["preLinkText"].Captures[i].Value;
				if (!(string.IsNullOrEmpty(preLinkText)))
					textBlock.Inlines.Add(new Run(preLinkText));

				// And then add a Hyperlink inlune containing the hyperlink itself
				string linkUri = match.Groups["linkUri"].Captures[i].Value;
				string linkText = match.Groups["linkText"].Captures[i].Value;
				var hyperlink = new Hyperlink(new Run(linkText))
				{
					NavigateUri = new Uri(linkUri),
				};
				hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
				textBlock.Inlines.Add(hyperlink);
			}

			// Finally, add a Run inline for the text after the last hyperlink
			string postLinkText = match.Groups["postLinkText"].Value;
			if (!(string.IsNullOrEmpty(postLinkText)))
				textBlock.Inlines.Add(new Run(postLinkText));
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Documents.Hyperlink.RequestNavigate"/> event of a
		/// hyperlink.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The event's arguments.</param>
		private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.ToString());
		}
	}
}
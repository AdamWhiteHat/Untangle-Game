/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Project:	Untangle
 * 
 * Author:	Aleksandar Dalemski, a_dalemski@yahoo.com
 */

using System.Windows;

namespace Untangle
{
	/// <summary>
	/// Interaction logic for AboutBox.xaml: About box of the Untangle game application.
	/// </summary>
	public partial class AboutBox : Window
	{
		/// <summary>
		/// Initializes a new <see cref="AboutBox"/> instance.
		/// </summary>
		public AboutBox()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the <see cref="System.Windows.Controls.Primitives.ButtonBase.Click"/> event of
		/// the Close button of the About box.
		/// </summary>
		/// <param name="sender">The object which raised the event.</param>
		/// <param name="e">The event's arguments.</param>
		private void btn_Close_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Untangle.Generation;

namespace Untangle
{
	/// <summary>
	/// Gathers the parameters to generate a new level
	/// </summary>
	public partial class NewLevelParameters : Window
	{
		public int Columns;
		public int Rows;
		public int MinEdges;
		public int MaxEdges;

		public int GraphIndex;

		public bool GenerateIsSelected;
		public bool GraphNameIsSelected;

		public NewLevelParameters(int initialRowValue = 3, int initialColumnValue = 2, int initialMinEdgesValue = 2, int initialMaxEdgesValue = 4)
		{
			InitializeComponent();

			tbColumns.Text = initialColumnValue.ToString();
			tbRows.Text = initialRowValue.ToString();
			tbMinEdges.Text = initialMinEdgesValue.ToString();
			tbMaxEdges.Text = initialMaxEdgesValue.ToString();

			foreach (string graph in CriticalNonplanarGraphs.Graphs.Keys)
			{
				comboGraphNames.Items.Add(graph);
			}
			comboGraphNames.Text = "Select a graph...";

			expanderGenerate.IsExpanded = true;
			expanderNamedGraphs.IsExpanded = false;
		}

		private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !e.Text.All(c => char.IsDigit(c));
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			if (expanderGenerate.IsExpanded)
			{
				int.TryParse(tbColumns.Text, out Columns);
				int.TryParse(tbRows.Text, out Rows);
				int.TryParse(tbMinEdges.Text, out MinEdges);
				int.TryParse(tbMaxEdges.Text, out MaxEdges);
			}
			else
			{
				if (comboGraphNames.SelectedIndex > -1)
				{
					GraphIndex = comboGraphNames.SelectedIndex;
				}
			}

			this.DialogResult = true;
			this.Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void expanderGenerate_Expanded(object sender, RoutedEventArgs e)
		{
			expanderNamedGraphs.IsExpanded = false;
			UpdateSelected();
		}

		private void expanderNamedGraphs_Expanded(object sender, RoutedEventArgs e)
		{
			expanderGenerate.IsExpanded = false;
			UpdateSelected();
		}

		private void UpdateSelected()
		{
			GenerateIsSelected = expanderGenerate.IsExpanded;
			GraphNameIsSelected = expanderNamedGraphs.IsExpanded;
		}
	}
}

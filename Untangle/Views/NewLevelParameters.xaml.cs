using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Untangle.Generation;
using Untangle.Core;
using XAMLMarkupExtensions.Base;

namespace Untangle
{
	/// <summary>
	/// Gathers the parameters to generate a new level
	/// </summary>
	public partial class NewLevelParameters : Window, INotifyPropertyChanged, IDataErrorInfo
	{
		public int Columns
		{
			get { return _columns; }
			set
			{
				if (value != _columns)
				{
					_columns = value;
					RaisePropertyChanged();
				}
			}
		}
		private int _columns;

		public int Rows
		{
			get { return _rows; }
			set
			{
				if (value != _rows)
				{
					_rows = value;
					RaisePropertyChanged();
				}
			}
		}
		private int _rows;

		public int MinEdges
		{
			get { return _minEdges; }
			set
			{
				if (value != _minEdges)
				{
					_minEdges = value;
					RaisePropertyChanged();
				}
			}
		}
		private int _minEdges;

		public int MaxEdges
		{
			get { return _maxEdges; }
			set
			{
				if (value != _maxEdges)
				{
					_maxEdges = value;
					RaisePropertyChanged();
				}
			}
		}
		private int _maxEdges;

		public bool IsGeneratedTypeSelected
		{
			get { return _isGeneratedTypeSelected; }
			set
			{
				if (value != _isGeneratedTypeSelected)
				{
					_isGeneratedTypeSelected = value;
					RaisePropertyChanged();
				}
			}
		}
		private bool _isGeneratedTypeSelected;

		public bool IsGraphNameTypeSelected
		{
			get { return _isGraphNameTypeSelected; }
			set
			{
				if (value != _isGraphNameTypeSelected)
				{
					_isGraphNameTypeSelected = value;
					RaisePropertyChanged();
				}
			}
		}
		private bool _isGraphNameTypeSelected;

		public int GraphIndex
		{
			get { return _graphIndex; }
			set
			{
				if (value != _graphIndex)
				{
					_graphIndex = value;
					RaisePropertyChanged();
				}
			}
		}
		private int _graphIndex;


		public string Error { get; }


		/// <summary>
		/// For a simple, connected, planar graph with v vertices and e edges and f faces,
		/// the following simple conditions hold for v ≥ 3:
		/// Theorem 1. e ≤ 3v – 6;
		/// Theorem 2. If there are no cycles of length 3, then e ≤ 2v – 4.
		/// Theorem 3. f ≤ 2v – 4.
		/// Note: These conditions are necessary for planarity, but they are not sufficient.
		/// Therefore, they can only be used to prove that a graph is NOT planar,
		/// not that it IS planar.
		/// Euler's formula:
		/// If a finite, connected, planar graph is drawn in the plane
		/// without any edge intersections,
		/// and v is the number of vertices,
		/// e is the number of edges and
		/// f is the number of faces, then:
		/// v – e + f = 2
		/// </summary>

		public string this[string columnName]
		{
			get
			{
				int verticesEstimate = Rows * Columns;
				int rhs = (3 * verticesEstimate) - 6;

				switch (columnName)
				{
					case (nameof(Columns)):
						if (Columns < 2)
						{
							return $"{nameof(Columns)} cannot be less than 2.";
						}
						break;

					case (nameof(Rows)):
						if (Rows < 2)
						{
							return $"{nameof(Rows)} cannot be less than 2.";
						}
						break;

					case nameof(MinEdges):
						if (MinEdges < 2)
						{
							return $"{nameof(MinEdges)} must be at least 2 or greater.";
						}
						else if (MinEdges >= MaxEdges)
						{
							return $"{nameof(MinEdges)} must be strictly less than {nameof(MaxEdges)}. It cannot be equal to or greater.";
						}
						else if (MinEdges > rhs)
						{
							return $"According to Euler's formula, the number of edges must be less than or equal to (3*(rows*column))-6, or {rhs}. {nameof(MinEdges)} is currently greater than that";
						}

						break;

					case nameof(MaxEdges):
						if (MaxEdges < 2)
						{
							return $"{nameof(MaxEdges)} must greater than 2 or greater.";
						}
						else if (MaxEdges <= MinEdges)
						{
							return $"{nameof(MaxEdges)} must be greater than {nameof(MinEdges)}. It cannot be equal to or less than.";
						}
						else if (MaxEdges > rhs)
						{
							return $"According to Euler's formula, the number of edges must be less than or equal to (3*(rows*column))-6, or {rhs}. {nameof(MaxEdges)} is currently greater than that";
						}

						break;
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		public NewLevelParameters(int initialRowValue = 3, int initialColumnValue = 2,  int initialMaxEdgesValue = 4, int initialMinEdgesValue = 2)
		{
			InitializeComponent();

			_columns = initialColumnValue;
			_rows = initialRowValue;
			_minEdges = initialMinEdgesValue;
			_maxEdges = initialMaxEdgesValue;

			this.Loaded += NewLevelParameters_Loaded;
		}

		private void NewLevelParameters_Loaded(object sender, RoutedEventArgs e)
		{
			this.DataContext = this;

			tbRows.SetBinding(TextBox.TextProperty,
				 new Binding("Rows")
				 {
					 Source = this,
					 Mode = BindingMode.TwoWay,
					 ValidatesOnDataErrors = true,
					 NotifyOnValidationError = true
				 });

			Validation.AddErrorHandler(tbRows, OnErrors);

			tbColumns.SetBinding(TextBox.TextProperty,
				new Binding("Columns")
				{
					Source = this,
					Mode = BindingMode.TwoWay,
					ValidatesOnDataErrors = true,
					NotifyOnValidationError = true
				});

			tbMinEdges.SetBinding(TextBox.TextProperty,
				 new Binding("MinEdges")
				 {
					 Source = this,
					 Mode = BindingMode.TwoWay,
					 ValidatesOnDataErrors = true,
					 NotifyOnValidationError = true
				 });

			tbMaxEdges.SetBinding(TextBox.TextProperty,
				 new Binding("MaxEdges")
				 {
					 Source = this,
					 Mode = BindingMode.TwoWay,
					 ValidatesOnDataErrors = true,
					 NotifyOnValidationError = true
				 });

			Validation.AddErrorHandler(tbRows, OnErrors);
			Validation.AddErrorHandler(tbColumns, OnErrors);
			Validation.AddErrorHandler(tbMinEdges, OnErrors);
			Validation.AddErrorHandler(tbMaxEdges, OnErrors);

			comboGraphNames.SetBinding(ComboBox.SelectedIndexProperty,
				 new Binding("GraphIndex")
				 { Source = this, Mode = BindingMode.OneWayToSource });

			comboGraphNames.SetBinding(ComboBox.ItemsSourceProperty,
				 new Binding()
				 { Source = CriticalNonplanarGraphs.Graphs, Mode = BindingMode.OneWay });

			comboGraphNames.DisplayMemberPath = "Key";
			comboGraphNames.IsSynchronizedWithCurrentItem = true;
			comboGraphNames.StaysOpenOnEdit = true;
			comboGraphNames.SelectedIndex = 1;

			expander_loadKnown.SetBinding(Expander.IsExpandedProperty,
				new Binding("IsGraphNameTypeSelected")
				{ Source = this, Mode = BindingMode.TwoWay });

			expander_generateRandom.SetBinding(Expander.IsExpandedProperty,
				new Binding("IsGeneratedTypeSelected")
				{ Source = this, Mode = BindingMode.TwoWay });

			IsGeneratedTypeSelected = true;
			IsGraphNameTypeSelected = false;
		}


		private int _validationErrorCount = 0;
		private void OnErrors(object sender, ValidationErrorEventArgs args)
		{
			if (args.Action == ValidationErrorEventAction.Added)
			{
				_validationErrorCount++;
			}
			else if (args.Action == ValidationErrorEventAction.Removed)
			{
				_validationErrorCount--;
			}

			btnOk.IsEnabled = (_validationErrorCount == 0);
		}

		private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !e.Text.All(c => char.IsDigit(c));
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void expander_Expanded(object sender, RoutedEventArgs e)
		{
			Expander expander = sender as Expander;
			if (expander == null) { return; }
			Expander collapser = (expander == expander_generateRandom) ? expander_loadKnown : expander_generateRandom;

			if (expander.IsExpanded && collapser.IsExpanded)
			{
				collapser.IsExpanded = false;
			}
		}

		private void expander_Collapsed(object sender, RoutedEventArgs e)
		{
			Expander collapser = sender as Expander;
			if (collapser == null) { return; }
			Expander expander = (collapser == expander_generateRandom) ? expander_loadKnown : expander_generateRandom;

			if (!collapser.IsExpanded && !expander.IsExpanded)
			{
				expander.IsExpanded = true;
			}
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event with the specified
		/// <see cref="PropertyChangedEventArgs.PropertyName"/>.
		/// </summary>
		/// <param name="propertyName">The name of the property whose value has changed.</param>
		protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		// Columns
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Untangle.Core;

namespace Untangle.Utils
{
	public class DragDropInfo
	{
		public int FromIndex { get; set; }
		public ListBox ListSource { get; set; }
		public ListBoxItem DraggedItem { get; set; }

		public ObservableStack<HistoricalMove> SourceStack { get; set; }

		public DragDropInfo(ListBox source, ListBoxItem item, ObservableStack<HistoricalMove> sourceStack)
		{
			ListSource = source;
			DraggedItem = item;
			SourceStack = sourceStack;
			FromIndex = ListSource.Items.IndexOf(DraggedItem);
		}
	}
}

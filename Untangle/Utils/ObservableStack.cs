using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untangle.Utils
{
	public class ObservableStack<T> : ObservableCollection<T> where T : class
	{
		public ObservableStack()
			: base()
		{
		}

		public void Push(T item)
		{
			if (item != null)
			{
				base.InsertItem(0, item);
			}
		}

		public T Pop()
		{
			if (base.Count > 0)
			{
				T result = base[0];
				base.RemoveItem(0);
				return result;
			}
			else
			{
				return null;
			}
		}

		public T Peek()
		{
			if (base.Count > 0)
			{
				return base[0];
			}
			else
			{
				return null;
			}
		}
	}
}

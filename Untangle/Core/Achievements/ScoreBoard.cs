using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untangle.Core.Achievements
{
	public class ScoreBoard : IDictionary<string, Score>	
	{
		private Dictionary<string, Score> _dict;


		public Score this[string key] 
		{
			get
			{ return _dict[key]; }
			set			{ _dict[key] = value;			}

		}

		
		public ICollection<string> Keys { get; }
		public ICollection<Score> Values { get; }
		public int Count { get; }
		public bool IsReadOnly { get; }

		public void Add(string key, Score value)
		{
			throw new NotImplementedException();
		}

		public void Add(KeyValuePair<string, Score> item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<string, Score> item)
		{
			throw new NotImplementedException();
		}

		public bool ContainsKey(string key)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(KeyValuePair<string, Score>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, Score>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public bool Remove(string key)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<string, Score> item)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(string key, [MaybeNullWhen(false)] out Score value)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}

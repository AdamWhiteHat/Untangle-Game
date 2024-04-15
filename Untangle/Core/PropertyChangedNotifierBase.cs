using System.ComponentModel;

namespace Untangle.Core
{
	public abstract class PropertyChangedNotifierBase : INotifyPropertyChanged
	{
		/// <summary>
		/// Property Changed event. Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event with the specified
		/// <see cref="PropertyChangedEventArgs.PropertyName"/>.
		/// </summary>
		/// <param name="propertyName">The name of the property whose value has changed.</param>
		protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}

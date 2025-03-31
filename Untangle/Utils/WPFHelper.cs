using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Reflection;
using System.Windows.Data;

namespace Untangle.Utils
{
	public static class WPFHelper
	{

		public static T GetBoundPropertyValue<T>(this BindingExpression bindingExpression) where T : class
		{
			return GetBoundPropertyValue(bindingExpression) as T;
		}


		public static object GetBoundPropertyValue(this BindingExpression bindingExpression)
		{
			object sourceObject = bindingExpression.ResolvedSource;
			if (sourceObject == null)
			{
				return null;
			}

			PropertyInfo boundProperty = GetBoundProperty(bindingExpression);
			if (boundProperty == null)
			{
				return null;
			}

			object boundPropertyValue = boundProperty.GetValue(sourceObject, null);
			return boundPropertyValue;
		}
		public static PropertyInfo GetBoundProperty(this BindingExpression bindingExpression)
		{
			object sourceObject = bindingExpression.ResolvedSource;
			if (sourceObject == null)
			{
				return null;
			}
			string boundPropertyName = bindingExpression.ResolvedSourcePropertyName;

			Type sourceObjectType = sourceObject.GetType();
			PropertyInfo boundProperty = sourceObjectType.GetProperty(boundPropertyName);
			return boundProperty;
		}




		public static T GetParentOfType<T>(this DependencyObject element) where T : DependencyObject
		{
			return GetParents(element).OfType<T>().FirstOrDefault();
		}

		public static IEnumerable<T> ChildrenOfType<T>(DependencyObject element) where T : DependencyObject
		{
			return GetChildren(element).OfType<T>();
		}


		public static IEnumerable<DependencyObject> GetParents(DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			DependencyObject parent = GetParent(element);
			while (parent != null)
			{
				yield return parent;
				parent = GetParent(parent);
			}

			yield break;
		}

		public static DependencyObject GetParent(DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			DependencyObject result = null;
			try
			{
				result = VisualTreeHelper.GetParent(element);
			}
			catch (InvalidOperationException)
			{
				result = null;
			}

			if (result == null)
			{
				if (element is FrameworkContentElement)
				{
					result = ((FrameworkContentElement)element).Parent;
				}
				else if (element is FrameworkElement)
				{
					FrameworkElement frameworkElement = element as FrameworkElement;

					if (frameworkElement.Parent != null)
					{
						result = frameworkElement.Parent;
					}
					else
					{
						result = frameworkElement.TemplatedParent;
					}
				}
			}

			return result;
		}

		public static IEnumerable<DependencyObject> GetChildren(DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			int maxIndex = VisualTreeHelper.GetChildrenCount(element);
			for (int i = 0; i < maxIndex; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(element, i);
				if (child != null)
				{
					yield return child;
					foreach (DependencyObject item in GetChildren(child))
					{
						yield return item;
					}
				}
			}
		}
	}
}

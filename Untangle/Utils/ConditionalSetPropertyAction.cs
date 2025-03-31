using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Untangle.Utils
{
	/// <summary>
	/// Checks a condition property is equal to the supplied condition value.
	/// If equal, the set property is set to the set value.
	/// SetTargetObject optionally designates the object with the property to set.
	/// If SetTargetObject is not supplied then the object the trigger is attached to
	/// will be used.
	/// </summary>
	public class ConditionalSetPropertyAction : TriggerAction<FrameworkElement>
	{
		public ConditionalSetPropertyAction()
			: base()
		{
		}

		#region Conditional Properties

		/// <summary>
		/// The property the condition is predicated on.
		/// </summary>
		public string ConditionPropertyName
		{
			get { return (string)GetValue(ConditionPropertyNameProperty); }
			set { SetValue(ConditionPropertyNameProperty, value); }
		}
		public static readonly DependencyProperty ConditionPropertyNameProperty
			= DependencyProperty.Register("ConditionPropertyName", typeof(string),
			typeof(ConditionalSetPropertyAction));

		/// <summary>
		/// The value the condition property must be to satisfy the condition.
		/// </summary>
		public object ConditionPropertyValue
		{
			get { return GetValue(ConditionPropertyValueProperty); }
			set { SetValue(ConditionPropertyValueProperty, value); }
		}
		public static readonly DependencyProperty ConditionPropertyValueProperty
			= DependencyProperty.Register("ConditionPropertyValue", typeof(object),
			typeof(ConditionalSetPropertyAction));

		/// <summary>
		/// The object with the condition property.
		/// </summary>
		public object ConditionSourceObject
		{
			get { return GetValue(ConditionSourceObjectProperty); }
			set { SetValue(ConditionSourceObjectProperty, value); }
		}
		public static readonly DependencyProperty ConditionSourceObjectProperty
			= DependencyProperty.Register("ConditionSourceObject", typeof(object),
			typeof(ConditionalSetPropertyAction));

		#endregion

		#region Set Properties

		/// <summary>
		/// The property to be set, if the condition evaluates to true.
		/// </summary>
		public string SetPropertyName
		{
			get { return (string)GetValue(SetPropertyNameProperty); }
			set { SetValue(SetPropertyNameProperty, value); }
		}
		public static readonly DependencyProperty SetPropertyNameProperty
			= DependencyProperty.Register("SetPropertyName", typeof(string),
			typeof(ConditionalSetPropertyAction));

		/// <summary>
		/// The value to set the property, if the condition evaluates to true.
		/// </summary>
		public object SetTruePropertyValue
		{
			get { return GetValue(SetTruePropertyValueProperty); }
			set { SetValue(SetTruePropertyValueProperty, value); }
		}
		public static readonly DependencyProperty SetTruePropertyValueProperty
			= DependencyProperty.Register("SetTruePropertyValue", typeof(object),
			typeof(ConditionalSetPropertyAction));

		/// <summary>
		/// The value to set the property, if the condition evaluates to false.
		/// </summary>
		public object SetFalsePropertyValue
		{
			get { return GetValue(SetFalsePropertyValueProperty); }
			set { SetValue(SetFalsePropertyValueProperty, value); }
		}
		public static readonly DependencyProperty SetFalsePropertyValueProperty
			= DependencyProperty.Register("SetFalsePropertyValue", typeof(object),
			typeof(ConditionalSetPropertyAction));

		/// <summary>
		/// The object with the property to set.
		/// </summary>
		public object SetTargetObject
		{
			get { return GetValue(SetTargetObjectProperty); }
			set { SetValue(SetTargetObjectProperty, value); }
		}
		public static readonly DependencyProperty SetTargetObjectProperty
			= DependencyProperty.Register("SetTargetObject", typeof(object),
			typeof(ConditionalSetPropertyAction));

		#endregion

		/// <summary>
		/// Invoke conditional set property action
		/// </summary>
		/// <param name="parameter"></param>
		protected override void Invoke(object parameter)
		{
			object conditionTarget = ConditionSourceObject ?? AssociatedObject;
			PropertyInfo conditionPropertyInfo = conditionTarget.GetType().GetProperty(
													ConditionPropertyName,
														BindingFlags.Instance |
														BindingFlags.Public |
														BindingFlags.NonPublic |
														BindingFlags.InvokeMethod);

			object setTarget = SetTargetObject ?? AssociatedObject;
			PropertyInfo setPropertyInfo = setTarget.GetType().GetProperty(
													SetPropertyName,
														BindingFlags.Instance |
														BindingFlags.Public |
														BindingFlags.NonPublic |
														BindingFlags.InvokeMethod);

			object conditionValue = conditionPropertyInfo.GetValue(conditionTarget);
			bool conditionSatisfied = conditionValue == ConditionPropertyValue;

			if (conditionSatisfied)
			{
				setPropertyInfo.SetValue(setTarget, SetTruePropertyValue);
			}
			else
			{
				setPropertyInfo.SetValue(setTarget, SetFalsePropertyValue);
			}
		}
	}
}

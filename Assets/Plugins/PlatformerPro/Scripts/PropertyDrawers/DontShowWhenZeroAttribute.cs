using UnityEngine;

namespace PlatformerPro
{
	/// <summary>
	/// Attribute to indicate that a field should be hidden when another field has a value of zero.
	/// </summary>
	public class DontShowWhenZeroAttribute : PropertyAttribute
	{
		/// <summary>
		/// Name of the other property.
		/// </summary>
		public string otherProperty;

		/// <summary>
		/// If true this property will be shown when other property is zero.
		/// </summary>
		public bool showWhenZero;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.DontShowWhenZeroAttribute"/> class.
		/// </summary>
		/// <param name="otherProperty">Other property.</param>
		public DontShowWhenZeroAttribute(string otherProperty) 
		{
			this.otherProperty = otherProperty;
			showWhenZero = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.DontShowWhenZeroAttribute"/> class.
		/// </summary>
		/// <param name="otherProperty">Other property.</param>
		/// <param name="showWhenZero">If set to <c>true</c> show when zero.</param>
		public DontShowWhenZeroAttribute(string otherProperty, bool showWhenZero) 
		{
			this.otherProperty = otherProperty;
			this.showWhenZero = showWhenZero;
		}
	}
}

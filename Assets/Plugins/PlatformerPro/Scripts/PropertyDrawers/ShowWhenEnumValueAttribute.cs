using UnityEngine;

namespace PlatformerPro
{
	/// <summary>
	/// Attribute to indicate that a field should be shown or hidden when another enum field has a specific value.
	/// </summary>
	public class ShowWhenEnumValueAttribute : PropertyAttribute
	{
		/// <summary>
		/// Name of the other property.
		/// </summary>
		public string otherProperty;

		/// <summary>
		/// Allowable values fro the enum. Enum values must be converted ot ints.
		/// </summary>
		public int[] enumValues;
		
		/// <summary>
		/// If true this property will be shown when other the value is anything other than the enum values.
		/// </summary>
		public bool showWhenNotMatched;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.DontShowWhenAttribute"/> class.
		/// </summary>
		/// <param name="otherProperty">Other property.</param>
		/// <param name="enumValues">Required enum values</param>
		public ShowWhenEnumValueAttribute(string otherProperty, int[] enumValues) 
		{
			this.otherProperty = otherProperty;
			this.enumValues = enumValues;
			showWhenNotMatched = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.DontShowWhenAttribute"/> class.
		/// </summary>
		/// <param name="otherProperty">Other property.</param>
		/// <param name="enumValues">Required enum values</param>
		/// <param name="showWhenNotMacthed">If set to <c>true</c> show when not mathched.</param>
		public ShowWhenEnumValueAttribute(string otherProperty,  int[] enumValues, bool showWhenNotMatched) 
		{
			this.otherProperty = otherProperty;
			this.enumValues = enumValues;
			this.showWhenNotMatched = showWhenNotMatched;
		}
	}
}

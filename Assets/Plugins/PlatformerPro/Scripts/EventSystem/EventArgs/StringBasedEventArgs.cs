using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// A class for an event for which the key piece of data is a string.
	/// </summary>
	public interface IStringBasedEventArgs
	{

		/// <summary>
		/// Gets or sets the string value.
		/// </summary>
		/// <value>The previous scene.</value>
		string StringValue { get; }
	}
}
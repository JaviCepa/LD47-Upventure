using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Enemy AI event arguments.
	/// </summary>
	public class EnemyAIEventArgs : System.EventArgs, IStringBasedEventArgs
	{

		/// <summary>
		/// Gets or sets the event ID.
		/// </summary>
		/// <value>The previous scene.</value>
		public string EventID
		{
			get;
			protected set;
		}

		public string StringValue => EventID;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.EnemyAIEventArgs"/> class.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		public EnemyAIEventArgs(string eventId)
		{
			EventID = eventId;
		}

	}
	
}

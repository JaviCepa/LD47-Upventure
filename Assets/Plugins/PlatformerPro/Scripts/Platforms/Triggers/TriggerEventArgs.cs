using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Trigger event arguments.
	/// </summary>
	public class TriggerEventArgs : CharacterEventArgs
	{
		/// <summary>
		/// Gets or sets the trigger.
		/// </summary>
		/// <value>The character.</value>
		public Trigger Trigger
		{
			get;
			protected set;
		}

		/// <summary>
		/// Was the trigger not a switch (false), Switch in the off position (false), or switch in the On position (true).
		/// </summary>
		public bool SwitchState
		{
			get;
			protected set;
		}

		
		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.CharacterEventArgs"/> class.
		/// </summary>
		/// <param name="character">Character.</param>
		/// <param name="switchState">If the character is a switch is it on or off.</param>
		public TriggerEventArgs(Trigger trigger, Character character, bool switchState) : base(character)
		{
			Trigger = trigger;
			SwitchState = switchState;
		}
	}
}

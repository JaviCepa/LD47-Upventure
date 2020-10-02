using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Item event arguments.
	/// </summary>
	public class PurchaseFailEventArgs: ItemEventArgs
	{
        /// <summary>
        /// Gets the reason purchase failed.
        /// </summary>
        public PurchaseFailReason Reason
		{
			get;
			protected set;
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformerPro.PurchaseFailEventArgs"/> class assuming
        /// the amount = 1.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="character">Character.</param>
        /// <param name="reason">Why the purchase failed.</param>
        public PurchaseFailEventArgs(string type, Character character, PurchaseFailReason reason) : base (type, character)
		{
            Reason = reason;
            IntValue = 0;
            Amount = 0;
		}

	}
	
}

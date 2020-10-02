using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
    /// <summary>
    /// Condition which only passes if a referenced Door is in the given state
    /// </summary>
    public class DoorIsOpenCondition : AdditionalCondition 
    {
        /// <summary>
        /// The door.
        /// </summary>
        public Door door;

        public DoorState state;
        
        override public string Header => "Condition which only passes if a referenced Door is in the given state";
        
        /// <summary>
        /// Returns true if Door is open.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="character">Character.</param>
        /// <param name="other">Other.</param>
        override public bool CheckCondition(Character character, object other)
        {
            if (door.state == state) return true;
            return false;
        }
    }
}
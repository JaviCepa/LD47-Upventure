using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
    /// <summary>
    /// Condition which only passes if a referenced platform is Activated
    /// </summary>
    public class PlatformActivatedCondition : AdditionalCondition 
    {
        /// <summary>
        /// Platform
        /// </summary>
        public Platform platform;

        override public string Header => "Condition which only passes if a referenced Platform is Activated";
        
        /// <summary>
        /// Returns true if Platform is Activated.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="character">Character.</param>
        /// <param name="other">Other.</param>
        override public bool CheckCondition(Character character, object other)
        {
            if (platform.Activated) return true;
            return false;
        }
    }
}
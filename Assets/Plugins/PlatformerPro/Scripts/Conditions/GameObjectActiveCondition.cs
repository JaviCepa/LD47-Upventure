using UnityEngine;

namespace PlatformerPro
{
    /// <summary>
    /// Condition which only passes if a referenced GameObject is Active
    /// </summary>
    public class GameObjectActive : AdditionalCondition 
    {
        /// <summary>
        /// Target game object.
        /// </summary>
        public GameObject targetGameObject;

        /// <summary>
        /// If true this condition will be met when the GO is inactive.
        /// </summary>
        public bool trueWhenInactive;
        
        override public string Header => "Condition which only passes if a referenced GameObject is Activated/Deactivated in Hierarchy";

        /// <summary>
        /// Returns true if GameObject is Active
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="character">Character.</param>
        /// <param name="other">Other.</param>
        override public bool CheckCondition(Character character, object other)
        {
            if (targetGameObject.activeInHierarchy) return !trueWhenInactive;
            return trueWhenInactive;
        }
    }
}
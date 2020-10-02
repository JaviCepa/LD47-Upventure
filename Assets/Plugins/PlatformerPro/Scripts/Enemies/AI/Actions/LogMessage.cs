using UnityEngine;

namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// Logs a message to console.
    /// </summary>
    public class LogMessage : EnemyAction
    {

        /// <summary>
        /// Message to show.
        /// </summary>
        public string message;

        /// <summary>
        /// Do we show this always or just in the editor.
        /// </summary>
        public bool onlyInEditor = true;
        
        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        override public void DoAction(Enemy enemy)
        {
#if !UNITY_EDITOR
            if (onlyInEditor)
            {
#endif
                Debug.Log("EnemyAI: " + message);
#if !UNITY_EDITOR
            }
#endif
        }

    }
}
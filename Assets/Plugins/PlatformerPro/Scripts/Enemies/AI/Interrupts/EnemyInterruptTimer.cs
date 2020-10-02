using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.AI.Interrupts
{
    /// <summary>
    /// An interrupt that happens when a certain counter value is met.
    /// </summary>
    [NodeTint(0.75f,0.3f,0.3f)]
    public class EnemyInterruptTimer : Node, IProcessableEnemyNode
    {
        [Output] public EnemyNode exit;

        /// <summary>
        /// How long to wait.
        /// </summary>
        public float time = 1.0f;
        
        /// <summary>
        /// Do we fire this repeatedly at an inteverla of 'time'?
        /// </summary>
        public bool loop = false;
        
        /// <summary>
        /// Should we fire this timer every time, or use a percentage change.
        /// </summary>
        public bool fireAlways;
        
        /// <summary>
        /// When this timer expires how likely are we to trigger the action?
        /// </summary>
        [DontShowWhen("fireAlways")]
        public int chance = 100;

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "exit") return exit;
            return null;
        }
        
        virtual public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault();
        }
    }

}
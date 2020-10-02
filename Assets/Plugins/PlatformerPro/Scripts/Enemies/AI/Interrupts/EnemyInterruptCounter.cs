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
    public class EnemyInterruptCounter : Node, IProcessableEnemyNode
    {
        [Output] public EnemyNode exit;

        /// <summary>
        /// At what value do we trigger?
        /// </summary>
        public int triggerValue;

        /// <summary>
        /// Do we trigger only when matches or every time counter goes over.
        /// </summary>
        public bool triggerWhenOver;
        
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
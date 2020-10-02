using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.AI.Interrupts
{
    [NodeTint(0.75f,0.3f,0.3f)]
    public class EnemyInterruptAttackedNode : Node
    {
        [Output] public EnemyNode exit;

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "exit") return exit;
            return null;
        }
    }
}
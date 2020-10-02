using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.AI
{
    public class EnemyEntryNode : Node, IProcessableEnemyNode
    {
        [Output] public EnemyNode exit;

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
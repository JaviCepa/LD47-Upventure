using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace PlatformerPro.AI
{
    /// <summary>
    /// An enemy node which has different exit nodes, with one picked randomly.
    /// </summary>
    [NodeWidth(384)]
    public class EnemyNode_RandomOption : EnemyNode
    {
        [Header("Exit Conditions")] [Output(dynamicPortList = true)]
        public string[] exitConditions;

        override public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault(o => o.fieldName == $"exitConditions {selection}");
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            // if (port.fieldName == "exit") return exit;
            return null;
        }
        
        public int PickOption()
        {
            return Random.Range(0, exitConditions.Length);
        }
    }
}
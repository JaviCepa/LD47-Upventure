using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.Dialog
{
    /// <summary>
    /// A Dialog node which lets the player choose between optioons
    /// </summary>
    [NodeWidth (384)]
    public class DialogOptionNode : DialogNode
    {
        [Output(dynamicPortList = true)] public OptionWithCondition[] options;
        
        override public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault(o => o.fieldName == $"options {selection}");
        }
        
        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            // if (port.fieldName == "exit") return exit;
            return null;
        }
    }

    [System.Serializable]
    public class OptionWithCondition
    {
        public string optionText;
        public DialogCondition condition;
        [ItemType]
        public string itemId;
    }

    public enum DialogCondition
    {
        NONE,
        MUST_HAVE_ITEM,
        MUST_NOT_HAVE_ITEM
    }
}
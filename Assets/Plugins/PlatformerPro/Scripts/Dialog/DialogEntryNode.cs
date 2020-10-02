using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.Dialog
{
    public class DialogEntryNode : Node
    {
        [Output] public DialogNode exit;

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "exit") return exit;
            return null;
        }
    }
}
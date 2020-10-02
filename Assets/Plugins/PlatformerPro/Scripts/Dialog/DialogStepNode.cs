using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace PlatformerPro.Dialog
{
    /// <summary>
    /// A Dialog node which connects directly to the next step.
    /// </summary>
    public class DialogStepNode : DialogNode
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

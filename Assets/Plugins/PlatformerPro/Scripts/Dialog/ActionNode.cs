using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.Dialog
{
    /// <summary>
    /// A node which does something.
    /// </summary>
    [NodeTint(0.3f,0.5f,0.3f)]
    public abstract class ActionNode : Node
    {
        public abstract void DoAction(Character c, DialogSystem d);
    }
}
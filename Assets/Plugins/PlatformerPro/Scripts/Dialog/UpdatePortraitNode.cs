using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.Dialog
{
    /// <summary>
    /// A node which updates either character portrait or other portrait.
    /// </summary>
    public class UpdatePortraitNode : ActionNode
    {
        
        [Input (ShowBackingValue.Never)] public DialogNode entry;
        
        [Tooltip ("If true will update the other portrait sprite, otherwise it will update character portrait.")]
        public bool isOther = true;
        
        [Tooltip ("Portrait sprite to use")]
        public Sprite portraitSprite;

        override public void DoAction(Character c, DialogSystem d)
        {
            if (isOther)
            {
                d.UpdateOtherPortrait(portraitSprite);
            }
            else
            {
                d.UpdateCharacterPortrait(portraitSprite);
            }
        }
    }
}
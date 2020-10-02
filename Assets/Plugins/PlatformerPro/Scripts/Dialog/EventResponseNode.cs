using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace PlatformerPro.Dialog
{
    [NodeWidth (384)]
    [NodeTint(0.3f,0.5f,0.3f)]
    public class EventResponseNode : Node
    {
        [Input (ShowBackingValue.Never)] public DialogNode entry;

        public EventResponse response;
        
        public void DoEvent(GameObject go, Character c)
        {
            DialogResponder r = go.GetComponent<DialogResponder>();
            if (r == null) r = go.AddComponent<DialogResponder>();
            r.DoAction(response, c);
        }
    }

    public class DialogResponder : GenericResponder
    {
        public void DoAction(EventResponse r, Character c)
        {
            DoImmediateAction(r, new CharacterEventArgs(c));
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace PlatformerPro.Dialog
{
	public class DialogEventNode : Node
	{
		[Input (ShowBackingValue.Never)] public DialogNode entry;
		public string eventId;

		// Use this for initialization
		protected override void Init()
		{
			base.Init();
		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return base.GetValue(port);
		}
	}
}
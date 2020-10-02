using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace PlatformerPro.Dialog
{
	public abstract class DialogNode : Node
	{
		[Input (ShowBackingValue.Never)] public DialogNode entry;

		[FormerlySerializedAs("characterIsTalking")] [SerializeField] 
		protected bool isCharacter;
		public bool IsCharacter => isCharacter;

		[TextArea]
		public string dialogText;

		virtual public NodePort GetOutputForSelection(int selection)
		{
			return Outputs.FirstOrDefault();
		}
		
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
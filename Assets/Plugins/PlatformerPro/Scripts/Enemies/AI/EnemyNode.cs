using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace PlatformerPro.AI
{
    public abstract class EnemyNode : Node, IProcessableEnemyNode
    {
        /// <summary>
        /// Node entry point.
        /// </summary>
        [Input (ShowBackingValue.Never)] public EnemyNode entry;
        
        /// <summary>
        /// State to set the enemy in.
        /// </summary>
        [Header("Movement")]
        public EnemyState enemyState;

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
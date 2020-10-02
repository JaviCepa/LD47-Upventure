using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.AI.Interrupts
{
    [NodeWidth (384)]
    [NodeTint(0.75f,0.3f,0.3f)]
    public class EnemyInterruptDamagedNode : Node, IProcessableEnemyNode
    {
        [Output(dynamicPortList = true)] public DamageTypeCondition[] damageOptions;
        
        virtual public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault(o => o.fieldName == $"damageOptions {selection}");
        }
        
        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            // if (port.fieldName == "exit") return exit;
            return null;
        }
        
        /// <summary>
        /// Gets the DamageTypeFilter position (node position) that matches the given state and damage info.
        /// </summary>
        /// <param name="state">Current enemy state.</param>
        /// <param name="info">Damage info</param>
        /// <returns></returns>
        virtual public int MatchOption (EnemyState state, DamageInfo info)
        {
            for (int i = 0; i < damageOptions.Length; i++)
            {
                if (damageOptions[i].Matches(state, info)) return i;
            }
            return -1;
        }
    }

    [System.Serializable]
    public class DamageTypeCondition
    {
        public string name = "DamageCondition";
        
        [Header ("State Filter")]
        public bool anyState = true;
        
        [DontShowWhen("anyState")]
        public EnemyState state;
        
        [Header ("Damage Filter")]
        public DamageType damageType;
        public int minDamage;

        /// <summary>
        /// Check if this codition matches the given input.
        /// </summary>
        /// <param name="state">Current enemy state.</param>
        /// <param name="info">Damage info</param>
        /// <returns></returns>
        public bool Matches(EnemyState state, DamageInfo info)
        {
            if (!anyState && state != this.state) return false;
            if (damageType != DamageType.NONE && damageType != info.DamageType) return false;
            if (minDamage > info.Amount) return false;
            return true;
        }
    }
    
}
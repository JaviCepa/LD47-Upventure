using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace PlatformerPro.AI
{
    /// <summary>
    /// An enemy node which has different ways of exiting.
    /// </summary>
    [NodeWidth (384)]
    public class EnemyOptionNode : EnemyNode
    {
        [Header("Exit Conditions")]
        [Output(dynamicPortList = true)] public OptionWithCondition[] exitConditions;
        
        override public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault(o => o.fieldName == $"exitConditions {selection}");
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
        public EnemyStateExitType condition;
        
        [ShowWhenEnumValue ("condition", new int[] {(int)EnemyStateExitType.TIMER,(int)EnemyStateExitType.TIMER_PLUS_RANDOM})]
        public float timer;
        
        [ShowWhenEnumValue ("condition", new int[] {(int)EnemyStateExitType.TARGET_WITHIN_RANGE})]
        public float range;
        
        [ShowWhenEnumValue ("condition", new int[] {(int)EnemyStateExitType.HEALTH_PERCENTAGE,(int)EnemyStateExitType.TIMER_PLUS_RANDOM})]
        [Range(0, 100)] public int percentage;
        
        [FormerlySerializedAs("numberOfHits")] [ShowWhenEnumValue ("condition", new int[] {(int)EnemyStateExitType.NUMBER_OF_HITS, (int)EnemyStateExitType.COUNTER_REACHES})]
        public int requiredCount;

    }
    
}
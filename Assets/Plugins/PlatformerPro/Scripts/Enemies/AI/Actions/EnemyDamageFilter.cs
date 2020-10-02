using System.Linq;
using UnityEngine;
using XNode;

namespace PlatformerPro.AI.Actions
{
    [NodeTint(0.3f,0.3f,0.5f)]
    public class EnemyDamageFilter : Node
    {
        [Input (ShowBackingValue.Never)] public EnemyNode entry;

        [Header ("Damage Amount")]
        public DamageFilterType filterType;
       
        [ShowWhenEnumValue("filterType", new int[] {(int)DamageFilterType.SET_TO})]
        public int amount;
        
        [ShowWhenEnumValue("filterType", new int[] {(int)DamageFilterType.MULTIPLY_BY, (int)DamageFilterType.DIVIDE_BY})]
        public float floatAmount;

        /// <summary>
        /// Should we round down? By default we round up.
        /// </summary>
        [ShowWhenEnumValue("filterType", new int[] {(int)DamageFilterType.MULTIPLY_BY, (int)DamageFilterType.DIVIDE_BY})]
        public bool roundDown;
        
        [Header ("Damage Type")]
        public bool setDamageType;
        
        [DontShowWhen("setDamageType", true)]
        public DamageType damageType;
        
        virtual public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault();
        }

        public DamageInfo FilterDamage(DamageInfo info)
        {
            switch (filterType)
            {
                case DamageFilterType.SET_TO:
                    info.Amount = amount;
                    break;
                case DamageFilterType.MULTIPLY_BY:
                    info.Amount = (int) (((float)info.Amount * floatAmount) + (roundDown ? 0.0f : 0.5f));
                    break;
                case DamageFilterType.DIVIDE_BY:
                    info.Amount = (int) (((float)info.Amount / floatAmount) + (roundDown ? 0.0f : 0.5f));
                    break;
            }

            if (setDamageType)
            {
                info.DamageType = damageType;
            }

            return info;
        }

    }

    public enum DamageFilterType
    {
        SET_TO,
        MULTIPLY_BY,
        DIVIDE_BY
    }
}
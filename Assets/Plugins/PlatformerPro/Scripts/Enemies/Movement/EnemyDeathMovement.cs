using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Enemy death movement.
	/// </summary>
	public abstract class EnemyDeathMovement : EnemyMovement
	{

		/// <summary>
		/// What does this death movement respond to?
		/// </summary>
		/// <returns></returns>
		virtual public DamageMovementType damageMovementType {
            get  {
                return DamageMovementType.DAMAGE_AND_DEATH;
            }
        }

        /// <summary>
        /// Allow damage movement to extend invulnerable time (e.g. if enemy is frozen).
        /// </summary>
        /// <returns></returns>
        virtual public bool ExtendInvulnerableTime
        {
            get
            {
                return false;
            }
        }
    }
	
	/// <summary>
	/// What does this damage movement respond to.
	/// </summary>
	public enum DamageMovementType
	{
		DAMAGE_AND_DEATH,
		DAMAGE_ONLY,
		DEATH_ONLY
	}
}
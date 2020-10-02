using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// Enemy damage movement which freezes when hit by a certain kind of damage, and reverts to another damage movement otherwise.
	/// </summary>
	public class EnemyMovement_Damage_Freeze : EnemyDeathMovement
	{

        #region members

        /// <summary>
        /// Which animation state to play when dead?
        /// </summary>
        public DamageType freezeDamageType = DamageType.COLD;

        /// <summary>
        /// How long we freeze for.
        /// </summary>
        public float freezeTime = 1.0f;

        /// <summary>
        /// Which animation state to play when frozen?
        /// </summary>
        public AnimationState frozenState = AnimationState.HURT_OTHER_3;


        /// <summary>
        /// If damage isn't cold type damage give control to this movement.
        /// </summary>
        public EnemyDeathMovement damageMovement;

        /// <summary>
        /// Tracks the character being frozen.
        /// </summary>
        protected float frozenTimer;

        /// <summary>
        /// This movement can't be used for death.
        /// </summary>
        override public DamageMovementType damageMovementType
        {
            get
            {
                return DamageMovementType.DAMAGE_ONLY;
            }
        }

        /// <summary>
        /// Allow damage movement to extend invulnerable time (e.g. if enemy is frozen).
        /// </summary>
        /// <returns></returns>
        override public bool ExtendInvulnerableTime
        {
            get
            {
                return (frozenTimer > 0);
            }
        }

    #endregion

    #region constants

    /// <summary>
    /// Human readable name.
    /// </summary>
    private const string Name = "Freeze on damage";
		
		/// <summary>
		/// Human readable description.
		/// </summary>
		private const string Description = "Enemy damage movement which freezes when hit by a certian kind of damage, and reverts to another damage movement otherwise.";
		
		/// <summary>
		/// Static movement info used by the editor.
		/// </summary>
		new public static MovementInfo Info
		{
			get
			{
				return new MovementInfo(Name, Description);
			}
		}
		
		#endregion
		
		#region properties
		
		
		/// <summary>
		/// Gets the animation state that this movement wants to set.
		/// </summary>
		override public AnimationState AnimationState
		{
			get 
			{
                if (frozenTimer > 0) return frozenState;
                return damageMovement.AnimationState;
			}
		}
		
		#endregion

		
		#region public methods
		
		/// <summary>
		/// Initialise this movement and return a reference to the ready to use movement.
		/// </summary>
		override public EnemyMovement Init(Enemy enemy)
		{
            if (damageMovement == null) Debug.LogWarning("The Eenemy Damage Freeze movmenent requires another movement to handle non-freeze damage and death");
            damageMovement.Init(enemy);
			this.enemy = enemy;
			return this;
		}
		
		/// <summary>
		/// Moves the character.
		/// </summary>
		override public bool DoMove()
		{
            if (frozenTimer > 0) frozenTimer -= TimeManager.FrameTime;
            if (frozenTimer > 0) return true;
            return damageMovement.DoMove();
		}

		/// <summary>
		/// Do the damaged movement
		/// </summary>
		override public void DoDamage(DamageInfo info)
		{
            if (info.DamageType == freezeDamageType) frozenTimer = freezeTime;
            else damageMovement.DoDamage(info);
		}

		/// <summary>
		/// Do the death movement
		/// </summary>
		override public void DoDeath(DamageInfo info)
		{
            damageMovement.DoDeath(info);
		}

		#endregion

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerPro
{
    
    /// <summary>
    /// A damage movement which plays an animation based on the damage type, adds an optional knockback, and can sync
    /// invulnerable time with an animator.
    /// </summary>
    public class DamageMovement_AnimationSyncPerDamageType : DamageMovement
    {
        [Header("Animator")]
        public Animator damageAnimator;

        [Header("Damage Types")]
        public DamageResponse @default;
        public List<DamageTypeToDamageResponse> damageTypes;

        /// <summary>
        /// Which animation state to play when dead?
        /// </summary>
        [Header ("Death")]
        public AnimationState deathState = AnimationState.DEATH;
        
        /// <summary>
        /// Current damage response.
        /// </summary>
        protected DamageResponse currentResponse;

        /// <summary>
        /// have we started the animation.
        /// </summary>
        protected bool animationStarted;
        
        /// <summary>
        /// Have we finished the animation.
        /// </summary>
        protected bool animationFinished;

        /// <summary>
        /// Static movement info used by the editor.
        /// </summary>
        new public static MovementInfo Info
        {
            get
            {
                return new MovementInfo("Animation Sync Damage Movement", "A damage movement which plays an animation based on the damage type, adds an optional knockback, and can sync invulnerable time with an animator.", true);
            }
        }
        
        /// <summary>
        /// Init this instance.
        /// </summary>
        override public Movement Init(Character character, MovementVariable[] movementData)
        {
            this.character = character;
            return this;
        }

        /// <summary>
        /// Start the damage movement.
        /// </summary>
        /// <param name="info">Info.</param>
        override public void Damage(DamageInfo info, bool isDeath)
        {
            animationStarted = false;
            animationFinished = false;
            currentResponse = @default;
            for (int i = 0; i < damageTypes.Count; i++)
            {
                if (info.DamageType == damageTypes[i].damageType)
                {
                    currentResponse = damageTypes[i].response;
                }
            }
            this.isDeath = isDeath;
            if (currentResponse.knockBackAmount.sqrMagnitude > 0.1f)
            {
                float directionX = 0;
                if (info.DamageCauser != null) directionX = ((Component) info.DamageCauser).transform.position.x - transform.position.x;
                Vector2 knockBackForce = new Vector2(directionX > 0 ? -currentResponse.knockBackAmount.x : currentResponse.knockBackAmount.x, currentResponse.knockBackAmount.y);
                character.SetVelocityX(knockBackForce.x);
                character.SetVelocityY(knockBackForce.y);
            }
            else
            {
                character.SetVelocityX(0);
                character.SetVelocityY(0);
            }
        }

        /// <summary>
        /// Moves the character.
        /// </summary>
        override public void DoMove()
        {
            if (!isDeath && currentResponse.controlInvincibility && !animationFinished) character.CharacterHealth.SetInvulnerable();
            if (currentResponse.animationState == AnimationState.NONE)
            {
                animationStarted = true;
                animationFinished = true;
                if (!isDeath && currentResponse.controlInvincibility) character.CharacterHealth.SetVulnerable();
            }

            if (!animationStarted)
            {
                AnimatorStateInfo info = damageAnimator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(currentResponse.animationState.AsString()))
                {
                    animationStarted = true;
                }
            }
            else if (!animationFinished)
            {
                AnimatorStateInfo info = damageAnimator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(currentResponse.animationState.AsString()))
                {
                    if (info.normalizedTime >= 1.0f || (isDeath && info.normalizedTime >= currentResponse.deathAnimationStartTime))
                    {
                        animationFinished = true;
                        if (!isDeath && currentResponse.controlInvincibility) character.CharacterHealth.SetVulnerable();
                    }
                }
                else
                {
                    animationFinished = true;
                    if (!isDeath && currentResponse.controlInvincibility) character.CharacterHealth.SetVulnerable();
                }
            }
            
            // X move

            // Apply drag (we use a friction like equation which seems to look better than a drag like one)
            if (character.Velocity.x > 0) 
            {
                character.AddVelocity(-character.Velocity.x * currentResponse.drag * TimeManager.FrameTime, 0, true);
                if (character.Velocity.x < 0) character.SetVelocityX(0);
            }
            else if (character.Velocity.x < 0) 
            {
                character.AddVelocity(-character.Velocity.x * currentResponse.drag * TimeManager.FrameTime, 0, true);
                if (character.Velocity.x > 0) character.SetVelocityX(0);
            }
			
            // Translate
            character.Translate(character.Velocity.x * TimeManager.FrameTime, 0, true);

            // Y Move

            // Apply gravity
            if (!character.Grounded )
            {
                character.AddVelocity(0, TimeManager.FrameTime * character.DefaultGravity, true);
            }
            // Translate
            character.Translate(0, character.Velocity.y * TimeManager.FrameTime, true);
        }
        
        /// <summary>
        /// Force control until grounded and animation has completed.
        /// </summary>
        override public bool ForceMaintainControl()
        {
            // No need to give control back on a death animation
            if (isDeath) return true;
            if (animationFinished)
            {
                if (!isDeath && currentResponse.controlInvincibility) character.CharacterHealth.SetVulnerable();
                return false;
            }
            if (animationStarted)
            {
                AnimatorStateInfo info = damageAnimator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(currentResponse.animationState.AsString()))
                {
                    if (info.normalizedTime >= 1.0f)
                    {
                        if (!isDeath && currentResponse.controlInvincibility) character.CharacterHealth.SetVulnerable();
                        return false;
                        
                    }
                }
                else
                {
                    if (!isDeath && currentResponse.controlInvincibility) character.CharacterHealth.SetVulnerable();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the animation state that this movement wants to set.
        /// </summary>
        override public AnimationState AnimationState
        {
            get
            {
                if (animationFinished && isDeath) return deathState;
                if (currentResponse == null)
                {
                    Debug.LogWarning("Damage movement has control but no DamageResponse is assigned");
                    return AnimationState.NONE;
                }
                return currentResponse.animationState;
            }
        }
    }


    /// <summary>
    /// Maps damage type to a response.
    /// </summary>
    [System.Serializable]
    public class DamageTypeToDamageResponse
    {
        public DamageType damageType;
        public DamageResponse response;
    }
    
    /// <summary>
    /// Details of a damage response.
    /// </summary>
    [System.Serializable]
    public class DamageResponse
    {
        [Header ("Animation")]
        [Tooltip ("Animation state to set")]
        public AnimationState animationState;
        [Header ("Knockback")]
        [Tooltip ("Force to apply when knocking back.")]
        public Vector2 knockBackAmount;
        public float drag = 1;
        [Header ("Death")]
        [Tooltip ("How long (in normalised time) does the damage animation play before death animation starts?")]
        public float deathAnimationStartTime = 1.0f;
        [Header ("Invincibility")]
        [Tooltip ("Should we stay invulnerable for the entire animation?")]
        public bool controlInvincibility;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerPro
{
    public class EnemyMovement_Damaged_AnimationSyncPerDamageType : EnemyDeathMovement, ICompletableMovement
    {
        [Header("Animator")]
        public Animator damageAnimator;

        [Header("Damage Types")]
        public DamageResponse basic;
        public List<DamageTypeToDamageResponse> damageTypes;
        
        [Header("Movement")]
        public float drag = 1.1f;
        public float quiesceSpeed = 0.2f;
        
        /// <summary>
        /// Which animation state to play when dead?
        /// </summary>
        [Header ("Death")]
        public AnimationState deathState;
        
        /// <summary>
        /// On death the GameObject will be destroyed after this many seconds.
        /// </summary>
        [Tooltip ("On death the GameObject will be destroyed after this many seconds. Use 0 to skip destroying the enemy (for example if you want to set up a pool or alternative death actions.")]
        public float destroyDelay;
        
        protected DamageResponse currentResponse;

        protected bool animationStarted;

        protected bool animationFinished;
        
        protected bool isDeath;
        
        /// <summary>
        /// Initialise this movement and return a reference to the ready to use movement.
        /// </summary>
        override public EnemyMovement Init(Enemy enemy)
        {
            this.enemy = enemy;
            return this;
        }
        
        /// <summary>
        /// Start the damage movement.
        /// </summary>
        /// <param name="info">Info.</param>
        override public void DoDamage(DamageInfo info)
        {
            animationFinished = false;
            animationStarted = false;
            currentResponse = basic;
            for (int i = 0; i < damageTypes.Count; i++)
            {
                if (info.DamageType == damageTypes[i].damageType) currentResponse = damageTypes[i].response;
            }
            if (currentResponse.knockBackAmount.sqrMagnitude > 0.1f)
            {
                float multiplier = 1.0f;
                Vector2 knockBackForce = new Vector2(info.Direction.x > 0 ? -currentResponse.knockBackAmount.x : currentResponse.knockBackAmount.x, currentResponse.knockBackAmount.y);
                knockBackForce *= multiplier;
                enemy.SetVelocityX(knockBackForce.x);
                enemy.SetVelocityY(knockBackForce.y);
            }
        }
        
        /// <summary>
        /// Do the death movement
        /// </summary>
        override public void DoDeath(DamageInfo info)
        {
            DoDamage(info);
            isDeath = true;
            if (destroyDelay > 0) StartCoroutine(DestroyAfterDelay());
        }
        	
        /// <summary>
        /// Moves the character.
        /// </summary>
        override public bool DoMove()
        {
            if (currentResponse.animationState == AnimationState.NONE)
            {
                animationStarted = true;
                animationFinished = true;
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
                    if (info.normalizedTime >= 1.0f)
                    {
                        enemy.MovementComplete();
                        animationFinished = true;
                    }
                }
                else
                {
                    enemy.MovementComplete();
                    animationFinished = true;
                }
            }
            
            // X move
            if (Mathf.Abs(enemy.Velocity.x) > quiesceSpeed)
            {
                // Apply drag (we use a friction like equation which seems to look better than a drag like one)
                if (enemy.Velocity.x > 0)
                {
                    enemy.AddVelocity(-enemy.Velocity.x * currentResponse.drag * TimeManager.FrameTime, 0);
                    if (enemy.Velocity.x < 0) enemy.SetVelocityX(0);
                }
                else if (enemy.Velocity.x < 0)
                {
                    enemy.AddVelocity(-enemy.Velocity.x * currentResponse.drag * TimeManager.FrameTime, 0);
                    if (enemy.Velocity.x > 0) enemy.SetVelocityX(0);
                }

                // Translate
                enemy.Translate(enemy.Velocity.x * TimeManager.FrameTime, 0, true);
            }
            
            // Y Move

            // Apply gravity
            if (!enemy.Grounded )
            {
                enemy.AddVelocity(0, TimeManager.FrameTime * enemy.Gravity);
            }
            // Translate
            enemy.Translate(0, enemy.Velocity.y * TimeManager.FrameTime, true);

            return true;
        }

        /// <summary>
        /// Allow damage movement to extend invulnerable time (e.g. if enemy is frozen).
        /// </summary>
        /// <returns></returns>
        override public bool ExtendInvulnerableTime
        {
            get
            {
                if (animationFinished) return false;
                if (currentResponse.controlInvincibility) return true;
                return false;
            }
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
                    Debug.LogWarning("EnemyDamageMovement Damage movement has control but no DamageResponse is assigned");
                    return AnimationState.NONE;
                }
                return currentResponse.animationState;
            }
        }
        
        /// <summary>
        /// Wait a while then destroy the enemy.
        /// </summary>
        protected IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds (destroyDelay);
            Destroy (gameObject);
        }
        
    }
}

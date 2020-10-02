using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// Enemy movement which spawns a projectile and sets an animation override.
	/// </summary>
	public class EnemyMovement_SimpleShoot : EnemyMovement
	{
		
		#region members
		
		/// <summary>
		/// The prefab to use for the projectile.
		/// </summary>
		[Header ("Projectile")]
		[Tooltip ("The prefab to use for the projectile.")]
		public GameObject projectilePrefab;
		
		/// <summary>
		/// If true don't actually fire a project, just prepare it. Used so an animation event can trigger projectile firing.
		/// </summary>
		[Tooltip ("If true don't actually fire a project, just prepare it. Used so an animation event can trigger projectile firing.")]
		public bool delayFire;
		
		/// <summary>
		/// How often to shoot
		/// </summary>
		[Header ("Timing")]
		[Tooltip ("How often to shoot.")]
		public float rateOfFire = 1.0f;

		/// <summary>
		/// How often to shoot
		/// </summary>
		[Tooltip ("How long to stay in the shoot state.")]
		public float shootTime = 0.25f;
		
		/// <summary>
		/// The damage amount.
		/// </summary>
		[Header ("Damage")]
		[Tooltip ("The amount of damage done.")]
		public int damageAmount;

		/// <summary>
		/// The type of the damage.
		/// </summary>
		[Tooltip ("The type of damage.")]
		public DamageType damageType;

	
		/// <summary>
		/// Name of the animation override
		/// </summary>
		[Header("Animation")]
		[Tooltip("Name of the animation override. This is always set but can be ignored if you want to use animation state.")]
		public string overrideName = "SHOOT";
		
		/// <summary>
		/// Should we set an animation state and override (true) or just an override (false).
		/// </summary>
		[Tooltip ("Should we set an animation state and override (true) or just an override (false).")]
		public bool setAnimationState;
		
		/// <summary>
		/// Animation to use when shooting.
		/// </summary>
		[DontShowWhen("setAnimationState", showWhenTrue = true)]
		[Tooltip("Animation to use when shooting")]
		public AnimationState shootAnimationState = AnimationState.ATTACK_SHOOT;
		
		/// <summary>
		/// Animation to use when not shooting.
		/// </summary>
		[DontShowWhen("setAnimationState", showWhenTrue = true)]
		[Tooltip("Animation to use when not shooting")]
		public AnimationState idleAnimationState = AnimationState.IDLE;

		/// <summary>
		/// When this is zero ... shoot!
		/// </summary>
		protected float firingTimer;
		
		/// <summary>
		/// Cached reference to a projectile aimer, or null if there is no aimer.
		/// </summary>
		protected ProjectileAimer projectileAimer;

		/// <summary>
		/// Are we currently shooting.
		/// </summary>
		protected bool isShooting;

        /// <summary>
        /// Cached projectile ready to be fired by animation event.
        /// </summary>
        protected Projectile preparedProjectile;

        /// <summary>
        /// Cached projectile ready to be used when projectile fired by animation event.
        /// </summary>
        protected Vector2 preparedDirection;

        #endregion

        #region constants

        /// <summary>
        /// Human readable name.
        /// </summary>
        private const string Name = "Shoot/Simple";
		
		/// <summary>
		/// Human readable description.
		/// </summary>
		private const string Description = "Enemy movement which spawns a projectile and sets an animation override.";
		
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
				if (setAnimationState)
				{
					if (isShooting) return shootAnimationState;
					return idleAnimationState;
				}
				return AnimationState.NONE;
			}
		}
		
		#endregion
		
		#region Unity hooks

		/// <summary>
		/// Unity Update() hook.
		/// </summary>
		void Update()
		{
			if (shootTime > 0) firingTimer -= TimeManager.FrameTime;
		}

		#endregion

		#region public methods
		
		/// <summary>
		/// Initialise this movement and return a reference to the ready to use movement.
		/// </summary>
		override public EnemyMovement Init(Enemy enemy)
		{
			this.enemy = enemy;
			projectileAimer = GetComponent<ProjectileAimer>();
			return this;
		}
		
		/// <summary>
		/// Moves the character.
		/// </summary>
		override public bool DoMove()
		{
			if (firingTimer <= 0.0f)
			{
				DoShoot();
			}
			return true;
		}

		#endregion

		#region protected methods

		/// <summary>
		/// Do the shoot.
		/// </summary>
		virtual protected void DoShoot()
		{
			firingTimer = rateOfFire;
			StartCoroutine (ShootRoutine ());
		}

		/// <summary>
		/// Fire projectile then temporarily set an animation override.
		/// </summary>
		virtual protected IEnumerator ShootRoutine()
		{
			// Instantiate prefab

			GameObject go = (GameObject) GameObject.Instantiate(projectilePrefab);
			Projectile projectile = go.GetComponent<Projectile>();
			if (projectileAimer != null) 
			{
				go.transform.position = enemy.transform.position + (Vector3)projectileAimer.GetAimOffset(enemy);
			}
			else
			{
				go.transform.position = enemy.transform.position;
			}
			
			if (projectile != null) {
                DoPrepare(projectile);
                if (!delayFire) FirePreparedProjectile();
            }

			enemy.AddAnimationOverride (overrideName);
			isShooting = true;
			yield return new WaitForSeconds(shootTime);
			enemy.RemoveAnimationOverride(overrideName);
			isShooting = false;
		}

        /// <summary>
        /// Prepares a projectile ready for firing.
        /// </summary>
        /// <param name="projectile">Projectile.</param>
        virtual protected void DoPrepare(Projectile projectile)
        {
            // Fire projectile if the projectile is of type projectile
            Vector2 direction = new Vector2(enemy.LastFacedDirection != 0 ? enemy.LastFacedDirection : 1, 0);
            // Use aimer to get direction fo fire if the aimer is configured
            if (projectileAimer != null) direction = projectileAimer.GetAimDirection(enemy);
            preparedDirection = direction;
            preparedProjectile = projectile;
        }

        /// <summary>
        /// Fired a previously prepared projectile.
        /// </summary>
        virtual public void FirePreparedProjectile()
        {
            if (preparedProjectile != null) preparedProjectile.Fire(damageAmount, damageType, preparedDirection, enemy);
            preparedProjectile = null;
        }

        override public bool LosingControl()
        {
	        firingTimer = 0;
	        // if (preparedProjectile != null && !preparedProjectile.isActiveAndEnabled) Destroy(preparedProjectile.gameObject);
	        preparedProjectile = null;
	        StopAllCoroutines();
	        return base.LosingControl();
        }
        
        #endregion
    }
}

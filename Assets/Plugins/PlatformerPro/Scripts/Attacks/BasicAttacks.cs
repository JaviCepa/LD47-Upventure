using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// Handles the setup of basic single button attacks for air and ground. This is a "special" movement.
	/// It derives from movement but has a lot of additional functionality.
	/// </summary>
	public class BasicAttacks : Movement
	{
		/// <summary>
		/// The attack data.
		/// </summary>
		public List<BasicAttackData> attacks;

		/// <summary>
		/// Does the attack also override movement. If false then the attack system will not control
		/// movement and animations handling will depend on attackSystemWantsAnimationStateOverride.
		/// </summary>
		public bool attackSystemWantsMovementControl;

		/// <summary>
		/// Does the attack override animation. If false then the attack system will only set an animation override not an animation state.
		/// </summary>
		public bool attackSystemWantsAnimationStateOverride;

		/// <summary>
		/// Index of the current attack or -1 if no current attack.
		/// </summary>
		protected int currentAttack;

		/// <summary>
		/// The timer for the current attack.
		/// </summary>
		protected float currentAttackTimer;

		/// <summary>
		/// Cached reference to a projectile aimer, or null if there is no aimer.
		/// </summary>
		protected ProjectileAimer projectileAimer;

		/// <summary>
		/// The index of the deferred attack.
		/// </summary>
		protected int deferredAttackIndex;

		/// <summary>
		/// Cached reference to the item manager used for ammo.
		/// </summary>
		protected ItemManager itemManager;

		/// <summary>
		/// The cooldown timers.
		/// </summary>
		protected float[] cooldownTimers;

		/// <summary>
		/// The charge timers.
		/// </summary>
		protected float[] chargeTimers;

        /// <summary>
        /// Cached projectile ready to be fired by animation event.
        /// </summary>
        protected Projectile preparedProjectile;

        /// <summary>
        /// Cached direction ready to be used when projectile fired by animation event.
        /// </summary>
        protected Vector2 preparedDirection;

        /// <summary>
        /// Cached attack data index ready to be used when projectile fired by animation event.
        /// </summary>
        protected int preparedAttackData;

        /// <summary>
        /// Tracks if the state named for the current attacks animation has started playing in the animator.
        /// </summary>
        protected bool attackAnimationStarted;
        
        #region movement info constants and properties

        /// <summary>
        /// Human readable name.
        /// </summary>
        private const string Name = "Basic Attacks";
		
		/// <summary>
		/// Human readable description.
		/// </summary>
		private const string Description = "Basic Attack class.";
		
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

		/// <summary>
		/// Gets a value indicating whether this instance is attacking.
		/// </summary>
		/// <value><c>true</c> if this instance is attacking; otherwise, <c>false</c>.</value>
		virtual public bool IsAttacking
		{
			get
			{
				if (currentAttack != -1) return true;
				return false;
			}
		}

		/// <summary>
		/// Gets the name of the active attack or null if no attack is aactive.
		/// </summary>
		/// <value><c>true</c> if active attack name; otherwise, <c>false</c>.</value>
		virtual public string ActiveAttackName
		{
			get
			{
				if (currentAttack != -1) return attacks[currentAttack].name;
				return null;
			}
		}

		/// <summary>
		/// Gets the normalised time of the active attack or -1 if no attack is aactive.
		/// </summary>
		/// <value><c>true</c> if active attack name; otherwise, <c>false</c>.</value>
		virtual public float ActiveAttackNormalisedTime
		{
			get
			{
				if (currentAttack != -1)
				{
					if (attacks[currentAttack].useAnimationTime)
					{
						AnimatorStateInfo info = AttackAnimator.GetCurrentAnimatorStateInfo(AttackAnimatorLayer);
						if (info.IsName(attacks[currentAttack].animation.AsString()))
						{
							return info.normalizedTime;
						}
						// If the animation started but is no longer active we have moved on from the attack animation in the animator and thus this attack is finished.
						if (attackAnimationStarted) return 1.0f;
						return -1.0f;
					}
					return currentAttackTimer / GetAttackTime(attacks[currentAttack]);
				}
				return -1.0f;
			}
		}

		/// <summary>
		/// Animator used for calculating attack time.
		/// </summary>
		public Animator AttackAnimator { get; set; }

		/// <summary>
		/// Layer to use for attack animations.
		/// </summary>
		public int AttackAnimatorLayer { get; set; }
		
		/// <summary>
		/// Gets the active attack location or ANY if no  activeattack.
		/// </summary>
		virtual public AttackLocation ActiveAttackLocation
		{
			get
			{
				if (currentAttack != -1) return attacks[currentAttack].attackLocation;
				return AttackLocation.ANY;
			}
		}

		/// <summary>
		/// Returns true if the current attacks hit box has hit an enemy.
		/// </summary>
		virtual public bool ActiveAttackHasHit
		{
			get
			{
				if (currentAttack != -1) return attacks[currentAttack].hitBox.HasHit;
				return false;
			}
		}

		/// <summary>
		/// Used by the inspector to determine if a given attack can have multiple attacks defined in it.
		/// </summary>
		virtual public bool CanHaveMultipleAttacks
		{
			get { return true; }
		}

		/// <summary>
		/// Used by the inspector to determine if a given attack can have multiple attacks defined in it.
		/// </summary>
		virtual public bool CanUserSetAttackType
		{
			get { return true; }
		}

		/// <summary>
		/// Gets the cahrge amount.
		/// </summary>
		/// <value>The charge.</value>
		virtual public float Charge
		{
			get
			{
				if (currentAttack != -1)
				{
					if (attacks [currentAttack].chargeThresholds.Length > 0)
					{
						for (int i = attacks [currentAttack].chargeThresholds.Length - 1; i >= 0; i--)
						{
							if (chargeTimers [currentAttack] >= attacks [currentAttack].chargeThresholds [i]) return (float)(i + 1);
						}
						return 0;
					}
					return chargeTimers [currentAttack];
				}
				return 0;
			}
		}
			
		/// <summary>
		/// Unity Update hook.
		/// </summary>
		void Update()
		{
			// Update cooldown timers
			if (cooldownTimers != null)
			{
				for (int i = 0; i < cooldownTimers.Length; i++) 
				{
					if (cooldownTimers[i] > 0) cooldownTimers[i] -= TimeManager.FrameTime;
				}
			}
			// Update button held times
			if (chargeTimers != null)
			{
				for (int i = 0; i < attacks.Count; i++) 
				{
					if (attacks [i].fireInputType == FireInputType.FIRE_WHEN_RELEASED)
					{
						
						if (character.Input.GetActionButtonState (attacks [i].actionButtonIndex) == ButtonState.HELD)
						{
							chargeTimers [i] += TimeManager.FrameTime;
						}
						else if (character.Input.GetActionButtonState (attacks [i].actionButtonIndex) == ButtonState.NONE)
						{
							chargeTimers [i] = 0;
						}
					}
				}
			}
		}

		/// <summary>
		/// Initialise this movement and retyrn a reference to the ready to use movement.
		/// </summary>
		override public Movement Init(Character character)
		{
			base.Init (character);
			PostInit ();
			return this;
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		virtual protected void PostInit()
		{
			bool hasCoolDowns = false;
			for (int i = 0; i < attacks.Count; i++)
			{
				if (attacks[i].hitBox != null) attacks[i].hitBox.Init(new DamageInfo(attacks[i].damageAmount, attacks[i].damageType, Vector2.zero, character));
				if (attacks[i].attackType == AttackType.PROJECTILE && attacks[i].ammoType != null && attacks[i].ammoType != "")
				{
					itemManager = character.GetComponentInChildren<ItemManager>();
					if (itemManager == null) Debug.LogWarning("Attack requires ammo but item manager could not be found");
				}
				if (attacks[i].coolDown > 0) hasCoolDowns = true;
			}
			if (hasCoolDowns) cooldownTimers = new float[attacks.Count];
			chargeTimers = new float[attacks.Count];
			projectileAimer = GetComponent<ProjectileAimer>();
			currentAttack = -1;
			// Default an animator, can be overriden by the AttackAnimationEventResponder
			if (AttackAnimator == null) AttackAnimator = character.GetComponentInChildren<Animator>();
		}


		/// <summary>
		/// Gets the charge level for a given attack (by index). Returns -1 if not charging. Does not process through the digital charge filters if present.
		/// </summary>
		/// <returns>The charge level for attack.</returns>
		/// <param name="index">Index.</param>
		virtual public float GetRawChargeForAttack(int index)
		{
			if (index < 0 || index >= attacks.Count)
			{
				Debug.LogWarning ("Invalid attack index");
				return -1;
			}
			// Its a bit hard to put together a list of conditions that apply generically, here we've just gone 
			// with button pressed and has ammo, you could add more!
			if (character.Input.GetActionButtonState(attacks[index].actionButtonIndex) != ButtonState.HELD) return -1.0f;
			if (!CheckAmmo(attacks[index])) return -1.0f;
			return chargeTimers [index];
		}

		/// <summary>
		/// Gets the charge level for a given attack (by index). Returns -1 if not charging.
		/// </summary>
		/// <returns>The charge level for attack.</returns>
		/// <param name="index">Index.</param>
		virtual public float GetChargeForAttack(int index)
		{
			if (index < 0 || index >= attacks.Count)
			{
				Debug.LogWarning ("Invalid attack index");
				return -1;
			}
			// Its a bit hard to put together a list of conditions that apply generically, here we've just gone 
			// with button pressed and has ammo, you could add more!
			if (character.Input.GetActionButtonState(attacks[index].actionButtonIndex) != ButtonState.HELD) return -1.0f;
			if (!CheckAmmo(attacks[index])) return -1.0f;
			// if (!CheckConditions(attacks[index])) return -1.0f;
			if (attacks [index].chargeThresholds.Length > 0)
			{
				for (int i = attacks [index].chargeThresholds.Length - 1; i >= 0; i--)
				{
					if (chargeTimers [index] >= attacks [index].chargeThresholds [i]) return (float)(i + 1);
				}
				return -1.0f;
			}
			return chargeTimers [index];
		}

		/// <summary>
		/// Gets the index of the givne atack in the attack data or -1 if no match found.
		/// </summary>
		/// <returns>The index for name.</returns>
		/// <param name="name">Name.</param>
		virtual public int GetIndexForName(string name)
		{
			for (int i = 0; i < attacks.Count; i++)
			{
				if (attacks [i].name == name) return i;
			}
			return -1;
		}

		/// <summary>
		/// Gets a value indicating whether this movement wants to initiate an attack.
		/// </summary>
		/// <value><c>true</c> if this instance should attack; otherwise, <c>false</c>.</value>
		virtual public bool WantsAttack()
		{
			// Can't attack if disabled
			if (!enabled) return false;

			// Can't attack if timer isn't 0
			if (currentAttackTimer > 0.0f) return false;

			// Check each attack
			for (int i = 0; i < attacks.Count; i ++)
			{
				// Not cooled down
				if (cooldownTimers == null || cooldownTimers.Length == 0 || cooldownTimers[i] <= 0)
				{
					// Ready to go...
					if (CheckLocation(attacks[i]) && CheckInput(attacks[i]) && CheckAmmo(attacks[i]) && CheckConditions())
					{
						return true;
					}
				}
			}
			return false;
		}


		/// <summary>
		///  Gets a value indicating whether this movement wants to set the animation state.
		/// </summary>
		/// <returns><c>true</c>, if wants to set animation state, <c>false</c> otherwise.</returns>
		virtual public bool WantsToSetAnimationState()
		{
			if (currentAttack != -1 && attackSystemWantsAnimationStateOverride) return true;
			return false;
		}

		/// <summary>
		/// Do whichever attack is available.
		/// </summary>
		/// <returns>true if this movement wants to main control of movement, false otherwise</returns>
		virtual public bool Attack()
		{
			// Check each attack
			for (int i = 0; i < attacks.Count; i ++)
			{
				if (CheckLocation(attacks[i]) && CheckInput(attacks[i]) && CheckAmmo(attacks[i]))
				{
					StartAttack(i);
					return attackSystemWantsMovementControl;
				}
			}
			// We couldn't find matching attack,this shouldn't happen
			Debug.LogWarning ("Called Attack() but no suitable attack was found");
			currentAttack = -1;
			currentAttackTimer = 0.0f;
			return false;
		}

		/// <summary>
		/// Forces an attack to stop
		/// </summary>
		virtual public void InterruptAttack()
		{
			StopAllCoroutines ();
			if (currentAttack != -1)
			{
				// Reset any animation overrides before currentAttack is cleared
				if (!attackSystemWantsAnimationStateOverride && !attackSystemWantsMovementControl) character.RemoveAnimationOverride (OverrideState);

				// Attack finished
				if (attacks [currentAttack].hitBox != null)
				{
					attacks[currentAttack].hitBox.ForceStop();
				}
				currentAttackTimer = 0.0f;
				string attackName = attacks[currentAttack].name;
				if (!string.IsNullOrEmpty(attacks[currentAttack].animationSpeedParam)) AttackAnimator.SetFloat(attacks[currentAttack].animationSpeedParam, 1.0f);
				currentAttack = -1;
				character.OnChangeAnimationState ();
				character.FinishedAttack(attackName);
			}
		}

		/// <summary>
		/// Called when the character hits an enemy.
		/// </summary>
		/// <param name="enemy">Enemy that was hit.</param>
		/// <param name="info">Damage info.</param>
		virtual public void HitEnemy(IMob enemy, DamageInfo info)
		{

            if (currentAttack != -1 && !attacks[currentAttack].loseDurabilityOnUse &&
                (attacks[currentAttack].weaponSlot != null &&
                 attacks[currentAttack].weaponSlot != "" && 
                 attacks[currentAttack].weaponSlot != "NONE"))
            {
                character.ItemManager.DamageItemInEquipmentSlot(attacks[currentAttack].weaponSlot, 1);
            }
        }

		/// <summary>
		/// Gets the  cool down time for an attack.
		/// </summary>
		/// <returns>The remaining cool down.</returns>
		/// <param name="attackIndex">Attack index.</param>
		virtual public float GetAttackCoolDown(int attackIndex)
		{
			if (cooldownTimers != null && attackIndex >= 0 && attackIndex < attacks.Count) return attacks[attackIndex].coolDown;
			return 0;
		}

		/// <summary>
		/// Gets the remaining cool down time for an attack.
		/// </summary>
		/// <returns>The cool down.</returns>
		/// <param name="attackIndex">Attack index.</param>
		virtual public float GetAttackCoolDownRemaining(int attackIndex)
		{
			float remaining = 0;
			if (cooldownTimers != null && attackIndex >= 0 && attackIndex < cooldownTimers.Length) remaining =  cooldownTimers[attackIndex];
			if (remaining < 0) remaining = 0;
			return remaining;
		}

		/// <summary>
		/// Is the character in the right place for the given attack.
		/// </summary>
		/// <returns><c>true</c>, if location was checked, <c>false</c> otherwise.</returns>
		/// <param name="attackData">Attack data.</param>
		virtual protected bool CheckLocation(BasicAttackData attackData)
		{
			if (attackData.attackLocation == AttackLocation.ANY) return true;
			if (attackData.attackLocation == AttackLocation.ANY_BUT_SPECIAL &&
			    !(character.ActiveMovement is SpecialMovement) && 
			    !(character.ActiveMovement is WallMovement))
			{
				return true;
			}
			if (attackData.attackLocation == AttackLocation.GROUNDED && 
			    character.Grounded && 
			    !character.OnLadder &&
			    (character.ActiveMovement is GroundMovement || 
			 		// If the attack has control we still want to be able to trigger combos
			     	(character.ActiveMovement is BasicAttacks && (character.AttackLocation != AttackLocation.AIRBORNE))) &&
			    // Don't allow when character is about to jump
			  	character.Input.JumpButton != ButtonState.DOWN) 
			{
				return true;
			}
			if (attackData.attackLocation == AttackLocation.AIRBORNE && 
			    (character.ActiveMovement is AirMovement || 
			 		// If the attack has control we still want to be able to trigger combos
			 	 	(character.ActiveMovement is BasicAttacks && (character.AttackLocation != AttackLocation.GROUNDED))) &&
			    !character.OnLadder)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Is the input correct for the given attack. This implmenetation is simple a key press, but another could
		/// be more complex (queueable combo attacks, or complex key combinations).
		/// </summary>
		/// <returns><c>true</c>, if input was checked, <c>false</c> otherwise.</returns>
		/// <param name="attackData">Attack data.</param>
		virtual protected bool CheckInput(BasicAttackData attackData)
		{
			switch (attackData.fireInputType)
			{
			case FireInputType.FIRE_WHEN_PRESSED:
				if (character.Input.GetActionButtonState(attackData.actionButtonIndex) == ButtonState.DOWN) return true;
				break;
			case FireInputType.FIRE_WHEN_HELD:
				if (character.Input.GetActionButtonState(attackData.actionButtonIndex) == ButtonState.HELD) return true;
				break;
			case FireInputType.FIRE_WHEN_RELEASED:
				if (character.Input.GetActionButtonState(attackData.actionButtonIndex) == ButtonState.UP) return true;
				break;
			}
			return false;
		}

		/// <summary>
		/// Checks that the player has enough ammo to fire.
		/// </summary>
		/// <returns><c>true</c>, if ammo was checked, <c>false</c> otherwise.</returns>
		/// <param name="attackData">Attack data.</param>
		virtual protected bool CheckAmmo(BasicAttackData attackData)
		{
			if (attackData.attackType == AttackType.MELEE || attackData.ammoType == null || attackData.ammoType == "") return true;
			if (itemManager.ItemCount (attackData.ammoType) > 0)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Checks any additional conditions.
		/// </summary>
		/// <returns><c>true</c>, if conditions was met, <c>false</c> otherwise.</returns>
		virtual protected bool CheckConditions() 
		{
			foreach (AdditionalCondition condition in conditions) 
			{
				if (!condition.CheckCondition(character, this)) return false;
			}
			return true;
		}

		/// <summary>
		/// Consumes ammo.
		/// </summary>
		/// <returns><c>true</c>, if ammo was checked, <c>false</c> otherwise.</returns>
		/// <param name="attackData">Attack data.</param>
		virtual protected void ConsumeAmmo(BasicAttackData attackData)
		{
			if (attackData.attackType == AttackType.MELEE || attackData.ammoType == null || attackData.ammoType == "") return; 
			itemManager.UseItem (attackData.ammoType, 1);
		}

		/// <summary>
		/// Starts the given attack.
		/// </summary>
		/// <param name="attackIndex">Attack index.</param>
		virtual protected void StartAttack(int attackIndex)
		{
			StopAllCoroutines();
			currentAttack = attackIndex;
			currentAttackTimer = 0.0f;
			attackAnimationStarted = false;
			if (attacks[attackIndex].attackType == AttackType.PROJECTILE)
			{
				StartCoroutine(DoProjectileAttack(attackIndex));
			}
			else if (attacks[attackIndex].useAnimationTime )
			{
				StartCoroutine(DoAnimationSyncedAttack(attackIndex));
			}
			else
			{
				StartCoroutine(DoAttack(attackIndex));
			}
		}

		/// <summary>
		/// Do a melee attack.
		/// </summary>
		/// <param name="attackIndex">Attack index.</param>
		virtual protected IEnumerator DoAttack(int attackIndex)
		{
			float attackTime = GetAttackTime(attacks[currentAttack]);
			DoAttackStart(attackIndex);
			if (attacks[currentAttack].hitBox != null && !attacks[currentAttack].useAnimationEvents)
			{
				attacks[attackIndex].hitBox.EnableImmediate(attackTime * attacks[currentAttack].attackHitBoxStart, attackTime * attacks[currentAttack].attackHitBoxEnd);
			}
			while (currentAttackTimer < attackTime)
			{
				currentAttackTimer += TimeManager.FrameTime;
				yield return true;
			}
			DoAttackEnd(attackIndex) ;
		}

		/// <summary>
		/// Do a melee attack with length controlled by the animation.
		/// </summary>
		/// <param name="attackIndex">Attack index.</param>
		virtual protected IEnumerator DoAnimationSyncedAttack(int attackIndex)
		{
			DoAttackStart(attackIndex);
			float animationSpeed = GetAttackTime(1.0f);
			bool animationFinished = false;
			bool hitboxEnabled = false;
			bool hitboxDisabled = false;
			if (AttackAnimator == null) Debug.LogWarning("Tried to do an animation synced attack without an animtor");
			while (!animationFinished)
			{
				AnimatorStateInfo info = AttackAnimator.GetCurrentAnimatorStateInfo(AttackAnimatorLayer);
				if (info.IsName(attacks[currentAttack].animation.AsString()))
				{
					
					attackAnimationStarted = true;
					if (info.normalizedTime >= 1.0f)
					{
						animationFinished = true;
					}
					if (!hitboxEnabled && !attacks[attackIndex].useAnimationEvents && info.normalizedTime >= attacks[attackIndex].attackHitBoxStart)
					{
						hitboxEnabled = true;
						if (attacks[attackIndex].hitBox != null) attacks[attackIndex].hitBox.EnableImmediate();
					}
					if (!hitboxDisabled && !attacks[attackIndex].useAnimationEvents && info.normalizedTime >= attacks[attackIndex].attackHitBoxEnd)
					{
						hitboxDisabled = true;
						if (attacks[attackIndex].hitBox != null) attacks[attackIndex].hitBox.DisableImmediate();
					}
				}
				currentAttackTimer += TimeManager.FrameTime;
				yield return true;
			}
			DoAttackEnd(attackIndex);
		}

		/// <summary>
		/// Start an attack.
		/// </summary>
		/// <param name="attackIndex"></param>
		virtual protected void DoAttackStart(int attackIndex)
		{
			if (attacks[attackIndex].resetVelocityX) character.SetVelocityX(0);
			if (attacks[attackIndex].resetVelocityY) character.SetVelocityY(0);
			if (!string.IsNullOrEmpty(attacks[attackIndex].animationSpeedParam)) AttackAnimator.SetFloat(attacks[attackIndex].animationSpeedParam, 1.0f / GetAttackTime(1.0f));
			if (attacks [attackIndex].hitBox == null)
			{
				Debug.LogWarning ("Trying to attack but no hit hitbox set.");
			}
			else
			{
				int damageAmount = GetAttackDamage(attacks[attackIndex]);
				DamageType damageType = attacks[attackIndex].damageType;
				if (!string.IsNullOrEmpty(attacks[attackIndex].weaponSlot))
				{
					ItemInstanceData weapon = character.EquipmentManager.GetItemForSlot(attacks[attackIndex].weaponSlot);
					if (weapon != null && weapon.Data != null && weapon.Data.damageType != DamageType.NONE) damageType = weapon.Data.damageType;
				}
				attacks [attackIndex].hitBox.UpdateDamageInfo(damageAmount, damageType);
			}
		}
		
		/// <summary>
		/// Finish an attack.
		/// </summary>
		/// <param name="attackIndex"></param>
		virtual protected void DoAttackEnd(int attackIndex)
		{
			// Attack finished
			currentAttack = -1;
			if (attacks [attackIndex].hitBox != null)
			{
				attacks[attackIndex].hitBox.ForceStop();
			}
			if (!string.IsNullOrEmpty(attacks[attackIndex].animationSpeedParam)) AttackAnimator.SetFloat(attacks[attackIndex].animationSpeedParam, 1.0f);
			currentAttackTimer = 0.0f;
			character.OnChangeAnimationState ();
			character.FinishedAttack(attacks[attackIndex].name);
			if (attacks[attackIndex].loseDurabilityOnUse && (attacks[attackIndex].weaponSlot != null && attacks[attackIndex].weaponSlot != "" && attacks[attackIndex].weaponSlot != "NONE"))
			{
				character.ItemManager.DamageItemInEquipmentSlot(attacks[attackIndex].weaponSlot, 1);
			}
			// Set cooldown
			if (cooldownTimers != null) cooldownTimers [attackIndex] = attacks [attackIndex].coolDown;
			if (attacks[attackIndex].hitBox != null) attacks[attackIndex].hitBox.DisableImmediate();
		}

		/// <summary>
		/// Do a projectile attack.
		/// </summary>
		/// <param name="attackIndex">Attack index.</param>
		virtual protected IEnumerator DoProjectileAttack(int attackIndex)
		{
			if (attacks[attackIndex].useAnimationTime) Debug.LogWarning("Projectile attacks don't support animation syncing");
			bool hasFired = false;
			float attackTime = GetAttackTime(attacks[attackIndex]);
			// Update delay by speed
			float projectileDelay = attacks[attackIndex].projectileDelay * (attackTime / attacks[attackIndex].attackTime);
			while(currentAttackTimer < attackTime)
			{
				currentAttackTimer += TimeManager.FrameTime;
				if (!hasFired && currentAttackTimer >= projectileDelay) 
				{
					InstantiateProjectile(attackIndex);
					ConsumeAmmo (attacks [attackIndex]);
					hasFired = true;
				}
				yield return true;
			}
			// Reset any animation overrides before currentAttack is cleared
			if (!attackSystemWantsAnimationStateOverride && !attackSystemWantsMovementControl) character.RemoveAnimationOverride (OverrideState);

			// Attack finished
			currentAttack = -1;
			currentAttackTimer = 0.0f;
			character.OnChangeAnimationState ();
			character.FinishedAttack(attacks[attackIndex].name);

			// Set cooldown
			if (cooldownTimers != null) cooldownTimers [attackIndex] = attacks [attackIndex].coolDown;
		}

		/// <summary>
		/// Enable the current attacks hit box immediately. Usually used when using animation events to enable hitboxes.
		/// </summary>
		virtual public void EnableHitBox()
		{
			if (currentAttack == -1 || attacks[currentAttack].hitBox == null ) return;
			attacks[currentAttack].hitBox.EnableImmediate();
		}
		
		/// <summary>
		/// Disable the current attacks hit box immediately. Usually used when using animation events to enable hitboxes.
		/// </summary>
		virtual public void DisableHitBox()
		{
			if (currentAttack == -1 || attacks[currentAttack].hitBox == null ) return;
			attacks[currentAttack].hitBox.DisableImmediate();
		}
		
		/// <summary>
		/// Instantiates the deferred projectile.
		/// </summary>
		virtual public void InstantiateProjectile()
		{
			InstantiateProjectile (-1);
		}

		/// <summary>
		/// Instatiates a projectile.
		/// </summary>
		/// <param name="attackIndex">Index of the projectile to instantiate.</param>
		virtual public void InstantiateProjectile(int attackIndex)
		{
			// If attack index == -1 then we should use the deferred attack.
			if (attackIndex == -1) attackIndex = deferredAttackIndex;
			// Instantiate prefab
			GameObject go = (GameObject) GameObject.Instantiate(attacks[attackIndex].projectilePrefab);
			Projectile projectile = go.GetComponent<Projectile>();
			if (projectileAimer != null) 
			{
				go.transform.position = character.transform.position + (Vector3)projectileAimer.GetAimOffset(character);
			}
			else
			{
				go.transform.position = character.transform.position;
			}
			
			if (projectile != null) {
                DoPrepare(projectile, attackIndex);
                if (!attacks[attackIndex].delayFire) {
                    FirePreparedProjectile();
                }
            }
			// If the projectile is found and the go is still alive call finish
			if (projectile != null && go != null) projectile.Finish ();
		}

        /// <summary>
        /// Prepares a projectile ready for firing.
        /// </summary>
        /// <param name="projectile">Projectile.</param>
        /// <param name="projectile">Index of attack data.</param>
        virtual protected void DoPrepare(Projectile projectile, int index)
        {
            // Fire projectile if the projectile is of type projectile
            Vector2 direction = new Vector2(character.LastFacedDirection != 0 ? character.LastFacedDirection : 1, 0);
            // Use aimer to get direction fo fire if the aimer is configured
            if (projectileAimer != null) direction = projectileAimer.GetAimDirection(character);
            preparedDirection = direction;
            preparedProjectile = projectile;
            preparedAttackData = index;
        }

        /// <summary>
        /// Fired a previously prepared projectile.
        /// </summary>
        virtual public void FirePreparedProjectile()
        {
	        if (preparedAttackData != -1 && preparedProjectile != null)
	        {
		        int damageAmount =  GetAttackDamage(attacks[preparedAttackData]);
		        DamageType damageType = attacks[preparedAttackData].damageType;
		        if (!string.IsNullOrEmpty(attacks[currentAttack].weaponSlot))
		        {
			        ItemInstanceData weapon = character.EquipmentManager.GetItemForSlot(attacks[currentAttack].weaponSlot);
			        if (weapon != null && weapon.Data != null && weapon.Data.damageType != DamageType.NONE) damageType = weapon.Data.damageType;
		        }
		        preparedProjectile.Fire(damageAmount, damageType, preparedDirection, character);
	        }
            preparedProjectile = null;
            preparedAttackData = -1;
        }

        /// <summary>
        /// Gets the attack damage including modifiers
        /// </summary>
        /// <returns>The attack damage.</returns>
        /// <param name="data">Base AttackData</param>
        virtual public int GetAttackDamage(BasicAttackData data)
        {
	        float multiplier = 1.0f;
	        // Apply wielded items
	        if (character.EquipmentManager != null && !skipEquipmentMultipliers)
	        {
		        multiplier *= character.EquipmentManager.TotalDamageMultiplier;
	        }
	        // Apply upgrades
	        if (character.ItemManager != null && !skipUpgradeMultipliers)
	        {
		        multiplier *= character.ItemManager.TotalDamageMultiplier;
	        }	        
	        // Apply power-ups
	        if (character.PowerUpManager != null && !skipPowerUpMultipliers)
	        {
		        multiplier *= character.PowerUpManager.TotalDamageMultiplier;
	        }
	        // Multiply and round up
	        return (int) (((float) data.damageAmount * multiplier) + 0.5f);
        }

        /// <summary>
        /// Gets the attack time including modifiers
        /// </summary>
        /// <returns>The attack damage.</returns>
        /// <param name="data">Base AttackData</param>
        virtual public float GetAttackTime(BasicAttackData data)
        {
	        return GetAttackTime(data.attackTime);
        }

        virtual public float GetAttackTime(float baseTime)
        {
	        float multiplier = 1.0f;
	        // Apply wielded items
	        if (character.EquipmentManager != null && !skipEquipmentMultipliers)
	        {
		        multiplier *= character.EquipmentManager.TotalWeaponSpeedMultiplier;
	        }
	        // Apply upgrades
	        if (character.ItemManager != null && !skipUpgradeMultipliers)
	        {
		        multiplier *= character.ItemManager.TotalWeaponSpeedMultiplier;
	        }	        
	        // Apply power-ups
	        if (character.PowerUpManager != null && !skipPowerUpMultipliers)
	        {
		        multiplier *= character.PowerUpManager.TotalWeaponSpeedMultiplier;
	        }
	        // Multiply and round up
	        return baseTime / multiplier;
        }
        
        
        /// <summary>
        /// Gets the animation state that this movement wants to set.
        /// </summary>
        /// <value>The state of the animation.</value>
        override public AnimationState AnimationState
		{
			get 
			{
				if (currentAttack != -1) return attacks[currentAttack].animation;
				return AnimationState.NONE;
			}
		}

		/// <summary>
		/// Gets the priority of the animation state that this movement wants to set.
		/// </summary>
		override public int AnimationPriority
		{
			get 
			{
				if (currentAttack != -1) return 10;
				return 0;
			}
		}

		/// <summary>
		/// Gets the animation override state that this movement wants to set.
		/// </summary>
		override public string OverrideState
		{
			get 
			{
				// If we dont want control and we dont want state then set an override state
				if (!attackSystemWantsMovementControl && !attackSystemWantsAnimationStateOverride && this.AnimationState != AnimationState.NONE) return this.AnimationState.AsString();
				return null;
			}
		}

		/// <summary>
		/// If the attack is in progress force control.
		/// </summary>
		override public bool ForceMaintainControl()
		{
			if (currentAttack != -1 && attackSystemWantsMovementControl) return true;
			return false;
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="PlatformerPro.Movement"/> expects
		/// gravity to be applied after its movement finishes.
		/// </summary>
		override public bool ShouldApplyGravity
		{
			get
			{
				if (attackSystemWantsMovementControl && currentAttack != -1 ) return attacks[currentAttack].applyGravity;
				return true;
			}
		}

		/// <summary>
		/// Should we block jumping movement?
		/// </summary>
		/// <returns><c>true</c>, if jump should be blocked, <c>false</c> otherwise.</returns>
		virtual public bool BlockJump()
		{
			if (currentAttack == -1) return false;
			return attacks [currentAttack].blockJump;
		}
		
		/// <summary>
		/// Should we block wall movement?
		/// </summary>
		/// <returns><c>true</c>, if wall cling should be blocked, <c>false</c> otherwise.</returns>
		virtual public bool BlockWall()
		{
			if (currentAttack == -1) return false;
			return attacks [currentAttack].blockWall;
		}
		
		/// <summary>
		/// Should we block climb movement?
		/// </summary>
		/// <returns><c>true</c>, if climb should be blocked, <c>false</c> otherwise.</returns>
		virtual public bool BlockClimb()
		{
			if (currentAttack == -1) return false;
			return attacks [currentAttack].blockClimb;
		}

		/// <summary>
		/// Should we block climb movement?
		/// </summary>
		/// <returns><c>true</c>, if jump should be blocked, <c>false</c> otherwise.</returns>
		virtual public bool BlockSpecial()
		{
			if (currentAttack == -1) return false;
			return attacks [currentAttack].blockSpecial;
		}
	}

}
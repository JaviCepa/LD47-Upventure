using UnityEngine;
#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_1 && !UNITY_5_2
using UnityEngine.SceneManagement;
#endif
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Tracks character health and triggers character death.
	/// </summary>
	[RequireComponent (typeof(Character))]
	public class CharacterHealth : Persistable
	{
		
#if UNITY_EDITOR

		/// <summary>
		/// Gets the header string used to describe the component.
		/// </summary>
		/// <value>The header.</value>
		override public string Header
		{
			get
			{
				return "Manages and stores characters health and lives.";
			}
		}

		/// <summary>
		/// Gets a link to a youtube video.
		/// </summary>
		/// <value>The header.</value>
		override public string DocLink
		{
			get
			{
				return "https://jnamobile.zendesk.com/hc/en-gb/categories/200246030-Platformer-PRO-Documentation";
			}
		}
#endif

		/// <summary>
		/// Health the character starts with.
		/// </summary>
		public int startingHealth;

		/// <summary>
		/// Maximum health of the character.
		/// </summary>
		public int maxHealth;

		/// <summary>
		/// Lives the character starts with. Use 0 for no lives.
		/// </summary>
		public int startingLives;

		/// <summary>
		/// Maximum lives of the character.
		/// </summary>
		public int maxLives;

		/// <summary>
		/// How long the character is invulnerable to damage after being hit.
		/// </summary>
		[Tooltip ("How long the character is invulnerable for after being damaged.")]
		public float invulnerableTime;

		/// <summary>
		/// The damage immunity.
		/// </summary>
		[Tooltip ("Use this to specify immunity (damage reduction) for different types of damage.")]
		public DamageImmunity[] damageImmunity;

		/// <summary>
		/// What do we do on death.
		/// </summary>
		[Tooltip ("What do we do on death.")]
		public DeathAction[] deathActions;

		/// <summary>
		/// Should we not play a damage animation if we die (instead we might go straight to death animation).
		/// </summary>
		[Tooltip ("Should we not play a damage animation if we die (instead we might go straight to death animation).")]
		public bool skipDamageAnimationOnDeath;

		/// <summary>
		/// Should we do the death actions on game over or go straight to GameOver actions
		/// </summary>
		[Tooltip ("Should we do the death actions on game over or go straight to GameOver actions")]
		public bool skipDeathActionsOnGameOver;

		/// <summary>
		/// If true send damage events even when damage amount is zero.
		/// </summary>
		[Tooltip ("If true send damage events even when damage amount is zero")]
		public bool sendEventsOnZeroDamage;

		/// <summary>
		/// If true send damage events even when damage amount is zero.
		/// </summary>
		[Tooltip ("If true health will be saved between levels as well as lives.")]
		public bool saveHealthBetweenLevels;
		
		
		/// <summary>
		/// The player preference identifier.
		/// </summary>
		public const string UniqueDataIdentifier = "CharacterHealth.Lives";

		#region protected members

		/// <summary>
		/// Characters current health.
		/// </summary>
		protected int health;

		/// <summary>
		/// Characters current lives.
		/// </summary>
		protected int lives;

		/// <summary>
		/// Timer for invulnerability. Character is invulnerable when this is greater than zero.
		/// </summary>
		protected float invulnerableTimer;

		/// <summary>
		/// Cached Character reference
		/// </summary>
		protected Character character;

		/// <summary>
		/// If we are dying ignore damage messages.
		/// </summary>
		protected bool dying;

		/// <summary>
		/// Cached copy of damage event args to save on allocations.
		/// </summary>
		protected DamageInfoEventArgs damageEventArgs;

		/// <summary>
		/// Checks if we have calculated max health this frame. We only do this once per frame.
		/// </summary>
		protected bool updateMaxHealthThisFrame;
		
		/// <summary>
		/// Tracks value of last calculated max health which includes 
		/// </summary>
		protected int lastCalculatedMaxHealth;
		
		#endregion

		#region events
		
		/// <summary>
		/// Event for health gain.
		/// </summary>
		public event System.EventHandler <HealedEventArgs> Healed;
		
		/// <summary>
		/// Event for damage.
		/// </summary>
		public event System.EventHandler <DamageInfoEventArgs> Damaged;
		
		/// <summary>
		/// Event for death.
		/// </summary>
		public event System.EventHandler <DamageInfoEventArgs> Died;
		
		/// <summary>
		/// Event for game over.
		/// </summary>
		public event System.EventHandler <CharacterEventArgs> GameOver;

		/// <summary>
		/// Event for health gain.
		/// </summary>
		public event System.EventHandler <HealedEventArgs> GainLives;

		/// <summary>
		/// Event for when max lives or health changes.
		/// </summary>
		public event System.EventHandler <HealedEventArgs> MaxValuesUpdated;

		/// <summary>
		/// Raises the healed event.
		/// </summary>
		/// <param name="amount">Amount healed.</param>
		virtual protected void OnHealed(int amount)
		{
			if (Healed != null)
			{
				Healed(this, new HealedEventArgs(amount));
			}
		}
		
		/// <summary>
		/// Raises the damaged event.
		/// </summary>
		/// <param name="info">Info.</param>
		virtual protected void OnDamaged(DamageInfo info)
		{
			if (Damaged != null)
			{
				damageEventArgs.UpdateDamageInfoEventArgs(info);
				Damaged(this, damageEventArgs);
			}
		}
		
		/// <summary>
		/// Raises the died event.
		/// </summary>
		/// <param name="info">Info.</param>
		virtual protected void OnDied(DamageInfo info)
		{
			if (Died != null)
			{
				damageEventArgs.UpdateDamageInfoEventArgs(info);
				Died(this, damageEventArgs);
			}
		}
		
		/// <summary>
		/// Raises the game over event.
		/// </summary>
		/// <param name="info">Info.</param>
		virtual protected void OnGameOver()
		{
			if (GameOver != null)
			{
				GameOver(this, new CharacterEventArgs(character));
			}
		}

		/// <summary>
		/// Raises the gain lives event.
		/// </summary>
		/// <param name="amount">Amount.</param>
		virtual protected void OnGainLives(int amount)
		{
			if (GainLives != null)
			{
				GainLives(this, new HealedEventArgs(amount));
			}
		}

		/// <summary>
		/// Raises the max values updated event.
		/// </summary>
		virtual protected void OnMaxValuesUpdated()
		{
			if (MaxValuesUpdated != null)
			{
				MaxValuesUpdated(this, new HealedEventArgs(0));
			}
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the character reference.
		/// </summary>
		/// <value>The character.</value>
		override public Character Character
		{
			get
			{
				return character;
			}
            set
            {
                Debug.LogWarning("CharacterHealth doesn't allow character to be changed");
            }
        }

		/// <summary>
		/// Is the character currently invulnerable?
		/// </summary>
		/// <value>The character.</value>
		virtual public bool IsInvulnerable
		{
			get
			{
				// Dead characters can't take damage
				if (health <= 0) return true;
				return invulnerableTimer > 0.0f;
			}
		}

		
		/// <summary>
		/// Gets the current health
		/// </summary>
		virtual public int CurrentHealth
		{
			get
			{
				return health;
			}
			set
			{
				if (value > 0)
				{
					health = (value > MaxHealth) ? MaxHealth : value;
				}
				else
				{
					Debug.LogError ("Cannot set character health to zero or less. Use Kill() if that is what you are trying to do.");
				}
			}
		}

		/// <summary>
		/// Gets or sets the max health
		/// </summary>
		virtual public int MaxHealth
		{
			get
			{
				return lastCalculatedMaxHealth;
			}
			set
			{
				maxHealth = value;
				if (maxHealth < 1) maxHealth = 1;
				DoUpdateMaxHealth();
				if (health > maxHealth) health = maxHealth;
			}
		}

		/// <summary>
		/// Gets or sets the max lives
		/// </summary>
		virtual public int MaxLives
		{
			get
			{
				return maxLives;
			}
			set
			{
				maxLives = value;
				if (value < 1) value = 1;
				if (lives > maxLives) lives = maxLives;
				OnMaxValuesUpdated();
			}
		}

		/// <summary>
		/// Gets a value representing the current health as a percentage between 0 (none)
		/// and 1 (full).
		/// </summary>
		/// <returns>The percentage health between 0 and 1.</returns>
		virtual public float CurrentHealthAsPercentage
		{
			get
			{
				return (float)health / (float)MaxHealth;
			}
		}
		
		/// <summary>
		/// Gets the current number of lives.
		/// </summary>
		virtual public int CurrentLives
		{
			get
			{
				return lives;
			}
			set
			{
				if (lives != value)
				{
					int originalLives = lives;
					lives = value;
					if (lives > maxLives) lives = maxLives;
					if (lives <= 0)
					{
						Kill();
					}
					else
					{
						OnGainLives(lives - originalLives);
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Unity Update hook.
		/// </summary>
		void Update()
		{
			if (Application.isPlaying)
			{
				if (invulnerableTimer > 0.0f) invulnerableTimer -= TimeManager.FrameTime;
			}
		}
		
		/// <summary>
		/// Unity LateUpdate hook.
		/// </summary>
		void LateUpdate()
		{
			if (updateMaxHealthThisFrame) DoUpdateMaxHealth();
		}
		
		/// <summary>
		/// Init with the specified character.
		/// </summary>
		/// <param name="character">Character.</param>
		virtual public void Init(Character character)
		{
			this.character = character;
			ConfigureEventListeners ();
			if (!loaded)
			{
				if (health == 0)
					health = startingHealth;
				if (lives <= 0) lives = startingLives;
			}
			dying = false;
			damageEventArgs = new DamageInfoEventArgs ();
			UpdateMaxHealth();
		}

		protected override void ConfigureEventListeners()
		{
			base.ConfigureEventListeners();
			if (Character.ItemManager != null)
			{
				Character.ItemManager.Loaded += (sender, args) => DoUpdateMaxHealth();
				Character.ItemManager.InventoryChanged += (sender, args) => UpdateMaxHealth();
				Character.ItemManager.ItemDepleted += (sender, args) => UpdateMaxHealth();
				Character.ItemManager.ItemConsumed += (sender, args) => UpdateMaxHealth();
				Character.ItemManager.ItemDestroyed += (sender, args) => UpdateMaxHealth();
				Character.ItemManager.ItemCollected += (sender, args) => UpdateMaxHealth();
				Character.ItemManager.ItemDropped += (sender, args) => UpdateMaxHealth();
			}
			if (Character.EquipmentManager != null)
			{
				Character.ItemManager.Loaded += (sender, args) => DoUpdateMaxHealth();
				Character.EquipmentManager.ItemEquipped += (sender, args) => UpdateMaxHealth();
				Character.EquipmentManager.ItemUnequipped += (sender, args) => UpdateMaxHealth();
			}
		}

		/// <summary>
        /// Reset the health back to its starting value
        /// </summary>
        virtual public void ResetHealth()
        {
            health = startingHealth;
        }

        /// <summary>
        /// Heal the character by the specified amount.
        /// </summary>
        /// <param name="amount">Amount.</param>
        virtual public void Heal(int amount)
		{
			health += amount;
			if (health > MaxHealth) health = MaxHealth;
			OnHealed(amount);
		}

		/// <summary>
		/// Makes the character invulnerable for the specified time.
		/// </summary>
		/// <param name="time">How long the cahracter is unvulnerable for.</param>
		virtual public void SetInvulnerable(float time)
		{
			invulnerableTimer = time;
		}

		/// <summary>
		/// Makes the character invulnerable for the specified time.
		/// </summary>
		/// <param name="time">How long the cahracter is unvulnerable for.</param>
		virtual public void SetInvulnerable()
		{
			// Not technically permantently invulnerable, but it lasts for a very long time.
			invulnerableTimer = float.MaxValue;
		}

		/// <summary>
		/// Makes the character invulnerable for the specified time.
		/// </summary>
		/// <param name="time">How long the cahracter is unvulnerable for.</param>
		virtual public void SetVulnerable()
		{
			invulnerableTimer = 0;
		}

		/// <summary>
		/// Damage the character by the specified amount.
		/// </summary>
		/// <param name="amount">Amount.</param>
		virtual public void Damage(int amount)
		{
			if (!dying && !IsInvulnerable)
			{
				health -= amount;
				DamageInfo info = new DamageInfo(amount, DamageType.NONE, Vector2.zero, character);
				OnDamaged(info);
				if (health <= 0) 
				{	
					if (!skipDamageAnimationOnDeath) character.Damage (info);
					DoDeath(info);
				}
				else
				{
					invulnerableTimer = invulnerableTime;
					character.Damage (info);
				}
			}
		}

		/// <summary>
		/// Damage the character with the specified damage information. This is the preferred method of receiving damage as it allows 
		/// the hazard and hit direction to be processed (for example you could have immunity or damage reduction for a certain hazard).
		/// </summary>
		/// <param name="info">The damage info.</param>
		virtual public void Damage(DamageInfo info)
		{
			if (!dying && !IsInvulnerable)
			{
				int actualDamageAmount = info.Amount;
				// Check for damage immunity
				if (damageImmunity != null && damageImmunity.Length > 0)
				{
					for (int i = 0; i < damageImmunity.Length; i++)
					{
						if (info.DamageType == damageImmunity[i].damageType)
						{
							actualDamageAmount = (int)(0.5f + (float)info.Amount * (1.0f - damageImmunity[i].immunity));
							break;
						}
					}
				}
				// Don't send damage event or adjust health if damage amount is zero
				if (actualDamageAmount == 0 && !sendEventsOnZeroDamage) return;
				health -= actualDamageAmount;
				OnDamaged(info);
				if (health <= 0) 
				{
					if (!skipDamageAnimationOnDeath) character.Damage (info);
					DoDeath(info);
				}
				else
				{
					character.Damage(info);
					invulnerableTimer = invulnerableTime;
				}
			}
		}

		/// <summary>
		/// Kills the character.
		/// </summary>
		virtual public void Kill()
		{
			if (!dying)
			{
				health = 0;
				DoDeath(new DamageInfo(0, DamageType.NONE, Vector2.zero));
			}
		}

		/// <summary>
		/// Kills the character.
		/// </summary>
		virtual public void Kill(DamageInfo info)
		{
			if (!dying) {
				health = 0;
				DoDeath(info);
			}
		}

		virtual protected void UpdateMaxHealth()
		{
			updateMaxHealthThisFrame = true;
		}

		virtual protected void DoUpdateMaxHealth()
		{
			lastCalculatedMaxHealth = maxHealth;
			// Equipped items
			if (character.EquipmentManager != null)
			{
				lastCalculatedMaxHealth += character.EquipmentManager.TotalMaxHealthAdjustment;
			}
			// Upgrades
			if (character.ItemManager != null)
			{
				lastCalculatedMaxHealth += character.ItemManager.TotalMaxHealthAdjustment;
			}
			// Power-ups
			if (character.PowerUpManager != null)
			{
				lastCalculatedMaxHealth += character.PowerUpManager.TotalMaxHealthAdjustment;
			}
			if (health > lastCalculatedMaxHealth) health = lastCalculatedMaxHealth;
			OnMaxValuesUpdated();
			updateMaxHealthThisFrame = false;
		}
		
		/// <summary>
		/// Do the death actions
		/// </summary>
		virtual protected void DoDeath(DamageInfo info)
		{
			// Check for GameOver
			lives--;
			OnDied(info);
			dying = true;
			character.Kill (info);
			if (lives <= 0)
			{
				if (!skipDeathActionsOnGameOver)
				{
					for (int i = 0; i < deathActions.Length; i++)
					{
						StartCoroutine(DoDeathAction(deathActions[i], info));
					}
				}
				DoGameOver(info);
			}
			else
			{
				for (int i = 0; i < deathActions.Length; i++)
				{
					StartCoroutine(DoDeathAction(deathActions[i], info));
				}
			}
		}
		
		/// <summary>
		/// End the game
		/// </summary>
		virtual protected void DoGameOver(DamageInfo info)
		{
			OnGameOver();
		}

        override protected void PhaseChange(object sender, GamePhaseEventArgs e)
        {
            if (e.Phase == GamePhase.READY && CurrentHealth == 0) ResetHealth();
        }

        /// <summary>
        /// Does a death action.
        /// </summary>
        /// <param name="action">Action.</param>
        virtual protected IEnumerator DoDeathAction(DeathAction action, DamageInfo info)
		{
			// Wait...
			if (action.delay > 0) yield return new WaitForSeconds(action.delay);
			// Then act
			switch (action.actionType)
			{
				case DeathActionType.DESTROY_CHARACTER:
					Destroy(character.gameObject);
					break;
				case DeathActionType.RESPAWN:
					health = startingHealth;
					dying = false;
					LevelManager.Instance.Respawn(character);
					break;
			case DeathActionType.RELOAD_SCENE:
					#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_1 && !UNITY_5_2
					SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
					#else
					Application.LoadLevel(Application.loadedLevel);
					#endif
					break;
				case DeathActionType.LOAD_ANOTHER_SCENE:
					#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_1 && !UNITY_5_2
					LevelManager.PreviousLevel = SceneManager.GetActiveScene().name;
					SceneManager.LoadScene(action.supportingData);
					#else
					LevelManager.PreviousLevel = Application.loadedLevelName;
					Application.LoadLevel(action.supportingData);
					#endif
					break;
				case DeathActionType.SEND_MESSAGE:
					action.supportingGameObject.SendMessage(action.supportingData, SendMessageOptions.DontRequireReceiver);
					break;
				case DeathActionType.CLEAR_RESPAWN_POINTS:
					if (LevelManager.Instance != null) LevelManager.Instance.ClearRespawns();
					break;
				case DeathActionType.RESET_DATA:
					foreach (Persistable persistable in action.supportingGameObject.GetComponents<Persistable>())
					{
						persistable.ResetSaveData();
					}
					break;
				case DeathActionType.RESET_SCORE:
					ScoreManager.GetInstanceForType(action.supportingData).ResetScore();
					break;
			}
		}

		#region Persitable methods
		
		/// <summary>
		/// Gets the data to save.
		/// </summary>
		override public object SaveData
		{
			get
			{
				return new int[]{CurrentLives, CurrentHealth};
			}
		}
		
		/// <summary>
		/// Get a unique identifier to use when saving the data (for example this could be used for part of the file name or player prefs name).
		/// </summary>
		/// <value>The identifier.</value>
		override public string Identifier
		{
			get
			{
				return UniqueDataIdentifier;
			}
		}
		
		/// <summary>
		/// Applies the save data to the object.
		/// </summary>
		override public void ApplySaveData(object t)
		{
			if (t is int[] && t != null && ((int[])t).Length == 2)
			{
				this.lives = ((int[])t)[0];
                if (saveHealthBetweenLevels)
                {
                    this.health = ((int[])t)[1];
                    if (this.health <= 0) this.health = startingHealth;
                }
				OnLoaded();
			}
			else Debug.LogError("Tried to apply unexpected data: " + t.GetType());
		}
		
		/// <summary>
		/// Get the type of object this Persistable saves.
		/// </summary>
		override public System.Type SavedObjectType()
		{
			return typeof(int[]);
		}

		/// <summary>
		/// Resets the save data back to default.
		/// </summary>
		override public void ResetSaveData()
		{
			lives = startingLives;
			if (saveHealthBetweenLevels) health = startingHealth;
#if UNITY_EDITOR
			Save(this);
#endif
		}

		#endregion

	}

}
/**
 * This code is part of Platformer PRO and is copyright John Avery 2014.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// Responds to power up collection by implementing the power.
	/// </summary>
	public class PowerUpManager : GenericResponder, ICharacterReference
	{

		/// <summary>
		/// The actions needed to hard reset the character back to starting state.
		/// </summary>
		public PowerUpResponse resetResponse;

		/// <summary>
		/// Details about the reponse this character has to a given power up.
		/// </summary>
		public List<PowerUpResponse> responses;

		/// <summary>
		/// Should we ignore zero valued modifiers in item settings.
		/// </summary>
		[Header("Modifiers")]
		public bool ignoreZeroValuedMultipliers = true;

		/// <summary>
		/// The currently active power ups.
		/// </summary>
		protected List<string> activePowerUps;

		/// <summary>
		/// The cached character reference.
		/// </summary>
		protected Character character;


		/// <summary>
		/// Track power up resets so that If a power up will reset, but then another power up of the same 
		/// time is collected we can cancel the reset.
		/// </summary>
		protected Dictionary<string, Coroutine> resetActionMap;

		/// <summary>
		/// Event for powered up state changes.
		/// </summary>
		public event System.EventHandler<ItemEventArgs> PowerUp;

		protected float totalJumpHeightMultiplier;

		/// <summary>
		/// Gets the total JumpHeightMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalJumpHeightMultiplier => totalJumpHeightMultiplier;


		protected float totalMoveSpeedMultiplier;

		/// <summary>
		/// Gets the total MoveSpeedMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalMoveSpeedMultiplier => totalMoveSpeedMultiplier;

		protected float totalRunSpeedMultiplier;

		/// <summary>
		/// Gets the total RunSpeedMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalRunSpeedMultiplier => totalRunSpeedMultiplier;

		protected float totalAccelerationMultiplier;

		/// <summary>
		/// Gets the total AccelerationMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalAccelerationMultiplier => totalAccelerationMultiplier;

		protected int totalMaxHealthAdjustment;

		/// <summary>
		/// Gets the total MaxHealthAdjustment of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public int TotalMaxHealthAdjustment => totalMaxHealthAdjustment;

		protected float totalDamageMultiplier;

		/// <summary>
		/// Gets the total DamageMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalDamageMultiplier => totalDamageMultiplier;

		protected float totalWeaponSpeedMultiplier;

		/// <summary>
		/// Gets the total WeaponSpeedMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalWeaponSpeedMultiplier => totalWeaponSpeedMultiplier;


		public Character Character
		{
			get { return character; }
			set { Debug.LogWarning("PowerUpManager doesn't allow character to be changed"); }
		}

		/// <summary>
		/// Raises the power up event.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="character">Character.</param>
		virtual protected void OnPowerUp(string type, Character character)
		{
			if (PowerUp != null)
			{
				PowerUp(this, new ItemEventArgs(type, character));
			}
		}

		/// <summary>
		/// Gets the active power ups. Note this is not a copy.
		/// </summary>
		/// <value>The active power ups.</value>
		public List<string> ActivePowerUps
		{
			get { return activePowerUps; }
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		virtual public void Init(Character character)
		{
			this.character = character;
			resetActionMap = new Dictionary<string, Coroutine>();
			activePowerUps = new List<string>();
			HardReset();
		}

		/// <summary>
		/// Unity on destroy
		/// </summary>
		void OnDestroy()
		{
			if (character != null && character.CharacterHealth != null) character.CharacterHealth.Died -= HandleDied;
		}

		/// <summary>
		/// Handles the character dying by doing a hard reset.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandleDied(object sender, DamageInfoEventArgs e)
		{
			HardReset();
		}

		/// <summary>
		/// Sets the power up timer.
		/// </summary>
		/// <param name="time">Time.</param>
		/// <param name="resetResponses">Which set of reset actions to run when the timer expires.</param>
		virtual protected void SetTimer(float time, PowerUpResponse resetResponse)
		{
			if (resetActionMap.ContainsKey(resetResponse.typeId))
			{
				StopCoroutine(resetActionMap[resetResponse.typeId]);
				resetActionMap.Remove(resetResponse.typeId);
			}

			resetActionMap.Add(resetResponse.typeId, StartCoroutine(DoResetAfterTime(time, resetResponse)));
		}

		/// <summary>
		/// Does the listed power up reset after the given time.
		/// </summary>
		/// <param name="time">The powers up lifetime.</param>
		/// <param name="resetResponses">Which set of reset actions to run when the timer expires.</param>
		virtual protected IEnumerator DoResetAfterTime(float time, PowerUpResponse resetResponse)
		{
			yield return new WaitForSeconds(time);
			if (resetActionMap.ContainsKey(resetResponse.typeId))
			{
				resetActionMap.Remove(resetResponse.typeId);
			}

			ResetPowerUp(resetResponse);
		}

		/// <summary>
		/// Determines whether the given power-up type is active.
		/// </summary>
		/// <returns><c>true</c> if the given power-up type is active; otherwise, <c>false</c>.</returns>
		/// <param name="type">Power up type.</param>
		virtual public bool IsActive(string type)
		{
			if (activePowerUps.Contains(type)) return true;
			return false;
		}

		/// <summary>
		/// Resets the power up with the given responses.
		/// </summary>
		/// <param name="resetResponses">Which set of reset actions to run.</param>
		virtual public void ResetPowerUp(PowerUpResponse resetResponse)
		{

			if (activePowerUps.Contains(resetResponse.typeId)) activePowerUps.Remove(resetResponse.typeId);

			character.ItemManager.RemoveItemEffects(resetResponse.typeId);

			if (resetResponse.resetActions != null)
			{
				for (int i = 0; i < resetResponse.resetActions.Length; i++)
					DoAction(resetResponse.resetActions[i], null);
			}

			// Remove any associated event listeners
			if (resetResponse.damageResetListener != null)
			{
				CharacterHealth health = character.GetComponentInChildren<CharacterHealth>();
				if (health != null)
				{
					health.Damaged -= resetResponse.damageResetListener;
					resetResponse.damageResetListener = null;
				}
			}

			RecalculateEffectsOfItems();
		}

		/// <summary>
		/// Collect the specified power up by type.
		/// </summary>
		/// <param name="powerUpType">Power up type.</param>
		/// <returns>true if a response was found.</returns>
		public bool Collect(string powerUpType)
		{
			// Apply powers
			foreach (PowerUpResponse response in responses)
			{
				if (response.typeId == powerUpType)
				{
					ItemTypeData powerUpData = ItemTypeManager.Instance.GetTypeData(powerUpType);
					activePowerUps.Add(response.typeId);
					character.ItemManager.ApplyItemEffects(powerUpData);
					for (int i = 0; i < response.actions.Length; i++) DoAction(response.actions[i], null);
					OnPowerUp(powerUpType, character);
					if (powerUpData.effectDuration > 0)
					{
						SetTimer(powerUpData.effectDuration, response);
					}

					if (response != null && (response.actions != null || response.resetActions != null) &&
					    powerUpData.resetEffectOnDamage)
					{
						CharacterHealth health = character.GetComponentInChildren<CharacterHealth>();
						if (health == null)
						{
							Debug.LogWarning("Power up cannot reset on damage as no character health could be found.");
						}
						else
						{
							if (response.damageResetListener != null)
							{
								health.Damaged -= response.damageResetListener;
								response.damageResetListener = null;
							}

							response.damageResetListener = delegate(object sender, DamageInfoEventArgs e)
							{
								ResetPowerUp(response);
							};
							health.Damaged += response.damageResetListener;
						}
					}

					RecalculateEffectsOfItems();
					return true;
				}
			}

			RecalculateEffectsOfItems();
			return false;
		}

		/// <summary>
		/// Runs the 
		/// </summary>
		public void HardReset()
		{
			for (int i = 0; i < resetResponse.actions.Length; i++) DoAction(resetResponse.actions[i], null);
			if (activePowerUps != null) activePowerUps.Clear();
			RecalculateEffectsOfItems();
		}

		/// <summary>
		/// Updates multiplier stats.
		/// </summary>
		virtual protected void RecalculateEffectsOfItems()
		{
			if (ActivePowerUps == null) return;
			totalJumpHeightMultiplier = 1.0f;
			totalMoveSpeedMultiplier = 1.0f;
			totalRunSpeedMultiplier = 1.0f;
			totalAccelerationMultiplier = 1.0f;
			totalDamageMultiplier = 1.0f;
			totalWeaponSpeedMultiplier = 1.0f;
			totalMaxHealthAdjustment = 0;

			foreach (string itemId in ActivePowerUps)
			{
				ItemTypeData i = ItemTypeManager.Instance.GetTypeData(itemId);
				if (i == null || i.itemBehaviour != ItemBehaviour.POWER_UP) break;
				if (!ignoreZeroValuedMultipliers && i.jumpHeightMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued jump multiplier");
				}
				else
				{
					totalJumpHeightMultiplier *= i.jumpHeightMultiplier;
				}

				if (!ignoreZeroValuedMultipliers && i.moveSpeedMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued move speed multiplier");
				}
				else
				{
					totalMoveSpeedMultiplier *= i.moveSpeedMultiplier;
				}

				if (!ignoreZeroValuedMultipliers && i.runSpeedMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued run speed multiplier");
				}
				else
				{
					totalRunSpeedMultiplier *= i.runSpeedMultiplier;
				}

				if (!ignoreZeroValuedMultipliers && i.accelerationMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued acceleration speed multiplier, you should use a value of 1.0f");
				}
				else
				{
					totalAccelerationMultiplier *= i.accelerationMultiplier;
				}

				if (!ignoreZeroValuedMultipliers && i.damageMultiplier == 0.0f)
				{
					Debug.LogWarning("Skipping zero valued damage multiplier, you should use a value of 1.0f");
				}
				else
				{
					totalDamageMultiplier *= i.damageMultiplier;
				}

				if (i.weaponSpeedMultiplier < 0.0f)
				{
					Debug.LogWarning("Weapon speed modifier cannot be 0 or less, try a value of 1.0f");
				}
				else
				{
					totalWeaponSpeedMultiplier *= i.weaponSpeedMultiplier;
				}

				// Max health adds not multiplies
				totalMaxHealthAdjustment += i.maxHealthAdjustment;
			}
		}
	}
	
}
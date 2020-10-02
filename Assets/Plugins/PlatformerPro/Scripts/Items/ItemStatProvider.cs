using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlatformerPro
{
	public abstract class ItemStatProvider : Persistable
	{
		protected float totalJumpHeightMultiplier = 1.0f;

		/// <summary>
		/// Gets the total JumpHeightMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalJumpHeightMultiplier => totalJumpHeightMultiplier;


		protected float totalMoveSpeedMultiplier = 1.0f;

		/// <summary>
		/// Gets the total MoveSpeedMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalMoveSpeedMultiplier => totalMoveSpeedMultiplier;

		protected float totalRunSpeedMultiplier  = 1.0f;

		/// <summary>
		/// Gets the total RunSpeedMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalRunSpeedMultiplier => totalRunSpeedMultiplier;

		protected float totalAccelerationMultiplier  = 1.0f;

		/// <summary>
		/// Gets the total AccelerationMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalAccelerationMultiplier => totalAccelerationMultiplier;

		protected int totalMaxHealthAdjustment  = 1;

		/// <summary>
		/// Gets the total MaxHealthAdjustment of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public int TotalMaxHealthAdjustment => totalMaxHealthAdjustment;

		protected float totalDamageMultiplier  = 1.0f;

		/// <summary>
		/// Gets the total DamageMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalDamageMultiplier => totalDamageMultiplier;

		protected float totalWeaponSpeedMultiplier = 1.0f;

		/// <summary>
		/// Gets the total WeaponSpeedMultiplier of all equipped items, updated whenever items are equipped.
		/// </summary>
		virtual public float TotalWeaponSpeedMultiplier => totalWeaponSpeedMultiplier;

		/// <summary>
		/// Updates multiplier stats.
		/// </summary>
		abstract protected void RecalculateEffectsOfItems();
	}
}
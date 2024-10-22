/**
 * This code is part of Platformer PRO and is copyright John Avery 2014.
 */

#if UNITY_EDITOR
using PlatformerPro.Validation;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// A wrapper class for handling movement that proxies the movement function
	/// to the desired implementation.
	/// </summary>
	public class BaseMovement <T> : Movement where T : Movement
	{

		#region members

		/// <summary>
		/// The class that will do the movement.
		/// </summary>
		protected Movement implementation;

		/// <summary>
		/// The type of movement as a string.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected string movementType;

		/// <summary>
		/// Data that should be applied to the movement type on init.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected MovementVariable[] movementData;

		#endregion

		#region properties

		/// <summary>
		/// The type of movement as a string.
		/// </summary>
		virtual public string MovementType
		{
			get
			{
				return movementType;
			}
			set
			{
				movementType = value;
			}
		}

		/// <summary>
		/// Data that should be applied to the movement type on init.
		/// </summary>
		virtual public MovementVariable[] MovementData
		{
			get
			{
				return movementData;
			}
			set
			{
				movementData = value;
			}
		}

		#endregion

		#region movement info constants and properties
		
		/// <summary>
		/// Human readable name.
		/// </summary>
		private const string Name = "Base Movement";
		
		/// <summary>
		/// Human readable description.
		/// </summary>
		private const string Description = "The base movement class, you shouldd't be seeing this did you forget to create a new MovementInfo?";

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

		#region public methods

		/// <summary>
		/// Initialise this movement.
		/// </summary>
		sealed override public Movement Init(Character character)
		{
			base.Init(character);
			if ((MovementType != null) && 
			    (
				    (this.GetType() == typeof(GroundMovement))   ||
					(this.GetType() == typeof(AirMovement))     ||
					(this.GetType() == typeof(WallMovement))    ||
					(this.GetType() == typeof(ClimbMovement))   ||
					(this.GetType() == typeof(DamageMovement))  ||
					(this.GetType() == typeof(DeathMovement))   ||
					(this.GetType() == typeof(SpecialMovement))
				))
			{
				try
				{
#if UNITY_5_3_OR_NEWER
					// TODO: this forces movements to be in the same namespace as this, look to fix!
					System.Type type = this.GetType().Assembly.GetType(this.GetType().Namespace + "." + MovementType);
					implementation = (Movement)gameObject.AddComponent(type);
					implementation.skipEquipmentMultipliers = skipEquipmentMultipliers;
					implementation.skipEquipmentMultipliers = skipEquipmentMultipliers;
					implementation.skipUpgradeMultipliers = skipUpgradeMultipliers;
					implementation.skipPowerUpMultipliers = skipPowerUpMultipliers;
#else
					implementation = (Movement)gameObject.AddComponent(MovementType);
					implementation.skipEquipmentMultipliers = skipEquipmentMultipliers;					
#endif
					if (!enabled) implementation.enabled = false;
				}
				catch (System.InvalidCastException)
				{
					Debug.LogError ("Provided class is not a Movement class: " + MovementType);
				}

				if (implementation == null)
				{
					Debug.LogError ("Unable to create movement of type: " + MovementType);
				}
				else
				{
					return implementation.Init (character, movementData);
				}
			}
            implementation = this;
			return Init (character, movementData);
		}

		/// <summary>
		/// A custom enable which base movements can use to pass on enable values.
		/// </summary>
		/// <value>The enabled.</value>
		override public bool Enabled
		{
			get
			{
				return implementation.enabled;
			}
			set
			{
				enabled = value;
				if (implementation != null) implementation.enabled = value;
			}
		}

		/// <summary>
		/// Gets the underlying implementation.
		/// </summary>
		/// <value>The implementation.</value>
		override public Movement Implementation 
		{
			get
			{
				return implementation;
			}
		}

		/// <summary>
		/// Determines whether this instances movement data is different from the supplied originalMovementData.
		/// </summary>
		/// <returns><c>true</c> if this instances movement data different the specified originalMovementData; otherwise, <c>false</c>.</returns>
		/// <param name="originalMovementData">Original movement data.</param>
		public bool IsMovementDataDifferent(MovementVariable[] originalMovementData)
		{
			// Early outs for nulls
			if (movementData != null && originalMovementData == null) return true;
			if (movementData == null && originalMovementData != null) return true;
			if (movementData == null && originalMovementData == null) return false;

			if (movementData.Length != originalMovementData.Length) return true;

			for (int i = 0; i < movementData.Length; i++)
			{
				if (movementData[i] != null && originalMovementData[i] == null) return true;
				if (movementData[i] == null && originalMovementData[i] != null) return true;
				if (movementData != null && originalMovementData != null && 
				    movementData[i].GetHashCode() != originalMovementData[i].GetHashCode())
				{
					return true;
				}
			}

			return false;
		}

		#endregion

#if UNITY_EDITOR

		/// <summary>
		/// Custom movement validation.
		/// </summary>
		/// <returns>A list of validation errors</returns>
		/// <param name="c">Character to validate against.</param>
		override public List<ValidationResult> ValidateMovement(Character c)
		{
			List<ValidationResult> result = new List<ValidationResult>();
			Movement m = Init (c);
			if (m.Implementation != null) m = m.Implementation;
			if (m != this)
			{
				result = m.ValidateMovement (c);
				implementation = null;
				DestroyImmediate (m);
			}
			return result;
		}

#endif
	}

}
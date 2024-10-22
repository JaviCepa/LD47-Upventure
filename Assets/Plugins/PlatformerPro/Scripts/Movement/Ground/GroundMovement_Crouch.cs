#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// Base ground movement for crouch functions which handles core behaviour like 
	/// </summary>
	public abstract class GroundMovement_Crouch : GroundMovement
	{
		
		#region members

		/// <summary>
		/// Should we shrink the hit box?
		/// </summary>
		public bool shrinkHurtBox;

		/// <summary>
		/// How much should we shrink the hit box by?
		/// </summary>
		[Range(1,3)]
		public float shrinkHurtBoxFactor;

		/// <summary>
		/// Should we shrink the head and ignore some sides?
		/// </summary>
		public bool shrinkHeadAndSides;

		/// <summary>
		/// The new height of the head (side colliders higher than this will be ignored).
		/// </summary>
		public float newHeadHeight;

		/// <summary>
		/// Action button to use for crouch, -1 means use DOWN instead.
		/// </summary>
		public int actionButton;

		/// <summary>
		/// If true pressing down toggles the crouch rather than requigin crouch to be held.
		/// </summary>
		public bool crouchToggle;
		
		/// <summary>
		/// Cached reference to the hitboxes collider.
		/// </summary>
		protected Collider2D hurtBoxCollider;

		/// <summary>
		/// The original extents of the head colliders of the character.
		/// </summary>
		protected Vector2[] originalHeadExtents;

		/// <summary>
		/// Cached copy of the calculated head extents.
		/// </summary>
		protected Vector2[] newHeadExtents;

		/// <summary>
		/// Should we show colldier settings in inspetor?
		/// </summary>
		protected bool showColliderSettings;

		/// <summary>
		/// Stores crouch state.
		/// </summary>
		protected bool crouchToggled;
		
		#endregion
		
		#region constants
		
		/// <summary>
		/// Human readable name.
		/// </summary>
		private const string Name = "Base Crouch";
		
		/// <summary>
		/// Human readable description.
		/// </summary>
		private const string Description = "The base crouch movement class, you shouldn't be seeing this did you forget to create a new MovementInfo?.";
		
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

		/// <summary>
		/// The index of shrink hit box in the movement data.
		/// </summary>
		protected const int ShrinkHurtBoxIndex = 0;
		
		/// <summary>
		/// The index of the shrink hit box factor in the movement data.
		/// </summary>
		protected const int ShrinkHurtBoxFactorIndex = 1;
		
		/// <summary>
		/// The index of shrink head and sides in the movement data.
		/// </summary>
		protected const int ShrinkHeadAndSidesIndex = 2;
		
		/// <summary>
		/// The index of the head height in the movement data.
		/// </summary>
		protected const int NewHeadHeightIndex = 3;

		/// <summary>
		/// The index of the action button in the movement data.
		/// </summary>
		protected const int ActionButtonIndex = 4;

		/// <summary>
		/// The index of the crouch toggle setting in the movement data.
		/// </summary>
		protected const int CrouchToggleIndex = 5;
		
		/// <summary>
		/// The default shrink hit box factor.
		/// </summary>
		protected const float DefaultShrinkHurtBoxFactor = 2.0f;

		/// <summary>
		/// The default height of the new head.
		/// </summary>
		protected const float DefaultNewHeadHeight = 0.5f;

		/// <summary>
		/// The default action button.
		/// </summary>
		protected const int DefaultActionButton = -1;

		/// <summary>
		/// The size of the movement variable array.
		/// </summary>
		protected const int MovementVariableCount = 6;
		

		#endregion

		#region properties

		/// <summary>
		/// Gets the animation state that this movement wants to set.
		/// </summary>
		override public AnimationState AnimationState
		{
			get 
			{
				return AnimationState.CROUCH;
			}
		}
		
		/// <summary>
		/// Returns the direction the character is facing. 0 for none, 1 for right, -1 for left.
		/// This overriden version always returns the input direction.
		/// </summary>
		override public int FacingDirection
		{
			get 
			{
				return character.Input.HorizontalAxisDigital;
			}
		}


		#endregion

		#region public methods
		
		/// <summary>
		/// Initialise the movement with the given movement data.
		/// </summary>
		/// <param name="character">Character.</param>
		/// <param name="movementData">Movement data.</param>
		override public Movement Init(Character character, MovementVariable[] movementData)
		{
			AssignReferences (character);
			if (movementData != null && movementData.Length >= MovementVariableCount)
			{
				shrinkHurtBox = movementData[ShrinkHurtBoxIndex].BoolValue;
				shrinkHurtBoxFactor = movementData[ShrinkHurtBoxFactorIndex].FloatValue;
				shrinkHeadAndSides = movementData[ShrinkHeadAndSidesIndex].BoolValue;
				newHeadHeight = movementData[NewHeadHeightIndex].FloatValue;
				actionButton =  movementData[ActionButtonIndex].IntValue;
				crouchToggle =  movementData[CrouchToggleIndex].BoolValue;
			}
			else
			{
				Debug.LogError("Invalid movement data.");
			}

			if (shrinkHurtBox)
			{
				CharacterHurtBox chb = character.GetComponentInChildren<CharacterHurtBox>();
				if (chb != null) hurtBoxCollider = chb.GetComponent<Collider2D>();
				if (hurtBoxCollider == null) Debug.LogError ("Crouch will try to shrink hurt box collider, but no hurt box collider could be found.");
			}

			if (shrinkHeadAndSides)
			{
				// Make sure the original head extent array is large enough to store each head collider height.
				// And initialise the new extents so we don't need to create anything later.
				int headCount = 0;
				for (int i = 0; i < character.Colliders.Length; i++)
				{
					if (character.Colliders[i].RaycastType == RaycastType.HEAD) headCount++;
				}
				originalHeadExtents = new Vector2[headCount];
				newHeadExtents = new Vector2[headCount];
				headCount = 0;
				for (int i = 0; i < character.Colliders.Length; i++)
				{
					if (character.Colliders[i].RaycastType == RaycastType.HEAD)
					{
						originalHeadExtents[headCount] = character.Colliders[i].Extent;
						newHeadExtents[headCount] = new Vector2(character.Colliders[i].Extent.x, newHeadHeight);
						headCount++;
					}
				}
			}

			return this;
		}

		/// <summary>
		/// Basic implementation for crouch, if the characters holds down and is on the ground they want to crouch.
		/// </summary>
		/// <returns><c>true</c> if control is wanted, <c>false</c> otherwise.</returns>
		override public bool WantsGroundControl()
		{
			if (!enabled) return false;
#if UNITY_EDITOR
			if (character.DefaultGroundMovement == this)
			{
				Debug.LogWarning("Its unlikely you want your crouch movement to be the default ground movement, this means you will crouch all the time. " +
					"To fix ensure you have another GroundMovement added and make this movement higher in the movement list.");
				return false;
			}
#endif
			if (character.Grounded && CheckInputAndToggle())
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Moves the character.
		/// </summary>
		override public void DoMove()
		{
			character.SetVelocityX(0);
		}

		/// <summary>
		/// Check crouching and crouch toggle
		/// </summary>
		/// <returns><c>true</c>, if crouch input was entered, <c>false</c> otherwise.</returns>
		virtual public bool CheckInputAndToggle()
		{
			bool input = CheckInput();
			if (crouchToggle)
			{
				if (crouchToggled && input)
				{
					crouchToggled = false;
				}
				else if (!crouchToggled && input)
				{
					crouchToggled = true;
				}
				return crouchToggled;
			}
			return input;
		}
		
        /// <summary>
        /// Check for crouch input.
        /// </summary>
        /// <returns><c>true</c>, if crouch input was entered, <c>false</c> otherwise.</returns>
        virtual public bool CheckInput()
		{
			if (crouchToggled)
			{
				if (actionButton == -1 && character.Input.VerticalAxisDigital == 1) return true;
				if (actionButton > -1 && character.Input.GetActionButtonState (actionButton) == ButtonState.HELD) return true;
				return false;
			}
			if (actionButton == -1 && character.Input.VerticalAxisDigital == -1) return true;
			if (actionButton > -1 && character.Input.GetActionButtonState (actionButton) == ButtonState.HELD) return true;
			return false;
		}
		
		/// <summary>
		/// Shrink the characters colliders.
		/// </summary>
		virtual public void Shrink()
		{
			if (shrinkHurtBox) ShrinkHurtBox();
			if (shrinkHeadAndSides) ShrinkHeadAndSides();
		}
		
		/// <summary>
		/// Grow the characters colliders back to original size.
		/// </summary>
		virtual public void Grow()
		{
			if (shrinkHurtBox) GrowHurtBox();
			if (shrinkHeadAndSides) GrowHeadAndSides();
		}


		#endregion

		#region protected methods

		/// <summary>
		/// Shrinks the hit box.
		/// </summary>
		virtual protected void ShrinkHurtBox()
		{
			if (hurtBoxCollider is BoxCollider2D)
			{
				float newHeight = ((BoxCollider2D)hurtBoxCollider).size.y / shrinkHurtBoxFactor;
				float difference = ((BoxCollider2D)hurtBoxCollider).size.y - newHeight;
#if UNITY_5_3_OR_NEWER
				((BoxCollider2D)hurtBoxCollider).offset = new Vector2(((BoxCollider2D)hurtBoxCollider).Offset().x, ((BoxCollider2D)hurtBoxCollider).Offset().y - (difference / 2.0f));
#else
				((BoxCollider2D)hurtBoxCollider).center = new Vector2(((BoxCollider2D)hurtBoxCollider).Offset().x, ((BoxCollider2D)hurtBoxCollider).Offset().y - (difference / 2.0f));
#endif
				((BoxCollider2D)hurtBoxCollider).size = new Vector2(((BoxCollider2D)hurtBoxCollider).size.x, newHeight);
			}
			else
			{
				// Not a box collider lets just scale it
				hurtBoxCollider.gameObject.transform.localScale = new Vector3(hurtBoxCollider.gameObject.transform.localScale.x, hurtBoxCollider.gameObject.transform.localScale.y / shrinkHurtBoxFactor, hurtBoxCollider.gameObject.transform.localScale.z);
			}
		}

		/// <summary>
		/// Grows the hit box.
		/// </summary>
		virtual protected void GrowHurtBox()
		{
			if (hurtBoxCollider is BoxCollider2D)
			{
				float newHeight = ((BoxCollider2D)hurtBoxCollider).size.y * shrinkHurtBoxFactor;
				float difference = newHeight - ((BoxCollider2D)hurtBoxCollider).size.y;
#if UNITY_5_3_OR_NEWER
				((BoxCollider2D)hurtBoxCollider).offset = new Vector2(((BoxCollider2D)hurtBoxCollider).Offset().x, ((BoxCollider2D)hurtBoxCollider).Offset().y + (difference / 2.0f));
#else
				((BoxCollider2D)hurtBoxCollider).center = new Vector2(((BoxCollider2D)hurtBoxCollider).Offset().x, ((BoxCollider2D)hurtBoxCollider).Offset().y + (difference / 2.0f));
#endif

				((BoxCollider2D)hurtBoxCollider).size = new Vector2(((BoxCollider2D)hurtBoxCollider).size.x, newHeight);
			}
			else
			{
				// Not a box collider lets just scale it
				hurtBoxCollider.gameObject.transform.localScale = new Vector3(hurtBoxCollider.gameObject.transform.localScale.x, hurtBoxCollider.gameObject.transform.localScale.y * shrinkHurtBoxFactor, hurtBoxCollider.gameObject.transform.localScale.z);
			}
		}

		/// <summary>
		/// Shrinks the head and sides.
		/// </summary>
		virtual protected void ShrinkHeadAndSides()
		{
			int headCount = 0;
			for (int i = 0; i < character.Colliders.Length; i++)
			{
				if (character.Colliders[i].RaycastType == RaycastType.HEAD)
				{
					character.Colliders[i].LookAhead = character.Colliders[i].Extent.y - newHeadExtents[headCount].y;
					character.Colliders[i].Extent = newHeadExtents[headCount];
					headCount++;
				}
				else if ((character.Colliders[i].RaycastType == RaycastType.SIDE_LEFT || character.Colliders[i].RaycastType == RaycastType.SIDE_RIGHT)
				         && character.Colliders[i].Extent.y > newHeadHeight)
				{
					character.Colliders[i].Disabled = true;
				}

			}
		}

		/// <summary>
		/// Grows the head and sides.
		/// </summary>
		virtual protected void GrowHeadAndSides()
		{
			int headCount = 0;
			for (int i = 0; i < character.Colliders.Length; i++)
			{
				if (character.Colliders[i].RaycastType == RaycastType.HEAD)
				{
					character.Colliders[i].Extent = originalHeadExtents[headCount];
					character.Colliders[i].LookAhead = 0;
					headCount++;
				}
				else if ((character.Colliders[i].RaycastType == RaycastType.SIDE_LEFT || character.Colliders[i].RaycastType == RaycastType.SIDE_RIGHT)
				         && character.Colliders[i].Extent.y > newHeadHeight)
				{
					character.Colliders[i].Disabled = false;
				}
			}
		}

		#endregion

		#if UNITY_EDITOR
		
		#region draw inspector
		
		/// <summary>
		/// Draws the inspector.
		/// </summary>
		public static MovementVariable[] DrawInspector(MovementVariable[] movementData, ref bool showDetails, Character target)
		{
			if (movementData != null && movementData.Length == MovementVariableCount - 1)
			{
				Debug.LogWarning("Upgrading Crouch movement data. Double check new values.");
				MovementVariable[] tmp = movementData;
				movementData = new MovementVariable[MovementVariableCount];
				System.Array.Copy(tmp, movementData, tmp.Length);
			}
			if (movementData == null || movementData.Length < MovementVariableCount)
			{
				movementData = new MovementVariable[MovementVariableCount];
			}

			// Input
			if (movementData [ActionButtonIndex] == null) movementData [ActionButtonIndex] = new MovementVariable (DefaultActionButton);
			bool useActionButton = EditorGUILayout.Toggle (new GUIContent ("Use ActionButton", "If true the character will crouch when action button is pressed, else they will crouch when DOWN is pressed."), movementData [ActionButtonIndex].IntValue >= 0);
			if (useActionButton) 
			{
				if (movementData [ActionButtonIndex].IntValue < 0) movementData [ActionButtonIndex].IntValue = 0;
				movementData [ActionButtonIndex].IntValue = EditorGUILayout.IntField (new GUIContent ("Action Button", "Index of the action button to use for crouch."), movementData [ActionButtonIndex].IntValue);
			}
			else
			{
				movementData [ActionButtonIndex].IntValue = -1;
			}

			// Crouch toggle
			if (movementData [CrouchToggleIndex] == null) movementData [CrouchToggleIndex] = new MovementVariable (false);
			movementData [CrouchToggleIndex].BoolValue = EditorGUILayout.Toggle (new GUIContent ("Crouch Toggle", "If true the crouch button will be a toogle on/off if false you will need to hold the button."), movementData [CrouchToggleIndex].BoolValue);

			showDetails = EditorGUILayout.Foldout(showDetails, "Advanced Settings");
			if (showDetails)
			{
				GUILayout.Label("Shrink Settings", EditorStyles.boldLabel);
				// Shrink hurt box
				if (movementData [ShrinkHurtBoxIndex] == null) movementData [ShrinkHurtBoxIndex] = new MovementVariable (true);
				movementData [ShrinkHurtBoxIndex].BoolValue = EditorGUILayout.Toggle (new GUIContent ("Shrink Hurt Box", "If true the characters hurt box will be made smaller when the chracter crouches."), movementData [ShrinkHurtBoxIndex].BoolValue);

				if (movementData [ShrinkHurtBoxIndex].BoolValue)
				{
					if (movementData [ShrinkHurtBoxFactorIndex] == null) movementData [ShrinkHurtBoxFactorIndex] = new MovementVariable (DefaultShrinkHurtBoxFactor);
					movementData [ShrinkHurtBoxFactorIndex].FloatValue = EditorGUILayout.FloatField (new GUIContent ("Hurt Box Shrink Factor", "Hurt box y size will be divided by this number."), movementData [ShrinkHurtBoxFactorIndex].FloatValue);
					if (movementData [ShrinkHurtBoxFactorIndex].FloatValue < 1) movementData [ShrinkHurtBoxFactorIndex].FloatValue = 1;
				}

				// Shrink head colliders box
				if (movementData [ShrinkHeadAndSidesIndex] == null) movementData [ShrinkHeadAndSidesIndex] = new MovementVariable (true);
				movementData [ShrinkHeadAndSidesIndex].BoolValue = EditorGUILayout.Toggle (new GUIContent ("Shrink Head and Sides", "If true the characters head colliders will be lowered and some side colliders will be Disabled."), movementData [ShrinkHeadAndSidesIndex].BoolValue);

				if (movementData [ShrinkHeadAndSidesIndex].BoolValue)
				{
					if (movementData [NewHeadHeightIndex] == null) movementData [NewHeadHeightIndex] = new MovementVariable (DefaultNewHeadHeight);
					movementData [NewHeadHeightIndex].FloatValue = EditorGUILayout.FloatField (new GUIContent ("New Head Height", "The new height for the top of the characters head colliders (relative to character transform). Side colliders higher than this will also be ignored."), movementData [NewHeadHeightIndex].FloatValue);
				}
			}
			
			EditorGUILayout.Space();
			
			
			return movementData;
		}
		
		#endregion
		
		#endif
	}
	
}


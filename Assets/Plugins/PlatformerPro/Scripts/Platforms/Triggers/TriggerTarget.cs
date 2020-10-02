using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Holds receiver data.
	/// </summary>
	[System.Serializable]
	public class TriggerTarget
	{	
		/// <summary>
		/// The GameObject that receives the triggers action.
		/// </summary>
		public GameObject receiver;
		
		/// <summary>
		/// The action that triggers when the character enters the trigger.
		/// </summary>
		public TriggerActionType enterAction;
		
		/// <summary>
		/// The action that triggers when the character leaves the proximity.
		/// </summary>
		public TriggerActionType leaveAction;

		/// <summary>
		/// Cached platform reference.
		/// </summary>
		[HideInInspector]
		public Platform platform;

		/// <summary>
		/// Cached camera zone reference.
		/// </summary>
		[HideInInspector]
		public CameraZone cameraZone;

		/// <summary>
		/// Sprite to use when swtiching sprites.
		/// </summary>
		public Sprite newSprite;

		
		/// <summary>
		/// Cached combiner reference.
		/// </summary>
		[HideInInspector]
		public TriggerCombiner combiner;

		/// <summary>
		/// If true this action only fires when the trigger is on. If false it will only fire when the trigger is off.
		/// </summary>
		[Tooltip("If true this action only fires when the trigger is on. If false it will only fire when the trigger is of. Ignored if this trigger is not a switch.")]
		public bool fireWhenOn;
	}

	/// <summary>
	/// What does the trigger do when triggered.
	/// </summary>
	public enum TriggerActionType
	{
		NOTHING							= 0,
		SEND_MESSAGE					= 1,
		ACTIVATE_PLATFORM				= 2,
		DEACTIVATE_PLATFORM				= 4,
		ACTIVATE_GAMEOBJECT				= 8,
		DEACTIVATE_GAMEOBJECT			= 16,
		CHANGE_CAMERA_ZONE				= 32,
		SWITCH_SPRITE					= 64,
		SHOW_DIALOG						= 128,
		HIDE_DIALOG						= 256,
		OPEN_DOOR						= 512,
		CLOSE_DOOR						= 1024,
		ACTIVATE_SPAWN_POINT			= 2048,
		PLAY_ANIMATION					= 4096,
		FORWARD							= 8192,
        SHOW_SHOP                       = 16384
	}

}
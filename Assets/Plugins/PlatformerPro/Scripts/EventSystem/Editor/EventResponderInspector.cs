#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.CodeDom;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlatformerPro.Dialog;
using PlatformerPro.Extras;

namespace PlatformerPro
{
	[CustomEditor(typeof(EventResponder), true)]
	public class EventResponderInspector : Editor
	{
		/// <summary>
		/// Cached and typed target reference.
		/// </summary>
		protected EventResponder myTarget;

		/// <summary>
		/// Cached types for target.
		/// </summary>
		protected string[] types;

		/// <summary>
		/// Cached events for type.
		/// </summary>
		protected string[] events;

		protected System.Type type;
		protected System.Reflection.EventInfo eventInfo;
		protected System.Type parameterType;

		public static readonly string[] switchOptions = {"ANY", "OFF", "ON"};

		public static readonly string[] CharacterEventTypes =  { typeof(Character).Name, typeof(CharacterHealth).Name, typeof(ItemManager).Name, typeof(EquipmentManager).Name, typeof(PowerUpManager).Name };
		
		/// <summary>
		/// Draw the GUI.
		/// Draw the GUI.
		/// </summary>
		public override void OnInspectorGUI()
		{
			myTarget = (EventResponder)target;

			PlatformerProMonoBehaviourInspector.DrawHeaderStatic(myTarget);
			Undo.RecordObject (target, "Event Update");
			myTarget.loadedCharacters = EditorGUILayout.Toggle(new GUIContent("Loaded Character Events", "Listen to events sent from the last character that get loaded in to the scene. Single player only."), myTarget.loadedCharacters);
			string typeName = null;
			if (!myTarget.loadedCharacters)
			{
				GameObject sender = (GameObject) EditorGUILayout.ObjectField(
					new GUIContent("Sender", "Add the Game Object that holds the target component."), myTarget.sender,
					typeof(GameObject), true);
				myTarget.sender = sender;
				if (myTarget.sender == null) myTarget.sender = myTarget.gameObject;
				if (myTarget.sender != null) types = GetComponentsOnGameObject(myTarget.sender);
				int typeIndex = System.Array.IndexOf(types, myTarget.typeName);
				if (typeIndex == -1 || typeIndex >= types.Length) typeIndex = 0;
				if (types != null && types.Length > 0)
				{	
					typeName = types[EditorGUILayout.Popup("Component", typeIndex, types)];
				}
				else
				{
					EditorGUILayout.HelpBox("No components found on this GameObject.", MessageType.Info);
				}
				myTarget.typeName = typeName;
			}
			else
			{
				myTarget.sender = null;
				types = CharacterEventTypes;
				int typeIndex = System.Array.IndexOf(types, myTarget.typeName);
				if (typeIndex == -1 || typeIndex >= types.Length) typeIndex = 0;
				if (types != null && types.Length > 0)
				{	
					typeName = types[EditorGUILayout.Popup("Component", typeIndex, types)];
				}
				else
				{
					// Shouldn't happen but can't hurt to leave it here
					EditorGUILayout.HelpBox("No components found on this GameObject.", MessageType.Info);
				}
				myTarget.typeName = typeName;
			}
			
			if (myTarget.sender != null || myTarget.loadedCharacters)
			{ 
				if (myTarget.typeName != null && myTarget.typeName.Length > 0)
				{
					events = GetEventNamesForType(myTarget.typeName);
					if (events != null && events.Length > 0)
					{
						int eventIndex = System.Array.IndexOf(events, myTarget.eventName);
						if (eventIndex == -1 || eventIndex >= events.Length) eventIndex = 0;
						string name = events[EditorGUILayout.Popup("Event", eventIndex, events)];
						myTarget.eventName = name;

						type = typeof(Character).Assembly.GetType ("PlatformerPro." + typeName);
						if (type == null) type = typeof(Character).Assembly.GetTypes().Where(t=>t.Name == typeName).FirstOrDefault();
						eventInfo = type.GetEvent(myTarget.eventName);
						parameterType = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters()[1].ParameterType;

						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
						// Dialog event
						if (parameterType != null && typeof(DialogEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("Event ID", "ID of the event sent by the dialog system"), myTarget.stringFilter);
						}
						// Enemy AI event
						if (parameterType != null && typeof(EnemyAIEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("Event ID", "ID of the event sent by the enemy AI"), myTarget.stringFilter);
						}
						// Trigger event
						if (parameterType != null && typeof(TriggerEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.intFilter = EditorGUILayout.Popup(new GUIContent("Switch State", "State the switch must be in if the trigger is a switch"), myTarget.intFilter, switchOptions );
						}
						// Animation event
						if (parameterType != null && typeof(AnimationEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.animationStateFilter = (AnimationState) EditorGUILayout.EnumPopup(new GUIContent("Animation State", "The animation state which will trigger this event response, use NONE for any state"),
							                          myTarget.animationStateFilter);
						}
						// Damage event
						if (parameterType != null && typeof(DamageInfoEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.damageTypeFilter = (DamageType) EditorGUILayout.EnumPopup(new GUIContent("Damage Type", "The damage type which will trigger this event response, use NONE for any type"),
							                                                                       myTarget.damageTypeFilter);
							if (myTarget.intFilter <= 0)
							{
								if (GUILayout.Button("Set Min Damage"))
								{
									myTarget.intFilter = 1;
								}
							}
							else
							{
								EditorGUILayout.BeginHorizontal();
								myTarget.intFilter = EditorGUILayout.IntField(new GUIContent("Min Damage", "The minimum damage required to trigger response."), myTarget.intFilter);
								if (GUILayout.Button("No Min Damage"))
								{
									myTarget.intFilter = 0;
								}
								EditorGUILayout.EndHorizontal();
							}
							if (myTarget.intFilter < 0) myTarget.intFilter = 0;
							if (myTarget.altIntFilter < 0)
							{
								if (GUILayout.Button("Set Max Damage"))
								{
									myTarget.altIntFilter = myTarget.intFilter;
								}
							}
							else
							{
								EditorGUILayout.BeginHorizontal();
								myTarget.altIntFilter = EditorGUILayout.IntField(new GUIContent("Max Damage", "The maximum damage able to trigger response."), myTarget.altIntFilter);
								if (myTarget.altIntFilter < myTarget.intFilter) myTarget.altIntFilter = myTarget.intFilter;
								if (GUILayout.Button("No Max Damage"))
								{
									myTarget.altIntFilter = -1;
								}
								EditorGUILayout.EndHorizontal();
							}
						}
						// Button event
						if (parameterType != null && typeof(ButtonEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.buttonStateFilter = (ButtonState) EditorGUILayout.EnumPopup(new GUIContent("Button State", "The button state which triggers this response, use ANY for any type"),
							                                                                   myTarget.buttonStateFilter);
						}
						// Phase event
						if (parameterType != null && typeof(SequenceEnemyPhaseEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("Phase", "Name of the phase or empty string for any phase."),
							                                                                myTarget.stringFilter);
						}
						// State event
						if (parameterType != null && typeof(StateEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("State", "Name of the state or empty string for any state."),
							                                                                myTarget.stringFilter);
						}
						// Attack event
						if (parameterType != null && typeof(AttackEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("Attack", "Name of the attack or empty string for any attack."),
							                                                  myTarget.stringFilter);
						}
						// Extra Damage event
						if (parameterType != null && typeof(ExtraDamageInfoEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("Attack", "Name of the attack or empty string for any attack."),
							                                                  myTarget.stringFilter);
						}
						// Item event
						if (parameterType != null && typeof(ItemEventArgs).IsAssignableFrom(parameterType))
						{
                            myTarget.stringFilter = ItemTypeAttributeDrawer.DrawItemTypeSelectorLayout(myTarget.stringFilter, new GUIContent("Item Type", "Name of the item type or empty for any item."), true);
							myTarget.intFilter = EditorGUILayout.IntField(new GUIContent("Amount", "Minimum amount that must be in the inventory."), myTarget.intFilter);
						}
						// Activation event
						if (parameterType != null && typeof(ActivationEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("Item", "Name of the Activation Item or empty string for any item."),
							                                                  myTarget.stringFilter);
						}
						
						// Respawn event
						if (parameterType != null && typeof(RespawnEventArgs).IsAssignableFrom(parameterType))
						{
							myTarget.stringFilter = EditorGUILayout.TextField(new GUIContent("Respawn point", "ID of the respawn point the character is spawning at"), myTarget.stringFilter);
						}
					
						// Healed event
						if (parameterType != null && typeof(HealedEventArgs).IsAssignableFrom(parameterType))
						{
							if (myTarget.intFilter <= 0)
							{
								if (GUILayout.Button("Set Min Heal"))
								{
									myTarget.intFilter = 1;
								}
							}
							else
							{
								EditorGUILayout.BeginHorizontal();
								myTarget.intFilter = EditorGUILayout.IntField(new GUIContent("Min heal", "The minimum amount of healing required to trigger response."), myTarget.intFilter);
								if (GUILayout.Button("No Min Heal"))
								{
									myTarget.intFilter = 0;
								}
								EditorGUILayout.EndHorizontal();
							}
							if (myTarget.intFilter < 0) myTarget.intFilter = 0;
							if (myTarget.altIntFilter < 0)
							{
								if (GUILayout.Button("Set Max Heal"))
								{
									myTarget.altIntFilter = myTarget.intFilter;
								}
							}
							else
							{
								EditorGUILayout.BeginHorizontal();
								myTarget.altIntFilter = EditorGUILayout.IntField(new GUIContent("Max Heal", "The maximum amount of healing that can trigger response."), myTarget.altIntFilter);
								if (myTarget.altIntFilter < myTarget.intFilter) myTarget.altIntFilter = myTarget.intFilter;
								if (GUILayout.Button("No Max Heal"))
								{
									myTarget.altIntFilter = -1;
								}
								EditorGUILayout.EndHorizontal();
							}
						}

						
						// Charge event args
						if (parameterType != null && parameterType.IsAssignableFrom(typeof(ChargeEventArgs)))
						{
							myTarget.intFilter = EditorGUILayout.IntField(new GUIContent("Charge Level", "Charge Level that the actions apply to. 0 for no filter."), myTarget.intFilter);
						}

						// Always show a game phase filter
						myTarget.gamePhaseFilter = (GamePhase) EditorGUILayout.EnumPopup(new GUIContent("Game Loading Phase", "The phase which will trigger this event response."), myTarget.gamePhaseFilter);
						
					}
					else
					{
						EditorGUILayout.HelpBox("No events found on this component.", MessageType.Info);
					}
				}
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
			if (myTarget.actions != null)
			{
				for (int i = 0; i < myTarget.actions.Length; i++)
				{
					
					EditorGUILayout.BeginVertical ("HelpBox");
		
					GUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace ();
					if (i == 0) GUI.enabled = false;
					if (GUILayout.Button ("Move Up", EditorStyles.miniButtonLeft))
					{
						EventResponse tmp = myTarget.actions[i-1];
						myTarget.actions[i-1] = myTarget.actions[i];
						myTarget.actions[i] = tmp;
						break;
					}
					GUI.enabled = true;
					if (i == myTarget.actions.Length - 1) GUI.enabled = false;
					if (GUILayout.Button ("Move Down", EditorStyles.miniButtonRight))
					{
						EventResponse tmp = myTarget.actions[i+1];
						myTarget.actions[i+1] = myTarget.actions[i];
						myTarget.actions[i] = tmp;
						break;
					}
					GUI.enabled = true;
					// Remove
					GUILayout.Space(4);
					bool removed = false;
					if (GUILayout.Button("Remove", EditorStyles.miniButton))
					{
						myTarget.actions = myTarget.actions.Where (a=>a != myTarget.actions[i]).ToArray();
						removed = true;
					}
					GUILayout.EndHorizontal ();
					if (!removed) RenderAction(myTarget, myTarget.actions[i]);
					EditorGUILayout.EndVertical();
				}
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			// Add new actions
			if (GUILayout.Button("Add Action"))
			{
				if (myTarget.actions == null)
				{
					myTarget.actions = new EventResponse[1];
				}
				else
				{
					// Copy and grow array
					EventResponse[] tmpActions = myTarget.actions;
					myTarget.actions = new EventResponse[tmpActions.Length + 1];
					System.Array.Copy(tmpActions, myTarget.actions, tmpActions.Length);
				}
			}
			EditorGUILayout.EndHorizontal();

		}
	
		/// <summary>
		/// Draws an event response action in the inspector.
		/// </summary>
		/// <param name="action">Action.</param>
		public static void RenderAction(object target, EventResponse action)
		{
			if (!(target is EventResponder || target is PowerUpManager || target is EventResponseNode)) 
			{
				Debug.LogWarning ("Unexpected type passed to RenderAction()");
				return;
			}

			bool allowSceneObjects = ( target is EventResponseNode) ? false : true;
			
			if (action == null) action = new EventResponse();
			// TODO No need to create this every update
			GUIContent[] popUps = new GUIContent[System.Enum.GetValues(typeof(EventResponseType)).Length];
			
            int i = 0;
			foreach (object t in System.Enum.GetValues(typeof(EventResponseType)))
			{
				popUps[i] = new GUIContent(((EventResponseType)t).GetName(), "");
				i++;
			}
			
			int actionIndex = (int)action.responseType;
			actionIndex = EditorGUILayout.Popup( new GUIContent("Action Type", "The type of action to do when this event occurs."), actionIndex, popUps);
			action.responseType = (EventResponseType) actionIndex;

			if (!allowSceneObjects)
			{
				if (action.responseType == EventResponseType.PLAY_SFX ||
				    action.responseType == EventResponseType.START_EFFECT  ||
					action.responseType == EventResponseType.PLAY_PARTICLES ||
					action.responseType == EventResponseType.PAUSE_PARTICLES ||
				    action.responseType == EventResponseType.ACTIVATE_GAMEOBJECT  ||
				    action.responseType == EventResponseType.ACTIVATE_PLATFORM  ||
				    action.responseType == EventResponseType.ENABLE_BEHAVIOUR  ||
				    action.responseType == EventResponseType.ACTIVATE_ITEM ||
				    action.responseType == EventResponseType.DEACTIVATE_GAMEOBJECT  ||
				    action.responseType == EventResponseType.DEACTIVATE_PLATFORM  ||
				    action.responseType == EventResponseType.DISABLE_BEHAVIOUR  ||
				    action.responseType == EventResponseType.DEACTIVATE_ITEM ||
				    action.responseType == EventResponseType.SPAWN_ITEM ||
				    action.responseType == EventResponseType.SHOW_DIALOG
				)
				{
					EditorGUILayout.HelpBox("This event type cannot be sent from a Dialog. Instead you send a DialogEvent and listen for that event using an EventResponder in your scene.",  MessageType.Error);
					return;
				}
			}

			// Delay
			action.delay = EditorGUILayout.FloatField( new GUIContent("Action Delay", "how long to wait before doing the action."), action.delay);
			if (action.delay < 0.0f) action.delay = 0.0f;
			else if (action.delay > 0.0f) EditorGUILayout.HelpBox("If you use many events with delay you may notice some garbage collection issues on mobile devices", MessageType.Info);

			// Dialog Object
			if (action.responseType == EventResponseType.SHOW_DIALOG)
			{
				action.targetScriptableObject = (ScriptableObject) EditorGUILayout.ObjectField(new GUIContent("Dialog", "The dialog to show"), action.targetScriptableObject, typeof(DialogGraph), false);
				action.targetComponent = (Component) GetObjectReference(typeof(DialogSystem), action, allowSceneObjects, "Dialog System", "The dialog system to use, will use the default if empty.", false, true);
			}
			    
			// Camera Zone
			if (action.responseType == EventResponseType.SWITCH_CAMERA_ZONE)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(CameraZone), action, allowSceneObjects, "Camera Zone", "Camera Zone to switch to", false, true);
			}

			// Game Object
			if (action.responseType == EventResponseType.ACTIVATE_GAMEOBJECT ||
			    action.responseType == EventResponseType.DEACTIVATE_GAMEOBJECT ||
			    action.responseType == EventResponseType.SEND_MESSSAGE)
			{
				action.targetGameObject = (GameObject) GetObjectReference(typeof(GameObject), action, allowSceneObjects, "Game Object", "The game object that will be acted on", true);
			}

			// Component
			if (action.responseType == EventResponseType.ENABLE_BEHAVIOUR ||
			    action.responseType == EventResponseType.DISABLE_BEHAVIOUR)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(Component), action, allowSceneObjects, "Behaviour", "The behaviour will be acted on", true, true);
			}

			// Particle system
			if (action.responseType == EventResponseType.PLAY_PARTICLES ||
			    action.responseType == EventResponseType.PAUSE_PARTICLES)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(ParticleSystem), action, allowSceneObjects, "Particle System", "The particle system that will be acted on", true, true);
			}

			// Send message
			if (action.responseType == EventResponseType.SEND_MESSSAGE) action.message = EditorGUILayout.TextField(new GUIContent("Message", "The message to send via send message"), action.message);

			// Animation Override
			if (action.responseType == EventResponseType.OVERRIDE_ANIMATON)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(Character), action, allowSceneObjects, "Character", "Character to update.", true, true);
				action.overrideState = EditorGUILayout.TextField(new GUIContent("Override State", "The name of the override state."), action.overrideState);
			}

			// Clear Animation Override
			if (action.responseType == EventResponseType.CLEAR_ANIMATION_OVERRIDE)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(Character), action, allowSceneObjects, "Character", "Character to update.", true, true);
				action.overrideState = EditorGUILayout.TextField(new GUIContent("Override State", "The name of the override state to clear."), action.overrideState);
			}

			// Force Animation
			if (action.responseType == EventResponseType.SPECIAL_MOVE_ANIMATION)			
			{
				EditorGUILayout.HelpBox ("This type has been deprectaed use PLAY_ANIMATION with a Character as your target instead.", MessageType.Warning);
				action.targetComponent = (Component) GetObjectReference(typeof(Character), action, allowSceneObjects, "Character", "Character to update.", true, true);
				action.animationState = (AnimationState) EditorGUILayout.EnumPopup(new GUIContent("Animation State", "The name of the override state."), action.animationState);
			}

			// Sprite
			if (action.responseType == EventResponseType.SWITCH_SPRITE)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(SpriteRenderer), action, allowSceneObjects, "Sprite Renderer", "SpriteRenderer to update.", true, true);
				action.newSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("New Sprite", "Sprite to switch in."), action.newSprite , typeof(Sprite), true);
			}

			// SFX
			if (action.responseType == EventResponseType.PLAY_SFX)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(SoundEffect), action, allowSceneObjects, "Sound Effect", "The sound effect to play.", true, true);
			}

			// MUSIC PLAYER
			if (action.responseType == EventResponseType.PLAY_SONG ||
			    action.responseType == EventResponseType.STOP_SONG)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(MusicPlayer), action, allowSceneObjects, "Music Player", "The music player to use.", false, true);
				if (action.responseType == EventResponseType.PLAY_SONG)
				{
					action.message = EditorGUILayout.TextField(new GUIContent("Song Name", "The name of the song to play."), action.message);
				}
			}

			
			// Vulnerable/Invulnerable
			if (action.responseType == EventResponseType.MAKE_VULNERABLE ||
			    action.responseType == EventResponseType.MAKE_INVULNERABLE)
			{
				if (action.targetComponent is CharacterHealth)
				{
					action.targetComponent = action.targetComponent.gameObject.GetComponentInParent(typeof(IMob));
				}
				action.targetComponent = (Component) GetObjectReference(typeof(IMob), action, allowSceneObjects, "Character", "The character or enemy that will be acted on", true, true);
			}

			// Load level
			if (action.responseType == EventResponseType.LOAD_SCENE) {
				action.message = EditorGUILayout.TextField(new GUIContent("Scene Name", "The name of the scene to load (make sure its added to the build settings)."), action.message);
			}

			// Load level
			if (action.responseType == EventResponseType.LOCK ||
			    action.responseType == EventResponseType.UNLOCK) {
				action.message = EditorGUILayout.TextField(new GUIContent("Lock Name", "The name of the lock (often a level name but it doesn't have to be)."), action.message);
			}

			// Respawn
			if (action.responseType == EventResponseType.RESPAWN ||
			    action.responseType == EventResponseType.SET_ACTIVE_RESPAWN ) {
				action.message = EditorGUILayout.TextField(new GUIContent("Respawn Point Name", "The name of the respawn point to respawn at, leave blank for whatever is currently active."), action.message);
			}

			// Effects
			if (action.responseType == EventResponseType.START_EFFECT)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(FX_Base), action, allowSceneObjects, "Effect", "The effect that will be played.", true, true);
				action.targetGameObject = (GameObject) GetObjectReference(typeof(GameObject), action, allowSceneObjects, "Callback Object", "The game object that will be called back when the effect is finished", true);
				if (action.targetComponent != null && action.targetGameObject != null)
				{
					action.message = EditorGUILayout.TextField(new GUIContent("Callback Message", "The name message to send on call back."), action.message);
					EditorGUILayout.HelpBox("Note that many effects do not support call backs.", MessageType.Info);
				}
			}

			// Animations
			if (action.responseType == EventResponseType.PLAY_ANIMATION ||
			    action.responseType == EventResponseType.STOP_ANIMATION)
			{
				action.targetGameObject = (GameObject) GetObjectReference(typeof(GameObject), action, allowSceneObjects, "Character/Animator", "GameObject holding a Character, Animation or Animator to play or stop.", true);
			}
			
			// Animation state
			if (action.responseType == EventResponseType.PLAY_ANIMATION)
			{
				if (action.targetGameObject != null) {
					Character character = action.targetGameObject.GetComponent<Character>();
					SpecialMovement specialMovement = action.targetGameObject.GetComponentInChildren<SpecialMovement>();
					if (specialMovement != null)
					{
						action.animationState = (AnimationState)EditorGUILayout.EnumPopup (new GUIContent ("Animation State", "The state to play."), action.animationState);
					}
					else
					{
						if (character != null)
						{
							EditorGUILayout.HelpBox("If using a Character it is recommended that you add a SpecialMovement_PlayAnimation to handle playing animations.", MessageType.Warning);
						}
						Animator animator = action.targetGameObject.GetComponent<Animator> ();
						if (animator != null)
						{
							action.message = EditorGUILayout.TextField (new GUIContent ("Animation State", "Name of the Animation to play."), action.message);
						}
						else
						{
							Animation animation = action.targetGameObject.GetComponent<Animation>();
							if (animation != null)
							{
								action.message = EditorGUILayout.TextField( new GUIContent("Animation Clip", "Name of the Animation Clip to play."), action.message);
							}
						}
					}
				}
				else
				{
					// Assume character will be passed in event
					action.animationState = (AnimationState)EditorGUILayout.EnumPopup (new GUIContent ("Animation State", "The state to play."), action.animationState);
				}
			}
			
			// Animation state
			if (action.responseType == EventResponseType.STOP_ANIMATION)
			{
				SpecialMovement specialMovement = action.targetGameObject == null ? null : action.targetGameObject.GetComponentInChildren<SpecialMovement>();
				Animator animator = action.targetGameObject == null ? null : action.targetGameObject.GetComponent<Animator>();
				if (specialMovement == null && animator != null)
				{
					EditorGUILayout.HelpBox("You cannot stop an Animator only an Animation or Character Special Movement. Instead use PLAY_ANIMATION and provide an IDLE or DEFAULT state", MessageType.Warning);
				}
			}

			// Scores
			if (action.responseType == EventResponseType.ADD_SCORE ||
			    action.responseType == EventResponseType.RESET_SCORE) 
			{
				action.message = EditorGUILayout.TextField(new GUIContent("Score Type", "ID string for the score type."), action.message);
			}
			if (action.responseType == EventResponseType.ADD_SCORE)
		    {
				action.intValue = EditorGUILayout.IntField(new GUIContent("Amount", "How much score to add."), action.intValue);
			}

			// Max health/lives
			if (action.responseType == EventResponseType.UPDATE_MAX_HEALTH ||
			    action.responseType == EventResponseType.SET_MAX_HEALTH ||
			    action.responseType == EventResponseType.UPDATE_MAX_LIVES ||
			    action.responseType == EventResponseType.SET_MAX_LIVES ||
			    action.responseType == EventResponseType.ADD_LIVES ||
			    action.responseType == EventResponseType.HEAL ||
			    action.responseType == EventResponseType.DAMAGE ||
			    action.responseType == EventResponseType.KILL) {
				if (action.targetComponent is CharacterHealth)
				{
					action.targetComponent = action.targetComponent.gameObject.GetComponentInParent(typeof(IMob));
				}
				action.targetComponent = (Component) GetObjectReference(typeof(Character), action, allowSceneObjects, "Character", "The character that will be acted on", true, true);
			}
				

			// Update Max health/lives/item max
			if (action.responseType == EventResponseType.UPDATE_MAX_HEALTH ||
			    action.responseType == EventResponseType.UPDATE_MAX_LIVES ||
			    action.responseType == EventResponseType.ADD_LIVES) {
				action.intValue = EditorGUILayout.IntField(new GUIContent("Amount", "How much to add or remove."), action.intValue);
			}

			// Heal
			if (action.responseType == EventResponseType.HEAL) {
				action.intValue = EditorGUILayout.IntField(new GUIContent("Amount", "How much to heal."), action.intValue);
			}

			// Damage
			if (action.responseType == EventResponseType.DAMAGE) {
				action.intValue = EditorGUILayout.IntField(new GUIContent("Amount", "How much damage to cause."), action.intValue);
				action.damageType = (DamageType) EditorGUILayout.EnumPopup(new GUIContent("Damage Type", "Type of damage."), (DamageType) action.damageType);
			}

			// Set Max health/lives/item max
			if (action.responseType == EventResponseType.SET_MAX_HEALTH ||
			    action.responseType == EventResponseType.SET_MAX_LIVES) {
				action.intValue = EditorGUILayout.IntField(new GUIContent("New Value", "The new value."), action.intValue);
			}

			// Named properties
			if (action.responseType == EventResponseType.SET_TAGGED_PROPERTY ||
			    action.responseType == EventResponseType.ADD_TO_TAGGED_PROPERTY ||
			    action.responseType == EventResponseType.MULTIPLY_TAGGED_PROPERTY)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(Character), action, allowSceneObjects, "Character", "Character to set property for.", true, true);
				action.message = EditorGUILayout.TextField(new GUIContent("Property", "Named property type."), action.message);
				action.floatValue = EditorGUILayout.FloatField(new GUIContent("New Value", "The new value (float)."), action.floatValue);
			}

			if (action.responseType == EventResponseType.SET_TAGGED_PROPERTY)
			{
				EditorGUILayout.HelpBox("For booleans use 0 for false, anything else for true.", MessageType.Info);
			}

			// Spawn item
			if (action.responseType == EventResponseType.SPAWN_ITEM)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(Component), action, allowSceneObjects, "Item Spawner", "Reference to the Spawner or RandomItemSpawner", true, true);
				if (action.targetComponent != null)
				{
					if (!(action.targetComponent is RandomItemSpawner) && !(action.targetComponent is Spawner))
					{
						Component possibleMatch = action.targetComponent.gameObject.GetComponent<RandomItemSpawner>();
						if (possibleMatch == null) possibleMatch = action.targetComponent.gameObject.GetComponent<Spawner>();
						action.targetComponent = possibleMatch;
					}
				}
			}

			// Velocity
			if (action.responseType == EventResponseType.ADD_VELOCITY ||
			    action.responseType == EventResponseType.SET_VELOCITY) 
			{
				action.targetComponent = (Component) GetObjectReference(typeof(Component), action, allowSceneObjects, "Reciever", "The behaviour that will have velocity added or set", true, true);
				if (allowSceneObjects && !(action.targetComponent is IMob || action.targetComponent is Rigidbody2D))
				{
					EditorGUILayout.HelpBox("Component must be a Character, Enemy or Rigidbody 2D. To drag specific components try locking an inspector window.", MessageType.Warning);
				}
				action.vectorValue = EditorGUILayout.Vector2Field(new GUIContent("Velocity", "Velocity to add or set."), action.vectorValue);
				action.boolValue = EditorGUILayout.Toggle(new GUIContent("Velocity is Relative", "Is velocity relative to facing direction or rigidbody rotation?"), action.boolValue);
                action.floatValue = EditorGUILayout.FloatField(new GUIContent("Fall Off", "If non-zero reduce the modifier based on distance from this transform multiplied by this falloff value"), action.floatValue);
            }

			// Depth
			if (action.responseType == EventResponseType.SET_DEPTH) 
			{
				action.intValue = EditorGUILayout.IntField(new GUIContent("Depth", "Depth to set"), action.intValue);
			}

			// Power-Up
			if (action.responseType == EventResponseType.POWER_UP)
			{
				action.message = EditorGUILayout.TextField(new GUIContent("Power-Up Type", "The name of the power up to apply."), action.message);
			}

			// items
			if (action.responseType == EventResponseType.COLLECT_ITEM ||
				action.responseType == EventResponseType.CONSUME_ITEM)
			{
				action.message = EditorGUILayout.TextField(new GUIContent("Item", "The name of the item."), action.message);
				action.intValue = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of item to collect/consume."), action.intValue);
			}

			// Activation groups
			if (action.responseType == EventResponseType.ACTIVATE_ITEM ||
			    action.responseType == EventResponseType.DEACTIVATE_ITEM)
			{
				action.targetComponent = (Component) GetObjectReference(typeof(ActivationGroup), action, allowSceneObjects, "Activation Group", "Activation Group to use, if empty we will try to find an ActivationGroup on the Character triggering event.", false, true);
				action.message = EditorGUILayout.TextField(new GUIContent("Activation Item", "The id of the activation item."), action.message);
			} 
			
			// Platforms
			if (action.responseType == EventResponseType.ACTIVATE_PLATFORM ||
			    action.responseType == EventResponseType.DEACTIVATE_PLATFORM)
			{
				if (!(action.targetComponent is Platform)) action.targetComponent = null;
				action.targetComponent = (Component) GetObjectReference(typeof(Platform), action, allowSceneObjects, "Platform", "Reference to the Platform", false, true);
			}
			
			// Enemy Message
			if (action.responseType == EventResponseType.ENEMY_MESSAGE)
			{
				if (!(action.targetComponent is EnemyAI)) action.targetComponent = null;
				action.targetComponent = (Component) GetObjectReference(typeof(EnemyAI), action, allowSceneObjects, "Enemy AI", "Reference to the Enemy AI", false, true);
				action.message = EditorGUILayout.TextField(new GUIContent("Message", "The message to send to the enemy AI"), action.message);
			}
			
			// Set Character
            if (action.responseType == EventResponseType.SET_CHARACTER)
            {
	            action.targetComponent = (Component) GetObjectReference(typeof(Component), action, allowSceneObjects, "Character Ref", "Character ref update.", true, true);
	            if (!(action.targetComponent is ICharacterReference))
                {
                    if (action.targetComponent != null)
                    {
                        // Try to find right component
                        GameObject go = action.targetComponent.gameObject;
                        action.targetComponent = go.GetComponent<CharacterHurtBox>();
                        if (action.targetComponent == null)
                        {
                            action.targetComponent = go.GetComponent<CharacterHitBox>();
                        }
                        if (action.targetComponent == null)
                        {
                            action.targetComponent = go.GetComponent(typeof(ICharacterReference));
                        }
                    }
                }
            }

        }

		private static UnityEngine.Object GetObjectReference(Type type, EventResponse action, bool allowSceneObjects, String label, String message, bool onCharacterOnly, bool isComponent = false)
		{
			if (!allowSceneObjects)
			{
				if (onCharacterOnly)
				{
					// Special cases
					if (action.responseType == EventResponseType.PLAY_ANIMATION ||
					    action.responseType == EventResponseType.STOP_ANIMATION ||
					    action.responseType == EventResponseType.ADD_VELOCITY ||
					    action.responseType == EventResponseType.SET_VELOCITY)
					{
						EditorGUILayout.HelpBox("This event will fire on the Character that initiated the dialog. If you wish to operate on a scene object create a DialogEvent then " +
						                        "listen to that DialogEvent with an EventResponder in the scene.", MessageType.Info);
						return null;
					}
					// Character case
					if (type == typeof(Character) || type == typeof(IMob))
					{
						return null;
					}
					
					// Others (show warning)
					EditorGUILayout.HelpBox("Dialog events can't fire on scene objects other than the Character. If you wish to operate on a scene object create a DialogEvent then " +
						"listen to that DialogEvent with an EventResponder in the scene.", MessageType.Warning);
					return null;
				}
				// Special cases that aren't on character
				if (action.responseType == EventResponseType.PLAY_SONG ||  action.responseType == EventResponseType.STOP_SONG) return null;
			}

			if (isComponent)
			{
				return EditorGUILayout.ObjectField(new GUIContent(label, message), action.targetComponent, type, allowSceneObjects);
			}
			return EditorGUILayout.ObjectField(new GUIContent(label, message), (action.targetGameObject) , type, allowSceneObjects);
		}

		/// <summary>
        /// Get the names of all events for a given type.
        /// </summary>
        /// <returns>The event names for type.</returns>
        /// <param name="type">Type.</param>
        protected string[] GetEventNamesForType(string typeName)
		{
			System.Type type = typeof(Character).Assembly.GetType ("PlatformerPro." + typeName);
			if (type == null) type = typeof(Character).Assembly.GetTypes().Where(t=>t.Name == typeName).FirstOrDefault();
			if (type == null) return new string[0];
			System.Reflection.EventInfo[] events = type.GetEvents (System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (events == null) return new string[0];
			return type.GetEvents(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Select(e=>e.Name).OrderBy(s=>s).ToArray();
		}

		/// <summary>
		/// Gets the type names of all components on a game object.
		/// </summary>
		/// <returns>The Components on game object.</returns>
		/// <param name="go">Go.</param>
		protected string[] GetComponentsOnGameObject(GameObject go)
		{
			return go.GetComponents(typeof(Component)).Select (c=>c.GetType().Name).OrderBy(s=>s).ToArray();
		}
	}

}
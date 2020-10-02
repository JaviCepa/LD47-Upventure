using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PlatformerPro.AI;
using PlatformerPro.AI.Actions;
using PlatformerPro.AI.Interrupts;
using XNode;
using Random = UnityEngine.Random;

namespace PlatformerPro
{

	/// <summary>
	/// Enemy AI which is driven by a user defined behaviour graph.
	/// </summary>
	public class EnemyAI_AIGraph : EnemyAI
	{

		override public string Header => "Enemy AI which is driven by a user defined behaviour graph.";

		/// <summary>
		/// The graphs to use to control the enemies behaviour. The first entry is the starting phase.
		/// </summary>
		[Tooltip ("The graphs to use to control the enemies behaviour. The first entry in the starting phase")]
		public List<EnemyNamedAIGraphs> graphs;
		
		/// <summary>
		/// Distance at which the target is "lost".
		/// </summary>
		[Header("Target Finding")]
		[Tooltip ("Distance at which the target is 'lost'.")]
		public float maxTargetDistance;
		
		/// <summary>
		/// If true we always face the target if we have one.
		/// </summary>
		[Tooltip ("If true we always face the target if we have one.")]
		public bool alwaysFaceTarget;

		/// <summary>
		/// Leeway required before we change facing direction when facing target.
		/// </summary>
		[Tooltip("Leeway required before we change facing direction when facing target.")]
		[DontShowWhen("alwaysFaceTarget",true)]
		public float faceTargetLeeway = 0.25f;
		
		/// <summary>
		/// How far can the enemy see?
		/// </summary>
		[Header("Sight")]
		[Tooltip ("Range of the sight sense. Set to 0 to ignore.")]
		public float sightDistance = 5.0f;
		
		/// <summary>
		/// Y position of the characters 'eyes'.
		/// </summary>
		[DontShowWhenZero ("sightDistance")]
		public float sightYOffset;
		
		/// <summary>
		/// Layers to check for obstacle and characters.
		/// </summary>
		[DontShowWhenZero ("sightDistance")] 
		public LayerMask sightLayers;
		
		/// <summary>
		/// Range of the proximity sense.
		/// </summary>
		[Header ("Hearing")]
		[Tooltip ("Range of the hearing/proximity sense. Set to 0 to ignore.")]
		public float hearingRadius = 1.0f;
		
		/// <summary>
		/// Centre point of the proximity sense.
		/// </summary>
		[Tooltip ("Centre point of the hearing/proximity sense.")]
		[DontShowWhenZero ("hearingRadius")]
		public Vector2 hearingOffset;

		/// <summary>
		/// Layers to check for proximity sense.
		/// </summary>
		[Tooltip ("Layers to check for the hearing/proximity sense.")]
		[DontShowWhenZero ("hearingRadius")]
		public LayerMask hearingLayers;
		
		#region protected members
		/// <summary>
		/// Where are we in the enemy graph?
		/// </summary>
		protected EnemyNode currentNode;
		
		/// <summary>
		/// Track if we have run the check for player call so we don't eat resources by running it more than once a frame.
		/// </summary>
		protected bool hasRunCheckForCharacter;
		
		/// <summary>
		/// Result of the last check for character.
		/// </summary>
		protected bool checkForCharacterResult;
		
		/// <summary>
		/// Stores how long we have been in current state. If the same state is played multiple times this tracks the time 
		/// in the CURRENT iteration.
		/// </summary>
		protected float stateTimer;
		
		/// <summary>
		/// Stores how long we have been in current state. If the same state is played multiple times this tracks the time 
		/// in the CURRENT iteration.
		/// </summary>
		protected float graphTimer;
		
		/// <summary>
		/// Stores the completion state of current move.
		/// </summary>
		protected bool stateMovementComplete;
		
		/// <summary>
		/// Stores the number of times we have been damaged while in current state.
		/// </summary>
		protected int hitCount;

		/// <summary>
		/// Cached transform reference.
		/// </summary>
		protected Transform myTransform;
		
		/// <summary>
		/// Used to store results of hearing overlap call.
		/// </summary>
		protected Collider2D[] proximityColliders;

		/// <summary>
		/// Cached reference to the damage handler.
		/// </summary>
		protected EnemyInterruptDamagedNode damageInterrupt;

		/// <summary>
		/// Cached reference to the counter handlers.
		/// </summary>
		protected EnemyInterruptCounter[] counterInterrupts;
		
		/// <summary>
		/// Cached reference to the timer handlers.
		/// </summary>
		protected EnemyInterruptTimer[] timerInterrupts;
		
		/// <summary>
		/// Cached reference to the health interrupt handlers.
		/// </summary>
		protected EnemyInterruptHealth[] healthInterrupts;
				
		/// <summary>
		/// Cached reference to the message interrupt handlers.
		/// </summary>
		protected EnemyInterruptEnemyMessage[] messageInterrupts;
		
		/// <summary>
		/// Timers for each timer handler.
		/// </summary>
		protected float[] timers;
		
		/// <summary>
		/// If the AI is locked we can't change state. Generally used to wait for a movement to complete.
		/// </summary>
		protected bool locked;

		/// <summary>
		/// Associated graph name to graph.
		/// </summary>
		protected Dictionary<string, EnemyAIGraph> graphLookup;

		/// <summary>
		/// Currently active graph
		/// </summary>
		protected EnemyAIGraph currentGraph;
		
		/// <summary>
		/// Track if anything has already made us transition this frame. If so we can't transition again.
		/// </summary>
		protected bool hasTransitionedThisFrame;
		
		/// <summary>
		/// Track if an interrupt has already made us transition this frame. If so we can't transition again.
		/// </summary>
		protected bool hasHadInterruptThisFrame;

		/// <summary>
		/// Track if we have run our decision logic already. If so we wont run it again. Instead just return the current state.
		/// </summary>
		protected bool hasDecided;
		
		#endregion
		
		/// <summary>
		/// The max colliders for statically allocated colliders.
		/// </summary>
		private const int MaxColliders = 8;
		
		#region public properties 

		/// <summary>
		/// A generic counter you can use to control loops etc.
		/// </summary>
		protected int counter;
		virtual public int Counter
		{
			get => counter;
			set
			{
				counter = value;
				for (int i = 0; i < counterInterrupts.Length; i++)
				{
					if (counter == counterInterrupts[i].triggerValue || counterInterrupts[i].triggerWhenOver && counter > counterInterrupts[i].triggerValue)
					{
						if (!hasHadInterruptThisFrame)
						{
							// Interrupts beat transtions TODO: Is there special case for conflicting interrupts needed
							hasTransitionedThisFrame = false;
							if (HandleExit(counterInterrupts[i], 0))
							{
								hasHadInterruptThisFrame = true;
							}
						}
					}	
				}
			}
		}
		
		#endregion
		#region Unity hooks
		
		void Update()
		{
			DoUpdate();
		}
		
		void OnDestroy()
		{
			if (enemy != null)
			{
				enemy.Damaged -= HandleEnemyDamaged;
				enemy.MovementHasCompleted -= HandleMoveCompleted;
			}
		}
		#endregion

		#region public methods
		
		override public void Init(Enemy enemy)
		{
			base.Init (enemy);
			myTransform = transform;
			proximityColliders = new Collider2D[MaxColliders];
			if (graphs.Count == 0 || graphs[0].graph == null)
			{
				Debug.LogWarning("EnemyAI_AIGraph has no graphs defined. Disabling");
				enemy.gameObject.SetActive(false);
				return;
			}
			enemy.MovementHasCompleted += HandleMoveCompleted;
			enemy.Damaged += HandleEnemyDamaged;
			graphLookup = new Dictionary<string, EnemyAIGraph>();
			foreach (EnemyNamedAIGraphs g in graphs)
			{
				graphLookup.Add(g.name, g.graph);
			}
			SwitchPhase(graphs[0].graph);
		}
		
		/// <summary>
		/// Decide the next move
		/// </summary>
		override public EnemyState Decide()
		{
			// Don't decide on state more than once
			if (hasDecided) return currentNode.enemyState;
			hasDecided = true;			
			CheckForCharacter();
			if (currentNode == null) return EnemyState.DEFAULT;
			if (locked)
			{
				return currentNode.enemyState;
			}
			ProcessCurrentNode();
			return currentNode.enemyState;
		}

		/// <summary>
		/// Sense the character.
		/// </summary>
		override public bool Sense()
		{
			bool hasTarget = enemy.CurrentTarget != null;
			bool result = CheckForCharacter();
			return result != hasTarget;
		}
		
#if UNITY_EDITOR
		/// <summary>
		/// Gets a list of all enemyStates used in this movement. Intended for editor use.
		/// </summary>
		override public EnemyState[] Info
		{
			get
			{
				List<EnemyState> states = new List<EnemyState>();
				if (graphs == null || graphs.Count == 0) return states.ToArray();
				foreach (EnemyAIGraph g in graphs.Select(n=>n.graph))
				{
					foreach (Node n in g.nodes)
					{
						if (n is EnemyNode node)
						{
							states.Add(node.enemyState);
						}
					}
				}
				return states.Distinct().ToArray();
			}
		}
#endif
		
		/// <summary>
		/// Handle damage.
		/// </summary>
		/// <param name="info">Damage details</param>
		/// <returns>The updated damage info.</returns>
		override public DamageInfo DoDamage(DamageInfo info)
		{
			if (damageInterrupt == null) return info;
			int match = damageInterrupt.MatchOption(enemy.State, info);
			if (match == -1) return info;
			List<NodePort> connections = damageInterrupt.GetOutputForSelection(match)?.GetConnections();
			if (connections == null) return info;
			DamageInfo updatedInfo = info;
			// Do Damage filters
			foreach (NodePort n in connections)
			{
				if (n.node is EnemyDamageFilter filter)
				{
					updatedInfo = filter.FilterDamage(updatedInfo);
				}
			}
			if (!hasHadInterruptThisFrame)
			{
				// Interrupts beat transtions TODO: Is there special case for conflicting interrupts needed
				hasTransitionedThisFrame = false;
				if (HandleExit(damageInterrupt, match))
				{
					hasHadInterruptThisFrame = true;
				}
			}
			else
			{
				HandleActions(damageInterrupt, match);
			}

			return updatedInfo;
		}

		/// <summary>
		/// Called to tell the AI that we were able to handle the desired state.
		/// </summary>
		override public void StateTransitioned()
		{
			locked = false;
		}
		
		/// <summary>
		/// Called to tell the AI that we were not able to handle the desired state. Usually due to a movement that wont release control.
		/// </summary>
		override public void StateNotTransitioned()
		{
			locked = true;
		}

		/// <summary>
		/// Reset the number of hits counter to 0.
		/// </summary>
		public void ResetHitCounter()
		{
			hitCount = 0;
		}

		override public void Message(string message)
		{
			if (messageInterrupts != null)
			{
				for (int i = 0; i < messageInterrupts.Length; i++)
				{
					if (messageInterrupts[i].message == message)
					{
						if (!hasHadInterruptThisFrame)
						{
							// Interrupts beat transtions TODO: Is there special case for conflicting interrupts needed
							hasTransitionedThisFrame = false;
							if (HandleExit(messageInterrupts[i], 0))
							{
								hasHadInterruptThisFrame = true;
							}
						}
						else
						{
							HandleActions(messageInterrupts[i], 0);
						}
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Run each frame to determine and execute move.
		/// </summary>
		virtual protected void DoUpdate()
		{
			hasRunCheckForCharacter = false;
			hasTransitionedThisFrame = false;
			hasHadInterruptThisFrame = false;
			hasDecided = false;
			stateTimer += TimeManager.FrameTime;
			UpdateTimers();
		}

		/// <summary>
		/// Check the timer interrupts to see if any should fire.
		/// </summary>
		virtual protected void UpdateTimers()
		{
			if (timerInterrupts == null || timerInterrupts.Length == 0) return;
			for (int i = 0; i < timerInterrupts.Length; i++)
			{
				if (timers[i] == float.MinValue)
				{
					continue;
				}
				timers[i] -= TimeManager.FrameTime;
				if (timers[i] <= 0)
				{
					if (timerInterrupts[i].fireAlways || timerInterrupts[i].chance > Random.Range(0, 100))
					{
						if (!hasHadInterruptThisFrame)
						{
							// Interrupts beat transtions TODO: Is there special case for conflicting interrupts needed
							hasTransitionedThisFrame = false;
							if (HandleExit(timerInterrupts[i], 0))
							{
								hasHadInterruptThisFrame = true;
							}
						}
						else
						{
							HandleActions(timerInterrupts[i], 0);
						}
					}
					if (timerInterrupts[i].loop)
					{
						timers[i] = timerInterrupts[i].time;
					}
					else
					{
						timers[i] = float.MinValue;
					}
				}
			}
		}
		
		/// <summary>
		/// Handle the enemy getting damaged.
		/// </summary>
		virtual protected void HandleEnemyDamaged(object sender, DamageInfoEventArgs e)
		{
			if (e.DamageInfo.Amount > 0) hitCount += 1;
			for (int i = 0; i < healthInterrupts.Length; i++)
			{
				if (enemy.health == healthInterrupts[i].health)
				{
					if (!hasHadInterruptThisFrame)
					{
						// Interrupts beat transtions TODO: Is there special case for conflicting interrupts needed
						hasTransitionedThisFrame = false;
						if (HandleExit(healthInterrupts[i], 0))
						{
							hasHadInterruptThisFrame = true;
						}
					}
					else
					{
						HandleActions(healthInterrupts[i], 0);
					}
				}
			}
		}
		
		/// <summary>
		/// Handle the enemy movement completing.
		/// </summary>
		virtual protected void HandleMoveCompleted(object sender, EventArgs e)
		{
			stateMovementComplete = true;
			locked = false;
		}

		/// <summary>
		/// Find the entry point of the graph and set up the first action.
		/// </summary>
		virtual protected void SwitchPhase(EnemyAIGraph graph)
		{
			if (graph == null)
			{
				Debug.LogWarning("Tried to switch phase to empty graph. Deactivating enemy");
				gameObject.SetActive(false);
				return;
			}

			currentGraph = graph;
			
			// Find entry point
			EnemyEntryNode entry = (EnemyEntryNode) currentGraph.nodes.Where(n => typeof(EnemyEntryNode).IsInstanceOfType(n)).FirstOrDefault();
			if (entry == null)
			{
				Debug.LogWarning("The AI Graph didn't have an entry point. Deactivating enemy.");
				gameObject.SetActive(false);
				return;
			}

			if (!HandleExit(entry, 0))
			{
				Debug.LogWarning("Enemy AI entry went straight to end, i.e. the Enemy entry point didn't connect to any states. Deactivating enemy.");
				gameObject.SetActive(false);
				return;
			}
			
			// Find damage interrupt if it exists
			damageInterrupt = (EnemyInterruptDamagedNode) currentGraph.nodes.Where(n => typeof(EnemyInterruptDamagedNode).IsInstanceOfType(n)).FirstOrDefault();
			
			// Find counter interrupts if they exist
			counterInterrupts = currentGraph.nodes.Where(n => typeof(EnemyInterruptCounter).IsInstanceOfType(n)).Select(e => (EnemyInterruptCounter)e).ToArray();

			// Find health interrupts if they exist
			healthInterrupts = currentGraph.nodes.Where(n => typeof(EnemyInterruptHealth).IsInstanceOfType(n)).Select(e => (EnemyInterruptHealth)e).ToArray();
			
			// Find enemy message interrupts if they exist
			messageInterrupts = currentGraph.nodes.Where(n => typeof(EnemyInterruptEnemyMessage).IsInstanceOfType(n)).Select(e => (EnemyInterruptEnemyMessage)e).ToArray();
			
			// Find timer interrupts if they exist
			timerInterrupts = currentGraph.nodes.Where(n => typeof(EnemyInterruptTimer).IsInstanceOfType(n)).Select(e => (EnemyInterruptTimer)e).ToArray();
			timers = new float[timerInterrupts.Length];
			for (int i = 0; i < timerInterrupts.Length; i++)
			{
				timers[i] = timerInterrupts[i].time;
				if (timerInterrupts[i].time <= 0) Debug.Log("Timer with time <= 0 will never fire.");
			}
			
			// Reset
			ResetPhase();
		}

		/// <summary>
		/// Check each state in the graph to determine if it should exit and if so update the current movement.
		/// </summary>
		virtual protected void ProcessCurrentNode()
		{
			if (currentNode is EnemyOptionNode node)
			{
				OptionWithCondition[] exits = node.exitConditions;
				for (int i = 0; i < exits.Length; i++)
				{
					if (CheckForExit(exits[i]))
					{
						HandleExit(currentNode, i);
						return;
					}
				}
			} 
			else if (currentNode is EnemyStepNode)
			{
				if (stateMovementComplete)
				{
					HandleExit(currentNode, 0);
				}
			}
			else
			{
				Debug.LogWarning("Unknown Enemy AI node type");
			}
		}

		/// <summary>
		/// Handle moving through node exit to a new state. Includes calling any actions on the node.
		/// </summary>
		/// <param name="node">Current node</param>
		/// <param name="option">Option position</param>
		/// <returns>true if the state changed, false otherwise</returns>
		virtual protected bool HandleExit(IProcessableEnemyNode node, int option)
		{
			List<NodePort> connections = node.GetOutputForSelection(option)?.GetConnections();
			if (connections == null) return false;
			HandleActions(connections);
			// Check for phase Change
			ChangePhaseNode phaseNode = (ChangePhaseNode) connections.FirstOrDefault(n => n.node is ChangePhaseNode)?.node;
			if (phaseNode != null)
			{
				if (graphLookup.ContainsKey(phaseNode.nextPhase))
				{
					SwitchPhase(graphLookup[phaseNode.nextPhase]);
					return false;
				}
				Debug.LogWarning("Unable to change phase, phase not found: " + phaseNode.nextPhase +". Deactivating enemy.");
				gameObject.SetActive(false);
				return false;
			}
			// Don't allow more than one transition on any given frame
			if (hasTransitionedThisFrame) return false;
			// We should only connect to one EnemyNode
			EnemyNode targetNode = (EnemyNode) connections.FirstOrDefault(n => n.node is EnemyNode)?.node;
			if (targetNode != null)
			{
				hasTransitionedThisFrame = true;
				currentNode = targetNode;
				if (currentNode is EnemyNode_RandomOption randomNode)
				{
					int randomOption = randomNode.PickOption();
					List<NodePort> randomConnections = randomNode.GetOutputForSelection(randomOption)?.GetConnections();
					if (randomConnections == null)
					{
						Debug.LogWarning("Random selection node must have an exit on every node");
						return false;
					}
					HandleActions(randomConnections);
					EnemyNode randomTargetNode = (EnemyNode) randomConnections.FirstOrDefault(n => n.node is EnemyNode)?.node;
					if (randomTargetNode == null)
					{
						Debug.LogWarning("Random selection node must have an exit on every node");
						return false;
					}
					currentNode = randomTargetNode;
				} 
				ResetState();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Do the actions associated with node exit.
		/// </summary>
		/// <param name="connections"></param>
		virtual protected void HandleActions(IProcessableEnemyNode node, int option)
		{
			List<NodePort> connections = node.GetOutputForSelection(option)?.GetConnections();
			if (connections == null) return;
			HandleActions(connections);
		}
		
		/// <summary>
		/// Do the actions associated with node exit.
		/// </summary>
		/// <param name="connections"></param>
		virtual protected void HandleActions(List<NodePort> connections)
		{
			// Do actions
			foreach (NodePort n in connections)
			{
				if (n.node is EnemyAction action)
				{
					action.DoAction(enemy);
				}
			}
		}

		/// <summary>
		/// Checks a specific exit condition to see if its conditions have been met and returns true if they have.
		/// </summary>
		virtual protected bool CheckForExit(OptionWithCondition exit)
		{
			switch (exit.condition)
			{
				case EnemyStateExitType.TIMER:
					if (stateTimer >= exit.timer) return true;
					break;
				case EnemyStateExitType.MOVE_COMPLETE:
					if (stateMovementComplete)
					{
						return true;
					}
					break;
				case EnemyStateExitType.TIMER_PLUS_RANDOM:
					if (stateTimer >= exit.timer)
					{
						stateTimer = 0;
						if (Random.Range (0, 101) < exit.percentage) return true;
					}
					break;
				case EnemyStateExitType.NUMBER_OF_HITS:
					if (hitCount >= exit.requiredCount)
					{
						return true;
					}
					break;
				case EnemyStateExitType.COUNTER_REACHES:
					if (Counter >= exit.requiredCount)
					{
						return true;
					}
					break;
				case EnemyStateExitType.HEALTH_PERCENTAGE:
					if ((enemy.health / enemy.StartingHealth) <= exit.percentage) 
					{
						return true;
					}
					break;
				case EnemyStateExitType.SENSE_PLAYER:
					if (CheckForCharacter()) return true;
					break;
				case EnemyStateExitType.LOST_PLAYER_TARGET:
					CheckForCharacter();
					if (enemy.CurrentTarget == null) return true;
					break;
				case EnemyStateExitType.TARGET_WITHIN_RANGE:
					CheckForCharacter();
					if (enemy.CurrentTarget != null && (
						    (exit.range == 0) ||
						    (exit.range > 0 && Vector2.Distance(enemy.CurrentTargetTransform.position, transform.position) <= exit.range) ||
					        (exit.range < 0 && Vector2.Distance(enemy.CurrentTargetTransform.position, transform.position) >= -exit.range)))
					{
						return true;
					}
					break;
				case EnemyStateExitType.ALWAYS:
					return true;
				case EnemyStateExitType.NONE:
					break;
				default:
					Debug.Log ("Condition type not yet implemented: " + exit.condition);
					break;
			}
			return false;
		}
		
		virtual protected void ResetState()
		{
			stateMovementComplete = false;
			stateTimer = 0;
		}

		virtual protected void ResetPhase()
		{
			ResetState();
			counter = 0;
			hitCount = 0;
		}
		
		#region Senses
			/// <summary>
		/// Try to find a character by looking and listening.
		/// </summary>
		/// <returns><c>true</c>, if for character was checked, <c>false</c> otherwise.</returns>
		virtual protected bool CheckForCharacter()
		{
			if (hasRunCheckForCharacter) return checkForCharacterResult;
			hasRunCheckForCharacter = true;

			// If we have a target check if we have lost it
			if (enemy.CurrentTarget != null)
			{
				if (Vector2.Distance(enemy.CurrentTarget.transform.position, transform.position) > maxTargetDistance)
				{
					enemy.CurrentTarget = null;
					checkForCharacterResult = false;
					return false;
				}
				checkForCharacterResult = true;
				return true;
			}
			if (See ()) 
			{
				checkForCharacterResult = true;
				return true;
			}
			if (Hear ())
			{
				checkForCharacterResult = true;
				return true;
			}
			checkForCharacterResult = false;
			return false;
		}

		/// <summary>
		/// Look for a character.
		/// </summary>
		virtual protected bool See()
		{
			Character character = null;
			ICharacterReference characterRef = null;
			if (sightDistance > 0.0f)
			{
				Vector3 offset = new Vector3 (0, sightYOffset, 0);
				RaycastHit2D hit = Physics2D.Raycast(myTransform.position + offset, new Vector3(enemy.LastFacedDirection, 0, 0), sightDistance, sightLayers);
				if (hit.collider != null)
				{
					characterRef = (ICharacterReference) hit.collider.gameObject.GetComponent(typeof(ICharacterReference));
					if (characterRef == null)
					{
						character = hit.collider.gameObject.GetComponent<Character>();
					} 
					else
					{
						character = characterRef.Character;
					}
					if (character != null)
					{
						enemy.CurrentTarget = character;
						return true;
					}
				}
			} 
			return false;
		}

		/// <summary>
		/// Listen for a character.
		/// </summary>
		virtual protected bool Hear()
		{
			Character character = null;
			ICharacterReference characterRef = null;
			if (hearingRadius > 0.0f)
			{
				Vector2 offset = (Vector2)transform.position + (enemy.LastFacedDirection == -1 ? new Vector2(-hearingOffset.x, hearingOffset.y) : hearingOffset);
				int hits = Physics2D.OverlapCircleNonAlloc (offset, hearingRadius, proximityColliders, hearingLayers);
				if (hits > 0)
				{
					// Always pick first target in list
					characterRef = (ICharacterReference) proximityColliders[0].gameObject.GetComponent(typeof(ICharacterReference));
					if (characterRef == null)
					{
						character = proximityColliders[0].gameObject.GetComponent<Character>();
					} 
					else
					{
						character = characterRef.Character;
					}
					enemy.CurrentTarget = character;
					return true;
				}
			}
			return false;
		}

		#endregion

	}
	
	/// <summary>
	/// Associates a name to a graph.
	/// </summary>
	[System.Serializable]
	public class EnemyNamedAIGraphs
	{
		public string name;
		public EnemyAIGraph graph;
	}
}


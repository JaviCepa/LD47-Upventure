using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Relates an enemy state to an enemy movement.
	/// </summary>
	[System.Serializable]
	[ExecuteInEditMode]
	public class EnemyStateToMovement
	{

		/// <summary>
		/// When an enemy is in this state the movement should be executed.
		/// </summary>
		public EnemyState state;

		/// <summary>
		/// The enemy movement 
		/// </summary>
		public EnemyMovement movement;
	}

	/// <summary>
	/// High level enemy states for which different movements can be played by a EnemyMovement_Distributor.
	/// </summary>
	public enum EnemyState
	{
		DEFAULT,
		HITTING,
		ATTACKING,
		SHOOTING,
		GUARD,
		PATROL,
		FLEE,
		DAMAGED,
		DEAD,
		FALLING,
		CHARGING,
		HIDING,
		FLYING,
		DEFENDING,
		WAITING,
		DEPLOYING,
		ENTERING,
		EXITING,
		PREPARING,
		SHOOTING_LEFT,
		SHOOTING_RIGHT,
		CHARGING_LEFT,
		CHARING_RIGHT,
		ATTACKING_ALT_0,
		ATTACKING_ALT_1,
		ATTACKING_ALT_2,
		ATTACKING_ALT_3,
		ATTACKING_ALT_4,
		CUSTOM_0,
		CUSTOM_1,
		CUSTOM_2,
		CUSTOM_3,
		CUSTOM_4,
		CUSTOM_5,
		CUSTOM_6,
		CUSTOM_7,
		CUSTOM_8,
		CUSTOM_9,
		DAMAGED_ALT_0,
		DAMAGED_ALT_1,
		DAMAGED_ALT_2,
		DAMAGED_ALT_3,
		DAMAGED_ALT_4,
		IDLE,
		CAUTIOUS,
		AGGRESSIVE,
	}



}
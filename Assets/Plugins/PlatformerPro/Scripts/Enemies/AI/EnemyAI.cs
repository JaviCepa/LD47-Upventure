using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// An enemy AI is typically extended for SIMPLE enemy AI controllers and is used to move control between basic enemy 
	/// movements. For COMPLEX AI you will most likely implement your enemy as a character and your enemy AI as an Input
	/// to that character.
	/// </summary>
	public class EnemyAI : PlatformerProMonoBehaviour
	{

		/// <summary>
		/// How long between decisions, increasing this improves performance but makes the enemy less responsive.
		/// </summary>
		[Tooltip ("How long between decisions, increasing this improves performance but makes the enemey less responsive.")]
		public float decisionInterval;

		/// <summary>
		/// Timer for tracking decision interval.
		/// </summary>
		protected float decisionTimer;

		/// <summary>
		/// Enemy reference.
		/// </summary>
		protected Enemy enemy;

		/// <summary>
		/// Update the timer by frame time, we drive this externally so we can halt the mob from a single place (the Enemy script).
		/// </summary>
		virtual public bool UpdateTimer()
		{
			decisionTimer -= TimeManager.FrameTime;
			if (decisionTimer <= 0) 
			{
				decisionTimer = decisionInterval;
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Decide the next move.
		/// </summary>
		virtual public EnemyState Decide()
		{
			return EnemyState.DEFAULT;
		}

		/// <summary>
		/// Handle damage.
		/// </summary>
		/// <param name="info">Damage details</param>
		/// <returns>The updated damage info.</returns>
		virtual public DamageInfo DoDamage(DamageInfo info)
		{
			return info;
		}
		
		/// <summary>
		/// The sense routine used to detect when something changes. Sense is called 
		/// every frame and should be kept as simple as possible. Use Decide() for more
		/// complex logic.
		/// </summary>
		virtual public bool Sense()
		{
			return false;
		}

		/// <summary>
		/// Init this enemy AI.
		/// </summary>
		/// <param name="enemy">Enemy we are the brain for.</param>
		virtual public void Init(Enemy enemy)
		{
			this.enemy = enemy;
		}

		/// <summary>
		/// Called to tell the AI that we were able to handle the desired state.
		/// </summary>
		virtual public void StateTransitioned()
		{
			
		}
		
		/// <summary>
		/// Called to tell the AI that we were not able to handle the desired state. Usually due to a movement that wont release control.
		/// </summary>
		virtual public void StateNotTransitioned()
		{
			
		}

		/// <summary>
		/// Handle a message sent to the enemy.
		/// </summary>
		/// <param name="message"></param>
		// DUNGEON CODE
		virtual public void Message(string message)
		{
			
		}
		
		/// <summary>
		/// If we are going to save the enemy, this data will be saved if save state is enabled.
		/// </summary>
		virtual public string ExtraSaveData
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

#if UNITY_EDITOR

		/// <summary>
		/// Static info used by the editor.
		/// </summary>
		virtual public EnemyState[] Info
		{
			get
			{
				return new EnemyState[]{EnemyState.DEFAULT};
			}
		}

#endif
	}

}
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// An enemy movement that patrols back and forth.
	/// </summary>
	public class EnemyMovement_Patrol : EnemyMovement
	{
		#region members

		/// <summary>
		/// The distance from starting position to the right extent.
		/// </summary>
		public float rightOffset;
		
		/// <summary>
		/// The distance from starting position to the left extent.
		/// </summary>
		public float leftOffset;
		
		/// <summary>
		/// The speed the platform moves at.
		/// </summary>
		public float speed;

		/// <summary>
		/// Will the enemy change direction when it hits the character?
		/// </summary>
		public bool bounceOnHit;

		/// <summary>
		/// Will the enemy change direction when it finds an edge?
		/// </summary>
		public bool walkOffEdges = true;

		public AnimationState animationState = AnimationState.WALK;
		
		/// <summary>
		/// The right extent.
		/// </summary>
		protected float rightExtent;
		
		/// <summary>
		/// The left extent.
		/// </summary>
		protected float leftExtent;

		#endregion

		#region constants
		
		/// <summary>
		/// Human readable name.
		/// </summary>
		private const string Name = "Patrol";
		
		/// <summary>
		/// Human readable description.
		/// </summary>
		private const string Description = "An enemy movement that patrols back and forth. It doesn't consider any geometry or collisions, it walks a fixed path.";

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

		#region properties

		/// <summary>
		/// Gets the animation state that this movement wants to set.
		/// </summary>
		override public AnimationState AnimationState
		{
			get 
			{
				// We are always walking if patrolling
				return animationState;
			}
		}
		
		/// <summary>
		/// Returns the direction the character is facing. 0 for none, 1 for right, -1 for left.
		/// </summary>
		override public int FacingDirection
		{
			get 
			{
				if (speed > 0) return 1;
				if (speed < 0) return -1;
				return 0;
			}
		}

		#endregion


		#region public methods
		
		/// <summary>
		/// Initialise this movement and return a reference to the ready to use movement.
		/// </summary>
		override public EnemyMovement Init(Enemy enemy)
		{
			this.enemy = enemy;

			leftExtent = enemy.transform.position.x - leftOffset;
			rightExtent = enemy.transform.position.x + rightOffset;

			return this;
		}

		/// <summary>
		/// Moves the character.
		/// </summary>
		override public bool DoMove()
		{
			if (enemy.State != EnemyState.FALLING && !walkOffEdges)
			{
				if (CheckForEdge(speed > 0 ? 1 : -1))
				{
                    enemy.SwitchDirection();
					return true;
				}
			}
			if (speed > 0)
			{
				// We have the additional check so we can beter support enemies starting at the wrong spot
				if (enemy.Transform.position.x >= rightExtent)
				{
                    enemy.SwitchDirection();
                }
				else
				{
					float actualSpeed = speed * Mathf.Abs (Mathf.Cos (enemy.SlopeTargetRotation * Mathf.Deg2Rad));
					enemy.Translate(actualSpeed * TimeManager.FrameTime, 0, false);
					if (enemy.Transform.position.x > rightExtent)
					{
						// Should we set this directly or add a new method to enemy?
						enemy.Transform.position = new Vector3(rightExtent, enemy.Transform.position.y, enemy.Transform.position.z);
                        enemy.SwitchDirection();
                    }
				}
			}
			else if (speed < 0)
			{
				if (enemy.Transform.position.x <= leftExtent)
				{
                    enemy.SwitchDirection();
                }
				else
				{
					float actualSpeed = speed * Mathf.Abs (Mathf.Cos (enemy.SlopeTargetRotation * Mathf.Deg2Rad));
					enemy.Translate(actualSpeed * TimeManager.FrameTime, 0, false);
					if (enemy.Transform.position.x < leftExtent)
					{
						// Should we set this directly or add a new method to enemy?
						enemy.Transform.position = new Vector3(leftExtent, enemy.Transform.position.y, enemy.Transform.position.z);
                        enemy.SwitchDirection();
                    }
				}
			}
			return true;
		}

		/// <summary>
		/// Called when the enemy hits the character.
		/// </summary>
		/// <param name="character">Character.</param>
		/// <param name="info">Damage info.</param>
		override public void HitCharacter(Character character, DamageInfo info)
		{
            if (bounceOnHit) enemy.SwitchDirection();
		}

		/// <summary>
		/// Called by the enemy to switch (x) direction of the movement. Note that not all 
		/// movements need to support this, they may do nothing.
		/// </summary>
		override public void SwitchDirection()
		{
			speed *= -1;
		}

		#endregion

		/// <summary>
		/// Often a movement will need some kind of direction information such as where the cahracter is in relation to the enemy.
		/// Use this to set that information. Note there is no specific rule for what that information is, it could be anything.
		/// </summary>
		/// <param name="direction">Direction.</param>
		override public void SetDirection(Vector2 direction)
		{
			if (direction.x > 0 && speed > 0) speed *= -1;
			if (direction.x < 0 && speed < 0) speed *= -1;
		}
		
#if UNITY_EDITOR

		/// <summary>
		/// Draw handles for showing extents.
		/// </summary>
		void OnDrawGizmos()
		{
			DrawGizmos();
		}

		/// <summary>
		/// Draw gizmos for showing extents.
		/// </summary>
		virtual public void DrawGizmos()
		{
			// These handles don't make sense once the game is playing as they would move
			if (!Application.isPlaying)
			{
				float left = 0.0f; float right = 0.0f;

				// TODO Better bounds finding, we should look for a hazard and use the colldier on that by default
				Collider2D collider2D = GetComponent<Collider2D>();
				if (collider2D is EdgeCollider2D)
				{
					for (int i = 0; i < ((EdgeCollider2D)collider2D).points.Length; i++)
					{
						if (((EdgeCollider2D)collider2D).points[i].x > right) right = ((EdgeCollider2D)collider2D).points[i].x;
						if (((EdgeCollider2D)collider2D).points[i].x < left) left = ((EdgeCollider2D)collider2D).points[i].x;
					}
				}
				else if (collider2D is BoxCollider2D)
				{
					right = ((BoxCollider2D)collider2D).size.x / 2.0f;
					left = ((BoxCollider2D)collider2D).size.x / -2.0f;
				}

				Gizmos.color = Color.red;
				Gizmos.DrawLine(transform.position,  transform.position + new Vector3(rightOffset + right, 0, 0));
				Gizmos.DrawLine(transform.position + new Vector3(rightOffset + right, 0.25f, 0), transform.position + new Vector3(rightOffset + right, -0.25f, 0));
				Gizmos.DrawLine(transform.position,  transform.position + new Vector3(-leftOffset + left, 0, 0));
				Gizmos.DrawLine(transform.position + new Vector3(-leftOffset + left, 0.25f, 0),  transform.position + new Vector3(-leftOffset, -0.25f, 0));
			}
		}
#endif

	}

}
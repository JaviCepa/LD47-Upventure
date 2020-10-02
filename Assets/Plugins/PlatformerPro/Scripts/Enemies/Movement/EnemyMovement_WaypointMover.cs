using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlatformerPro.Tween;

namespace PlatformerPro
{
	/// <summary>
	/// An enemy movement that moves through a set of waypoints.
	/// </summary>
	public class EnemyMovement_WaypointMover : EnemyMovement, ICompletableMovement
	{
		
		/// <summary>
		/// The way points.
		/// </summary>
		[Header("Movement")]
		[Tooltip ("The way points.")]
		public List<Vector2> wayPoints;

		/// <summary>
		/// Speed to move towards the next waypoint at.
		/// </summary>
		[Tooltip ("Speed to move towards the next waypoint at.")]
		public float moveSpeed;

		/// <summary>
		/// Should we loop or stop we reach the last position?
		/// </summary>
		[Tooltip ("Should we loop or stop we reach the last position?")]
		public bool loop;

		/// <summary>
		/// How long the enemy pauses at each waypoint.
		/// </summary>
		public float pauseTime = 0.0f;
		
		/// <summary>
		/// Any tweening to apply.
		/// </summary>
		[Tooltip ("Any tweening to apply.")]
		public TweenMode tweenMode;
		
        /// <summary>
        /// Should weupdate facing direction based on travel direction.
        /// </summary>
        [Header("Animation")]
        [Tooltip("Should we update facing direction based on travel direction?")]
        public bool setFacingDirection;

        /// <summary>
        /// The animation state to set while charging.
        /// </summary>
        [Tooltip ("The animation state to set while charging.")]
        public AnimationState animationState = AnimationState.RUN;
        
        /// <summary>
        /// The animation state to set while charging.
        /// </summary>
        [Tooltip ("The animation state to set while charging.")]
        public AnimationState pausedAnimationState = AnimationState.IDLE;
        
        
        /// <summary>
        /// When do we send the move complete event?
        /// </summary>
        [Header("Move Complete")]
        [Tooltip ("When do we send the move complete event?")]
        public WaypointMoveCompleteType sendMoveComplete;
        
        /// <summary>
        /// Tweener which handles any moves.
        /// </summary>
        protected IMobPositionTweener tweener;

		/// <summary>
		/// Has tween started?
		/// </summary>
		protected bool tweenStarted;

		/// <summary>
		/// Index of the current way point.
		/// </summary>
		protected int currentWayPoint;

		/// <summary>
		/// Tracks how long we have been paused.
		/// </summary>
		protected float pauseTimer;
		
		#region constants
		
		/// <summary>
		/// Human readable name.
		/// </summary>
		private const string Name = "Waypoint mover";
		
		/// <summary>
		/// Human readable description.
		/// </summary>
		private const string Description = "An enemy movement that moves through a set of waypoints.";
		
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
		
		/// <summary>
		/// Initialise this movement and return a reference to the ready to use movement.
		/// </summary>
		override public EnemyMovement Init(Enemy enemy)
		{
			base.Init (enemy);
			currentWayPoint = 0;
			if (wayPoints.Count < 1) 
			{
				Debug.LogWarning("Waypoint mover needs at least one waypoint");
				enabled = false;
			}
			return this;
		}

		/// <summary>
		/// Does the movement.
		/// </summary>
		override public bool DoMove()
		{
			// Early out don't move if not enabled
			if (!enabled) return true;

			// Paused
			if (pauseTimer > 0)
			{
				pauseTimer -= TimeManager.FrameTime;
				return true;
			}
			
			// Early out if we have reached the last way point
			if (!loop && currentWayPoint >= wayPoints.Count) return true;

			// Loop
			if (loop && currentWayPoint >= wayPoints.Count) currentWayPoint = 0;

			// If we start  at the first waypoint, skip ahead
			if (currentWayPoint == 0 && Vector2.Distance(wayPoints[currentWayPoint], enemy.transform.position) < 0.001f) currentWayPoint++;

			if (!tweenStarted)
			{
				// Special case We use speed like a delay if the mode is set to SNAP
				if (tweenMode == TweenMode.SNAP)
				{
					StartCoroutine(SnapAfterDelay(wayPoints[currentWayPoint]));
				}
				else
				{
					Vector2 targetPosition = wayPoints[currentWayPoint];
					tweener = GetComponent<IMobPositionTweener> ();
					if (tweener == null)
					{
						tweener = enemy.gameObject.AddComponent<IMobPositionTweener> ();
						tweener.UseGameTime = true;
					}
					tweener.TweenWithRate(tweenMode, enemy, targetPosition, moveSpeed, MoveComplete);
					tweenStarted = true;
				}
			}
			return true;
		}

		/// <summary>
		/// Moves after a delay.
		/// </summary>
		/// <returns>The after delay.</returns>
		virtual protected IEnumerator SnapAfterDelay(Vector2 targetPosition)
		{
			tweenStarted = true;
			yield return new WaitForSeconds (moveSpeed);
			tweener = GetComponent<IMobPositionTweener> ();
			if (tweener == null)
			{
				tweener = enemy.gameObject.AddComponent<IMobPositionTweener> ();
				tweener.UseGameTime = true;
			}
			tweener.TweenWithRate(TweenMode.SNAP, enemy, targetPosition, moveSpeed, MoveComplete);
		}

		/// <summary>
		/// Called when this movement is gaining control.
		/// </summary>
		override public void GainingControl()
		{
			if (tweener != null) tweener.Stop ();
			tweenStarted = false;
		}
		
		/// <summary>
		/// Called when this movement is losing control.
		/// </summary>
		/// <returns><c>true</c>, if a final animation is being played and control should not revert <c>false</c> otherwise.</returns>
		override public bool LosingControl()
		{
			tweener.Stop ();
			tweenStarted = false;
			return false;
		}

		/// <summary>
		/// Callback for when move is done.
		/// </summary>
		virtual public void MoveComplete(IMob target, Vector2 finalPosition)
		{
			switch (sendMoveComplete)
			{
				case WaypointMoveCompleteType.EVERY_WAYPOINT:
					enemy.MovementComplete();
					break;
				case WaypointMoveCompleteType.LAST_WAYPOINT:
					if (currentWayPoint >= wayPoints.Count - 1) enemy.MovementComplete ();
					break;
				case WaypointMoveCompleteType.ON_LOOP_TO_FIRST_WAYPOINT:
					if (currentWayPoint == 0) enemy.MovementComplete ();
					break;
			}
			currentWayPoint++;
			tweenStarted = false;
			pauseTimer = pauseTime;
		}

				
		/// <summary>
		/// Gets the animation state that this movement wants to set.
		/// </summary>
		override public AnimationState AnimationState
		{
			get
			{
				if (pauseTimer > 0) return pausedAnimationState;
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
                if (setFacingDirection)
                {
	                int tmpCurrentWaypoint = currentWayPoint;
	                if (tmpCurrentWaypoint >= wayPoints.Count) tmpCurrentWaypoint = 0;
                    int previousWayPoint = tmpCurrentWaypoint - 1;
                    if (previousWayPoint < 0) previousWayPoint = wayPoints.Count - 1;
                    if (wayPoints[tmpCurrentWaypoint].x - wayPoints[previousWayPoint].x > 0) 
                    {
                        return 1;
                    } 
                    if (wayPoints[tmpCurrentWaypoint].x - wayPoints[previousWayPoint].x < 0)
                    {
                        return -1;
                    }
                }
                return 0;
            }
        }
    }

	public enum WaypointMoveCompleteType
	{
		LAST_WAYPOINT,
		ON_LOOP_TO_FIRST_WAYPOINT,
		EVERY_WAYPOINT,
		NEVER
	}
}
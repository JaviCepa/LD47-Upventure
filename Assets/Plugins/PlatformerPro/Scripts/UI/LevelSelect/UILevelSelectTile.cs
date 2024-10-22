﻿using UnityEngine;
#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_1 && !UNITY_5_2
using UnityEngine.SceneManagement;
#endif
using System.Collections;

namespace PlatformerPro.Extras
{
	/// <summary>
	/// A top down tile in a level select view on which the character can walk.
	/// </summary>
	[RequireComponent (typeof(BoxCollider2D))]
    public class UILevelSelectTile : MonoBehaviour
	{
	
		/// <summary>
		/// Can we move in X on this tile?
		/// </summary>
		public bool canMoveInX = true;

		/// <summary>
		/// Can we move in Y on this tile?
		/// </summary>
		public bool canMoveInY = false;

		/// <summary>
		/// If this is not empty then this tile will load the given scene name when the user presses Jump/Action while standing on it.
		/// </summary>
		public string levelLoadSceneName;

		/// <summary>
		/// If this is not empty then this tile cannot be moved through until the assocaited level (or object) is unlocked.
		/// </summary>
		[ContextMenuItem ("Lock", "Lock")]
		[ContextMenuItem ("Unlock", "Unlock")]
		public string levelLockName;

        /// <summary>
        /// This sprite will be shown/hidden based on the completion state of the levelUnlockName.
        /// </summary>
        public SpriteRenderer lockSprite;

        /// <summary>
        /// If this is not empty then one or more sprites can be shown to indicate level completion.
        /// </summary>
        public string levelCompleteName;

        /// <summary>
        /// These sprites will be shown/hidden based on the completion state of of the levelCompleteName.
        /// </summary>
        public SpriteRenderer[] completeSprites;

        /// <summary>
        /// Is this tile currently locked.
        /// </summary>
        protected bool isLocked;

        protected BoxCollider2D myCollider;

		void Start()
		{
			Init ();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		virtual protected void Init()
		{
            myCollider = GetComponent<BoxCollider2D>();
            if (myCollider == null)
            {
                Debug.LogError("UILevelSelectTile must have a BoxCollider2D attached");
                return;
            }
            if (levelLockName != null && levelLockName != "")
            {
                isLocked = !LevelManager.Instance.IsUnlocked(levelLockName);
                if (lockSprite != null)
                {
                    if (isLocked)
                    {
                        lockSprite.enabled = true;
                    }
                    else
                    {
                        lockSprite.enabled = false;
                    }
                }
            }
            else
            {
                isLocked = false;
            }
            if (completeSprites != null && completeSprites.Length > 0)
            {
                int rating = LevelManager.Instance.Rating(levelCompleteName);
                for (int i = 0; i < completeSprites.Length; i++)
                {
                    if (rating > i)
                    {
                        completeSprites[i].enabled = true;
                    }
                    else
                    {
                        completeSprites[i].enabled = false;
                    }
                }
            }
		}

		/// <summary>
		/// Determines whether the character can move past this tile.
		/// </summary>
		/// <returns><c>true</c> if this instance can move past; otherwise, <c>false</c>.</returns>
		public bool AllowsMove(int x, int y)
		{
			// Can't move if locked.
			if (isLocked) return false;
			if (x != 0 && canMoveInX) return true;
			if (y != 0 && canMoveInY) return true;
			return false;
		}

        /// <summary>
        /// Gets the extent of the collider in the given direction.
        /// </summary>
        public Vector2 TargetPosition(int x, int y)
        {
            if (x != 0 && !canMoveInX) return Vector2.zero;
            if (y != 0 && !canMoveInY) return Vector2.zero;
            if (x == 1) return myCollider.GetXExtentsInWorldSpace()[1];
            if (x == -1) return myCollider.GetXExtentsInWorldSpace()[0];
            if (y == 1) return myCollider.GetYExtentsInWorldSpace()[1];
            if (y == -1) return myCollider.GetYExtentsInWorldSpace()[0];
            return Vector2.zero;
        }

        /// <summary>
        /// Snap the specified character to the centre of the path.
        /// </summary>
        /// <param name="character">Character.</param>
        public void Snap(UILevelSelectCharacter character)
		{
			if (canMoveInX && canMoveInY)  return;
			if (canMoveInX) character.transform.position = new Vector3(character.transform.position.x, transform.position.y, character.transform.position.z);
			if (canMoveInY) character.transform.position = new Vector3( transform.position.x, character.transform.position.y, character.transform.position.z);
		}

		/// <summary>
		/// Can we enter the level assocaited with this tile?
		/// </summary>
		/// <returns><c>true</c>, if we can enter the level, <c>false</c> otherwise.</returns>
		public bool AllowsEnter()
		{
			// Can't enter if locked.
			if (isLocked) return false;
			// Enter if we have a level
			if (levelLoadSceneName != null && levelLoadSceneName != "") return true;
			return false;
		}

		/// <summary>
		/// Enters the level.
		/// </summary>
		public void EnterLevel()
		{
			if (levelLoadSceneName != null && levelLoadSceneName != "")
			{
				#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_1 && !UNITY_5_2
				LevelManager.PreviousLevel = SceneManager.GetActiveScene().name;
				SceneManager.LoadScene(levelLoadSceneName);
				#else
				LevelManager.PreviousLevel = Application.loadedLevelName;
				Application.LoadLevel(levelLoadSceneName);
				#endif
			}
			else Debug.LogWarning ("Tried to enter a level but no level associated with this tile");
		}

#if UNITY_EDITOR

		/// <summary>
		/// Lock this instance.
		/// </summary>
		protected void Lock()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("You can't test lock or unlock in EDIT mode, press PLAY.");
			}
			else
			{
				LevelManager.Instance.LockLevel (levelLockName);
				Init ();
			}
		}

		/// <summary>
		/// Unlock this instance.
		/// </summary>
		protected void Unlock()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("You can't test lock or unlock in EDIT mode, press PLAY. If you want to reset data use the Persistence menu item.");
			}
			else
			{
				LevelManager.Instance.UnlockLevel (levelLockName);
				Init ();
			}
		}
		#endif
	}

}
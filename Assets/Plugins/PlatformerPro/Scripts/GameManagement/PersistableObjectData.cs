﻿using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	/// <summary>
	/// Stores data about a persistable object.
	/// </summary>
	[System.Serializable]
	public class PersistableObjectData
	{
		/// <summary>
		/// Unique ID for the object.
		/// </summary>
		public string guid;

		/// <summary>
		/// Objects state.
		/// </summary>
		public bool state;

		/// <summary>
		/// Additional state info.
		/// </summary>
		public string extraStateInfo;

		/// <summary>
		/// If non-null/non-empty this gameObject should be spawned in to the scene.
		/// </summary>
		public string spawnObjectName;

		/// <summary>
		/// Position to spawn object at is spawnObjectName isn't empty.
		/// </summary>
		public float spawnX, spawnY, spawnZ;
		
	}
}

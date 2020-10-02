using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace PlatformerPro
{
	/// <summary>
	/// Static class for managing persistable objects.
	/// </summary>
	public class PersistableObjectManager : PlatformerProMonoBehaviour
	{
		/// <summary>
		/// If true save files in a binary format. False use XML.
		/// </summary>
		protected bool useBinaryFormat;
		
		/// <summary>
		/// Should we save data on change.
		/// </summary>
		protected bool saveOnChange;

		/// <summary>
		/// The persistence data.
		/// </summary>
		protected Dictionary<string, PersistableObjectData> objectData;

		/// <summary>
		/// Used to ensure that the will exit scene save happens at end of frame after all other objects have set their state.
		/// </summary>
		protected bool doLateSave;


		/// <summary>
		/// Stores all pref kyes so we can reset data.
		/// </summary>
		protected static List<string> allPrefsIdentifiers = new List<string> ();

		/// <summary>
		/// A formatter used for binary serialisation.
		/// </summary>
		protected static BinaryFormatter binaryFormatter;
		
		/// <summary>
		/// Should we use binary format?
		/// </summary>
		public bool UseBinary => useBinaryFormat;

		/// <summary>
		/// Get a binary formatter for use in saving.
		/// </summary>
		public BinaryFormatter BinaryFormatter => binaryFormatter;
		
		/// <summary>
		/// Unity LateUpdate hook.
		/// </summary>
		void LateUpdate()
		{
			if (doLateSave)
			{
				doLateSave = false;
				Save ();
			}
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected void Init()
		{
			objectData = new Dictionary<string, PersistableObjectData> ();
			Load ();
			InitEvents ();
		}
			
		/// <summary>
		/// Unity Destory hook.
		/// </summary>
		void OnDestroy()
		{
			if (PlatformerProGameManager.Instance != null)
			{
				PlatformerProGameManager.Instance.PhaseChanged -= HandlePhaseChanged;
			}
		}
		
		/// <summary>
		/// Handles the phase changed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandlePhaseChanged (object sender, GamePhaseEventArgs e)
		{
			if (e.Phase == GamePhase.GAME_OVER)
			{
				ResetAll ();
			}
			else if (e.Phase == GamePhase.READY)
			{
				LevelManager.Instance.WillExitScene += HandleWillExitScene;
			}
		}

		/// <summary>
		/// Gets the state for the object with the given guid. This is not a copy! Creates new state if the guid is not found.
		/// </summary>
		/// <param name="guid">GUID.</param>
		/// <param name="defaultStateIsDisabled">If true the object starts disabled.</param>
		public PersistableObjectData GetState(string guid, bool defaultStateIsDisabled)
		{
			if (objectData.ContainsKey (guid))
			{
				return objectData [guid];
			}
			else
			{
				PersistableObjectData data = new PersistableObjectData ();
				data.guid = guid;
				data.state = !defaultStateIsDisabled;
				objectData.Add (guid, data);
				return data;
			}
		}

		/// <summary>
		/// Sets the state for the object with the given guid.
		/// </summary>
		/// <param name="guid">GUID.</param>
		/// <param name="state">State to set.</param>
		/// <param name="extraInfo">Extra info.</param>
		public void SetState(string guid, bool state, string extraInfo)
		{
			if (objectData.ContainsKey (guid))
			{
				if (!string.IsNullOrEmpty(objectData[guid].spawnObjectName))
				{
					if (!state)
					{
						objectData.Remove(guid);
					}
					else
					{
						Debug.LogWarning("Replacing a spawned object guid with a scene object guid, this is probably not right");	
					}
				}
				else
				{
					objectData [guid].state = state;
					objectData [guid].extraStateInfo = extraInfo;	
				}
			}
			else
			{
				PersistableObjectData data = new PersistableObjectData ();
				data.guid = guid;
				data.state = state;
				data.extraStateInfo = extraInfo;
				objectData.Add (guid, data);
			}
			if (saveOnChange) Save ();
		}

		public void SetState(string guid, bool state, string extraInfo, string spawnObjectName, Vector3 spawnObjectPosition)
		{
			if (string.IsNullOrEmpty(spawnObjectName))
			{
				SetState(guid, state, extraInfo);
				return;
			}
			if (objectData.ContainsKey (guid))
			{
				if (string.IsNullOrEmpty(objectData[guid].spawnObjectName))
				{
					Debug.LogWarning("Replacing a scene object guid with a spawned object guid, this is probably not right");	
				}
#if UNITY_EDITOR
				if (!PrefabDictionary.FindOrCreateInstance().ContainsName(spawnObjectName))
				{
					Debug.LogWarning("Saved a spawned object but the prefab wasn't in the PrefabDictionary");
				}
#endif
				if (!state)
				{
					objectData.Remove(guid);
				}
				else
				{
					objectData [guid].state = state;
					objectData [guid].extraStateInfo = extraInfo;
					objectData [guid].spawnObjectName = spawnObjectName;
					objectData [guid].spawnX = spawnObjectPosition.x;
					objectData [guid].spawnY = spawnObjectPosition.y;
					objectData [guid].spawnZ = spawnObjectPosition.z;
				}
			}
			else
			{
				if (!state)
				{
					Debug.LogWarning("Saving a spawned object with state false, this doesn't make sense");
				}
				else
				{
					PersistableObjectData data = new PersistableObjectData ();
					data.guid = guid;
					data.state = state;
					data.extraStateInfo = extraInfo;
					data.spawnObjectName = spawnObjectName;
					data.spawnX = spawnObjectPosition.x;
					data.spawnY = spawnObjectPosition.y;
					data.spawnZ = spawnObjectPosition.z;
					objectData.Add (guid, data);
				}
			}
			if (saveOnChange) Save ();
		}

		/// <summary>
		/// Updates persistable object state.
		/// </summary>
		public void Save()
		{
			if (useBinaryFormat)
			{
				SaveBinary();
			}
			else
			{
				SaveXml();
			}
		}
		
		/// <summary>
		/// Updates persistable object state using XML format.
		/// </summary>
		virtual protected void SaveXml() {
			using(StringWriter writer = new StringWriter())
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<PersistableObjectData>));
				serializer.Serialize(writer, GetSaveData());
				PlayerPrefs.SetString(PlayerPrefsIdentifier, writer.ToString());
			}
			if (!allPrefsIdentifiers.Contains (PlayerPrefsIdentifier))
			{
				allPrefsIdentifiers.Add (PlayerPrefsIdentifier);
				using (StringWriter writer = new StringWriter ())
				{
					XmlSerializer serializer = new XmlSerializer (typeof(List<string>));
					serializer.Serialize (writer, allPrefsIdentifiers);
					PlayerPrefs.SetString (UniqueDataIdentifier, writer.ToString ());
				}
			}
		}

		/// <summary>
		/// Updates persistable object state using binary format.
		/// </summary>
		virtual protected void SaveBinary()
		{
			object saveData = GetSaveData();
			var memoryStream = new MemoryStream();
			using (memoryStream)
			{
				binaryFormatter.Serialize(memoryStream, saveData);
			}
			PlayerPrefs.SetString(PlayerPrefsIdentifier, Convert.ToBase64String(memoryStream.ToArray()));
					
			if (!allPrefsIdentifiers.Contains (PlayerPrefsIdentifier))
			{
				allPrefsIdentifiers.Add (PlayerPrefsIdentifier);
				var allPrefsmemoryStream = new MemoryStream();
				using (allPrefsmemoryStream)
				{
					binaryFormatter.Serialize(allPrefsmemoryStream, allPrefsIdentifiers);
				}
				PlayerPrefs.SetString(UniqueDataIdentifier, Convert.ToBase64String(allPrefsmemoryStream.ToArray()));
			}
		}
		
		/// <summary>
		/// Reset persistable object state.
		/// </summary>
		public void ResetAll()
		{
			foreach (string id in allPrefsIdentifiers)
			{
				PlayerPrefs.SetString(id, "");
			}
		}

		/// <summary>
		/// Reset persistable object state.
		/// </summary>
		public void ResetCurrentlevel()
		{
			Debug.Log ("Resetting current levels persistable objects");
			PlayerPrefs.SetString(PlayerPrefsIdentifier, "");
		}

		/// <summary>
		/// Load the saved data from prefs.
		/// </summary>
		protected void Load()
		{
			if (useBinaryFormat)
			{
				LoadBinary();
			}
			else
			{
				LoadXml();
			}
		}
		
		/// <summary>
		/// Load the saved data from prefs in XMl format.
		/// </summary>
		virtual protected void LoadXml() {
			string identifiers = PlayerPrefs.GetString (UniqueDataIdentifier, "");
			if (allPrefsIdentifiers.Count == 0 && identifiers.Length > 0)
			{
				// Debug.Log ("Loading all prefs list");
				using (StringReader reader = new StringReader(identifiers)){
					XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
					allPrefsIdentifiers = (List<string>) serializer.Deserialize(reader);
				}
			}
			// Load persistable objects
			string data = PlayerPrefs.GetString(PlayerPrefsIdentifier, "");
			if (data.Length > 0)
			{
				List<PersistableObjectData> saveData;
				using (StringReader reader = new StringReader(data)){
					XmlSerializer serializer = new XmlSerializer(typeof(List<PersistableObjectData>));
					saveData = (List<PersistableObjectData>) serializer.Deserialize(reader);
					foreach (PersistableObjectData p in saveData)
					{
						objectData.Add (p.guid, p);
					}
				}
			}
			// Spawn any spawnables
			SpawnObjects(objectData.Values);
		}

		/// <summary>
		/// Load the saved data from prefs in XMl format.
		/// </summary>
		virtual protected void LoadBinary()
		{
			string identifiers = PlayerPrefs.GetString(UniqueDataIdentifier, "");
			if (allPrefsIdentifiers.Count == 0 && identifiers.Length > 0)
			{
				MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(identifiers));
				allPrefsIdentifiers = (List<string>) binaryFormatter.Deserialize(memoryStream);
			}

			// Load persistable objects
			string data = PlayerPrefs.GetString(PlayerPrefsIdentifier, "");
			if (data.Length > 0)
			{
				List<PersistableObjectData> saveData;
				MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(data));
				saveData = (List<PersistableObjectData>) binaryFormatter.Deserialize(memoryStream);
				foreach (PersistableObjectData p in saveData)
				{
					objectData.Add(p.guid, p);
				}
			}
			
			// Spawn any spawnables
			SpawnObjects(objectData.Values);
		}
		
		/// <summary>
		/// Spawns loaded objects.
		/// </summary>
		virtual protected void SpawnObjects(IEnumerable<PersistableObjectData> objectsToSpawn) 
		{
			// Spawn any spawnables
			foreach (PersistableObjectData p in objectsToSpawn)
			{
				if (!string.IsNullOrEmpty(p.spawnObjectName))
				{
					GameObject prefab = PrefabDictionary.FindOrCreateInstance().GetAssetByName(p.spawnObjectName);
					if (prefab == null)
					{
						Debug.LogWarning("Tried to spawn a persisted spawnable object but the prefab wasn't in the dictionary");
						break;
					}
					GameObject go = Instantiate (prefab);
					go.transform.position = new Vector3(p.spawnX, p.spawnY, p.spawnZ);
					PersistableObject po = go.GetComponentInChildren<PersistableObject>();
					if (po != null)
					{
						po.guid = p.guid;
						po.spawnedObjectName = p.spawnObjectName;
					}
					else
					{
						Debug.LogWarning("Spawned object should have a persistable object in its hierarchy");
					}
				}
			}
		}
		
		/// <summary>
		/// Find references and initialise all the event listeners..
		/// </summary>
		void InitEvents()
		{
			PlatformerProGameManager.Instance.PhaseChanged += HandlePhaseChanged;
		}

		/// <summary>
		/// Handle scene exit by saving
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandleWillExitScene (object sender, SceneEventArgs e)
		{
			// Defer save to end of frame
			doLateSave = true;
		}

		/// <summary>
		/// Handles the game ending.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event data.</param>
		virtual public void HandleGameOver (object sender, DamageInfoEventArgs e)
		{
			ResetAll ();
		}

		/// <summary>
		/// Convert dictionary into savable list.
		/// </summary>
		/// <returns>The save data.</returns>
		protected List<PersistableObjectData> GetSaveData()
		{
			return objectData.Values.ToList ();
		}

		#region static methods

		/// <summary>
		/// The player preference identifier.
		/// </summary>
		public const string UniqueDataIdentifier = "PersistableObjectManagerData";

		/// <summary>
		/// The player preference identifier.
		/// </summary>
		virtual public string PlayerPrefsIdentifier
		{
			get
			{
				string levelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name;
				return string.Format("{0}_{1}", UniqueDataIdentifier, levelName);
			}
		}

		/// <summary>
		/// Creates a new time manager.
		/// </summary>
		public static PersistableObjectManager CreateNewPersistableObjectManager(bool saveOnChange, bool useBinaryFormat)
		{
			binaryFormatter = new BinaryFormatter();
			GameObject go = new GameObject ();
			go.name = "PersistableObjectManager";
			go.hideFlags = HideFlags.HideInHierarchy;
			PersistableObjectManager instance = go.AddComponent<PersistableObjectManager> ();
			instance.saveOnChange = saveOnChange;
			instance.useBinaryFormat = useBinaryFormat;
			instance.Init ();
			return instance;
		}

		#endregion

	}
}

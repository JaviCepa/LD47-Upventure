using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace PlatformerPro {

	public class PrefabDictionary : MonoBehaviour
	{
		
		/// <summary>
		/// Assets used in the map.
		/// </summary>
		public List<GameObject> items;

		/// <summary>
		/// Map of assets to names for ffaster access.
		/// </summary>
		private Dictionary<string, GameObject> assets;

		/// <summary>
		/// The default dictionary prefab location. If editor can't find a Dictionary one will be created from this prefab.
		/// </summary>
		public const string DefaultDictionaryPrefabLocation =  "Assets/PlatformerPro/Prefabs/DefaultPrefabDictionary.prefab";
		
		/// <summary>
		/// Unity Awake hook.
		/// </summary>
		void Awake()
		{
			Instance = this;
			Init();
		}

		public static PrefabDictionary FindOrCreateInstance()
		{
			if (Instance != null && Instance.gameObject == null) Instance = null;
			if (Instance != null) return Instance;
			Instance = FindObjectOfType<PrefabDictionary>();
#if UNITY_EDITOR
			if (Instance == null)
			{
				Debug.LogWarning("No Dictionary found, creating one from the default prefab");
				GameObject assetDictionaryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultDictionaryPrefabLocation);
				if (assetDictionaryPrefab != null)
				{
					GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(assetDictionaryPrefab);
					go.name = "PrefabDictionary";
					Instance = go.GetComponent<PrefabDictionary>();
				}
				else
				{
					Debug.LogWarning(
						"No default reference dictionary prefab found. You will need to create your own ReferenceDictionary!");
				}
			}
#endif
			return Instance;
		}

		/// <summary>
		/// Gets reference to the Dictionary in the scene.
		/// </summary>
		/// <value>The instance.</value>
		protected static PrefabDictionary Instance { get; set; }

		/// <summary>
		/// Create a dictionary for fast access.
		/// </summary>
		protected void Init()
		{
			assets = new Dictionary<string, GameObject>();
			foreach (GameObject asset in items)
			{
				if (asset == null)
				{
					Debug.LogWarning("Empty entries found in Dictionary");
				}
				else
				{
					if (assets.ContainsKey(asset.name))
					{
						Debug.LogWarning("Duplicates found in Dictionary");
					}
					else
					{
						assets.Add(asset.name, asset);
					}
				}
			}
		}

		/// <summary>
		/// Gets the asset for the given name.
		/// </summary>
		/// <returns>The asset or null if name == NONE or the asset wasn't found.</returns>
		/// <param name="name">Asset name.</param>
		public GameObject GetAssetByName(string name)
		{
#if UNITY_EDITOR
			if (name == "NONE") return null;
			if (items == null) return null;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == null)
				{
					// TODO: Auto remove
					Debug.LogError("Empty items found in the dictionary, they must be removed");
					return null;
				}
			}

			GameObject item = items.Where(i => name == i?.name).FirstOrDefault();
			if (item != null) return item;
#endif
			if (assets == null) return null;
			if (assets.ContainsKey(name)) return assets[name];
			return null;
		}

		public string[] GetNames()
		{
			if (items == null) return new string[0];
			List<string> result = items.Select(i => i.name).ToList();
			result.Insert(0, "NONE");
			return result.ToArray();
		}

		public bool ContainsName(string name)
		{
			if (assets == null) return false;
			return assets.ContainsKey(name);
		}

		/// <summary>
		/// Get name for specific asset.
		/// </summary>
		/// <returns>The name of the asset.</returns>
		/// <param name="asset">Asset.</param>
		public string NameOfAsset(GameObject asset)
		{
			if (items == null) return null;
			if (asset == null) return "NONE";
			if (items.Contains(asset)) return asset.name;
			return null;
		}

		/// <summary>
		/// Add asset to dictionary.
		/// </summary>
		/// <returns>The new asset.</returns>
		/// <param name="asset">Assset.</param>
		public string AddNewAsset(GameObject asset)
		{
#if UNITY_EDITOR
			if (gameObject == null)
			{
				Debug.LogError("Unexpected configuration error in PrefabDictionary! Try creating a new one.");
				return null;
			}

			GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
			if (prefab != null)
			{
				if (asset == null) return null;
				if (items == null) items = new List<GameObject>();
				if (items.Contains(asset)) return asset.name;
				AssetDatabase.StartAssetEditing();
				items.Add(asset);
				PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
				AssetDatabase.SaveAssets();
				AssetDatabase.StopAssetEditing();
				return asset.name;
			}
			else
			{
				Debug.LogWarning(
					"Your dictionary isn't connected to a prefab. The settings will not be carried across scenes.");
			}
#endif
			if (asset == null) return null;
			if (items == null) items = new List<GameObject>();
			if (items.Contains(asset)) return asset.name;
			items.Add(asset);
			return asset.name;
		}

	}

}

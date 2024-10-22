﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;	
using System.Xml.Serialization;

namespace PlatformerPro 
{
	/// <summary>
	/// Stores data about an item type.
	/// </summary>
	[System.Serializable]
	public class ItemTypeData
	{

		/// <summary>
		/// Unique ID for this type.
		/// </summary>
		public string typeId;

		/// <summary>
		/// High-level class of the item, how does it fit in invetory, etc.
		/// </summary>
		public ItemClass itemClass;

		/// <summary>
		/// High-level behaviour of the item (WEAPON, POWER_UP, etc).
		/// </summary>
		public ItemBehaviour itemBehaviour;

		/// <summary>
		/// The items human readable name.
		/// </summary>
		public string humanReadableName;

        /// <summary>
        /// The items human readable description.
        /// </summary>
        public string description;

        /// <summary>
        /// Maximum number of items that can be in a single inventory slot.
        /// </summary>
        public int maxPerStack;

		/// <summary>
		/// Maximum number of items that can be carried.
		/// </summary>
		public int itemMax;

		/// <summary>
		/// How many items the character starts with.
		/// </summary>
		public int startingCount;

		/// <summary>
		/// The slot the item occupies, or null for none.
		/// </summary>
		public string slot;

		/// <summary>
		/// Damage type override.
		/// </summary>
		public DamageType damageType;

		/// <summary>
		/// The damage multiplier.
		/// </summary>
		public float damageMultiplier = 1.0f;

		/// <summary>
		/// The speed multiplier.
		/// </summary>
		public float weaponSpeedMultiplier = 1.0f;

		/// <summary>
		/// Do modifiers apply to both projectiles and melee attacks or just melee attacks.  WARNING: This is currently not used.
		/// </summary>
		public bool applyModifiersToProjectiles;

		/// <summary>
		/// The max durability of the item.
		/// </summary>
		public int maxDurability;

		/// <summary>
		/// Score to add to
		/// </summary>
		public string scoreType = "Default";

		/// <summary>
		/// how much to scroe when collected.
		/// </summary>
		public int scoreOnCollect;

		/// <summary>
		/// How much to score on consume.
		/// </summary>
		public int scoreOnConsume;

		/// <summary>
		/// For power-ups how long does the effect last.
		/// </summary>
		public float effectDuration;

		/// <summary>
		/// Does damage cause this effect to reset (for power-ups only).
		/// </summary>
		public bool resetEffectOnDamage;

		/// <summary>
		/// Does this item make the player invulnerable.
		/// </summary>
		public bool invulnerability;

		/// <summary>
		/// Details of damage immunity.
		/// </summary>
		public List <DamageImmunity> damageImmunity;

		/// <summary>
		/// Set a custom Jump Height multiplier.
		/// </summary>
		public float jumpHeightMultiplier = 1.0f;

		/// <summary>
		/// How many jumps does this power up enable?
		/// </summary>
		public int jumpCount;

		/// <summary>
		/// Set a custom Move Speed multiplier.
		/// </summary>
		public float moveSpeedMultiplier = 1.0f;

		/// <summary>
		/// Set a custom Run Speed multiplier.
		/// </summary>
		public float runSpeedMultiplier = 1.0f;

		/// <summary>
		/// Set a custom acceleration multiplier.
		/// </summary>
		public float accelerationMultiplier = 1.0f;

        /// <summary>
        /// How much health does this item add.
        /// </summary>
        public int healthAdjustment;

        /// <summary>
        /// How much consuming or wearing this item adds to max health.
        /// </summary>
        public int maxHealthAdjustment;

        /// <summary>
        /// How much consuming this adds to current breath.
        /// </summary>
        public int breathAdjustment;

        /// <summary>
        /// Icon used for the item in UI.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        protected Sprite icon;

		/// <summary>
		/// Name of sprite to use for icon in UI.
		/// </summary>
		public string iconSpriteName;

		/// <summary>
		/// Sprite used in game for the item.
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		protected Sprite inGameSprite;

		/// <summary>
		/// Name of sprite to use in game.
		/// </summary>
		public string inGameSpriteName;

		/// <summary>
		/// If true the item can be dropped from the inventory.
		/// </summary>
		public bool allowDrop;

		/// <summary>
		/// Prefab to use when item is dropped.
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		protected GameObject dropPrefab;

		/// <summary>
		/// Name of prefab to use when item is dropped.
		/// </summary>
		public string dropPrefabName;

		/// <summary>
		/// Default values for custom properties.
		/// </summary>
		public List<CustomItemProperty> defaultProperties;

		/// <summary>
		/// Price to buy this item.
		/// </summary>
		public int price;

		/// <summary>
		/// Item level. Used when comparing item to other items.
		/// </summary>
		public int level;

		/// <summary>
		/// If an item is EQUIPABLE and sits in an inventory stack do we equip the entire stack or just one.
		/// </summary>
		public bool equipAll;

        /// <summary>
        /// If a CONSUMABLE is in the inventory and we click it does it consume it.
        /// </summary>
        public bool clickToConsume;

        /// <summary>
        /// If an EQUIPPABLE items is equipped can we consume it?
        /// </summary>
        public bool consumableOnceEquipped;

        /// <summary>
        /// If true item upgrades are comulative. If false the upgrade applied is the same regardless of the number of items.
        /// </summary>
        public bool upgradesStack;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformerPro.ItemTypeData"/> class.
        /// </summary>
        public ItemTypeData() 
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformerPro.ItemTypeData"/> class by cloning an existing instance.
		/// </summary>
		/// <param name="original">Object to clone.</param>
		public ItemTypeData(ItemTypeData original) 
		{
			typeId = original.typeId;
			itemClass = original.itemClass;
			itemBehaviour = original.itemBehaviour;
			humanReadableName = original.humanReadableName;
            description = original.description;
            itemMax = original.itemMax;
			maxPerStack = original.maxPerStack;
			startingCount = original.startingCount;
			slot = original.slot;
			damageType = original.damageType;
			damageMultiplier = original.damageMultiplier;
			weaponSpeedMultiplier = original.weaponSpeedMultiplier;
			applyModifiersToProjectiles = original.applyModifiersToProjectiles;
			maxDurability = original.maxDurability;
			scoreType = original.scoreType;
			scoreOnCollect = original.scoreOnCollect;
			scoreOnConsume = original.scoreOnConsume;
			effectDuration = original.effectDuration;
			resetEffectOnDamage = original.resetEffectOnDamage;
			invulnerability = original.invulnerability;
			moveSpeedMultiplier = original.moveSpeedMultiplier;
			runSpeedMultiplier = original.runSpeedMultiplier;
			jumpHeightMultiplier = original.jumpHeightMultiplier;
			jumpCount = original.jumpCount;
            healthAdjustment = original.healthAdjustment;
            maxHealthAdjustment = original.maxHealthAdjustment;
            accelerationMultiplier = original.accelerationMultiplier;
			price = original.price;
			level = original.level;
            equipAll = original.equipAll;
            clickToConsume = original.clickToConsume;
            consumableOnceEquipped = original.consumableOnceEquipped;
            upgradesStack = original.upgradesStack;
            
            damageImmunity = new List<DamageImmunity> ();
			if (original.damageImmunity != null)
			{
				foreach (DamageImmunity di in original.damageImmunity)
				{
					damageImmunity.Add (new DamageImmunity (di));
				}
			}
			defaultProperties = new List<CustomItemProperty> ();
			if (original.defaultProperties != null)
			{
				foreach (CustomItemProperty ci in defaultProperties)
				{
					defaultProperties.Add (new CustomItemProperty (ci));
				}
			}
		}

		/// <summary>
		/// Gets the Icon sprite.
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		public Sprite Icon
		{
			get
			{
				if (icon == null && iconSpriteName != null)
				{
					icon = SpriteDictionary.GetSprite (iconSpriteName);
				}
				return icon;
			}
			set
			{
#if UNITY_EDITOR
				icon = value;
				if (icon != null)
				{
					iconSpriteName = SpriteDictionary.AddSprite(icon);
				} 
				else
				{
					iconSpriteName = null;
				}
#else
				Debug.LogError("Set should only be called from the editor");
#endif
			}
		}

		/// <summary>
		/// Gets the IN Game sprite.
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		public Sprite InGameSprite
		{
			get
			{
				if (inGameSprite == null && inGameSpriteName != null)
				{
					inGameSprite = SpriteDictionary.GetSprite (inGameSpriteName);
				}
				return inGameSprite;
			}
			set
			{
#if UNITY_EDITOR
				inGameSprite = value;
				if (inGameSprite != null)
				{
					inGameSpriteName = SpriteDictionary.AddSprite(inGameSprite);
				} 
				else
				{
					inGameSpriteName = null;
				}
#else
				Debug.LogError("Set should only be called from the editor");
#endif
			}
		}

		/// <summary>
		/// Gets the Drop Prefab.
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		public GameObject DropPrefab
		{
			get
			{
				if (dropPrefab == null && dropPrefabName != null)
				{
					dropPrefab = PrefabDictionary.FindOrCreateInstance().GetAssetByName(dropPrefabName);
				}
				return dropPrefab;
			}
			set
			{
				#if UNITY_EDITOR
				dropPrefab = value;
				if (dropPrefab != null)
				{
					dropPrefabName = PrefabDictionary.FindOrCreateInstance ().AddNewAsset (dropPrefab);
				} 
				else
				{
					dropPrefabName = null;
				}
				#else
				Debug.LogError("Set should only be called from the editor");
				#endif
			}
		}


		/// <summary>
		/// Load item data from the specified location.
		/// </summary>
		/// <param name="location">Location.</param>
		public static List<ItemTypeData> Load (string location) {
			try {
				byte[] itemTypeData = File.ReadAllBytes(location);
                return Load(itemTypeData);
			} catch (System.Exception ex) {
				Debug.LogError ("Failed to load item data from: " + location + " with error: " + ex.Message);
			}
			return new List<ItemTypeData>();
		}

        /// <summary>
        /// Load item data from the specified byte stream.
        /// </summary>
        /// <param name="itemTypeData">Item type data as byte array.</param>
        public static List<ItemTypeData> Load(byte[] itemTypeData)
        {
	        List<ItemTypeData> result = new List<ItemTypeData>();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ItemTypeData>));
                using (MemoryStream reader = new MemoryStream(itemTypeData))
                {
                    result = (List<ItemTypeData>)serializer.Deserialize(reader);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load item data from byte stream with error: " + ex.Message);
            }
            return result;
        }

    }
}
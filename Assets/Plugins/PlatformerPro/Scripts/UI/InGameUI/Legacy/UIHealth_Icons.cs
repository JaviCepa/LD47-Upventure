﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace PlatformerPro.Extras
{
	/// <summary>
	/// Shows images based on health.
	/// </summary>
	public class UIHealth_Icons : MonoBehaviour
	{

		/// <summary>
		/// The character health.
		/// </summary>
		[Tooltip ("Character health, if empty one will be searched for")]
		public CharacterHealth characterHealth;

		/// <summary>
		/// The sprites to use ordered from full to empty.
		/// </summary>
		[Tooltip ("The sprites to use ordered from low to high (but not empty, set empty below).")]
		public Sprite[] sprites;

		/// <summary>
		/// Sprite to show when health for this image is empty. Leave null for no sprite.
		/// </summary>
		[Tooltip ("Sprite to show when health for this image is empty. Leave empty for no sprite.")]
		public Sprite emptySprite;

		/// <summary>
		/// Cached copy of the health images.
		/// </summary>
		protected Image[] images;
		
		/// <summary>
		/// Reference to the character loader.
		/// </summary>
		protected PlatformerProGameManager characterLoader;

		/// <summary>
		/// Unity start hook.
		/// </summary>
		void Start()
		{
			Init ();
		}

		/// <summary>
		/// Do the destroy actions.
		/// </summary>
		void OnDestroy()
		{
			if (characterLoader != null)
			{
				characterLoader.CharacterLoaded -= HandleCharacterLoaded;
			}
			if (characterHealth != null)
			{
				characterHealth.Character .Respawned -= HandleRespawned;
				characterHealth.Healed -= HandleHealed;
				characterHealth.Damaged -= HandleDamaged;
				characterHealth.Loaded -= HandleLoaded;
			}
		}

		/// <summary>
		/// Registers the listeners.
		/// </summary>
		virtual protected void RegisterListeners()
		{
			characterHealth.Character.Respawned += HandleRespawned;
			characterHealth.Healed += HandleHealed;
			characterHealth.MaxValuesUpdated += HandleHealed;
			characterHealth.Damaged += HandleDamaged;
			characterHealth.Loaded += HandleLoaded;
			UpdateImages ();
		}

		void HandleRespawned (object sender, CharacterEventArgs e)
		{
			UpdateImages ();
		}
		
		/// <summary>
		/// Handles initial load.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandleLoaded (object sender, System.EventArgs e)
		{
			UpdateImages ();
		}

		/// <summary>
		/// Handles health change.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		virtual protected void HandleHealed (object sender, HealedEventArgs e)
		{
			UpdateImages ();
		}

		/// <summary>
		/// Handles health change.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		virtual protected void HandleDamaged (object sender, DamageInfoEventArgs e)
		{
			UpdateImages ();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		virtual protected void Init()
		{
			images = GetComponentsInChildren<Image> ();
			if (images == null || images.Length == 0) Debug.LogWarning ("No health images found by UIHealth_Icons. These should be children of the UIHealth_Icons GameObject.");
			else if (sprites == null || sprites.Length == 0) Debug.LogWarning ("No health images found.");
			else 
			{
				if (characterHealth == null) 
				{
					characterLoader = PlatformerProGameManager.Instance;
					characterLoader.CharacterLoaded += HandleCharacterLoaded;
				}
				if (characterHealth != null)
				{
					if (images.Length * sprites.Length != characterHealth.MaxHealth) Debug.LogWarning("Number of images times number of sprites should match the MaxHealth");
				}
			}
			if (characterHealth != null) RegisterListeners();
		}

		/// <summary>
		/// Handles the character being loaded.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		virtual protected void HandleCharacterLoaded (object sender, CharacterEventArgs e)
		{
			characterHealth = e.Character.GetComponent<CharacterHealth>();
			if (characterHealth == null) Debug.LogWarning ("The loaded character doesn't have a character health!");
			if (characterHealth != null) RegisterListeners();
		}

		/// <summary>
		/// Updates the images after a one frame delay.
		/// </summary>
		virtual protected void UpdateImages()
		{
			// Wait one frame so health gets updated
			StartCoroutine(DoUpdateImages());
		}
		
		/// <summary>
		/// Updates the images.
		/// </summary>
		virtual protected IEnumerator DoUpdateImages()
		{
			yield return true;
			int sprite = 0;
			int image = 0;
			bool spriteSet = false;
			for (int i = 0; i < characterHealth.MaxHealth; i++)
			{
				if (i >= (images.Length * sprites.Length))
				{
					Debug.LogWarning("Unable to show max health as there wasn't enough sprites");
					break;
				}
				if (sprite >= sprites.Length) 
				{
					image++;
					sprite = 0;
					spriteSet = false;
				}
				images[image].enabled = true;
				if (i < characterHealth.CurrentHealth)
				{
					images[image].enabled = true;
					images[image].sprite = sprites[sprite];
					
					spriteSet = true;
				}
				else if (!spriteSet)
				{
					if (emptySprite == null) images[image].enabled = false;
					else images[image].sprite = emptySprite;
				}
				sprite++;
			}
			// Disable any sprites bigger than max health
			for (int i = (characterHealth.MaxHealth / sprites.Length); i < images.Length; i++)
			{
				images[i].enabled = false;
			}
		}
	}
}
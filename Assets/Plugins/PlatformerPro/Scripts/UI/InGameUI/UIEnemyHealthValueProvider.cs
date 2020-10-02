using UnityEngine;
using System.Linq;

namespace PlatformerPro.Extras
{
	/// <summary>
	/// Provides the enemy health value for rendering by IValueRenderer components.
	/// </summary>
	public class UIEnemyHealthValueProvider : UIValueProvider
	{
		[SerializeField]
		protected Enemy enemy;

		public Enemy Enemy
		{
			set
			{
				if (enemy != null)
				{
					enemy.Damaged -= HandleChange;
					enemy.Died -= HandleChange;
				}
				enemy = value;
				if (enemy != null)
				{
					enemy.Damaged += HandleChange;
					enemy.Died += HandleChange;
				}
			}
		}
		/// <summary>
		/// Gets the header string used to describe the component.
		/// </summary>
		/// <value>The header.</value>
		override public string Header
		{
			get
			{
				return "Provides the enemies health as a value to be rendered by one or more IValueRenderers.";
			}
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		override protected void PostInit()
		{
			// If the game is already ready simulate character load
			if (PlatformerProGameManager.Instance.GamePhase == GamePhase.READY)
			{
				renderers = GetComponentsInChildren(typeof(IValueRenderer)).Cast<IValueRenderer>().ToList();
				HandleCharacterLoaded(null, new CharacterEventArgs(PlatformerProGameManager.Instance.GetCharacterForPlayerId(playerId)));
			}
			else
			{ 
				PlatformerProGameManager.Instance.CharacterLoaded += HandleCharacterLoaded;
				renderers = GetComponentsInChildren(typeof(IValueRenderer)).Cast<IValueRenderer>().ToList();
			}
		}

		/// <summary>
		/// Get character reference when character loaded.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		override protected void HandleCharacterLoaded(object sender, CharacterEventArgs e)
		{
			if (enemy == null)
			{
				Enemy = GetComponentInParent<Enemy>();	
			}
		}
		
		/// <summary>
		/// Gets the raw value.
		/// </summary>
		/// <value>The value.</value>
		override public object RawValue
		{
			get
			{
				if (enemy == null) return "";
				return enemy.health;
			}
		}

		/// <summary>
		/// Gets the int value.
		/// </summary>
		/// <value>The int value.</value>
		override public int IntValue
		{
			get
			{
				if (enemy == null) return 0;
				return enemy.health;
			}
		}

		/// <summary>
		/// Gets the int value.
		/// </summary>
		/// <value>The int value.</value>
		override public int IntMaxValue
		{
			get
			{
				if (enemy == null) return 0;
				return enemy.StartingHealth;
			}
		}

		/// <summary>
		/// Gets the value as percentage between 0 (0%) and 1 (100%).
		/// </summary>
		/// <value>The value.</value>
		override public float PercentageValue
		{
			get { return (float)enemy.health / (float)enemy.StartingHealth; }
		}

	}
}
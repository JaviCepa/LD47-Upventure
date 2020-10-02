using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PlatformerPro.Extras
{
	/// <summary>
	/// Renders a UI value provider as icons.
	/// </summary>
	public class UIIconRenderer : PlatformerProMonoBehaviour, IValueRenderer 
	{

        /// <summary>
        /// The sprites to use ordered from full to empty.
        /// </summary>
        [Tooltip("The sprites to use ordered from low to high (but not empty, set empty below).")]
        public Sprite[] sprites;

        /// <summary>
        /// Sprite to show when value for this image is empty. Leave null for no sprite.
        /// </summary>
        [Tooltip("Sprite to show when value for this image is empty. Leave empty for no sprite.")]
        public Sprite emptySprite;

        /// <summary>
        /// Cached copy of the health images.
        /// </summary>
        protected Image[] images;


		/// <summary>
		/// Gets the header string used to describe the component.
		/// </summary>
		/// <value>The header.</value>
		override public string Header
		{
			get
			{
				return "Renders the parent UIValueProvider as icons.";
			}
		}


		/// <summary>
		/// The text comopnent to update..
		/// </summary>
		protected Text myText;

		/// <summary>
		/// Unity Start() hook.
		/// </summary>
		void Start()
		{
            Init();
		}

        /// <summary>
        /// Init this instance.
        /// </summary>
        virtual protected void Init()
        {
            images = GetComponentsInChildren<Image>();
            if (images == null || images.Length == 0) Debug.LogWarning("No health images found by UIHealth_Icons. These should be children of the UIHealth_Icons GameObject.");
            else if (sprites == null || sprites.Length == 0) Debug.LogWarning("No health images found.");
        }

        /// <summary>
        /// Render the specified value.
        /// </summary>
        /// <param name="provider">Value.</param>
        public void Render(UIValueProvider provider)
		{
			if (provider == null || provider.RawValue == null) return;
            int sprite = 0;
            int image = 0;
            bool spriteSet = false;
            for (int i = 0; i < provider.IntMaxValue; i++)
            {
                if (sprite >= sprites.Length)
                {
                    image++;
                    sprite = 0;
                    spriteSet = false;
                }
                images[image].enabled = true;
                if (i < provider.IntValue)
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
            for (int i = ((provider.IntMaxValue + 1) / sprites.Length); i < images.Length; i++)
            {
                images[i].enabled = false;
            }
        }
	}
}
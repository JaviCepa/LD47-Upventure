using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PlatformerPro.Extras
{
	/// <summary>
	/// Shows dialog by slowly revealing text with option to fast forward.
	/// </summary>
	[RequireComponent (typeof(Text))]
	public class UIDialogTyper : MonoBehaviour
	{
		/// <summary>
		/// The reveal speed in characters per second.
		/// </summary>
		[Tooltip ("The reveal speed in characters per second.")]
		public float revealSpeed = 8.0f;

		/// <summary>
		/// The fast forward reveal speed in characters per second.
		/// </summary>
		[Tooltip ("The fast forward reveal speed in characters per second.")]
		public float fastForwardRevealSpeed = 24.0f;
	
		/// <summary>
		/// Input to use.
		/// </summary>
		[Tooltip ("Input to use, if null will attempt to find Character input.")]
		public Input input;

		/// <summary>
		/// If true the fast forward mode will also close this dialog when it hits the end of the dialog.
		/// </summary>
		[Tooltip ("If true the fast forward mode will also close this dialog when it hits the end of the dialog")]
		public bool fastForwardAutoClose;
		
		/// <summary>
		/// If true this is paused when the game is paused
		/// </summary>
		[Tooltip ("If true this is paused when the game is paused")]
		public bool pauseWhenGamePauses = true;
		
		/// <summary>
		/// Cached text component.
		/// </summary>
		protected Text text;

		/// <summary>
		/// Cached copy of full dialog.
		/// </summary>
		protected string dialogString;

		/// <summary>
		/// Current revealed position.
		/// </summary>
		protected float currentCharacterPosition;

		/// <summary>
		/// Have we initialised this dialog typer?
		/// </summary>
		protected bool initialised;

		/// <summary>
		/// Are we fast forwarding?
		/// </summary>
		protected bool isFastForward;

		/// <summary>
		/// Awe re ready to hide this dialog?
		/// </summary>
		protected bool readyToHide;

		/// <summary>
		/// Are we rady to hide this?
		/// </summary>
		public bool ReadyToHide => readyToHide;

		/// <summary>
		/// Are we rady to hide this?
		/// </summary>
		public bool AtEnd => (dialogString == null || ((int) currentCharacterPosition >= dialogString.Length));


		void Update()
		{
			if (pauseWhenGamePauses && TimeManager.Instance.Paused) return;
			if (input == null) return;
			if (dialogString == null || ((int)currentCharacterPosition >= dialogString.Length))  
			{
				if (FastForwardKeypressed())
				{
					readyToHide = true;
				}
			}
		}
		
		/// <summary>
		/// Unity start hook.
		/// </summary>
		virtual protected void Init()
		{
			if (input == null) 
			{
				Character character = (Character) FindObjectOfType(typeof(Character));
				if (character != null) 
				{
					input = character.Input;
				}
				else 
				{
					input = (Input) FindObjectOfType<Input>();
				}
				if (input == null) Debug.LogWarning("UIDialogTyper could not find input");
			}
			text = GetComponent<Text> ();
			dialogString = text.text;
			text.text = "";
			initialised = true;
		}

		/// <summary>
		/// Show this dialog.
		/// </summary>
		public void Show()
		{
			if (!initialised) Init ();
			StopAllCoroutines();
			StartCoroutine (RevealText());
		}

		/// <summary>
		/// Show this dialog with custom text.
		/// </summary>
		public void Show(string text)
		{
			if (!initialised) Init ();
			dialogString = text;
			StopAllCoroutines();
			StartCoroutine (RevealText());
		}
		
		/// <summary>
		/// Force hide this dialog.
		/// </summary>
		public void Hide()
		{
			StopAllCoroutines();
			if (text != null) text.text = "";
		}
		
		/// <summary>
		/// Reveal the text.
		/// </summary>
		protected IEnumerator RevealText()
		{
			readyToHide = false;
			isFastForward = false;
			text.text = "";
			currentCharacterPosition = 0;
			yield return true;
			while ((int)currentCharacterPosition <= dialogString.Length)
			{
				if (!pauseWhenGamePauses || !TimeManager.Instance.Paused)
				{
					FastForwardKeypressed();
					text.text = dialogString.Substring(0, (int) currentCharacterPosition);
					// Use Unity time so we can still show dialog when paused
					currentCharacterPosition += Time.deltaTime * (isFastForward ? fastForwardRevealSpeed : revealSpeed);
				}

				yield return true;
				
			}
			text.text = dialogString;
		}

		/// <summary>
		/// Checks for a fast forward keypress.
		/// </summary>
		/// <returns><c>true</c>, if forward key pressed, <c>false</c> otherwise.</returns>
		virtual protected bool FastForwardKeypressed()
		{
			if (input.JumpButton == ButtonState.DOWN) 
			{
				isFastForward = true;
				return true;
			}
			if (input.ActionButton == ButtonState.DOWN)
			{
				isFastForward = true;
				return true;
			}
			if (isFastForward && fastForwardAutoClose) return true;
			return false;
		}
	}
}

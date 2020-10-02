using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlatformerPro.Dialog;
using PlatformerPro.Extras;
using UnityEngine;
using UnityEngine.UI;
using XNode;

namespace PlatformerPro
{
    /// <summary>
    /// Drives UI and control for showing dialogs. If there's one of these in the scene you can use it to show all dialogs or you can
    /// set up different types of dialogs by including multiple instances. Dialog triggering events will drive the DialogSystem marked as
    /// default.
    /// </summary>
    public class DialogSystem : PlatformerProMonoBehaviour
    {
        public override string Header
        {
            get
            {
                return
                    "Drives UI and control for showing dialogs. If there's one of these in the scene you can use it to show all dialogs or you can " +
                    "set up different types of dialogs by including multiple instances. Dialog events will drive the DialogSystem marked as default unless you specify a dialog.";
            }
        }

        [Tooltip("Dialog tree to use by default.")]
        public DialogGraph defaultDialog;
        
        /// <summary>
        /// If true this is the default dialog system that will be used for default dialog actions.
        /// </summary>
        public bool isDefault = true;
        
        /// <summary>
        /// Content to show/hide when dialog starts/ends.
        /// </summary>
        [Header("UI Components")] 
        [Tooltip("Content to show/hide when dialog starts/ends")]
        public GameObject visibleContent;

       
        /// <summary>
        /// Content to show/hide when character is talking.  Can be null.
        /// </summary>
        [Header("UI Components - Character")] 
        [Tooltip("Content to show/hide when character is talking.  Can be null.")]
        public GameObject characterContent;

        /// <summary>
        /// Text field which holds the character dialog. This can be the same as the other text if you want to use a shared text area.
        /// </summary>
        [Tooltip(
            "Text field which holds the character dialog. This can be the same as the other text if you want to use a shared text area.")]
        public Text characterText;

        /// <summary>
        /// If non-null dialog graphs can update the character portrait by changing this image.
        /// </summary>
        [Tooltip ("If non-null dialog graphs can update the character portrait by changing this image.")]
        public Image characterPortrait;
        
        /// <summary>
        /// Content to show/hide when other is talking. Can be null.
        /// </summary>
        [Header("UI Components - Other")] 
        [Tooltip("Content to show/hide when other is talking. Can be null.")]
        public GameObject otherContent;

        /// <summary>
        /// Text field which holds the other dialog. This can be the same as the character text if you want to use a shared text area.
        /// </summary>
        [Tooltip(
            "Text field which holds the other dialog. This can be the same as the character text if you want to use a shared text area.")]
        public Text otherText;

        
        /// <summary>
        /// If non-null dialog graphs can update the character portrait by changing this image.
        /// </summary>
        [Tooltip ("If non-null dialog graphs can update the other portrait by changing this image.")]
        public Image otherPortrait;
        
        /// <summary>
        /// Content to show/hide when options are being displayed. Can be null.
        /// </summary>
        [Header("UI Components - Options")]
        [Tooltip("Content to show/hide when options are being displayed. Can be null.")]
        public GameObject optionContent;

        /// <summary>
        /// GameObject which has child GameObjects which have a Text Component. These will be used to show options. They are often part of a grid.
        /// </summary>
        [Tooltip(
            "GameObject which has child GameObjects which have a Text Component. These will be used to show options. They are often part of a grid.")]
        public GameObject optionTextParent;

        /// <summary>
        /// Color for text on the options.
        /// </summary>
        [Tooltip("Color for text on the options")]
        public Color optionColor = Color.white;

        /// <summary>
        /// Color for text on the selected option
        /// </summary>
        [Tooltip("Color for text on the selected option")]
        public Color selectedOptionColor = Color.yellow;

        /// <summary>
        /// GameObject to use as a pointer to the selected option (leave empty for no pointer). It will be moved to the same RectTransform position as the selected option.
        /// </summary>
        [Tooltip(
            "GameObject to use as a pointer to the selected option (leave empty for no pointer). It will be moved to the same RectTransform position as the selected option.")]
        public GameObject optionPointer;

        /// <summary>
        /// What animation should we trigger. Note this requires a Special Move Play Animation to be attached to each character. Use NONE to allow characters to keep moving during dialog.
        /// </summary>
        [Header("Behaviour")] public Input input;

        [Tooltip(
            "What animation should we trigger. Note this requires a Special Move Play Animation to be attached to each character. Use NONE to allow characters to keep moving during dialog.")]
        public AnimationState dialogAnimation = AnimationState.IDLE;

        /// <summary>
        /// "If true this dialog will trigger a pause - and still be functional while the game is paused. Note: this wont who pause menu.
        /// </summary>
        [Tooltip("If true this dialog will trigger a pause - and still be functional while the game is paused. Note: this wont who pause menu.")]
        public bool dialogTriggersPause;
        
        /// <summary>
        /// If true we show this dialog on start (technically on CharacterLoad).
        /// </summary>
        [Tooltip("If true we show this dialog on start.")]
        public bool showDialogOnStartup;
        
        /// <summary>
        /// Character that start dialog
        /// </summary>
        protected Character character;

        /// <summary>
        /// Cache of the texts found inside the option optionTextParent.
        /// </summary>
        protected Text[] optionTexts;
        
        /// <summary>
        /// Are we about to show, or showing options?
        /// </summary>
        protected OptionState optionState = OptionState.NONE;
        
        /// <summary>
        /// Which option is selected?
        /// </summary>
        protected int selectedOption;
        
        /// <summary>
        /// How many options in total?
        /// </summary>
        protected int optionCount;
        
        /// <summary>
        /// Relate options to option texts (included because you can hide options with conditions so its not a 1 to 1 mapping).
        /// </summary>
        protected List<int> optionIndexes;

        /// <summary>
        /// Where are we in the dialog graph?
        /// </summary>
        protected DialogNode currentNode;

        /// <summary>
        /// If we are using typers rather thn text fields, this is the typer reference for the character dialog.
        /// </summary>
        protected UIDialogTyper characterTyper;
        
        /// <summary>
        /// If we are using typers rather thn text fields, this is the typer reference for the other dialog.
        /// </summary>        
        protected UIDialogTyper otherTyper;

        /// <summary>
        /// Are we currently accepting input?
        /// </summary>
        protected bool shouldCheckForInput;

        /// <summary>
        /// Dialog graph we are currently showing.
        /// </summary>
        protected DialogGraph currentDialog;

        /// <summary>
        /// Sender for dialog events
        /// </summary>
        public event System.EventHandler<DialogEventArgs> DialogEvent;

        /// <summary>
        /// Raise the DialogEvents
        /// </summary>
        /// <param name="eventId"></param>
        protected void OnDialogEvent(string eventId)
        {
            if (DialogEvent != null) DialogEvent(this, new DialogEventArgs(eventId, character));
        }

        /// <summary>
        /// Unity start hook.
        /// </summary>
        void Start()
        {
            Init();
        }

        /// <summary>
        /// Unity update hook.
        /// </summary>
        void Update()
        {
            if (TimeManager.Instance.Paused && !dialogTriggersPause) return;
            if (shouldCheckForInput) CheckForInput();
        }

        virtual protected void Init()
        {
            if (Instance == null || isDefault) Instance = this;

            characterTyper = characterText.GetComponent<UIDialogTyper>();
            otherTyper = otherText.GetComponent<UIDialogTyper>();
            PlatformerProGameManager.Instance.CharacterLoaded += HandleCharacterLoaded;
            if (optionTextParent != null)
            {
                optionTexts = optionTextParent.GetComponentsInChildren<Text>();
                optionIndexes = new List<int>();
            }
        }

        /// <summary>
        /// If we start automtically we use this event handler to show the dialog.
        /// </summary>
        virtual protected void HandleCharacterLoaded(object sender, CharacterEventArgs e)
        {
            if (showDialogOnStartup) StartDialog(e.Character);
        }

        /// <summary>
        /// Show the default dialog.
        /// </summary>
        /// <param name="character">Character who started the dialog.</param>
        virtual public void StartDialog(Character character)
        {
            StartDialog(character, defaultDialog);
        }

        /// <summary>
        /// Show the specified dialog.
        /// </summary>
        /// <param name="character">Character who started the dialog.</param>
        /// <param name="dialog">Dialog to show.</param>
        virtual public void StartDialog(Character character, DialogGraph dialog)
        {

            // Early out allow only one dialog at a time
            if (currentDialog != null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Tried to start a dialog but one was already started");
#endif
                return;
            }

            if (dialogTriggersPause) TimeManager.Instance.Pause(false, true);
            currentDialog = dialog;
            this.character = character;
            input = character.Input;
            visibleContent.SetActive(true);
            if (dialogAnimation != AnimationState.NONE) 
            {
                foreach (Character c in PlatformerProGameManager.Instance.LoadedCharacters)
                {
                    if (c != null)
                    {
                        SpecialMovement_PlayAnimation specialMovement = c.GetComponentInChildren<SpecialMovement_PlayAnimation>();
                        if (specialMovement != null)
                        {
                            specialMovement.Play(dialogAnimation);
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Tried to play animation on character but no SpecialMovement_PlayAnimation was found");
                        }
                    }
                }
            }
            
            // Find entry point
            DialogEntryNode entry = (DialogEntryNode) currentDialog.nodes.Where(n => typeof(DialogEntryNode).IsInstanceOfType(n)).First();
            
            // There's only one output port, although it might have many connections
            List<NodePort> connections = entry.Outputs.First().GetConnections();
            foreach (NodePort n in connections)
            {
                DoNodeActions(n.node);
            }
            currentNode = (DialogNode) connections.First(n => typeof(DialogNode).IsInstanceOfType(n.node)).node;
            if (currentNode == null)
            {
                Debug.LogWarning("Dialog entry went straight to end, i.e. the DialogGraph has no dialog");
                EndDialog();
            }
            else
            {       
                ShowCurrentNode();
            }
        }

        virtual public void EndDialog() 
        {
            visibleContent.SetActive(false);
            currentDialog = null;
            currentNode = null;
            shouldCheckForInput = false;
            StartCoroutine(DoEndAfterDelay());
        }

        /// <summary>
        /// Pause after one frame to delay to consume the input.
        /// </summary>
        protected IEnumerator DoEndAfterDelay()
        {
            yield return true;
            yield return true;
            if (dialogTriggersPause) TimeManager.Instance.UnPause(false);  
            if (dialogAnimation != AnimationState.NONE) 
            {
                foreach (Character c in PlatformerProGameManager.Instance.LoadedCharacters)
                {
                    if (c != null)
                    {
                        SpecialMovement_PlayAnimation specialMovement = c.GetComponentInChildren<SpecialMovement_PlayAnimation>();
                        if (specialMovement != null)
                        {
                            specialMovement.StopAnimation();
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Tried to stop animation on character but no SpecialMovement_PlayAnimation was found");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Update the portrait for character.
        /// </summary>
        /// <param name="sprite">Sprite to use.</param>
        virtual public void UpdateCharacterPortrait(Sprite sprite)
        {
            if (characterPortrait != null) characterPortrait.sprite = sprite;
        }
        
        /// <summary>
        /// Update the portrait for other.
        /// </summary>
        /// <param name="sprite">Sprite to use.</param>
        virtual public void UpdateOtherPortrait(Sprite sprite)
        {
            if (otherPortrait != null) otherPortrait.sprite = sprite;
        }
        
        virtual protected void Step()
        {
            Step(0);
        }
        
        virtual public void Step(int option)
        {
            if (currentNode == null)
            {
                Debug.LogError("Dialog has not been initialised");
                return;
            }
            if (typeof(DialogNode).IsInstanceOfType(currentNode))
            {
                optionState = OptionState.NONE;
                // There's only one output port, although it might have many connections

                List<NodePort> connections = (optionIndexes.Count > selectedOption) ? currentNode.GetOutputForSelection(optionIndexes[selectedOption])?.GetConnections()  : currentNode.GetOutputForSelection(0)?.GetConnections();
                foreach (NodePort n in connections)
                {
                    DoNodeActions(n.node);
                }
                currentNode = (DialogNode) connections.FirstOrDefault(n => n.node is DialogNode)?.node;
                // If we have options to show, show them
                if (typeof(DialogOptionNode).IsInstanceOfType(currentNode))
                {
                    optionState = OptionState.READY_TO_SHOW;
                }
                if (currentNode == null)
                {
                    EndDialog();
                }
                else
                {       
                    ShowCurrentNode();
                }
                return;
            }
            Debug.LogError("Node type is not supported");
        }

        virtual protected void CheckForInput()
        {
            // TODO Option select
            if (optionState == OptionState.SELECTING)
            { 
                int originalOption = selectedOption;
                if (input.VerticalAxisDigital == 1 && input.VerticalAxisState == ButtonState.DOWN)
                {
                    selectedOption--;
                    if (selectedOption < 0) selectedOption = optionCount - 1;
                    UpdateOptionDisplay(originalOption, selectedOption);
                }
                else if (input.VerticalAxisDigital == -1 && input.VerticalAxisState == ButtonState.DOWN)
                {
                    selectedOption++;
                    if (selectedOption >= optionCount) selectedOption = 0;
                    UpdateOptionDisplay(originalOption, selectedOption);
                }
                else if (input.JumpButton == ButtonState.DOWN || input.ActionButton == ButtonState.DOWN)
                {
                    Step(selectedOption);
                }
            }
            else if (characterTyper != null && currentNode.IsCharacter)
            {
                if (characterTyper.ReadyToHide)
                {
                    if (optionState == OptionState.READY_TO_SHOW)
                    {
                        ShowOptions(((DialogOptionNode)currentNode).options);
                    }
                    else
                    {
                        Step();
                    }
                }  
                else if (characterTyper.AtEnd && optionState == OptionState.READY_TO_SHOW)
                {
                    ShowOptions(((DialogOptionNode)currentNode).options);
                }
            }
            else if (otherTyper != null && !currentNode.IsCharacter)
            {
                if (otherTyper.ReadyToHide)
                {
                    if (optionState == OptionState.READY_TO_SHOW)
                    {
                        ShowOptions(((DialogOptionNode)currentNode).options);
                    }
                    else
                    {
                        Step(selectedOption);
                    }
                }
                else if (otherTyper.AtEnd && optionState == OptionState.READY_TO_SHOW)
                {
                    ShowOptions(((DialogOptionNode)currentNode).options);
                }
            }
            else 
            {
                if (optionState == OptionState.READY_TO_SHOW)
                {
                    ShowOptions(((DialogOptionNode)currentNode).options);
                }
                else if (input.JumpButton == ButtonState.DOWN || input.ActionButton == ButtonState.DOWN)
                {
                    Step(selectedOption);
                }
            }
        }
        
        virtual protected void DoNodeActions(Node node)
        {
            if (node is ActionNode)
            {
                ((ActionNode)node).DoAction(character, this);
            }
           if (node is EventResponseNode responseNode)
           {
               responseNode.DoEvent(gameObject, character);
           }
           if (node is DialogEventNode entryNode)
           {
               OnDialogEvent(entryNode.eventId);
           }
        }

        virtual protected void ShowCurrentNode()
        {
            if ((currentNode).IsCharacter)
            {
                ShowCharacterContent(currentNode.dialogText);
            }
            else
            {
                ShowOtherContent(currentNode.dialogText);
            }
            shouldCheckForInput = true;
        }

        virtual protected void ShowCharacterContent(string text)
        {
            // TODO Effects
            if (characterContent != null) characterContent.SetActive(true);
            if (characterTyper != null)
            {
                characterTyper.Show(text);
            }
            else
            {
                characterText.text = text;
            }
            HideOtherContent();
            HideOptions();
        }

        virtual protected void ShowOptions(OptionWithCondition[] options)
        {
           
            if (optionContent != null) optionContent.SetActive(true);
            optionState = OptionState.SELECTING;
            selectedOption = 0;
            optionCount = options.Length;
            optionIndexes.Clear();
            if (optionTexts != null)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i].condition == DialogCondition.NONE ||
                        (options[i].condition == DialogCondition.MUST_HAVE_ITEM && character.ItemManager.HasItem(options[i].itemId)) ||
                        (options[i].condition == DialogCondition.MUST_NOT_HAVE_ITEM && !character.ItemManager.HasItem(options[i].itemId)))
                    {
                        optionIndexes.Add(i);
                    }
                }
                if (optionTexts.Length < optionIndexes.Count) Debug.LogWarning("Not enough option Text elements to show all the options");
                for (int i = 0; i < optionTexts.Length; i++)
                {
                    if (i < optionIndexes.Count)
                    {
                        optionTexts[i].text = options[optionIndexes[i]].optionText;
                    }
                    else
                    {
                        optionTexts[i].text = "";
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Tried to show options but couldn't find any Text elements in the optionTextParent: {optionTextParent}");
            }

            StartCoroutine(UpdateOptionDisplayAfterDelay(0, 0));
        }
        
        virtual protected IEnumerator UpdateOptionDisplayAfterDelay(int previous, int current)
        {
            // Delay one frame to give time for things to start
            yield return true;
            UpdateOptionDisplay(previous, current);
        }
        
        virtual protected void UpdateOptionDisplay(int previous, int current)
        {
            // Note: override this function to provide effects, animation, etc
            for (int i = 0; i < optionTexts.Length; i++)
            {
                optionTexts[i].color = (i == current) ? selectedOptionColor : optionColor;
            }

            if (optionPointer != null && current < optionTexts.Length)
            {
                optionPointer.transform.position = optionTexts[current].transform.position;
            }
        }
        
        virtual protected void ShowOtherContent(string text)
        {
            // TODO Effects
            if (otherContent != null) otherContent.SetActive(true);
            if (otherTyper != null)
            {
                otherTyper.Show(text);
            }
            else
            {
                otherText.text = text;
            }

            HideCharacterContent();
            HideOptions();
        }
        
        virtual protected void HideCharacterContent()
        {
            // TODO Effects
            if (characterContent != null) characterContent.SetActive(false);
            characterText.text = "";
            if (characterTyper != null) characterTyper.Hide();
        }
        
        virtual protected void HideOtherContent( )
        {
            if (otherContent != null) otherContent.SetActive(false);
            otherText.text = "";
            if (otherTyper != null) otherTyper.Hide();
        }
        
        virtual protected void HideOptions( )
        {
           if (optionContent != null) optionContent.SetActive(false);
        }
        
        #region Static methods and variables

        public static DialogSystem Instance { get; protected set; }
        #endregion
    }

    public enum OptionState
    {
        NONE,
        READY_TO_SHOW,
        SELECTING
    }
  
}
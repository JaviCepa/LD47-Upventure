

namespace PlatformerPro
{
    public class DialogEventArgs : CharacterEventArgs
    {

        public string EventId { get; protected set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformerPro.DialogEventArgs"/> class.
        /// </summary>
        /// <param name="eventId">Dialog event ID.</param>
        public DialogEventArgs (string eventId) : base()
        {
            EventId = eventId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformerPro.DialogEventArgs"/> class.
        /// </summary>
        /// <param name="eventId">Dialog event ID.</param>
        /// <param name="character">Character.</param>
        public DialogEventArgs(string eventId, Character character) : base (character)
        {
            EventId = eventId;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformerPro.DialogEventArgs"/> class.
        /// </summary>
        /// <param name="eventId">Dialog event ID.</param>
        /// <param name="character">Character.</param>
        /// <param name="playerId">Id of the player (0 for player 1).</param>
        public DialogEventArgs(string eventId, Character character, int playerId) : base (character, playerId)
        {
            EventId = eventId;
        }
    }
}
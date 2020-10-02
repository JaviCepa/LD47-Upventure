namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// Send an enemy AI event.
    /// </summary>
    public class EnemyEventAction : EnemyAction
    {

        /// <summary>
        /// ID of the event to send.
        /// </summary>
        public string eventId;
        
        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        override public void DoAction(Enemy enemy)
        {
            enemy.OnEnemyAIEvent(eventId);
        }

    }
}
namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// Sets counter to zero.
    /// </summary>
    public class ResetCounter : EnemyAction
    {
        
        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        override public void DoAction(Enemy enemy)
        {
            if (enemy.AI is EnemyAI_AIGraph ai)
            {
                ai.Counter = 0;
            }
        }

    }
}
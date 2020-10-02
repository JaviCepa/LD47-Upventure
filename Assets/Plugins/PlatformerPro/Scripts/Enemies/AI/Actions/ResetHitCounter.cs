namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// Sets hit counter to zero.
    /// </summary>
    public class ResetHitCounter : EnemyAction
    {
        
        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        override public void DoAction(Enemy enemy)
        {
            if (enemy.AI is EnemyAI_AIGraph ai)
            {
                ai.ResetHitCounter();
            }
        }

    }
}
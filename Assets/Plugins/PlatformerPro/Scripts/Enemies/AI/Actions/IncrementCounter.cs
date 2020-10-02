namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// Increments counter by 1.
    /// </summary>
    public class IncrementCounter : EnemyAction
    {
        
        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        override public void DoAction(Enemy enemy)
        {
            if (enemy.AI is EnemyAI_AIGraph ai)
            {
                ai.Counter++;
            }
        }

    }
}
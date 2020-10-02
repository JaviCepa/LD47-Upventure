namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// Mkaes enemy vulnerable.
    /// </summary>
    public class MakeVulnerable : EnemyAction
    {
        
        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        override public void DoAction(Enemy enemy)
        {
            enemy.MakeVulnerable();
        }

    }
}
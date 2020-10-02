namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// Makes enemy invulnerable for the given time.
    /// </summary>
    public class MakeInvulnerable : EnemyAction
    {

        /// <summary>
        /// Time to make enemy invulnerable.
        /// </summary>
        public float invulnerableTime = 1.0f;
        
        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        override public void DoAction(Enemy enemy)
        {
            enemy.MakeInvulnerable(invulnerableTime);
        }

    }
}
using XNode;

namespace PlatformerPro.AI
{
    /// <summary>
    /// An enemy node which changes the phase.
    /// </summary>
    public class ChangePhaseNode : Node
    {
        [Input (ShowBackingValue.Never)] public EnemyNode entry;
        
        /// <summary>
        /// Name of the next phase to go to.
        /// </summary>
        public string nextPhase;
        
    }
}
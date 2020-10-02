using XNode;

namespace PlatformerPro.AI
{
    /// <summary>
    /// An enemy node which connects directly to the next movement.
    /// </summary>
    public class EnemyStepNode : EnemyNode
    {
        [Output] public EnemyNode exit;

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "exit") return exit;
            return null;
        }
    }
}
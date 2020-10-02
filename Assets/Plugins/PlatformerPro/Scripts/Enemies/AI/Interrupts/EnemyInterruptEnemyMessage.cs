using System.Linq;
using XNode;

namespace PlatformerPro.AI.Interrupts
{
    /// <summary>
    /// An interrupt that happens when a certain health value is reached via damage.
    /// </summary>
        [NodeTint(0.75f,0.3f,0.3f)]
    public class EnemyInterruptEnemyMessage : Node, IProcessableEnemyNode
    {
        [Output] public EnemyNode exit;

        /// <summary>
        /// Health value to trigger at.
        /// </summary>
        public string message;
        
        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "exit") return exit;
            return null;
        }
        
        virtual public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault();
        }
    }

}
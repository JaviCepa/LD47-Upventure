using System.Linq;
using XNode;

namespace PlatformerPro.AI.Actions
{
    /// <summary>
    /// base class for enemy actions.
    /// </summary>
    [NodeTint(0.3f,0.5f,0.3f)]
    public abstract class EnemyAction : Node
    {
        [Input (ShowBackingValue.Never)] public EnemyNode entry;

        virtual public NodePort GetOutputForSelection(int selection)
        {
            return Outputs.FirstOrDefault();
        }

        /// <summary>
        /// Do this action
        /// </summary>
        /// <param name="enemy"></param>
        abstract public void DoAction(Enemy enemy); 
        
        // Use this for initialization
        protected override void Init()
        {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return base.GetValue(port);
        }

    }
}

using XNode;
namespace PlatformerPro.AI
{
    /// <summary>
    /// Interface for enemy nodes that can return their outputs.
    /// </summary>
    public interface IProcessableEnemyNode
    { 
        NodePort GetOutputForSelection(int selection);
    }
}
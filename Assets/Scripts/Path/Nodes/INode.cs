using UnityEngine;

namespace Retrover.Path
{
    public interface INode
    {
        Vector3 Position { get; set; }
        Vector3 Handle1 { get; set; }
        Vector3 Handle2 { get; set; }
        NodeType Type { get; }
    }
}
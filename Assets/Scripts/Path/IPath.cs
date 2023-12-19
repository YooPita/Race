using System.Collections.Generic;

namespace Retrover.Path
{
    public interface IPath
    {
        float Length { get; }
        int NodesCount { get; }
        int SegmentsCount { get; }

        void AddNode(INode node);
        void Bake();
        void RemoveNode(INode node);
        void RemoveNodeAtIndex(int index);
        PathTransform Transform(float length);
        public List<INode> NodesCopy();
        public INode NodeAtIndex(int index);
        ISegment SegmentAtIndex(int index);
    }
}
using Retrover.Path;
using System.Collections.Generic;

public class RoadChunk
{
    private ISegment _segment;
    private float _roadWidth;

    public RoadChunk(ISegment segment, float roadWidth)
    {
        _segment = segment;
        _roadWidth = roadWidth;
    }
}
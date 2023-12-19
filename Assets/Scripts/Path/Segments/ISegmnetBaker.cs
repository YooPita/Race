namespace Retrover.Path
{
    public interface ISegmnetBaker
    {
        ISegment Bake(PathBakeOptions options);
    }
}
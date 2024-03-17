using VContainer.Unity;
using VContainer;
using Retrover.Path;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        int seed = 56434312;

        builder.Register<WorldMap>(Lifetime.Singleton)
            .WithParameter("seed", seed)
            .WithParameter("chunkSize", 200)
            .WithParameter("pointsPerChunk", 2);

        builder.Register<WorldStream>(Lifetime.Singleton)
            .WithParameter("size", 3);

        builder.Register<WorldViewport>(Lifetime.Singleton).As<IWorldFocusPoint>()
            .WithParameter("minDistance", 25f);

        builder.Register<PathBakeOptions>(Lifetime.Singleton)
            .WithParameter("maximumAngleError", 0.1f)
            .WithParameter("minimumVertexDistance", 1f)
            .WithParameter("accuracy", 1);

        builder.RegisterEntryPoint<RoadRenderer>(Lifetime.Singleton)
            .WithParameter("seed", seed);

        builder.Register<RoadChunkFactory>(Lifetime.Singleton);

        builder.Register<RoadChunkView>(Lifetime.Transient)
            .WithParameter("seed", seed);

        builder.RegisterComponentInHierarchy<RoadMeshesPool>();
    }
}
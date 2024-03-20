using Reflex.Core;
using Reflex.Injectors;
using Retrover.Path;
using System.Collections.Generic;
using UnityEngine;

public class WorldSceneInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private RoadMeshesPool _roadMeshesPool;
    [SerializeField] private List<GameObject> _gameObjectsToInject;
    private ContainerBuilder _builder;

    public void InstallBindings(ContainerBuilder builder)
    {
        _builder = builder;

        int seed = 56434312;

        builder.AddAutoSingleton<WorldMap>(_ => new(seed: seed, chunkSize: 200, pointsPerChunk: 2));
        builder.AddAutoSingleton<WorldStream>(_ => new(size: 3));
        builder.AddAutoSingleton<WorldViewport>(_ => new(minDistance: 25f), typeof(IWorldFocusPoint));
        builder.AddAutoSingleton<PathBakeOptions>(_ => new(
            maximumAngleError: 0.1f, minimumVertexDistance: 1f, accuracy: 1));
        builder.AddEntryPoint<RoadRenderer>(_ => new(seed: seed));
        builder.AddAutoSingleton<RoadChunkFactory>(_ => new());
        builder.AddAutoTransient<RoadChunkView>(_ => new(seed: seed));
        builder.AddAutoSingleton(_roadMeshesPool);

        builder.OnContainerBuilt += OnContainerReady;
    }

    private void OnContainerReady(Container container)
    {
        GameObjectInjector.InjectRecursiveMany(_gameObjectsToInject, container);
    }

    private void OnDestroy()
    {
        if (_builder != null)
        {
            _builder.OnContainerBuilt -= OnContainerReady;
        }
    }
}

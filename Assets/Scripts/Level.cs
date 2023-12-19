using Retrover.Path;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private Road _road;
    private RoadClient _roadClient;
    [SerializeField] private GameObject _roadClientPrefab;
    [SerializeField] private PathBakeOptions _bakeOptions;
    [SerializeField] private RoadMeshes _roadMeshes;
    [SerializeField] private Camera _camera;

    void Start()
    {
        _road = new Road(Vector3.zero, _bakeOptions, _roadMeshes);

        GameObject roadInstance = Instantiate(_roadClientPrefab);
        _roadClient = roadInstance.GetComponent<RoadClient>();
        _roadClient.AttachToRoad(_road, 10, _camera.transform);
    }

    public void OnDrawGizmos()
    {
        if (_road == null)
            return;

        for (int i = 1; i < _road.Transforms.Count; i++)
        {
            switch (i % 4)
            {
                case 0:
                    Gizmos.color = Color.green;
                    break;
                case 1:
                    Gizmos.color = Color.yellow;
                    break;
                case 2:
                    Gizmos.color = Color.red;
                    break;
                case 3:
                    Gizmos.color = Color.blue;
                    break;
            }

            Gizmos.DrawLine(_road.Transforms[i].Position, _road.Transforms[i - 1].Position);
        }
    }

    [ContextMenu("Regenerate")]
    void DoSomething()
    {
        _road = new Road(Vector3.zero, _bakeOptions, _roadMeshes);
        _roadClient.AttachToRoad(_road, 10, _camera.transform);
    }
}

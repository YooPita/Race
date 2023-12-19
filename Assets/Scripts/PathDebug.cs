using Retrover.Path;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PathDebug : MonoBehaviour
{
    private float _nextActionTime = 0.0f;
    [SerializeField] private float _period = 0.1f;
    private IPath _path;
    private List<PathTransform> _transforms = new();
    [SerializeField] private Transform _position1;
    [SerializeField] private Transform _position2;
    [SerializeField] private Transform _position3;
    [SerializeField] private Transform _handle1;
    [SerializeField] private Transform _handle2;
    [SerializeField] private Transform _handle3;
    [SerializeField] private PathBakeOptions _options;

    private void Update()
    {
        if (Time.time > _nextActionTime)
        {
            _nextActionTime += _period;
            CreatePath();
        }
    }

    private void CreatePath()
    {
        _path = new Path(_options);
        _path.AddNode(new SymmetricNode(_position1.position, _handle1.position));
        _path.AddNode(new SymmetricNode(_position2.position, _handle2.position));
        _path.AddNode(new SymmetricNode(_position3.position, _handle3.position));
        _path.Bake();
        _transforms.Clear();
        for (int i = 0; i < _path.SegmentsCount; i++)
            _transforms.AddRange(_path.SegmentAtIndex(i).TransformsCopy());
    }

    public void OnDrawGizmos()
    {
        if (_path == null)
            return;

        for (int i = 1; i < _transforms.Count; i++)
        {
            Gizmos.color = (i % 2 == 0) ? Color.green : Color.yellow;

            Gizmos.DrawLine(_transforms[i].Position, _transforms[i - 1].Position);
        }
    }
}

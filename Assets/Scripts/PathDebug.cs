using Retrover.Path;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PathDebug : MonoBehaviour
{
    [SerializeField] private PathBakeOptions _bakeOptions;
    [SerializeField] private Transform _position1;
    [SerializeField] private Transform _position2;
    [SerializeField] private Transform _position3;
    [SerializeField] private Transform _handle1;
    [SerializeField] private Transform _handle2;
    [SerializeField] private Transform _handle3;
    [SerializeField] private bool _drawGizmos = true;
    private IPath _path;
    private List<PathTransform> _transforms = new();

    public IPath Path()
    {
        Regenerate();
        return _path;
    }

    [ContextMenu("Regenerate")]
    private void Regenerate()
    {
        CreatePath();
    }

    //private void Update()
    //{
    //    if (Time.time > _nextActionTime)
    //    {
    //        _nextActionTime += _period;
    //        CreatePath();
    //    }
    //}

    private void CreatePath()
    {
        _path = new Path(_bakeOptions);
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
        if (!_drawGizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_position1.position, 0.1f);
        Gizmos.DrawSphere(_position2.position, 0.1f);
        Gizmos.DrawSphere(_position3.position, 0.1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_handle1.position, _position1.position);
        Gizmos.DrawLine(_handle2.position, _position2.position);
        Gizmos.DrawLine(_handle3.position, _position3.position);
        Gizmos.DrawSphere(_handle1.position, 0.05f);
        Gizmos.DrawSphere(_handle2.position, 0.05f);
        Gizmos.DrawSphere(_handle3.position, 0.05f);

        if (_path == null)
            return;

        for (int i = 1; i < _transforms.Count; i++)
        {
            Gizmos.color = (i % 2 == 0) ? Color.green : Color.yellow;

            Gizmos.DrawLine(_transforms[i].Position, _transforms[i - 1].Position);
        }
    }
}

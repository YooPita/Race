using Reflex.Attributes;
using UnityEngine;

public class WorldPlayer : MonoBehaviour
{
    [Inject] private readonly IWorldFocusPoint _point;

    void Update()
    {
        _point.UpdatePosition(new Vector2(transform.position.x, transform.position.z));
    }
}

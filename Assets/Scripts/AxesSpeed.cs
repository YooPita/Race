using UnityEngine;

[System.Serializable]
public struct AxesSpeed
{
    public float MaxSpeed;
    private float _speed;

    public readonly float Speed => _speed;

    private readonly float Side => Mathf.Sign(_speed);

    public AxesSpeed(float maxSpeed)
    {
        MaxSpeed = maxSpeed;
        _speed = 0f;
    }

    public void Move(float value, SpeedSettings speedSettings)
    {
        value = Mathf.Clamp(value, -1f, 1f);

        if (value == 0f && _speed != 0f)
            Decelerate(speedSettings.deceleration);
        else if (value != 0f)
            Accelerate(value, speedSettings);

        _speed = Mathf.Clamp(_speed, -MaxSpeed, MaxSpeed);
    }

    private void Decelerate(float deceleration)
    {
        float nextSpeed = _speed - Side * deceleration * Time.deltaTime;
        _speed = Mathf.Sign(nextSpeed) != Side ? 0 : nextSpeed;
    }

    private void Accelerate(float value, SpeedSettings settings)
    {
        float acceleration = _speed == 0 || Side == Mathf.Sign(value) ?
            settings.acceleration : settings.deceleration;
        _speed += Mathf.Sign(value) * acceleration * Time.deltaTime;
    }
}
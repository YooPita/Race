[System.Serializable]
public struct SpeedSettings
{
    public float acceleration;
    public float deceleration;

    public SpeedSettings(float acceleration, float deceleration)
    {
        this.acceleration = acceleration;
        this.deceleration = deceleration;
    }
}
using UnityEngine;

public class RoadClient : MonoBehaviour
{
    [SerializeField] private Transform _horizontalBody;
    [SerializeField] private Transform _verticalBody;
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _cameraFollowSpeed = 0.7f;
    [SerializeField] private SpeedSettings _minSpeedSettings = new(0.8f, 4.5f);
    [SerializeField] private SpeedSettings _maxSpeedSettings = new(2.8f, 5.5f);
    [SerializeField] private SpeedSettings _verticalSpeedSettings = new(2.8f, 5.5f);
    [SerializeField] private float _horizontalMaxSpeed = 5.0f;
    [SerializeField] private float _verticalMaxSpeed = 5.0f;
    [SerializeField] private float _lowerSpeedBound = 2f;
    [SerializeField] private float _upperSpeedBound = 3f;
    private AxesSpeed _horizontalSpeed;
    private AxesSpeed _verticalSpeed;
    private Road _road;
    private float _roadPosition;
    private Transform _cameraBody;

    private void Start()
    {
        _horizontalSpeed = new AxesSpeed(_horizontalMaxSpeed);
        _verticalSpeed = new AxesSpeed(_verticalMaxSpeed);
    }

    private void Update()
    {
        if (_road == null)
            return;

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        SpeedSettings currentSpeedSettings = GetSpeedSettings(_verticalSpeed.Speed);
        
        _horizontalSpeed.Move(horizontalInput, currentSpeedSettings);
        if (verticalInput != 0)
            _verticalSpeed.Move(verticalInput, _verticalSpeedSettings);

        Vector3 horizontalMovement = new Vector3(_horizontalSpeed.Speed, 0, 0) * Time.deltaTime;
        _horizontalBody.Translate(horizontalMovement);
        _roadPosition += _verticalSpeed.Speed * Time.deltaTime;
        UpdatePosition();

        _road.UpdateRoad(_roadPosition);

        _cameraBody.position = Vector3.Lerp(_cameraBody.position, _cameraPivot.position, _cameraFollowSpeed * Time.deltaTime);
        _cameraBody.rotation = Quaternion.Lerp(_cameraBody.rotation, _cameraPivot.rotation, _cameraFollowSpeed * Time.deltaTime);
    }

    public void AttachToRoad(Road road, float roadPosition, Transform cameraTransform)
    {
        _cameraBody = cameraTransform;
        _road = road;
        _roadPosition = roadPosition;
        UpdatePosition();
    }

    private SpeedSettings GetSpeedSettings(float verticalSpeed)
    {
        float lerpFactor = Mathf.InverseLerp(_lowerSpeedBound, _upperSpeedBound, Mathf.Abs(verticalSpeed));
        return new SpeedSettings(
            Mathf.Lerp(_minSpeedSettings.acceleration, _maxSpeedSettings.acceleration, lerpFactor),
            Mathf.Lerp(_minSpeedSettings.deceleration, _maxSpeedSettings.deceleration, lerpFactor)
        );
    }

    private void UpdatePosition()
    {
        // Получаем позицию из PathTransform
        Vector3 position = _road.Position(_roadPosition).Position;
        _verticalBody.position = position;

        // Получаем нормаль и обнуляем Y компонент
        Vector3 normal = _road.Position(_roadPosition).Normal;
        normal.y = 0;

        // Проверяем, не является ли нормаль нулевым вектором
        if (normal != Vector3.zero)
        {
            // Создаем кватернион поворота, чтобы смотреть в направлении нормали, но с фиксированным Y
            Quaternion rotation = Quaternion.LookRotation(normal);
            _verticalBody.rotation = rotation;
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyMovement : MonoBehaviour, IControllable
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _rotationSpeed = 360f; // градусов в секунду
    private Rigidbody _rb;
    private Vector3 _direction;
    
    public float DirectionSqrMagnitude => _direction.sqrMagnitude;

    private void Awake() 
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update() 
    {
        RotateAlongMoveDirection();
    }

    private void FixedUpdate() 
    {
        MoveInternal();
    }

    public void Move(Vector3 direction)
    {
        _direction = direction;
    }

    private void MoveInternal()
    {
        var velocity = _direction * _speed;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;
    }

    private void RotateAlongMoveDirection()
    {
        if (_direction.sqrMagnitude != 0f)
        {
            // Вычисляем целевой поворот (куда хотим смотреть)
            Quaternion targetRotation = Quaternion.LookRotation(_direction);
            
            // Плавно поворачиваемся к цели
            // maxDegreesDelta = скорость * время
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }
    }
}

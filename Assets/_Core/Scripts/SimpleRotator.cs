using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [SerializeField] private Transform _objectToRotate;
    [SerializeField] private Vector3 _rotationDirection;
    [SerializeField] private float _rotationForce;

    private void OnValidate()
    {
        if (_objectToRotate == null)
            _objectToRotate = this.transform;
    }

    private void Update()
    {
        _objectToRotate.Rotate(_rotationDirection * _rotationForce * Time.deltaTime);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class SquashStretchDemo : MonoBehaviour
{
    public Collider ground;
    public Transform selectionMarker;
    public FloatSpring xSpring = new() { frequencyHz = 1.5f, dampingRatio = 0.35f };
    public FloatSpring zSpring = new() { frequencyHz = 1.5f, dampingRatio = 0.35f };
    public float stretchPerSpeed = 0.15f;
    public float maxStretch = 1.8f;

    float _restY;
    Vector3 _restScale;

    void Start()
    {
        _restY = transform.position.y;
        _restScale = transform.localScale;

        xSpring.equilibrium = transform.position.x;
        zSpring.equilibrium = transform.position.z;
        xSpring.SetValue(transform.position.x);
        zSpring.SetValue(transform.position.z);

        selectionMarker.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (ground.Raycast(ray, out RaycastHit hitInfo, 1000f))
            {
                xSpring.equilibrium = hitInfo.point.x;
                zSpring.equilibrium = hitInfo.point.z;

                selectionMarker.position = hitInfo.point + Vector3.up * 0.05f;
                selectionMarker.gameObject.SetActive(true);
            }
        }

        xSpring.Advance(Time.deltaTime);
        zSpring.Advance(Time.deltaTime);

        Vector3 velocity = new(xSpring.Velocity, 0f, zSpring.Velocity);
        float stretch = Mathf.Min(maxStretch, 1f + velocity.magnitude * stretchPerSpeed);
        float thin = 1f / Mathf.Sqrt(stretch);

        transform.position = new Vector3(xSpring.Value, _restY * thin, zSpring.Value);
        transform.localScale = new Vector3(_restScale.x * thin, _restScale.y * thin, _restScale.z * stretch);

        if (velocity.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
    }
}
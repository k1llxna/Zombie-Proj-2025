using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 camRotation = Vector3.zero;

    private float xRotation = 0f;
    private float currentRotation = 0f;

    [SerializeField]
    private float rotationCap = 85f;

    private Rigidbody rBody;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void RotateCam(Vector3 _camRotation)
    {
        camRotation = _camRotation;
    }

    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rBody.MovePosition(rBody.position + velocity * Time.fixedDeltaTime);
        }
    }

    void PerformRotation()
    {
        rBody.MoveRotation(rBody.rotation * Quaternion.Euler(rotation));
        // currentRotation -= xRotation;
        // currentRotation = Mathf.Clamp(currentRotation, -rotationCap, rotationCap);
        //cam.transform.localEulerAngles = new Vector3(currentRotation, 0f, 0f);
        cam.transform.Rotate(-camRotation);
    }
}

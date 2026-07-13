using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerUnderwaterMovement : MonoBehaviour
{
    [Header("Swim Speed")]
    public float swimSpeed = 3f;
    public float sprintMaxSpeed = 7f;
        
    [Header("Physics feel")]
    public float underwaterDrag = 4f;
    public float underwaterAngularDrag = 2f;
    public float dragResistanceScale = 0.05f; //for extra drag per metre depth
        
    [Header("Mouse Look")]
    public Transform cameraPivot;
    public Transform cameraTransform;
    public float mouseSensitivity = 2.5f;
    public float minPitch = -60f;
    public float maxPitch = 80f;

    [Header("References")]
    public OxygenSystem oxygenSystem;
    
    private Rigidbody _rigidbody;
    private float _pitch;
    [SerializeField] private float _currentDepth;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;

        if (cameraPivot == null)
        {
            cameraPivot = cameraTransform;
        }

        if (cameraPivot != null)
        {
            _pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);
        }
    }

    void Update()
    {
        HandleLook();
        UpdateDepth();
    }

    void FixedUpdate()
    {
        HandleSwim();
        ApplyWaterResistance();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up, mouseX, Space.World);

        _pitch = Mathf.Clamp(_pitch - mouseY, minPitch, maxPitch);
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }

    void HandleSwim()
    {
        float h =  Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        Transform lookReference = cameraTransform != null ? cameraTransform : transform;
        Vector3 swimDirection = lookReference.forward * v + lookReference.right * h;
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = sprinting ? sprintMaxSpeed : swimSpeed;

        if (oxygenSystem is not null)
        {
            oxygenSystem.isSprinting = sprinting;
        }
        
        _rigidbody.AddForce(swimDirection.normalized * speed, ForceMode.Acceleration);
    }

    void ApplyWaterResistance()
    {
        //Depth increases drag naturally
        float depthDrag = underwaterDrag + (_currentDepth * dragResistanceScale);
        _rigidbody.linearDamping = depthDrag;
        _rigidbody.angularDamping = underwaterAngularDrag;
    }

    void UpdateDepth()
    {
        _currentDepth = Mathf.Max(0f, -transform.position.y);
        if (oxygenSystem is not null)
        {
            oxygenSystem.SetDepth(_currentDepth);
        }
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    public void EnableUnderwaterMode(bool enable)
    {
        _rigidbody.useGravity = false;
        _rigidbody.linearDamping = enable ? underwaterDrag : 0;
        this.enabled = enable;
    }
}
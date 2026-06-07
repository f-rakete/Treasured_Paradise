using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayUnderwaterMovement : MonoBehaviour
{
    [Header("Swim Speed")]
    public float swimSpeed = 3f;
    public float sprintMaxSpeed = 7f;
        
    [Header("Physics feel")]
    public float underwaterDrag = 4f;
    public float underwaterAngularDrag = 2f;
    public float dragResistanceScale = 0.05f; //for extra drag per metre depth
        
    [Header("References")]
    public Transform cameraTransform;
    public OxygenSystem oxygenSystem;
    public float mouseSensitivity = 15f;
    
    private Rigidbody _rigidbody;
    private float _xRotation;
    private float _yRotation;
    [SerializeField] private float _currentDepth;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up, mouseX);
    }

    void HandleSwim()
    {
        float h =  Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        Vector3 swimDirection = cameraTransform.forward * v + cameraTransform.right * h;
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

    public void EnableUnderwaterMode(bool enable)
    {
        _rigidbody.useGravity = false;
        _rigidbody.linearDamping = enable ? underwaterDrag : 0;
        this.enabled = enable;
    }
}
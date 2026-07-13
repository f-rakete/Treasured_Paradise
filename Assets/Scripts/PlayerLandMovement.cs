using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLandMovement : MonoBehaviour
{
    [Header("Movement")] 
    public float walkSpeed = 10f;
    public float runSpeed = 19f;
    public float gravity = -9.81f;
    [FormerlySerializedAs("jumpForce")] public float jumpHeight = 4.5f;

    [Header("Look")] 
    public float mouseSensitivity = 5f;
    public Transform cameraTransform;

    [Header("Ground Check")] 
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Camera")] 
    public Camera CameraRig;
    public Camera CameraTarget;
    [Min(0f)] public float CameraFollowSpeed = 20f;
    public bool detachCameraRigOnStart = true;
    public bool followPlayerRootInsteadOfAnimatedTarget = true;
    public Vector3 cameraLocalOffset = new Vector3(0f, 2.43f, -1.97f);

    [SerializeField] private Animator characterAnimator;
    private CharacterController _characterController;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private float _xRotation;
    private bool _isGrounded;
    private bool _sprinting;
    private Transform _cameraRigTransform;
    private Transform _cameraTargetTransform;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (characterAnimator == null)
        {
            Debug.LogError("characterAnimator is not assigned on " + gameObject.name, gameObject);
        }

        _cameraRigTransform = CameraRig != null ? CameraRig.transform : cameraTransform;
        _cameraTargetTransform = CameraTarget != null ? CameraTarget.transform : null;

        if (_cameraRigTransform != null && cameraLocalOffset == Vector3.zero)
        {
            cameraLocalOffset = transform.InverseTransformPoint(_cameraRigTransform.position);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (detachCameraRigOnStart && _cameraRigTransform != null)
        {
            _cameraRigTransform.SetParent(null, true);
        }

        SnapCameraRigToFollowPosition();
    }

    void Update()
    {
        CheckIfGrounded();
        HandleMove();
        HandleLook();
    }

    void LateUpdate()
    {
        UpdateCameraRig();
    }

    void HandleMove()
    {
        if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        _moveInput = new Vector2(h, v);
        _sprinting = Input.GetKey(KeyCode.LeftShift);

        Vector3 move = transform.right * h + transform.forward * v;
        float speed = _sprinting ? runSpeed : walkSpeed;
        _characterController.Move(move * (speed * Time.deltaTime));

        //Jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //Gravity
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    void UpdateCameraRig()
    {
        if (_cameraRigTransform == null) return;

        Vector3 targetPosition = GetCameraFollowPosition();
        float followT = CameraFollowSpeed <= 0f ? 1f : 1f - Mathf.Exp(-CameraFollowSpeed * Time.deltaTime);
        _cameraRigTransform.position = Vector3.Lerp(_cameraRigTransform.position, targetPosition, followT);
        _cameraRigTransform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    void SnapCameraRigToFollowPosition()
    {
        if (_cameraRigTransform == null) return;

        _cameraRigTransform.position = GetCameraFollowPosition();
        _cameraRigTransform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    Vector3 GetCameraFollowPosition()
    {
        if (!followPlayerRootInsteadOfAnimatedTarget && _cameraTargetTransform != null)
        {
            return _cameraTargetTransform.position;
        }

        return transform.TransformPoint(cameraLocalOffset);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        float speed = _moveInput.magnitude * (_sprinting ? 1f : 0.5f);

        characterAnimator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up, mouseX);
    }

    //Called by PlayerSwitchMode when entering water
    public void EnableLandMode(bool enable)
    {
        _characterController.enabled = enable;
        this.enabled = enable;
    }

    void CheckIfGrounded()
    {
        Vector3 feetPosition = transform.position + Vector3.down * (_characterController.height / 2f);
        _isGrounded = Physics.CheckSphere(feetPosition, groundDistance, groundMask);
    }
}
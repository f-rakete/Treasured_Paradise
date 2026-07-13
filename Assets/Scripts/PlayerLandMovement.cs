using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLandMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 10f;
    public float runSpeed = 19f;
    public float gravity = -9.81f;
    [FormerlySerializedAs("jumpForce")] public float jumpHeight = 4.5f;
    [Min(0f)] public float turnSpeed = 12f;

    [Header("Look")]
    public float mouseSensitivity = 5f;
    public Transform cameraTransform;
    public float minPitch = -35f;
    public float maxPitch = 70f;

    [Header("Ground Check")]
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Camera")]
    public Transform CameraRig;
    public Transform CameraTarget;
    [Min(0f)] public float CameraFollowSpeed = 20f;
    public bool detachCameraRigOnStart = true;
    public bool followPlayerRootInsteadOfAnimatedTarget = true;
    public Vector3 cameraLocalOffset = new Vector3(0f, 2.43f, -1.97f);

    [SerializeField] private Animator characterAnimator;
    private CharacterController _characterController;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private float _cameraPitch;
    private float _cameraYaw;
    private bool _isGrounded;
    private bool _sprinting;
    private Transform _cameraRigTransform;
    private Transform _cameraTargetTransform;
    private float _cameraDistance;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (characterAnimator == null)
        {
            Debug.LogError("characterAnimator is not assigned on " + gameObject.name, gameObject);
        }

        _cameraRigTransform = CameraRig != null ? CameraRig : (cameraTransform != null && cameraTransform.parent != null ? cameraTransform.parent : cameraTransform);
        _cameraTargetTransform = CameraTarget;

        if (_cameraRigTransform != null && cameraLocalOffset == Vector3.zero)
        {
            cameraLocalOffset = transform.InverseTransformPoint(_cameraRigTransform.position);
        }

        _cameraDistance = Mathf.Max(0.1f, Mathf.Abs(cameraLocalOffset.z));
        _cameraYaw = transform.eulerAngles.y;
        _cameraPitch = _cameraRigTransform != null ? NormalizeAngle(_cameraRigTransform.eulerAngles.x) : 0f;
        _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);
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
        HandleLook();
        HandleMove();
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

        Vector3 move = GetCameraPlanarForward() * v + GetCameraPlanarRight() * h;
        move = Vector3.ClampMagnitude(move, 1f);
        float speed = _sprinting ? runSpeed : walkSpeed;
        _characterController.Move(move * (speed * Time.deltaTime));

        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
        }

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
        Vector3 smoothedTarget = Vector3.Lerp(GetCurrentCameraTargetPosition(), targetPosition, followT);
        ApplyCameraOrbit(smoothedTarget);
    }

    void SnapCameraRigToFollowPosition()
    {
        if (_cameraRigTransform == null) return;

        ApplyCameraOrbit(GetCameraFollowPosition());
    }

    Vector3 GetCameraFollowPosition()
    {
        Vector3 targetPosition = !followPlayerRootInsteadOfAnimatedTarget && _cameraTargetTransform != null
            ? _cameraTargetTransform.position
            : transform.position;

        return targetPosition + Vector3.up * cameraLocalOffset.y + GetCameraPlanarRight() * cameraLocalOffset.x;
    }

    Vector3 GetCurrentCameraTargetPosition()
    {
        Quaternion cameraRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
        return _cameraRigTransform.position + cameraRotation * Vector3.forward * _cameraDistance;
    }

    void ApplyCameraOrbit(Vector3 targetPosition)
    {
        Quaternion cameraRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
        _cameraRigTransform.SetPositionAndRotation(
            targetPosition - cameraRotation * Vector3.forward * _cameraDistance,
            cameraRotation);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        float speed = _moveInput.magnitude * (_sprinting ? 1f : 0.5f);

        if (characterAnimator != null)
        {
            characterAnimator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        }

        _cameraYaw += mouseX;
        _cameraPitch = Mathf.Clamp(_cameraPitch - mouseY, minPitch, maxPitch);
    }

    Vector3 GetCameraPlanarForward()
    {
        Vector3 forward = Quaternion.Euler(0f, _cameraYaw, 0f) * Vector3.forward;
        return forward.normalized;
    }

    Vector3 GetCameraPlanarRight()
    {
        Vector3 right = Quaternion.Euler(0f, _cameraYaw, 0f) * Vector3.right;
        return right.normalized;
    }

    static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        return angle > 180f ? angle - 360f : angle;
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

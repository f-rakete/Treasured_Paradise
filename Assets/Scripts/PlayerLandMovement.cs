using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLandMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 10f;
    public float runSpeed = 19f;
    public float gravity = -9.81f;
    [FormerlySerializedAs("jumpForce")] public float jumpHeight = 4.5f;

    [Header("Mouse Look")]
    [Tooltip("Mouse sensitivity in degrees per mouse unit.")]
    public float mouseSensitivity = 2.5f;
    [Tooltip("Pivot that pitches up and down. If empty, the assigned camera transform is used.")]
    public Transform cameraPivot;
    [Tooltip("Actual camera transform. If this is parented under the pivot, its local offset is maintained.")]
    public Transform cameraTransform;
    public float minPitch = -35f;
    public float maxPitch = 70f;
    public bool lockCursor = true;
    [Tooltip("Detach the camera pivot at runtime so character/head animation cannot move the camera.")]
    public bool decoupleCameraFromAnimation = true;
    public Vector3 pivotOffset = new Vector3(0f, 1.7f, 0f);
    public Vector3 cameraOffset = new Vector3(0f, 0f, -4f);

    [Header("Ground Check")]
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [SerializeField] private Animator characterAnimator;
    private CharacterController _characterController;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private float _pitch;
    private bool _isGrounded;
    private bool _sprinting;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (cameraPivot == null)
        {
            cameraPivot = cameraTransform;
        }

        if (cameraPivot != null)
        {
            Vector3 euler = cameraPivot.rotation.eulerAngles;
            _pitch = Mathf.Clamp(NormalizeAngle(euler.x), minPitch, maxPitch);

            if (decoupleCameraFromAnimation)
            {
                cameraPivot.SetParent(null, true);
            }
        }

        if (characterAnimator == null)
        {
            Debug.LogError("characterAnimator is not assigned on " + gameObject.name, gameObject);
        }
    }

    private void Start()
    {
        SetCursorLock(lockCursor);
    }

    private void Update()
    {
        CheckIfGrounded();
        HandleLook();
        HandleMove();
        UpdateAnimator();
    }

    private void LateUpdate()
    {
        ApplyCameraPose();
    }

    private void HandleMove()
    {
        if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        _moveInput = new Vector2(h, v);
        _sprinting = Input.GetKey(KeyCode.LeftShift);

        Vector3 move = transform.right * h + transform.forward * v;
        move = Vector3.ClampMagnitude(move, 1f);
        float speed = _sprinting ? runSpeed : walkSpeed;
        _characterController.Move(move * (speed * Time.deltaTime));

        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up, mouseX, Space.World);

        _pitch = Mathf.Clamp(_pitch - mouseY, minPitch, maxPitch);
        ApplyCameraPose();
    }

    private void ApplyCameraPose()
    {
        if (cameraPivot == null) return;

        cameraPivot.position = transform.position + transform.rotation * pivotOffset;
        cameraPivot.rotation = Quaternion.Euler(_pitch, transform.eulerAngles.y, 0f);

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = cameraOffset;
            cameraTransform.localRotation = Quaternion.identity;
        }
    }

    private void UpdateAnimator()
    {
        if (characterAnimator == null) return;

        float speed = Mathf.Clamp01(_moveInput.magnitude) * (_sprinting ? 1f : 0.5f);
        characterAnimator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    public void EnableLandMode(bool enable)
    {
        _characterController.enabled = enable;
        enabled = enable;

        if (enable)
        {
            SetCursorLock(lockCursor);
        }
    }

    private void CheckIfGrounded()
    {
        Vector3 feetPosition = transform.position + Vector3.down * (_characterController.height / 2f);
        _isGrounded = Physics.CheckSphere(feetPosition, groundDistance, groundMask);
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private static void SetCursorLock(bool shouldLock)
    {
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }
}

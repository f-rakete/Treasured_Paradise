using UnityEngine;

public class PlayerLandMovement : MonoBehaviour
{
    [Header("Movement")] 
    public float walkSpeed = 10f;
    public float runSpeed = 19f;
    public float gravity = -9.81f;
    public float jumpHeight = 4.5f;

    [Header("Ground Check")] 
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [SerializeField] private Animator characterAnimator;
    private CharacterController _characterController;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _sprinting;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (characterAnimator == null)
        {
            Debug.LogError("characterAnimator is not assigned on " + gameObject.name, gameObject);
        }
    }

    void Update()
    {
        CheckIfGrounded();
        HandleMove();
    }

    void HandleMove()
    {
        if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
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

        float animationSpeed = new Vector2(h, v).magnitude * (_sprinting ? 1f : 0.5f);
        characterAnimator.SetFloat("Speed", animationSpeed, 0.1f, Time.deltaTime);
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
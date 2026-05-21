using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public float CameraFollowSpeed;
    
    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _xRotation;
    private bool _isGrounded;
    private PlayerInput _playerInput;
   
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
    void Start()
    {
        _playerInput = new PlayerInput();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CheckIfGrounded();
        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up, mouseX);
    }

    void HandleMove()
    {
        bool grounded = _characterController.isGrounded;
        if (grounded && _velocity.y < 0f) _velocity.y = -2f;
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool running = Input.GetKey(KeyCode.LeftShift);
        
        Vector3 move = transform.right * h + transform.forward * v;
        float speed = running ? runSpeed : walkSpeed;
        _characterController.Move(move * speed * Time.deltaTime);
        
        //Jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        //Gravity
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
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

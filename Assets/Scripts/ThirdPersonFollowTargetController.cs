using UnityEngine;

public class ThirdPersonFollowTargetController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private float minPitch = -70f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private bool lockCursor = true;

    private float _pitch;
    private float _yaw;

    private void Awake()
    {
        if (playerBody == null)
        {
            playerBody = transform.parent;
        }

        Vector3 eulerAngles = transform.rotation.eulerAngles;
        _pitch = NormalizeAngle(eulerAngles.x);
        _yaw = eulerAngles.y;
    }

    private void Start()
    {
        if (!lockCursor)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _yaw += mouseX;
        _pitch = Mathf.Clamp(_pitch - mouseY, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}

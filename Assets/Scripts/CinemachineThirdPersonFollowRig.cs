using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineThirdPersonFollow))]
public class CinemachineThirdPersonFollowRig : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform trackingTarget;

    [Header("Third Person Follow")]
    [SerializeField] private Vector3 damping = new(0.1f, 0.5f, 0.3f);
    [SerializeField] private Vector3 shoulderOffset = new(0.7f, 0.3f, -0.5f);
    [SerializeField] private float verticalArmLength = 0.5f;
    [SerializeField, Range(0f, 1f)] private float cameraSide = 1f;
    [SerializeField] private float cameraDistance = 2f;

    private void Reset()
    {
        ApplySettings();
    }

    private void Awake()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (!TryGetComponent(out CinemachineCamera cinemachineCamera))
        {
            return;
        }

        if (trackingTarget != null)
        {
            cinemachineCamera.Follow = trackingTarget;
            cinemachineCamera.LookAt = trackingTarget;
        }

        if (!TryGetComponent(out CinemachineThirdPersonFollow thirdPersonFollow))
        {
            return;
        }

        thirdPersonFollow.Damping = damping;
        thirdPersonFollow.ShoulderOffset = shoulderOffset;
        thirdPersonFollow.VerticalArmLength = verticalArmLength;
        thirdPersonFollow.CameraSide = cameraSide;
        thirdPersonFollow.CameraDistance = cameraDistance;
    }
}

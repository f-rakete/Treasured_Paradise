using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CinemachineThirdPersonFollowBootstrap
{
    private const string VirtualCameraName = "PlayerThirdPersonFollowCamera";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ConfigureActiveSceneCamera()
    {
        SceneManager.sceneLoaded += (_, _) => ConfigureCameraRig();
        ConfigureCameraRig();
    }

    private static void ConfigureCameraRig()
    {
        Camera playerCamera = FindPlayerCamera();
        Transform trackingTarget = FindTrackingTarget();
        Transform playerBody = GameObject.FindWithTag("Player")?.transform;

        if (playerCamera == null || trackingTarget == null || playerBody == null)
        {
            Debug.LogWarning("Cinemachine third-person follow setup skipped: PlayerCamera, CameraRig, or Player was not found.");
            return;
        }

        DetachCameraFromLegacyRig(playerCamera);

        if (!playerCamera.TryGetComponent(out CinemachineBrain _))
        {
            playerCamera.gameObject.AddComponent<CinemachineBrain>();
        }

        ThirdPersonFollowTargetController targetController = trackingTarget.GetComponent<ThirdPersonFollowTargetController>();
        if (targetController == null)
        {
            targetController = trackingTarget.gameObject.AddComponent<ThirdPersonFollowTargetController>();
        }

        targetController.SetPlayerBody(playerBody);

        CinemachineCamera cinemachineCamera = GetOrCreateVirtualCamera();
        cinemachineCamera.Follow = trackingTarget;
        cinemachineCamera.LookAt = trackingTarget;

        CinemachineThirdPersonFollow thirdPersonFollow = cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
        if (thirdPersonFollow == null)
        {
            thirdPersonFollow = cinemachineCamera.gameObject.AddComponent<CinemachineThirdPersonFollow>();
        }

        thirdPersonFollow.Damping = new Vector3(0.1f, 0.5f, 0.3f);
        thirdPersonFollow.ShoulderOffset = new Vector3(0.7f, 0.3f, -0.5f);
        thirdPersonFollow.VerticalArmLength = 0.5f;
        thirdPersonFollow.CameraSide = 1f;
        thirdPersonFollow.CameraDistance = 2f;
    }

    private static Camera FindPlayerCamera()
    {
        GameObject playerCameraObject = GameObject.Find("PlayerCamera");
        if (playerCameraObject != null && playerCameraObject.TryGetComponent(out Camera playerCamera))
        {
            return playerCamera;
        }

        return Camera.main;
    }

    private static Transform FindTrackingTarget()
    {
        GameObject cameraTarget = GameObject.Find("CameraTarget");
        if (cameraTarget != null)
        {
            return cameraTarget.transform;
        }

        GameObject cameraRig = GameObject.Find("CameraRig");
        return cameraRig != null ? cameraRig.transform : null;
    }

    private static void DetachCameraFromLegacyRig(Camera playerCamera)
    {
        if (playerCamera.transform.parent != null)
        {
            playerCamera.transform.SetParent(null, true);
        }
    }

    private static CinemachineCamera GetOrCreateVirtualCamera()
    {
        GameObject virtualCameraObject = GameObject.Find(VirtualCameraName);
        if (virtualCameraObject == null)
        {
            virtualCameraObject = new GameObject(VirtualCameraName);
        }

        CinemachineCamera cinemachineCamera = virtualCameraObject.GetComponent<CinemachineCamera>();
        return cinemachineCamera != null ? cinemachineCamera : virtualCameraObject.AddComponent<CinemachineCamera>();
    }
}

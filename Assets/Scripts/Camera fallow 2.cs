using UnityEngine;

public class CameraFollow2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera")]
    public float distance = 6f;
    public float height = 2f;

    [Header("Mouse")]
    public float mouseSensitivity = 3f;

    [Header("Vertical Rotation")]
    public float minPitch = -20f;
    public float maxPitch = 70f;

    private float yaw = 0f;
    private float pitch = 20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        // =========================
        // MOUSE
        // =========================

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        // =========================
        // CAMERA ROTATION
        // =========================

        Quaternion rotation = Quaternion.Euler(
            pitch,
            yaw,
            0f
        );

        // =========================
        // CAMERA TARGET
        // =========================

        Vector3 target =
            player.position +
            Vector3.up * height;

        // =========================
        // CAMERA POSITION
        // =========================

        Vector3 offset =
            rotation * Vector3.back * distance;

        transform.position =
            target + offset;

        // =========================
        // CAMERA LOOK
        // =========================

        transform.rotation =
            Quaternion.LookRotation(
                target - transform.position
            );
    }
}
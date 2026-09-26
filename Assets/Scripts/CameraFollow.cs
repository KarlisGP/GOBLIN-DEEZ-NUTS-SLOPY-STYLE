using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public float distance = 6f;
    public float height = 2f;

    public float mouseSensitivity = 3f;

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
        // =========================
        // MOUSE INPUT
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
        // CAMERA POSITION
        // =========================

        Vector3 offset =
            rotation * new Vector3(
                0f,
                0f,
                -distance
            );

        Vector3 targetPosition =
            player.position +
            Vector3.up * height +
            offset;

        transform.position = targetPosition;

        // =========================
        // LOOK AT PLAYER
        // =========================

        transform.LookAt(
            player.position + Vector3.up * height
        );
    }
}
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Model")]
    public Transform playerModel;

    // Change this until the model faces the direction it should.
    // Try 90, -90, or 180.
    public float modelRotationOffset = -90f;

    private Rigidbody rb;

    private Vector3 movement;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Don't let physics make the player fall over.
        rb.freezeRotation = true;

        // Apply the model's correction ONCE.
        playerModel.localRotation = Quaternion.Euler(
            0f,
            modelRotationOffset,
            0f
        );
    }

    void Update()
    {
        // WASD
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Camera directions
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Ignore camera up/down
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Movement relative to camera
        movement = forward * z + right * x;

        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        // Move the PLAYER ROOT
        rb.MovePosition(
            rb.position + movement * moveSpeed * Time.fixedDeltaTime
        );

        // Rotate the PLAYER ROOT toward movement
        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.fixedDeltaTime
            );
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
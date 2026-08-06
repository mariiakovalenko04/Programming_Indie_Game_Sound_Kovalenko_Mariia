using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class SimpleFirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minimumLookAngle = -90f;
    [SerializeField] private float maximumLookAngle = 90f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;
    private bool cursorLocked = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();

            if (childCamera != null)
                playerCamera = childCamera.transform;
        }
    }

    private void Start()
    {
        SetCursorLock(true);
    }

    private void Update()
    {
        HandleCursor();
        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        if (!cursorLocked || playerCamera == null)
            return;

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minimumLookAngle,
            maximumLookAngle
        );

        playerCamera.localRotation = Quaternion.Euler(
            cameraPitch,
            0f,
            0f
        );
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        float speed = Input.GetKey(KeyCode.LeftShift)
            ? runSpeed
            : walkSpeed;

        Vector3 horizontalMovement =
            transform.TransformDirection(input) * speed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = horizontalMovement;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetCursorLock(false);

        if (!cursorLocked && Input.GetMouseButtonDown(0))
            SetCursorLock(true);
    }

    private void SetCursorLock(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SetCursorLock(false);
    }
}
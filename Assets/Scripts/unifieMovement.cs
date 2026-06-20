using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class unifiedMovementScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -20f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 15f;
    private Transform playerCamera; 

    // Internal Variables
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Automatically find the camera attached to the player!
        playerCamera = GetComponentInChildren<Camera>().transform;

        // Lock the mouse to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- INPUT GATHERING ---

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // --- MAIN GAME LOOP ---

    void Update()
    {
        // 1. First, turn the player to face the correct direction
        HandleCameraLook();

        // 2. Then, move the player forward in that new direction
        HandleMovement();
    }

    // --- UNIFIED LOGIC ---

    void HandleCameraLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Up and Down (Camera Only)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Left and Right (Spins the entire Player body)
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // Because we rotated the player above, 'transform.forward' now points exactly where we are looking!
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        // Apply walking movement
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Apply gravity
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keep snapped to the floor
        }
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
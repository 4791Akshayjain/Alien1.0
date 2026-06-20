using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class unifiedMovementScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    private float currentSpeed;
    public float gravity = -20f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 15f;
    private Transform playerCamera; 

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    [Space]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyRegenRate = 15f; // Energy restored per second
    public float sprintEnergyCost = 25f; // Energy used per second while sprinting

    // Internal Variables
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isSprinting = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>(); 
        
        // Automatically find the camera attached to the player!
        playerCamera = GetComponentInChildren<Camera>().transform;

        // Lock the mouse to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize player stats to full capacity
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentSpeed = walkSpeed;
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

    public void OnSprint(InputValue value)
    {
        // Tracks if the sprint key (like Left Shift) is currently held down
        isSprinting = value.isPressed;
    }

    // --- MAIN GAME LOOP ---

    void Update()
    {
        // 1. First, regenerate energy if we aren't spending it
        HandleEnergyRegen();

        // 2. Turn the player to face the correct direction
        HandleCameraLook();

        // 3. Move the player forward or sprint in that new direction
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
        // Determine speed based on input, movement state, and available energy
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isSprinting && isMoving && currentEnergy > 0f)
        {
            currentSpeed = sprintSpeed;
            // Drain energy over time
            currentEnergy -= sprintEnergyCost * Time.deltaTime;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Clamp energy so it never drops below 0
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        // Calculate world direction relative to where the player is looking
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        // Apply walking/sprinting movement
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Apply gravity mechanics
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keep snapped to the floor
        }
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleEnergyRegen()
    {
        // Regenerate energy naturally if the player is NOT actively sprinting while moving
        bool isSpendingEnergy = isSprinting && moveInput.magnitude > 0.1f;

        if (!isSpendingEnergy && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }
    }

    // --- STAT DAMAGE & HEALTH UTILITIES ---

    // Call this public method from hazard triggers or enemy projectile scripts
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Player took damage! Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    void PlayerDeath()
    {
        Debug.Log("Player has died!");
        // Your custom Game Over or Respawn logic goes right here
    }
}
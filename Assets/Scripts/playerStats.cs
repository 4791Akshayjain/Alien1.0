using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyRegenRate = 10f; // Energy restored per second

    void Start()
    {
        // Start the game with full health and energy
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        // Regenerate energy automatically over time
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy); // Keep it within bounds
        }
    }

    // Call this function from traps, enemies, or fall damage scripts
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Player took damage! Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Call this function when actions require energy (like sprinting or jumping)
    public bool UseEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true; // Action allowed
        }
        
        Debug.Log("Not enough energy!");
        return false; // Action denied
    }

    void Die()
    {
        Debug.Log("Player has died!");
        // Add your game over or respawn logic here
    }
}
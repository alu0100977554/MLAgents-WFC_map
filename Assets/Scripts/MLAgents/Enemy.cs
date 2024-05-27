using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an enemy of the shooter minigame
/// </summary>
public class Enemy : MonoBehaviour
{
    public const int _maxHealth = 1;

    // The area where the agent is in
    private ShooterArea _shooterArea;

    /// <summary>
    /// Current amount of remaining health
    /// </summary>
    public float CurrentHealth { get; private set; }

    /// <summary>
    /// The enemy gets hit by a shot
    /// </summary>
    /// <param name="damage">Damage dealt by the shooter</param>
    public void GetShot(int damage)
    {
        // Subtract the damage from the enemy's health
        Mathf.Clamp(CurrentHealth, 0f, CurrentHealth - damage);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Disables the enemy when its health drops below 1
    /// </summary>
    public void Die()
    {
        Debug.Log("Enemy died");
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Enables the enemy again
    /// </summary>
    public void Respawn()
    {
        // First, set the enemy to active
        this.gameObject.SetActive(true);

        // Maximun number of attemps to respawn without colliding with another object
        int attemptsReamining = 100;
        Vector3 potentialPosition = new Vector3(UnityEngine.Random.Range(_shooterArea._areaBounds.min.x, _shooterArea._areaBounds.max.x),
                                                1f,
                                                UnityEngine.Random.Range(_shooterArea._areaBounds.min.z, _shooterArea._areaBounds.max.z));

        // Check for collision
        while (Physics.CheckBox(potentialPosition, new Vector3(2f, 0.1f, 2f)) && attemptsReamining > 0) attemptsReamining--;

        this.transform.position = potentialPosition;

        // Reset healh
        CurrentHealth = _maxHealth;
    }
}

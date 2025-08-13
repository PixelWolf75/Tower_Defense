using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a visual health bar UI element that dynamically scales to represent current health.
/// </summary>
public class HealthBar : MonoBehaviour
{
    /// <summary>
    /// RawImage component that will be scaled to represent health percentage
    /// </summary>
    [SerializeField]
    RawImage imgHealthBar;

    /// <summary>
    /// Current health points (decreases as damage is taken)
    /// </summary>
    float health = 10f;

    /// <summary>
    /// Maximum health points (used for percentage calculations)
    /// </summary>
    float maxHealth = 10f;

    /// <summary>
    /// Resets the health bar to a new starting health value.
    /// Updates both current and maximum health, then refreshes the visual display.
    /// </summary>
    /// <param name="startingHealth">New health value to set as both current and maximum health</param>
    public void ResetHealth(int startingHealth)
    {
        health = (float)startingHealth;
        maxHealth = (float)startingHealth;
        UpdateHealthBar();
    }

    /// <summary>
    /// Reduces current health by the specified damage amount.
    /// Prevents health from going below zero and updates the visual display.
    /// </summary>
    /// <param name="damage">Amount of damage to subtract from current health</param>
    public void ReduceHealth(int damage)
    {
        health -= damage;

        // Clamp health to prevent negative values
        if (health < 0f)
        {
            health = 0f;
        }

        UpdateHealthBar();
    }

    /// <summary>
    /// Updates the visual appearance of the health bar based on current health percentage.
    /// Scales the RawImage horizontally to represent the health ratio visually.
    /// </summary>
    void UpdateHealthBar()
    {
        if (imgHealthBar != null)
        {
            // Calculate health percentage and apply as horizontal scale
            Vector3 scale = imgHealthBar.transform.localScale;
            scale.x = health / maxHealth; // Scale width based on health percentage
            imgHealthBar.transform.localScale = scale;
        }
    }

    /// <summary>
    /// initializes the health bar display on startup.
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        UpdateHealthBar();
    }
}
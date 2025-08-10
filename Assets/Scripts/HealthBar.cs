using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    RawImage imgHealthBar; // Changed to RawImage

    float health = 10f;
    float maxHealth = 10f;

    public void ResetHealth(int startingHealth)
    {
        health = (float)startingHealth;
        maxHealth = (float)startingHealth;
        UpdateHealthBar();
    }

    public void ReduceHealth(int damage)
    {
        health -= damage;
        if (health < 0f)
        {
            health = 0f;
        }
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (imgHealthBar != null)
        {
            // Scale the RawImage transform to shrink it smoothly
            Vector3 scale = imgHealthBar.transform.localScale;
            scale.x = health / maxHealth; // Scale width based on health percentage
            imgHealthBar.transform.localScale = scale;
        }
    }

    void Start()
    {
        UpdateHealthBar();
    }
}

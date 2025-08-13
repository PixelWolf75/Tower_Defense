using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles blinking/flashing visual effects for game objects by toggling renderer visibility.
/// Caches all child renderers and provides methods to start, stop, and control blinking behavior.
/// </summary>
public class Blink : MonoBehaviour
{
    /// <summary>
    /// The initial time interval between blink toggles in seconds
    /// </summary>
    [SerializeField]
    float startInterval = 0.3f;

    /// <summary>
    /// Current time interval between blink toggles, can be modified at runtime
    /// </summary>
    private float currentInterval = 0f;

    /// <summary>
    /// Reference to the active blinking coroutine, null when not blinking
    /// </summary>
    private Coroutine blinkCoroutine;

    /// <summary>
    /// Cached array of all child renderer components to optimize performance
    /// </summary>
    private Renderer[] renderers;

    /// <summary>
    /// Sets up the interval and caching renderers.
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        currentInterval = startInterval;

        renderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Starts the blinking effect if not already active and renderers are available.
    /// Creates and starts a coroutine that toggles renderer visibility at the current interval.
    /// </summary>
    public void StartBlinking()
    {
        if (blinkCoroutine == null && renderers != null && renderers.Length > 0)
        { 
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    /// <summary>
    /// Sets a new time interval for the blinking effect.
    /// </summary>
    /// <param name="newInterval">New time interval between blinks in seconds. 
    /// Values less than 0.01f are clamped to prevent zero/negative intervals.</param>
    public void SetBlinkInterval(float newInterval)
    {
        // Ensure interval is at least 0.01 seconds to prevent infinite loops or division by zero
        currentInterval = Mathf.Max(0.01f, newInterval);
    }

    /// <summary>
    /// Coroutine that handles the actual blinking logic by toggling renderer visibility.
    /// Runs indefinitely until the coroutine is stopped externally.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution</returns>
    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Toggle object visibility by checking first renderer state and inverting all
            if (renderers != null)
            {
                bool isVisible = renderers[0].enabled; // Use first renderer as reference
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = !isVisible; // Toggle all renderers to same state
                }
            }

            // Wait for the current interval before next toggle
            yield return new WaitForSeconds(currentInterval);
        }
    }

    /// <summary>
    /// Stops the blinking effect and ensures all renderers are visible.
    /// Cleans up the coroutine reference and restores normal visibility.
    /// </summary>
    public void CancelBlinking()
    {
        // Stop the coroutine if it's running
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;

            // Restore all renderers to visible state
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }
            }
        }
    }
}

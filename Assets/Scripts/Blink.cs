using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour
{
    //[SerializeField]
    //GameObject container = default;

    [SerializeField] 
    float startInterval = 0.3f;

    private float currentInterval = 0f;
    private Coroutine blinkCoroutine;
    private Renderer[] renderers; // Cache all child renderers

    // Start is called before the first frame update
    void Start()
    {
        //InvokeRepeating("BlinkObject", 0f, interval);
        currentInterval = startInterval;

        renderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"Found {renderers.Length} renderers to blink");
    }

    public void StartBlinking()
    {
        if (blinkCoroutine == null && renderers != null && renderers.Length > 0)
        { 
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    public void SetBlinkInterval(float newInterval)
    {
        currentInterval = Mathf.Max(0.01f, newInterval); // prevent zero/negative
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Toggle object visibility
            if (renderers != null)
            {
                bool isVisible = renderers[0].enabled; // Check first renderer state
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = !isVisible; // Toggle all renderers
                }
            }

            // Wait for the current interval
            yield return new WaitForSeconds(currentInterval);
        }
    }

    public void CancelBlinking()
    {
        if(blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;


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

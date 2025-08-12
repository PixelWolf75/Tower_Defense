using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class Brightness : MonoBehaviour
{
    [SerializeField]
    Slider brightnessSlider;

    [SerializeField]
    PostProcessProfile brightness;

    [SerializeField]
    PostProcessLayer layer;
    ColorGrading colorGrading;
    // Start is called before the first frame update
    void Start()
    {
        // Get AutoExposure from profile
        if (brightness != null)
        {
            brightness.TryGetSettings(out colorGrading);
        }

        // Set up slider
        if (brightnessSlider != null)
        {
            // Add listener for slider changes
            brightnessSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // Load and apply saved settings
        StartCoroutine(LoadBrightnessFromSettingsDelayed());
    }

    IEnumerator LoadBrightnessFromSettingsDelayed()
    {
        // Wait for SettingsManager to be ready
        while (SettingsManager.instance == null)
        {
            yield return null;
        }

        LoadBrightnessFromSettings();
    }

    void LoadBrightnessFromSettings()
    {
        if (SettingsManager.instance != null)
        {
            float savedBrightness = SettingsManager.instance.currentBrightness;
            if (brightnessSlider != null)
                brightnessSlider.value = savedBrightness;
            if (colorGrading != null)
                colorGrading.postExposure.value = savedBrightness;
        }
        else
        {
            Debug.LogWarning("SettingsManager not found!");
        }
    }

    // Called when slider value changes
    void OnSliderChanged(float newValue)
    {
        AdjustBrightness(newValue);
    }

    public void AdjustBrightness(float newBrightness)
    {
        if (colorGrading != null)
            colorGrading.postExposure.value = newBrightness;

        if (SettingsManager.instance != null)
            SettingsManager.instance.SetBrightness(newBrightness);

        Debug.Log($"Brightness adjusted to: {newBrightness}");
    }

    // Public method you can call from UI button if needed
    public void AdjustBrightness()
    {
        if (brightnessSlider != null)
        {
            AdjustBrightness(brightnessSlider.value);
        }
    }

    void OnDestroy()
    {
        // Clean up listener
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }
}

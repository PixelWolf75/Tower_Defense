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
    AutoExposure exposure;

    // Start is called before the first frame update
    void Start()
    {
        brightness.TryGetSettings(out exposure);

        // Load saved brightness setting
        LoadBrightnessFromSettings();

        AdjustBrightness();
    }

    void LoadBrightnessFromSettings()
    {
        if (SettingsManager.instance != null)
        {
            // Load the saved brightness value
            float savedBrightness = SettingsManager.instance.currentBrightness;

            // Apply to post process
            exposure.keyValue.value = savedBrightness;

            // Update slider to match saved value
            if (brightnessSlider != null)
            {
                brightnessSlider.value = savedBrightness;
            }

            Debug.Log($"Loaded brightness setting: {savedBrightness}");
        }
        else
        {
            Debug.LogWarning("SettingsManager not found! Using default brightness.");
        }
    }

    public void AdjustBrightness()
    {
        if (exposure == null) return;

        if (brightnessSlider.value != 0)
        {
            exposure.keyValue.value = brightnessSlider.value;
        }
        else
        {
            exposure.keyValue.value = 0.05f;
        }

        // Save to SettingsManager
        if (SettingsManager.instance != null)
        {
            SettingsManager.instance.SetBrightness(brightnessSlider.value);
        }
    }

    void OnDestroy()
    {

    }
}

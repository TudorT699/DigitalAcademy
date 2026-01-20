using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsVolumeWithIcons : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;
    public TMP_Text volumePercentText; 

    [Header("Save Key")]
    public string playerPrefsKey = "MasterVolume"; // Save volume between sessions

    [Header("Icon 1 (appears at 25%)")]
    public RectTransform icon1; // The sprite Image RectTransform
    public RectTransform icon1Outside; // Marker position outside
    public RectTransform icon1Inside; // Marker position inside

    [Header("Icon 2 (appears at 50%)")]
    public RectTransform icon2;
    public RectTransform icon2Outside;
    public RectTransform icon2Inside;

    [Header("Icon 3 (appears at 75%)")]
    public RectTransform icon3;
    public RectTransform icon3Outside;
    public RectTransform icon3Inside;

    [Header("Animation")]
    public float slideDuration = 0.25f; // How fast the sprite slides in/out

    private Coroutine icon1Coroutine; // Running animation for icon1
    private Coroutine icon2Coroutine; // Running animation for icon2
    private Coroutine icon3Coroutine; // Running animation for icon3

    private int currentTier = 0; // 0 = none shown, 1 = icon1, 2 = icon1+icon2, 3 = all

    void Start()
    {
        float saved = PlayerPrefs.GetFloat(playerPrefsKey, 1f); // Load saved volume (default 100%)

        int startTier = GetTierFromValue(saved); // Decide which tier should be active at startup
        SetIconsInstant(startTier);
        currentTier = startTier;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f; // Slider starts at 0
            volumeSlider.maxValue = 1f; // Slider ends at 1
            volumeSlider.value = saved; // Set slider to saved value

            ApplyVolume(saved); // Apply right away (volume + icons)

            volumeSlider.onValueChanged.AddListener(ApplyVolume); // Update while dragging
        }
        else
        {
            ApplyVolume(saved); // Still apply volume/icons even if slider missing
        }
    }

    void ApplyVolume(float value)
    {
        AudioListener.volume = value; // Sets global Unity volume

        PlayerPrefs.SetFloat(playerPrefsKey, value); // Save volume
        PlayerPrefs.Save(); // Write to disk

        if (volumePercentText != null) // If percent text exists
        {
            int percent = Mathf.RoundToInt(value * 100f); // Convert 0..1 to 0..100
            volumePercentText.text = percent + "%"; // Display it
        }

        int newTier = GetTierFromValue(value); // Decide which icons should be shown
        if (newTier != currentTier) // Only animate if tier actually changed
        {
            UpdateIcons(currentTier, newTier); // Slide icons in/out depending on change
            currentTier = newTier; // Save new tier
        }
    }

    int GetTierFromValue(float value)
    {
        if (value >= 0.75f) return 3; // 75%
        if (value >= 0.50f) return 2; // 50%
        if (value >= 0.25f) return 1; // 25%
        return 0; // below 25%
    }

    void UpdateIcons(int oldTier, int newTier)
    {
        // Going up only slide in the icons that are unlocked
        if (oldTier < 1 && newTier >= 1) SlideIconIn(icon1, icon1Outside, icon1Inside, ref icon1Coroutine); // Show icon1
        if (oldTier < 2 && newTier >= 2) SlideIconIn(icon2, icon2Outside, icon2Inside, ref icon2Coroutine); // Show icon2
        if (oldTier < 3 && newTier >= 3) SlideIconIn(icon3, icon3Outside, icon3Inside, ref icon3Coroutine); // Show icon3

        // Going down only slide out the icons that are no longer allowed
        if (oldTier >= 3 && newTier < 3) SlideIconOut(icon3, icon3Inside, icon3Outside, ref icon3Coroutine); // Hide icon3
        if (oldTier >= 2 && newTier < 2) SlideIconOut(icon2, icon2Inside, icon2Outside, ref icon2Coroutine); // Hide icon2
        if (oldTier >= 1 && newTier < 1) SlideIconOut(icon1, icon1Inside, icon1Outside, ref icon1Coroutine); // Hide icon1
    }

    void SetIconsInstant(int tier)
    {
        if (icon1Coroutine != null) StopCoroutine(icon1Coroutine);
        if (icon2Coroutine != null) StopCoroutine(icon2Coroutine);
        if (icon3Coroutine != null) StopCoroutine(icon3Coroutine);

        if (icon1 != null && icon1Outside != null && icon1Inside != null)
            icon1.anchoredPosition = (tier >= 1) ? icon1Inside.anchoredPosition : icon1Outside.anchoredPosition;

        if (icon2 != null && icon2Outside != null && icon2Inside != null)
            icon2.anchoredPosition = (tier >= 2) ? icon2Inside.anchoredPosition : icon2Outside.anchoredPosition;

        if (icon3 != null && icon3Outside != null && icon3Inside != null)
            icon3.anchoredPosition = (tier >= 3) ? icon3Inside.anchoredPosition : icon3Outside.anchoredPosition;
    }

    void SlideIconIn(RectTransform icon, RectTransform from, RectTransform to, ref Coroutine running)
    {
        if (icon == null || from == null || to == null) return; // Safety

        if (running != null) StopCoroutine(running); // Stop current animation if any
        running = StartCoroutine(Slide(icon, from.anchoredPosition, to.anchoredPosition)); // Start slide
    }

    void SlideIconOut(RectTransform icon, RectTransform from, RectTransform to, ref Coroutine running)
    {
        if (icon == null || from == null || to == null) return; // Safety

        if (running != null) StopCoroutine(running); // Stop current animation if any
        running = StartCoroutine(Slide(icon, from.anchoredPosition, to.anchoredPosition)); // Start slide
    }

    System.Collections.IEnumerator Slide(RectTransform icon, Vector2 startPos, Vector2 endPos)
    {
        float t = 0f; // Timer
        icon.anchoredPosition = startPos; // Force starting position

        while (t < slideDuration) // Animate for slideDuration seconds
        {
            t += Time.unscaledDeltaTime; // unscaled time so it works even if Time.timeScale changes
            float p = Mathf.Clamp01(t / slideDuration); // 0..1 progress
            float eased = SmoothStep01(p); // Make movement smoother than linear
            icon.anchoredPosition = Vector2.Lerp(startPos, endPos, eased); // Move icon
            yield return null; // Wait 1 frame
        }

        icon.anchoredPosition = endPos; // Snap exactly to end position
    }

    float SmoothStep01(float x)
    {
        return x * x * (3f - 2f * x); // SmoothStep curve (0..1)
    }
}
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Day / Night")]
    public float dayLength = 120f;

    [Header("Sun")]
    public Light sun;
    public float maxSunIntensity = 1f;

    [Header("Moon")]
    public Light moon;
    public float maxMoonIntensity = 0.15f;

    [Header("Settings")]
    public float startTime = 0.25f;

    private float currentTime;

    void Start()
    {
        currentTime = startTime * dayLength;
    }

    void Update()
    {
        // Advance time
        currentTime += Time.deltaTime;

        if (currentTime >= dayLength)
        {
            currentTime = 0f;
        }

        float timeOfDay = currentTime / dayLength;

        // Rotate sun
        float sunAngle = timeOfDay * 360f - 90f;

        transform.rotation = Quaternion.Euler(
            sunAngle,
            -30f,
            0f
        );

        // Calculate how high the sun is
        float sunHeight = Mathf.Sin(timeOfDay * Mathf.PI * 2f);

        // Sun brightness
        float sunBrightness = Mathf.Clamp01(sunHeight);

        sun.intensity = sunBrightness * maxSunIntensity;

        // Moon is strongest when sun is down
        float moonBrightness = 1f - sunBrightness;

        moon.intensity = moonBrightness * maxMoonIntensity;
    }
}
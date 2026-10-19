using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time")]
    public float dayLength = 120f;

    [Range(0f, 1f)]
    public float startTime = 0.5f;

    [Header("Sun")]
    public Light sun;
    public float maxSunIntensity = 1.2f;
    public float sunYaw = -30f;

    [Header("Fantasy Skybox")]
    public Material blendedSkyboxMaterial;
    public Camera mainCamera;
    public float panoramaRotation = 0f;

    [Header("Ambient Lighting")]
    public Color dayAmbient =
        new Color(0.65f, 0.65f, 0.65f, 1f);

    public Color nightAmbient =
        new Color(0.06f, 0.08f, 0.16f, 1f);

    private float currentTime;

    private Material runtimeSkybox;
    private Material previousSkybox;
    private Light previousSun;
    private AmbientMode previousAmbientMode;
    private Color previousAmbientLight;
    private CameraClearFlags previousCameraClearFlags;

    void Start()
    {
        dayLength = Mathf.Max(1f, dayLength);
        currentTime = Mathf.Repeat(startTime, 1f);

        if (sun == null)
            sun = GetComponent<Light>();

        if (sun == null)
        {
            Debug.LogError(
                "DayNightCycle: Assign your Sun Directional Light."
            );
            enabled = false;
            return;
        }

        if (blendedSkyboxMaterial == null ||
            !blendedSkyboxMaterial.HasProperty("_NightBlend"))
        {
            Debug.LogError(
                "Assign the FantasyDayNightSkybox material " +
                "using the Fantasy Day Night Blend shader."
            );
            enabled = false;
            return;
        }

        previousSkybox = RenderSettings.skybox;
        previousSun = RenderSettings.sun;
        previousAmbientMode = RenderSettings.ambientMode;
        previousAmbientLight = RenderSettings.ambientLight;

        if (mainCamera != null)
        {
            previousCameraClearFlags = mainCamera.clearFlags;
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }

        // Use a runtime copy so the original material asset
        // stays unchanged.
        runtimeSkybox = new Material(blendedSkyboxMaterial);
        RenderSettings.skybox = runtimeSkybox;
        RenderSettings.sun = sun;

        // Explicit ambient control for the day/night transition.
        RenderSettings.ambientMode = AmbientMode.Flat;

        UpdateCycle();
    }

    void Update()
    {
        currentTime = Mathf.Repeat(
            currentTime + Time.deltaTime /
            Mathf.Max(1f, dayLength),
            1f
        );

        UpdateCycle();
    }

    void UpdateCycle()
    {
        // 0 = midnight, 0.25 = sunrise,
        // 0.5 = midday, 0.75 = sunset.
        float sunAngle = currentTime * 360f - 90f;

        sun.transform.rotation = Quaternion.Euler(
            sunAngle, sunYaw, 0f
        );

        // Daylight is strongest when the Sun is high.
        float sunHeight = -Mathf.Cos(
            currentTime * Mathf.PI * 2f
        );

        // Blend across the horizon for gradual twilight.
        float daylight = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.InverseLerp(-0.15f, 0.2f, sunHeight)
        );

        float sunBrightness = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.InverseLerp(0f, 0.25f, sunHeight)
        );

        sun.intensity = sunBrightness * maxSunIntensity;

        // Smoothly blend the Fantasy panoramic textures.
        runtimeSkybox.SetFloat(
            "_NightBlend",
            1f - daylight
        );

        runtimeSkybox.SetFloat(
            "_Rotation",
            panoramaRotation
        );

        // Gradually darken the scene's ambient lighting.
        RenderSettings.ambientLight = Color.Lerp(
            nightAmbient,
            dayAmbient,
            daylight
        );
    }

    void OnDestroy()
    {
        if (RenderSettings.skybox == runtimeSkybox)
            RenderSettings.skybox = previousSkybox;

        if (RenderSettings.sun == sun)
            RenderSettings.sun = previousSun;

        RenderSettings.ambientMode = previousAmbientMode;
        RenderSettings.ambientLight = previousAmbientLight;

        if (mainCamera != null)
            mainCamera.clearFlags = previousCameraClearFlags;

        if (runtimeSkybox != null)
            Destroy(runtimeSkybox);
    }
}
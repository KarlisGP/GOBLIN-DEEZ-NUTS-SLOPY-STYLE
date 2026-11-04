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

    [Header("Moon Light")]
    [Tooltip("Optional. Leave empty and a directional light is created for you.")]
    public Light moonLight;
    public float maxMoonIntensity = 0.3f;
    public Color moonLightColor = new Color(0.55f, 0.65f, 1f, 1f);
    [Range(0f, 1f)]
    public float moonShadowStrength = 0.8f;
    public LightShadows moonShadows = LightShadows.Soft;

    [Header("Fantasy Skybox")]
    public Material blendedSkyboxMaterial;
    public Camera mainCamera;
    public float panoramaRotation = 0f;

    [Header("Ambient Lighting")]
    public Color dayAmbient = new Color(0.65f, 0.65f, 0.65f, 1f);
    public Color nightAmbient = new Color(0.06f, 0.08f, 0.16f, 1f);

    [Header("Visible Sun & Moon")]
    [Tooltip("Optional. Leave empty to use a generated soft disc.")]
    public Texture2D sunTexture;
    public Texture2D moonTexture;
    public Color sunColor = new Color(1f, 0.95f, 0.8f, 1f);
    public Color moonColor = new Color(0.85f, 0.9f, 1f, 1f);
    [Tooltip("Apparent size in degrees of sky.")]
    public float sunAngularSize = 8f;
    public float moonAngularSize = 6f;
    [Tooltip("Keep this well inside the camera's far clip plane.")]
    public float celestialDistance = 400f;

    private float currentTime;
    private float sunHeightValue;

    private Material runtimeSkybox;
    private Material previousSkybox;
    private Light previousSun;
    private AmbientMode previousAmbientMode;
    private Color previousAmbientLight;
    private CameraClearFlags previousCameraClearFlags;

    private Transform sunVisual, moonVisual;
    private Material sunMat, moonMat;
    private GameObject createdMoonLightObject;

    void Start()
    {
        dayLength = Mathf.Max(1f, dayLength);
        currentTime = Mathf.Repeat(startTime, 1f);

        if (sun == null)
            sun = GetComponent<Light>();

        if (sun == null)
        {
            Debug.LogError("DayNightCycle: Assign your Sun Directional Light.");
            enabled = false;
            return;
        }

        if (blendedSkyboxMaterial == null ||
            !blendedSkyboxMaterial.HasProperty("_NightBlend"))
        {
            Debug.LogError("Assign the FantasyDayNightSkybox material " +
                           "using the Fantasy Day Night Blend shader.");
            enabled = false;
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        previousSkybox = RenderSettings.skybox;
        previousSun = RenderSettings.sun;
        previousAmbientMode = RenderSettings.ambientMode;
        previousAmbientLight = RenderSettings.ambientLight;

        if (mainCamera != null)
        {
            previousCameraClearFlags = mainCamera.clearFlags;
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }

        runtimeSkybox = new Material(blendedSkyboxMaterial);
        RenderSettings.skybox = runtimeSkybox;
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = AmbientMode.Flat;

        SetupMoonLight();
        CreateCelestialBodies();
        UpdateCycle();
    }

    void Update()
    {
        currentTime = Mathf.Repeat(
            currentTime + Time.deltaTime / Mathf.Max(1f, dayLength), 1f);

        UpdateCycle();
    }

    // Camera-relative positioning happens after the camera has moved.
    void LateUpdate()
    {
        UpdateCelestialBodies();
    }

    // ---------- Moon light ----------

    void SetupMoonLight()
    {
        if (moonLight == null)
        {
            createdMoonLightObject = new GameObject("MoonLight");
            moonLight = createdMoonLightObject.AddComponent<Light>();
        }

        moonLight.type = LightType.Directional;
        moonLight.color = moonLightColor;
        moonLight.intensity = 0f;
        moonLight.shadows = moonShadows;
        moonLight.shadowStrength = moonShadowStrength;
        moonLight.enabled = false;
    }

    void UpdateCycle()
    {
        float sunAngle = currentTime * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, sunYaw, 0f);

        float sunHeight = -Mathf.Cos(currentTime * Mathf.PI * 2f);
        sunHeightValue = sunHeight;

        float daylight = Mathf.SmoothStep(0f, 1f,
            Mathf.InverseLerp(-0.15f, 0.2f, sunHeight));

        float sunBrightness = Mathf.SmoothStep(0f, 1f,
            Mathf.InverseLerp(0f, 0.25f, sunHeight));

        sun.intensity = sunBrightness * maxSunIntensity;

        // Moon light: the moon is exactly opposite the sun, so its light
        // travels in the opposite direction to the sun's light.
        if (moonLight != null)
        {
            moonLight.transform.rotation =
                Quaternion.Euler(sunAngle + 180f, sunYaw, 0f);

            float moonBrightness = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(0f, 0.25f, -sunHeight));

            moonLight.intensity = moonBrightness * maxMoonIntensity;
            moonLight.color = moonLightColor;
            moonLight.shadowStrength = moonShadowStrength;
            moonLight.shadows = moonShadows;

            // Disable completely when it's not contributing, which
            // saves a shadow pass during the day.
            moonLight.enabled = moonLight.intensity > 0.001f;
        }

        // Disable the sun's shadow pass at night as well.
        sun.enabled = sun.intensity > 0.001f;

        runtimeSkybox.SetFloat("_NightBlend", 1f - daylight);
        runtimeSkybox.SetFloat("_Rotation", panoramaRotation);

        RenderSettings.ambientLight =
            Color.Lerp(nightAmbient, dayAmbient, daylight);
    }

    // ---------- Sun & Moon visuals ----------

    void CreateCelestialBodies()
    {
        sunVisual = CreateBody("SunVisual",
            sunTexture != null ? sunTexture : MakeDiscTexture(true),
            out sunMat);

        moonVisual = CreateBody("MoonVisual",
            moonTexture != null ? moonTexture : MakeDiscTexture(false),
            out moonMat);
    }

    Transform CreateBody(string name, Texture2D tex, out Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;

        Destroy(go.GetComponent<Collider>());

        var r = go.GetComponent<MeshRenderer>();
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;

        // URP Unlit, set up as an alpha-blended transparent material.
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null)
        {
            Debug.LogWarning("URP Unlit shader not found, falling back to Sprites/Default.");
            sh = Shader.Find("Sprites/Default");
        }

        mat = new Material(sh);

        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f);   // 0 = Opaque, 1 = Transparent
            mat.SetFloat("_Blend", 0f);     // 0 = Alpha
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetShaderPassEnabled("ShadowCaster", false);
        }

        mat.renderQueue = 3000;

        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        else mat.mainTexture = tex;

        SetMatColor(mat, Color.white);

        r.material = mat;
        return go.transform;
    }

    void SetMatColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else m.color = c;
    }

    void UpdateCelestialBodies()
    {
        if (mainCamera == null || sunVisual == null) return;

        Vector3 camPos = mainCamera.transform.position;
        Vector3 toSun = -sun.transform.forward; // direction toward the sun

        PlaceBody(sunVisual, camPos, toSun, sunAngularSize);
        PlaceBody(moonVisual, camPos, -toSun, moonAngularSize);

        float sunAlpha = Mathf.Clamp01(
            Mathf.InverseLerp(-0.05f, 0.1f, sunHeightValue));
        float moonAlpha = Mathf.Clamp01(
            Mathf.InverseLerp(-0.05f, 0.1f, -sunHeightValue));

        Color sc = sunColor; sc.a = sunAlpha;
        Color mc = moonColor; mc.a = moonAlpha;
        SetMatColor(sunMat, sc);
        SetMatColor(moonMat, mc);

        sunVisual.gameObject.SetActive(sunAlpha > 0.001f);
        moonVisual.gameObject.SetActive(moonAlpha > 0.001f);
    }

    void PlaceBody(Transform body, Vector3 camPos, Vector3 dir, float angularSize)
    {
        float dist = Mathf.Min(celestialDistance,
                               mainCamera.farClipPlane * 0.9f);

        body.position = camPos + dir.normalized * dist;
        body.rotation = Quaternion.LookRotation(body.position - camPos);

        float size = 2f * dist * Mathf.Tan(angularSize * 0.5f * Mathf.Deg2Rad);
        body.localScale = new Vector3(size, size, 1f);
    }

    // Generates a soft disc (glow for the sun, crisp edge for the moon).
    Texture2D MakeDiscTexture(bool glow)
    {
        const int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float a;
                if (glow)
                {
                    float core = 1f - Mathf.SmoothStep(0.25f, 0.3f, d);
                    float halo = Mathf.Pow(Mathf.Clamp01(1f - d), 3f) * 0.6f;
                    a = Mathf.Clamp01(core + halo);
                }
                else
                {
                    a = 1f - Mathf.SmoothStep(0.45f, 0.5f, d);
                    a = Mathf.Max(a, Mathf.Pow(Mathf.Clamp01(1f - d), 4f) * 0.25f);
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        return tex;
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

        if (runtimeSkybox != null) Destroy(runtimeSkybox);
        if (sunVisual != null) Destroy(sunVisual.gameObject);
        if (moonVisual != null) Destroy(moonVisual.gameObject);
        if (sunMat != null) Destroy(sunMat);
        if (moonMat != null) Destroy(moonMat);
        if (createdMoonLightObject != null) Destroy(createdMoonLightObject);
    }
}
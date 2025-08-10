using UnityEngine;

public class LampDartboard : Dartboard
{
    [Header("Light Settings")]
    [SerializeField] private Color lightsOutSky;
    [SerializeField] private Color normalSky;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private float lightsOutFogEnd;

    private Color normalLight;
    private float normalFogEnd;
    private static bool lightsOn = true;
    private Light skyLight;
    private GameObject clouds;

    protected override void Awake()
    {
        base.Awake();

        GameObject skyLightObj = GameObject.Find("SkyLight");

        if (skyLightObj != null)
        {
            skyLight = skyLightObj.GetComponent<Light>();
            if (skyLight == null)
            {
                Debug.LogError("SkyLight GameObject found but has no Light component!");
                return;
            }
        }
        else
        {
            Debug.LogError("SkyLight GameObject not found in scene!");
            return;
        }

        clouds = GameObject.Find("Clouds");
        if (clouds == null)
        {
            Debug.LogWarning("Clouds GameObject not found in scene!");
        }

        if (wallMaterial == null)
        {
            Debug.LogError("Wall material is not assigned! Light switching won't work.");
            return;
        }

        normalLight = skyLight.color;
        normalFogEnd = RenderSettings.fogEndDistance;
    }

    private void Start()
    {
        if (wallMaterial == null || skyLight == null) return;

        SetLightingState(true);
    }

    protected override void OnHit(Dart dart)
    {
        if (wallMaterial != null && skyLight != null)
        {
            LightSwitch();
        }
    }

    private void LightSwitch()
    {
        lightsOn = !lightsOn;
        SetLightingState(lightsOn);

        Debug.Log(lightsOn ? "Lights turned ON" : "Lights turned OFF");
    }

    private void SetLightingState(bool isOn)
    {
        if (isOn)
        {
            // Lights on
            wallMaterial.color = normalSky;
            RenderSettings.fogColor = normalSky;
            RenderSettings.fogEndDistance = normalFogEnd;

            if (Camera.main != null)
                Camera.main.backgroundColor = normalSky;

            skyLight.color = normalLight;

            if (clouds != null)
                clouds.SetActive(true);
        }
        else
        {
            // lights off
            wallMaterial.color = lightsOutSky;
            RenderSettings.fogColor = lightsOutSky;
            RenderSettings.fogEndDistance = lightsOutFogEnd;

            if (Camera.main != null)
                Camera.main.backgroundColor = lightsOutSky;

            skyLight.color = lightsOutSky;

            if (clouds != null)
                clouds.SetActive(false);
        }
    }
}
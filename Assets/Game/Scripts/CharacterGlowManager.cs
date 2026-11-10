/*using UnityEngine;

public class CharacterGlowManager : MonoBehaviour
{
    [Header("Glow Materials")]
    public Material normalMaterial;
    public Material glowMaterial;

    [Header("Glow Settings")]
    [Range(1f, 20f)]
    public float glowIntensityMultiplier = 8.0f;
    public Color glowColor = Color.cyan;

    private Renderer[] allRenderers;

    void Start()
    {
        // Character ke root aur sabhi child components se renderers utha lo
        allRenderers = GetComponentsInChildren<Renderer>(true);
    }

    // Is function ko aap jab chahein (jab hider move kare) call kar sakte hain
    public void SetGlowActive(bool isActive)
    {
        if (allRenderers == null) return;

        foreach (Renderer rend in allRenderers)
        {
            if (rend != null)
            {
                Material targetMat = isActive ? glowMaterial : normalMaterial;
                if (targetMat == null) continue;

                // Poori body ke sabhi sub-materials par aik sath apply hoga
                Material[] mats = rend.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = targetMat;

                    if (isActive)
                    {
                        mats[i].EnableKeyword("_EMISSION");
                        float currentPulse = glowIntensityMultiplier + Mathf.Sin(Time.time * 10f) * 3f;

                        // HDR Emission Color boost (Puri body ko sun light ki tarah chamkane ke liye)
                        if (mats[i].HasProperty("_EmissionColor"))
                        {
                            mats[i].SetColor("_EmissionColor", glowColor * currentPulse);
                        }

                        if (mats[i].HasProperty("_GlowStrength"))
                        {
                            mats[i].SetFloat("_GlowStrength", currentPulse * 10f);
                        }
                    }
                    else
                    {
                        if (mats[i].HasProperty("_EmissionColor"))
                        {
                            mats[i].SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
                rend.materials = mats;
            }
        }
    }
}*/

using UnityEngine;
using UnityEngine.AI;

public class CharacterGlowManager : MonoBehaviour
{
    [Header("Materials")]
    public Material normalMaterial;
    public Material glowMaterial;

    [Header("Dynamic Point Light Settings")]
    public float lightRange = 10f;
    public float lightIntensity = 25f;
    public Color lightColor = Color.cyan;

    private Renderer[] allRenderers;
    private NavMeshAgent navAgent;
    private HiderController playerController;
    private Light dynamicPointLight;

    void Start()
    {
        allRenderers = GetComponentsInChildren<Renderer>(true);
        navAgent = GetComponent<NavMeshAgent>();
        playerController = GetComponent<HiderController>();

        // Character ke center (Chest height) par Light create karo taake poori body chamke
        GameObject lightObj = new GameObject("CharacterPointLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(0f, 1.2f, 0f); // 1.2 height matlab chest/upper body

        dynamicPointLight = lightObj.AddComponent<Light>();
        dynamicPointLight.type = LightType.Point;
        dynamicPointLight.range = lightRange;
        dynamicPointLight.intensity = 0f;
        dynamicPointLight.color = lightColor;
        dynamicPointLight.shadows = LightShadows.Soft;
    }

    void Update()
    {
        bool isMoving = false;

        if (playerController != null)
        {
            isMoving = playerController.IsMoving;
        }
        else if (navAgent != null)
        {
            isMoving = navAgent.velocity.magnitude > 0.05f || true;
        }

        ApplyGlow(isMoving);
    }

    void ApplyGlow(bool glowActive)
    {
        if (allRenderers == null || glowMaterial == null || normalMaterial == null) return;

        if (dynamicPointLight != null)
        {
            float targetIntensity = glowActive ? lightIntensity : 0f;
            dynamicPointLight.intensity = Mathf.Lerp(dynamicPointLight.intensity, targetIntensity, Time.deltaTime * 10f);
        }

        foreach (Renderer rend in allRenderers)
        {
            if (rend != null)
            {
                rend.material = glowActive ? glowMaterial : normalMaterial;

                Material mat = rend.material;
                if (mat != null)
                {
                    if (glowActive)
                    {
                        mat.EnableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            // Intensity ko aur tez kar diya hai taake body poori tarah chamke
                            float intensityFactor = 15f + Mathf.Sin(Time.time * 8f) * 5f;
                            mat.SetColor("_EmissionColor", lightColor * intensityFactor);
                        }
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            mat.SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
            }
        }
    }
}
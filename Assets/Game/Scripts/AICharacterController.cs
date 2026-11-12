
/*using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AICharacterController : MonoBehaviour
{
    [Header("AI Glow Materials")]
    public Material normalMaterial;
    public Material glowMaterial;

    private NavMeshAgent agent;
    public enum Role { BlindMan, Hider }
    public Role aiRole;

    [Header("AI Settings")]
    public float normalSpeed = 6f;
    public float stealthSpeed = 2.5f;
    public float catchDistance = 1.5f;
    private Transform playerTransform;

    // --- Blind Man Smart States ---
    private enum BlindManState { Patrolling, InvestigatingNoise, WaitingAtLocker }
    private BlindManState currentBmState = BlindManState.Patrolling;
    private float lockerWaitTimer = 0f;
    private Vector3 targetDestination;

    [Header("AI Stealth Limits (5s Active, 10s Cooldown)")]
    public float maxStealthDuration = 5f;
    public float stealthCooldownTime = 10f;
    private float currentStealthTime;
    private float currentCooldownTime;
    private bool isStealthCoolingDown = false;
    public bool IsStealthActive { get; private set; }

    private Renderer aiRenderer;
    private Material aiMaterial;
    private Color originalBaseColor;
    private float currentGlowTimer = 0f;
    private Renderer[] aiRenderers;

    private float startSafetyTimer = 2.0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        aiRenderers = GetComponentsInChildren<Renderer>();

        if (aiRenderers.Length > 0 && aiRenderers[0] != null)
        {
            aiMaterial = aiRenderers[0].material;
            originalBaseColor = aiMaterial.color;
        }

        currentStealthTime = maxStealthDuration;

        if (aiRole == Role.Hider)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterHider();
            }
        }

        // Find Player reference
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            HiderController hider = FindFirstObjectByType<HiderController>();
            if (hider != null) playerTransform = hider.transform;
            else
            {
                BlindManController blind = FindFirstObjectByType<BlindManController>();
                if (blind != null) playerTransform = blind.transform;
            }
        }
        else
        {
            playerTransform = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (startSafetyTimer > 0f)
        {
            startSafetyTimer -= Time.deltaTime;
            return; // Pehla 2 second tak AI catch check nahi karega taake sab spread ho jayen
        }

        // UpdateGlowVisuals();

        // --- BLIND MAN SMART AI LOGIC ---
        if (aiRole == Role.BlindMan)
        {
            HandleBlindManLogic();
        }
        // --- HIDER AI LOGIC ---
        else if (aiRole == Role.Hider)
        {
            HandleAIHiderStealthAndMovement();
        }
    }

    // Smart Blind Man Behaviour with States & Patrolling
    void HandleBlindManLogic()
    {
        if (playerTransform == null) return;

        // Check if player is running and making noise (and NOT hidden in a locker)
        bool isPlayerRunningNoisily = false;
        bool isPlayerHidden = false;

        if (!isPlayerHidden)
        {
            HiderController hiderCtrl = playerTransform.GetComponent<HiderController>();
            if (hiderCtrl != null)
            {
                if (hiderCtrl.IsMoving && !hiderCtrl.IsStealthActive)
                {
                    isPlayerRunningNoisily = true;
                }
            }
            else
            {
                AICharacterController aiHiderCtrl = playerTransform.GetComponent<AICharacterController>();
                if (aiHiderCtrl != null && aiHiderCtrl.aiRole == Role.Hider)
                {
                    if (aiHiderCtrl.agent.velocity.magnitude > 0.1f && !aiHiderCtrl.IsStealthActive)
                    {
                        isPlayerRunningNoisily = true;
                    }
                }
            }
        }

        // State Machine for Blind Man
        switch (currentBmState)
        {
            case BlindManState.Patrolling:
                agent.speed = normalSpeed;

                // Agar player ne shor machaya toh foran investigation state mein jao
                if (isPlayerRunningNoisily)
                {
                    currentBmState = BlindManState.InvestigatingNoise;
                    SetSmartNoiseDestination(playerTransform.position);
                    break;
                }

                // Random patrolling agar destination par pahunch jaye
                if (!agent.hasPath || agent.remainingDistance < 1f)
                {
                    Vector3 randomDir = Random.insideUnitSphere * 20f;
                    randomDir += transform.position;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomDir, out hit, 20f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                }
                break;

            case BlindManState.InvestigatingNoise:
                agent.speed = normalSpeed;

                // Agar player abhi bhi shor macha raha hai toh destination update karo (with tactical offset)
                if (isPlayerRunningNoisily)
                {
                    SetSmartNoiseDestination(playerTransform.position);
                }

                // Agar player locker ke andar chala gaya aur AI destination ke qareeb pahunch gaya
                if (isPlayerHidden && agent.remainingDistance < 3f)
                {
                    // Wahan pahunch kar 7 second wait karega (Locker state)
                    currentBmState = BlindManState.WaitingAtLocker;
                    lockerWaitTimer = 7f; // 7 seconds wait
                    agent.ResetPath();
                    break;
                }

                // Agar destination par pahunch gaya aur player nahi mila toh wapas patrol par chala jaye
                if (!agent.hasPath || agent.remainingDistance < 1f)
                {
                    currentBmState = BlindManState.Patrolling;
                }
                break;

            case BlindManState.WaitingAtLocker:
                // Locker ke bahar khara ho kar 7 sec wait karega, fir patrol par wapas chala jayega
                lockerWaitTimer -= Time.deltaTime;
                agent.ResetPath();

                if (lockerWaitTimer <= 0f)
                {
                    currentBmState = BlindManState.Patrolling;
                }

                // Agar is doran player ne dobara shor machaya toh foran chase shuru kar do
                if (isPlayerRunningNoisily)
                {
                    currentBmState = BlindManState.InvestigatingNoise;
                    SetSmartNoiseDestination(playerTransform.position);
                }
                break;
        }

        // Catch Player Check
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= catchDistance && !isPlayerHidden)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHiderCaught();
                gameObject.SetActive(false);
            }
        }
    }

    // Tactical Flanking: Multiple AI agents will take slightly different routes to the noise
    void SetSmartNoiseDestination(Vector3 noisePosition)
    {
        // Safety Check: Agar agent active nahi hai ya NavMesh par nahi hai toh code aage na barhaye
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        Vector3 finalDestination = noisePosition;

        AICharacterController[] allAI = FindObjectsByType<AICharacterController>(FindObjectsSortMode.None);
        int index = 0;
        int totalBlindMen = 0;

        foreach (var ai in allAI)
        {
            if (ai.aiRole == Role.BlindMan)
            {
                if (ai == this) index = totalBlindMen;
                totalBlindMen++;
            }
        }

        if (totalBlindMen > 1)
        {
            float angle = index * (360f / totalBlindMen);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 3f;
            finalDestination += offset;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(finalDestination, out hit, 5f, NavMesh.AllAreas))
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(noisePosition);
            }
        }
    }

    void HandleAIHiderStealthAndMovement()
    {
        // 1. Hider AI Movement Logic: Agar path nahi hai toh random jagah move karega taake ek jagah chipke nahi
        if (!agent.hasPath || agent.remainingDistance < 1.5f)
        {
            Vector3 randomDir = Random.insideUnitSphere * 20f;
            randomDir += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, 20f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        bool isMoving = agent.velocity.magnitude > 0.1f;

        float distanceToBlindMan = float.MaxValue;
        BlindManController[] allBlindMen = FindObjectsByType<BlindManController>(FindObjectsSortMode.None);
        foreach (var bm in allBlindMen)
        {
            if (bm != null)
            {
                float dist = Vector3.Distance(transform.position, bm.transform.position);
                if (dist < distanceToBlindMan) distanceToBlindMan = dist;
            }
        }

        bool shouldBeStealth = distanceToBlindMan < 6f && currentStealthTime > 0f && !isStealthCoolingDown;

        if (isStealthCoolingDown)
        {
            currentCooldownTime -= Time.deltaTime;
            if (currentCooldownTime <= 0f)
            {
                isStealthCoolingDown = false;
                currentStealthTime = maxStealthDuration;
            }
        }
        else
        {
            if (shouldBeStealth)
            {
                IsStealthActive = true;
                currentStealthTime -= Time.deltaTime;

                if (currentStealthTime <= 0f)
                {
                    currentStealthTime = 0f;
                    isStealthCoolingDown = true;
                    currentCooldownTime = stealthCooldownTime;
                }
            }
            else
            {
                IsStealthActive = false;
                if (!shouldBeStealth && currentStealthTime < maxStealthDuration && !isStealthCoolingDown)
                {
                    currentStealthTime += Time.deltaTime * 0.5f;
                    currentStealthTime = Mathf.Clamp(currentStealthTime, 0f, maxStealthDuration);
                }
            }
        }

        agent.speed = IsStealthActive ? stealthSpeed : normalSpeed;

        float maxGlowTime = (GameSettings.Instance != null) ? GameSettings.Instance.glowDuration.Value : 5f;

        // --- FIXED GLOW TIMER FOR AI HIDER ---
        if (isMoving && !IsStealthActive)
        {
            foreach (var bm in allBlindMen)
            {
                if (bm != null)
                {
                    bm.OnHearNoise(transform.position);
                }
            }
            currentGlowTimer = maxGlowTime; // Yeh line timer active karti hai!
        }
        else
        {
            if (currentGlowTimer > 0)
            {
                currentGlowTimer -= Time.deltaTime;
            }
        }
    }

   /* private void UpdateGlowVisuals()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0) return;

        bool isGlowingActive = currentGlowTimer > 0;
        Material targetMat = isGlowingActive ? glowMaterial : normalMaterial;

        if (targetMat == null) return;

        foreach (Renderer rend in renderers)
        {
            if (rend != null)
            {
                Material[] mats = rend.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = targetMat;

                    if (isGlowingActive)
                    {
                        float pulseStrength = 15f + Mathf.Sin(Time.time * 10f) * 8f;

                        if (mats[i].HasProperty("_GlowStrength"))
                        {
                            mats[i].SetFloat("_GlowStrength", pulseStrength);
                        }

                        if (mats[i].HasProperty("_EmissionColor"))
                        {
                            mats[i].SetColor("_EmissionColor", Color.cyan * pulseStrength);
                        }
                    }
                    else
                    {
                        if (mats[i].HasProperty("_GlowStrength"))
                        {
                            mats[i].SetFloat("_GlowStrength", 0f);
                        }
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

/*using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AICharacterController : MonoBehaviour
{
    [Header("AI Glow Materials")]
    public Material normalMaterial;
    public Material glowMaterial;

    private NavMeshAgent agent;
    public enum Role { BlindMan, Hider }
    public Role aiRole;

    [Header("AI Settings")]
    public float normalSpeed = 6f;
    public float stealthSpeed = 2.5f;
    public float catchDistance = 1.5f;
    private Transform playerTransform;

    // --- Blind Man Smart States ---
    private enum BlindManState { Patrolling, InvestigatingNoise, WaitingAtLocker }
    private BlindManState currentBmState = BlindManState.Patrolling;
    private float lockerWaitTimer = 0f;

    [Header("AI Stealth Limits (5s Active, 10s Cooldown)")]
    public float maxStealthDuration = 5f;
    public float stealthCooldownTime = 10f;
    private float currentStealthTime;
    private float currentCooldownTime;
    private bool isStealthCoolingDown = false;
    public bool IsStealthActive { get; private set; }

    private float startSafetyTimer = 2.0f;

    // --- Dynamic Point Light for Body Glow ---
    private Light dynamicPointLight;
    private Renderer[] aiRenderers;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        aiRenderers = GetComponentsInChildren<Renderer>(true);

        currentStealthTime = maxStealthDuration;

        if (aiRole == Role.Hider)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterHider();
            }

            // Hider ke liye Dynamic Point Light banao
            GameObject lightObj = new GameObject("CharacterPointLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            dynamicPointLight = lightObj.AddComponent<Light>();
            dynamicPointLight.type = LightType.Point;
            dynamicPointLight.range = 10f;
            dynamicPointLight.intensity = 0f;
            dynamicPointLight.color = Color.cyan;
            dynamicPointLight.shadows = LightShadows.Soft;
        }

        // Find Player reference
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            HiderController hider = FindFirstObjectByType<HiderController>();
            if (hider != null) playerTransform = hider.transform;
            else
            {
                BlindManController blind = FindFirstObjectByType<BlindManController>();
                if (blind != null) playerTransform = blind.transform;
            }
        }
        else
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (startSafetyTimer > 0f)
        {
            startSafetyTimer -= Time.deltaTime;
            return;
        }

        if (aiRole == Role.BlindMan)
        {
            HandleBlindManLogic();
        }
        else if (aiRole == Role.Hider)
        {
            HandleAIHiderStealthAndMovement();
            UpdateHiderVisualsAndLight();
        }
    }

    void HandleBlindManLogic()
    {
        if (playerTransform == null) return;

        bool isPlayerRunningNoisily = false;
        bool isPlayerHidden = false;

        if (playerTransform != null)
        {
            // Check karo kya human player locker ke andar hidden hai?
            HiderController hiderCtrl = playerTransform.GetComponent<HiderController>();
            if (hiderCtrl != null)
            {
                if (hiderCtrl.isHiddenInLocker)
                {
                    isPlayerHidden = true;
                }
                else if (hiderCtrl.IsMoving && !hiderCtrl.IsStealthActive)
                {
                    isPlayerRunningNoisily = true;
                }
            }
            else
            {
                // Check karo kya AI hider hidden hai?
                AICharacterController aiHiderCtrl = playerTransform.GetComponent<AICharacterController>();
                if (aiHiderCtrl != null && aiHiderCtrl.aiRole == Role.Hider)
                {
                    if (aiHiderCtrl.agent != null && !aiHiderCtrl.agent.enabled)
                    {
                        isPlayerHidden = true;
                    }
                    else if (aiHiderCtrl.agent.velocity.magnitude > 0.1f && !aiHiderCtrl.IsStealthActive)
                    {
                        isPlayerRunningNoisily = true;
                    }
                }
            }
        }

        switch (currentBmState)
        {
            case BlindManState.Patrolling:
                agent.speed = normalSpeed;

                // Agar player bhaag raha hai ya noise ki hai, toh chase karo
                if (isPlayerRunningNoisily)
                {
                    currentBmState = BlindManState.InvestigatingNoise;
                    SetSmartNoiseDestination(playerTransform.position);
                    break;
                }

                if (!agent.hasPath || agent.remainingDistance < 1f)
                {
                    Vector3 randomDir = Random.insideUnitSphere * 20f;
                    randomDir += transform.position;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomDir, out hit, 20f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                }
                break;

            case BlindManState.InvestigatingNoise:
                agent.speed = normalSpeed;

                if (isPlayerRunningNoisily)
                {
                    SetSmartNoiseDestination(playerTransform.position);
                }

                // Yahan fix kiya hai: Ab yeh real physical distance check karega ke AI locker ke qareeb pahuncha hai ya nahi
                if (isPlayerHidden && Vector3.Distance(transform.position, playerTransform.position) < 4f)
                {
                    currentBmState = BlindManState.WaitingAtLocker;
                    lockerWaitTimer = 7f; // 7 seconds wait karega
                    agent.ResetPath();
                    break;
                }

                if (!agent.hasPath || agent.remainingDistance < 1f)
                {
                    currentBmState = BlindManState.Patrolling;
                }
                break;

            case BlindManState.WaitingAtLocker:
                lockerWaitTimer -= Time.deltaTime;
                agent.ResetPath();

                // 7 seconds poore hone ke baad wapas Patrolling par chala jayega
                if (lockerWaitTimer <= 0f)
                {
                    currentBmState = BlindManState.Patrolling;
                }

                // Agar wait karte waqt player ne phir se noise kar di, toh dobara chase shuru kar do
                if (isPlayerRunningNoisily)
                {
                    currentBmState = BlindManState.InvestigatingNoise;
                    SetSmartNoiseDestination(playerTransform.position);
                }
                break;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= catchDistance && !isPlayerHidden)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHiderCaught();
                gameObject.SetActive(false);
            }
        }
    }

    void SetSmartNoiseDestination(Vector3 noisePosition)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        Vector3 finalDestination = noisePosition;
        AICharacterController[] allAI = FindObjectsByType<AICharacterController>(FindObjectsSortMode.None);
        int index = 0, totalBlindMen = 0;

        foreach (var ai in allAI)
        {
            if (ai.aiRole == Role.BlindMan)
            {
                if (ai == this) index = totalBlindMen;
                totalBlindMen++;
            }
        }

        if (totalBlindMen > 1)
        {
            float angle = index * (360f / totalBlindMen);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 3f;
            finalDestination += offset;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(finalDestination, out hit, 5f, NavMesh.AllAreas))
        {
            if (agent.isOnNavMesh) agent.SetDestination(hit.position);
        }
        else
        {
            if (agent.isOnNavMesh) agent.SetDestination(noisePosition);
        }
    }

    void HandleAIHiderStealthAndMovement()
    {
        Transform closestBlindMan = null;
        float minDst = float.MaxValue;

        BlindManController[] allBlindMen = FindObjectsByType<BlindManController>(FindObjectsSortMode.None);
        foreach (var bm in allBlindMen)
        {
            if (bm != null)
            {
                float dist = Vector3.Distance(transform.position, bm.transform.position);
                if (dist < minDst) { minDst = dist; closestBlindMan = bm.transform; }
            }
        }

        AICharacterController[] allAI = FindObjectsByType<AICharacterController>(FindObjectsSortMode.None);
        foreach (var ai in allAI)
        {
            if (ai != null && ai.aiRole == Role.BlindMan)
            {
                float dist = Vector3.Distance(transform.position, ai.transform.position);
                if (dist < minDst) { minDst = dist; closestBlindMan = ai.transform; }
            }
        }

        // --- FIXED LOCKER HIDING LOGIC ---
        if (minDst < 6f)
        {
            GameObject nearestLocker = FindNearestLocker();
            if (nearestLocker != null)
            {
                float distToLocker = Vector3.Distance(transform.position, nearestLocker.transform.position);

                // Locker ke bahar safe spot par destination set karo taake NavMesh block na ho
                agent.SetDestination(nearestLocker.transform.position);

                // Agar AI locker ke 2.5 meter ke qareeb aa gaya hai, toh chup jao!
                if (distToLocker < 2.5f)
                {
                    HideInLocker(true);
                    return;
                }
            }
        }

        HideInLocker(false);

        // Stealth & Flee logic
        bool shouldBeStealth = minDst < 8f && currentStealthTime > 0f && !isStealthCoolingDown;

        if (isStealthCoolingDown)
        {
            currentCooldownTime -= Time.deltaTime;
            if (currentCooldownTime <= 0f) { isStealthCoolingDown = false; currentStealthTime = maxStealthDuration; }
        }
        else
        {
            if (shouldBeStealth)
            {
                IsStealthActive = true;
                currentStealthTime -= Time.deltaTime;
                if (currentStealthTime <= 0f) { currentStealthTime = 0f; isStealthCoolingDown = true; currentCooldownTime = stealthCooldownTime; }
            }
            else
            {
                IsStealthActive = false;
                if (!shouldBeStealth && currentStealthTime < maxStealthDuration && !isStealthCoolingDown)
                {
                    currentStealthTime += Time.deltaTime * 0.5f;
                    currentStealthTime = Mathf.Clamp(currentStealthTime, 0f, maxStealthDuration);
                }
            }
        }

        agent.speed = IsStealthActive ? stealthSpeed : normalSpeed;

        if (closestBlindMan != null && minDst < 10f)
        {
            Vector3 fleeDir = transform.position - closestBlindMan.position;
            Vector3 targetEscapePos = transform.position + fleeDir.normalized * 10f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetEscapePos, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            if (!agent.hasPath || agent.remainingDistance < 1.5f)
            {
                Vector3 randomDir = Random.insideUnitSphere * 20f + transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDir, out hit, 20f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }
    }

    GameObject FindNearestLocker()
    {
        Transform[] allTrans = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        GameObject closest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (var t in allTrans)
        {
            if (t != null && (t.name.Contains("Locker") || t.name.Contains("locker")))
            {
                float dist = Vector3.Distance(t.position, currentPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = t.gameObject;
                }
            }
        }
        return closest;
    }

    void HideInLocker(bool hide)
    {
        // AI ko invisible aur un-targetable kar do jab wo locker mein ho
        foreach (var rend in aiRenderers)
        {
            if (rend != null) rend.enabled = !hide;
        }
        if (agent != null) agent.enabled = !hide;
    }

    // --- STRICT GLOW & MATERIAL CONTROL ---
    // --- FIXED MATERIAL & GLOW SWAPPING FUNCTION ---
    void UpdateHiderVisualsAndLight()
    {
        // Check karo ke AI asal mein chal raha hai ya nahi
        bool isMoving = agent != null && agent.velocity.magnitude > 0.1f;

        // Agar AI move kar raha hai AUR stealth active NAHI hai, tab hi glow on hoga
        bool showGlow = isMoving && !IsStealthActive;

        // Point light ki intensity control
        if (dynamicPointLight != null)
        {
            float targetLightIntensity = showGlow ? 25f : 0f;
            dynamicPointLight.intensity = Mathf.Lerp(dynamicPointLight.intensity, targetLightIntensity, Time.deltaTime * 10f);
        }

        // Target material select karo
        Material targetMat = showGlow ? glowMaterial : normalMaterial;
        if (targetMat == null) return;

        foreach (Renderer rend in aiRenderers)
        {
            if (rend != null)
            {
                // Material assign karo
                rend.material = targetMat;
                Material mat = rend.material;

                if (mat != null)
                {
                    if (showGlow)
                    {
                        mat.EnableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            float pulseStrength = 15f + Mathf.Sin(Time.time * 8f) * 5f;
                            mat.SetColor("_EmissionColor", Color.cyan * pulseStrength);
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

        // Sirf tab noise bhejo jab sach mein glow active ho
        if (showGlow)
        {
            BlindManController[] allBlindMen = FindObjectsByType<BlindManController>(FindObjectsSortMode.None);
            foreach (var bm in allBlindMen)
            {
                if (bm != null) bm.OnHearNoise(transform.position);
            }
        }
    }
}*/

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class AICharacterController : MonoBehaviour
{
    [Header("AI Glow Materials")]
    public Material normalMaterial;
    public Material glowMaterial;

    private NavMeshAgent agent;
    public enum Role { BlindMan, Hider }
    public Role aiRole;

    [Header("AI Settings")]
    public float normalSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float stealthSpeed = 2.5f;
    public float catchDistance = 2.2f;
    public float chaseStoppingDistance = 0.15f;
    private Transform playerTransform;

    // --- State Machine Enums & Variables for Blind Man ---
    public enum BlindManState { Patrolling, InvestigatingNoise, WaitingAtLocker, Chasing }
    public BlindManState currentBmState = BlindManState.Patrolling;

    private float lockerWaitTimer = 0f;
    private float lockerDetectionDistance = 6f;
    private float lockerIgnoreDuration = 12f;
    private float lockerIgnoreTimer = 0f;
    private Transform ignoredLockerTarget = null;
    private Transform currentTarget = null;

    [Header("AI Stealth Limits (5s Active, 10s Cooldown)")]
    public float maxStealthDuration = 5f;
    public float stealthCooldownTime = 10f;
    private float currentStealthTime;
    private float currentCooldownTime;
    private bool isStealthCoolingDown = false;
    public bool IsStealthActive { get; private set; }

    private float startSafetyTimer = 2.0f;

    // --- Dynamic Point Light for Body Glow ---
    private Light dynamicPointLight;
    private Renderer[] aiRenderers;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        aiRenderers = GetComponentsInChildren<Renderer>(true);

        currentStealthTime = maxStealthDuration;

        if (aiRole == Role.Hider)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterHider();
            }

            GameObject lightObj = new GameObject("CharacterPointLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            dynamicPointLight = lightObj.AddComponent<Light>();
            dynamicPointLight.type = LightType.Point;
            dynamicPointLight.range = 10f;
            dynamicPointLight.intensity = 0f;
            dynamicPointLight.color = Color.cyan;
            dynamicPointLight.shadows = LightShadows.Soft;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            HiderController hider = FindFirstObjectByType<HiderController>();
            if (hider != null) playerTransform = hider.transform;
            else
            {
                BlindManController blind = FindFirstObjectByType<BlindManController>();
                if (blind != null) playerTransform = blind.transform;
            }
        }
        else
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (startSafetyTimer > 0f)
        {
            startSafetyTimer -= Time.deltaTime;
            return;
        }

        if (aiRole == Role.BlindMan)
        {
            HandleBlindManLogic();
        }
        else if (aiRole == Role.Hider)
        {
            HandleAIHiderStealthAndMovement();
            UpdateHiderVisualsAndLight();
        }
    }

    void HandleBlindManLogic()
    {
        // 1. Basic Safety Checks
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        agent.isStopped = false;
        agent.stoppingDistance = chaseStoppingDistance;

        // 2. Locker Ignore Timer Update
        if (lockerIgnoreTimer > 0f)
        {
            lockerIgnoreTimer -= Time.deltaTime;
            if (lockerIgnoreTimer <= 0f)
            {
                lockerIgnoreTimer = 0f;
                ignoredLockerTarget = null;
            }
        }

        // 3. Scan for Hiders (Player + AI Hiders)
        Transform bestTarget = null;
        float minDst = float.MaxValue;
        bool targetIsRunning = false;
        bool targetIsHidden = false;
        bool targetIsStealth = false;

        // Check User Player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            HiderController hiderComp = FindFirstObjectByType<HiderController>();
            if (hiderComp != null) playerObj = hiderComp.gameObject;
        }

        if (playerObj != null)
        {
            float dist = Vector3.Distance(transform.position, playerObj.transform.position);
            if (dist < minDst)
            {
                minDst = dist;
                bestTarget = playerObj.transform;

                HiderController hiderCtrl = playerObj.GetComponent<HiderController>();
                if (hiderCtrl != null)
                {
                    targetIsHidden = hiderCtrl.isHiddenInLocker;
                    targetIsStealth = hiderCtrl.IsStealthActive;
                    targetIsRunning = hiderCtrl.IsMoving && !targetIsStealth && !targetIsHidden;
                }
            }
        }

        // Check AI Hiders
        AICharacterController[] allAI = FindObjectsByType<AICharacterController>(FindObjectsSortMode.None);
        foreach (var ai in allAI)
        {
            if (ai != null && ai.aiRole == Role.Hider && ai.gameObject.activeSelf)
            {
                float dist = Vector3.Distance(transform.position, ai.transform.position);
                if (dist < minDst)
                {
                    minDst = dist;
                    bestTarget = ai.transform;
                    targetIsHidden = (ai.agent != null && !ai.agent.enabled);
                    targetIsStealth = ai.IsStealthActive;
                    targetIsRunning = (ai.agent != null && ai.agent.velocity.magnitude > 0.25f && !targetIsStealth && !targetIsHidden);
                }
            }
        }

        // Handle Locker Waiting State Separately if active
        if (currentBmState == BlindManState.WaitingAtLocker)
        {
            HandleLockerWaitingLogic();
            return;
        }

        // Check if target is hidden in locker and we are close enough
        if (targetIsHidden && minDst <= lockerDetectionDistance && bestTarget != ignoredLockerTarget)
        {
            EnterLockerWaitingState(bestTarget);
            return;
        }

        // 4. State Machine Execution
        switch (currentBmState)
        {
            case BlindManState.Patrolling:
                agent.speed = normalSpeed;

                if (bestTarget != null && !targetIsHidden)
                {
                    if (targetIsRunning || minDst < 8f)
                    {
                        currentBmState = BlindManState.InvestigatingNoise;
                        SetSafeDestination(bestTarget.position);
                    }
                    else if (!targetIsStealth && minDst < 4f)
                    {
                        currentBmState = BlindManState.Chasing;
                        SetSafeDestination(bestTarget.position);
                    }
                }
                else
                {
                    PatrolRandomly();
                }
                break;

            case BlindManState.InvestigatingNoise:
                agent.speed = sprintSpeed;
                agent.stoppingDistance = 0.3f;

                if (bestTarget != null && !targetIsHidden)
                {
                    if (minDst <= 10f)
                    {
                        currentBmState = BlindManState.Chasing;
                    }
                    else
                    {
                        SetSafeDestination(bestTarget.position);
                    }
                }
                else
                {
                    if (!agent.hasPath || agent.remainingDistance < 1f)
                    {
                        currentBmState = BlindManState.Patrolling;
                        PatrolRandomly();
                    }
                }
                break;

            case BlindManState.Chasing:
                agent.speed = sprintSpeed;
                agent.stoppingDistance = chaseStoppingDistance;

                if (bestTarget != null && !targetIsHidden)
                {
                    SetSafeDestination(bestTarget.position);

                    // CATCH CHECK
                    if (minDst <= catchDistance)
                    {
                        ExecuteCatch(bestTarget, playerObj);
                    }
                }
                else
                {
                    currentBmState = BlindManState.Patrolling;
                    PatrolRandomly();
                }
                break;
        }
    }

    void EnterLockerWaitingState(Transform lockerTarget)
    {
        currentBmState = BlindManState.WaitingAtLocker;
        lockerWaitTimer = 7f;
        currentTarget = lockerTarget;
        agent.isStopped = true;
        agent.ResetPath();
        Debug.Log("Blind Man: Investigating locker, waiting 7 seconds.");
    }

    void HandleLockerWaitingLogic()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        lockerWaitTimer -= Time.deltaTime;

        if (lockerWaitTimer <= 0f)
        {
            lockerWaitTimer = 0f;
            ignoredLockerTarget = currentTarget;
            lockerIgnoreTimer = lockerIgnoreDuration;

            currentTarget = null;
            agent.isStopped = false;
            agent.ResetPath();
            currentBmState = BlindManState.Patrolling;

            Vector3 escapePos = transform.position + (Random.insideUnitSphere * 25f);
            escapePos.y = transform.position.y;
            SetSafeDestination(escapePos);
            Debug.Log("Blind Man: 7 seconds over, leaving locker area!");
        }
    }

    void SetSafeDestination(Vector3 targetPos)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    void PatrolRandomly()
    {
        if (!agent.hasPath || agent.remainingDistance < 1.5f)
        {
            Vector3 randomDir = Random.insideUnitSphere * 25f + transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, 20f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }

    void ExecuteCatch(Transform target, GameObject playerObj)
    {
        if (playerObj != null && target == playerObj.transform)
        {
            Debug.Log(">>> GAME OVER: User Player Caught by Blind Man!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver(1);
            }
        }
        else
        {
            AICharacterController caughtAiHider = target.GetComponent<AICharacterController>();
            if (caughtAiHider != null)
            {
                Debug.Log("AI HIDER CAUGHT!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnHiderCaught();
                }
                caughtAiHider.gameObject.SetActive(false);
                currentBmState = BlindManState.Patrolling;
            }
        }
    }

    public void OnHearNoise(Vector3 noisePosition)
    {
        if (currentBmState == BlindManState.WaitingAtLocker) return;
        if (currentBmState == BlindManState.Chasing) return;

        currentBmState = BlindManState.InvestigatingNoise;
        SetSafeDestination(noisePosition);
    }

    void HandleAIHiderStealthAndMovement()
    {
        Transform closestBlindMan = null;
        float minDst = float.MaxValue;

        BlindManController[] allBlindMen = FindObjectsByType<BlindManController>(FindObjectsSortMode.None);
        foreach (var bm in allBlindMen)
        {
            if (bm != null)
            {
                float dist = Vector3.Distance(transform.position, bm.transform.position);
                if (dist < minDst) { minDst = dist; closestBlindMan = bm.transform; }
            }
        }

        AICharacterController[] allAI = FindObjectsByType<AICharacterController>(FindObjectsSortMode.None);
        foreach (var ai in allAI)
        {
            if (ai != null && ai.aiRole == Role.BlindMan)
            {
                float dist = Vector3.Distance(transform.position, ai.transform.position);
                if (dist < minDst) { minDst = dist; closestBlindMan = ai.transform; }
            }
        }

        if (minDst < 6f)
        {
            GameObject nearestLocker = FindNearestLocker();
            if (nearestLocker != null)
            {
                float distToLocker = Vector3.Distance(transform.position, nearestLocker.transform.position);

                agent.speed = sprintSpeed;
                agent.SetDestination(nearestLocker.transform.position);

                if (distToLocker < 2.5f)
                {
                    HideInLocker(true);
                    return;
                }
            }
        }

        HideInLocker(false);

        bool shouldBeStealth = minDst < 8f && currentStealthTime > 0f && !isStealthCoolingDown;

        if (isStealthCoolingDown)
        {
            currentCooldownTime -= Time.deltaTime;
            if (currentCooldownTime <= 0f) { isStealthCoolingDown = false; currentStealthTime = maxStealthDuration; }
        }
        else
        {
            if (shouldBeStealth)
            {
                IsStealthActive = true;
                currentStealthTime -= Time.deltaTime;
                if (currentStealthTime <= 0f) { currentStealthTime = 0f; isStealthCoolingDown = true; currentCooldownTime = stealthCooldownTime; }
            }
            else
            {
                IsStealthActive = false;
                if (!shouldBeStealth && currentStealthTime < maxStealthDuration && !isStealthCoolingDown)
                {
                    currentStealthTime += Time.deltaTime * 0.5f;
                    currentStealthTime = Mathf.Clamp(currentStealthTime, 0f, maxStealthDuration);
                }
            }
        }

        if (closestBlindMan != null && minDst < 10f)
        {
            agent.speed = IsStealthActive ? stealthSpeed : sprintSpeed;

            Vector3 fleeDir = transform.position - closestBlindMan.position;
            Vector3 targetEscapePos = transform.position + fleeDir.normalized * 10f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetEscapePos, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            agent.speed = IsStealthActive ? stealthSpeed : normalSpeed;

            if (!agent.hasPath || agent.remainingDistance < 1.5f)
            {
                Vector3 randomDir = Random.insideUnitSphere * 20f + transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDir, out hit, 20f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }
    }

    GameObject FindNearestLocker()
    {
        Transform[] allTrans = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        GameObject closest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (var t in allTrans)
        {
            if (t != null && (t.name.Contains("Locker") || t.name.Contains("locker")))
            {
                float dist = Vector3.Distance(t.position, currentPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = t.gameObject;
                }
            }
        }
        return closest;
    }

    void HideInLocker(bool hide)
    {
        foreach (var rend in aiRenderers)
        {
            if (rend != null) rend.enabled = !hide;
        }
        if (agent != null) agent.enabled = !hide;
    }

    void UpdateHiderVisualsAndLight()
    {
        bool isMoving = agent != null && agent.velocity.magnitude > 0.1f;
        bool showGlow = isMoving && !IsStealthActive;

        if (dynamicPointLight != null)
        {
            float targetLightIntensity = showGlow ? 25f : 0f;
            dynamicPointLight.intensity = Mathf.Lerp(dynamicPointLight.intensity, targetLightIntensity, Time.deltaTime * 10f);
        }

        Material targetMat = showGlow ? glowMaterial : normalMaterial;
        if (targetMat == null) return;

        foreach (Renderer rend in aiRenderers)
        {
            if (rend != null)
            {
                rend.material = targetMat;
                Material mat = rend.material;

                if (mat != null)
                {
                    if (showGlow)
                    {
                        mat.EnableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            float pulseStrength = 15f + Mathf.Sin(Time.time * 8f) * 5f;
                            mat.SetColor("_EmissionColor", Color.cyan * pulseStrength);
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

        if (showGlow)
        {
            BlindManController[] allBlindMen = FindObjectsByType<BlindManController>(FindObjectsSortMode.None);
            foreach (var bm in allBlindMen)
            {
                if (bm != null) bm.OnHearNoise(transform.position);
            }
        }
    }
}

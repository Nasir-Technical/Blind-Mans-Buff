/*using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class HiderController : MonoBehaviour
{
    private CharacterController controller;
    private Renderer hiderRenderer;

    public float normalSpeed = 6f;
    public float stealthSpeed = 2.5f;
    public float mouseSensitivity = 2f;
    public bool IsMoving { get; private set; }
    public bool IsStealthActive { get; private set; } // AI ke liye stealth status

    private float verticalRotation = 0f;
    private float currentGlowTimer = 0f;

    private float gravity = -9.81f;
    private Vector3 velocity;

    private Material hiderMaterial;
    private Color originalBaseColor;
    private Camera playerCam;

    [Header("Stealth Limits (5s Active, 10s Cooldown)")]
    public float maxStealthDuration = 5f;
    public float stealthCooldownTime = 10f;
    private float currentStealthTime;
    private float currentCooldownTime;
    private bool isStealthCoolingDown = false;
    private bool canUseStealth = true;

    [Header("Glow Materials")]
    public Material normalMaterial;
    public Material glowMaterial; // Yahan HiderGlowMat assign hoga

    void Start()
    {
        controller = GetComponent<CharacterController>();
        hiderRenderer = GetComponentInChildren<Renderer>();

        if (hiderRenderer != null)
        {
            hiderMaterial = hiderRenderer.material;
            originalBaseColor = hiderMaterial.color;
        }

        playerCam = GetComponentInChildren<Camera>();
        if (playerCam != null)
        {
            playerCam.enabled = true;
            playerCam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            playerCam.transform.localRotation = Quaternion.identity;
            playerCam.clearFlags = CameraClearFlags.Skybox;

            AudioListener listener = playerCam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        currentStealthTime = maxStealthDuration;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        // Agar controller hi nahi hai ya band (disabled) hai, toh update yahin rok do!
        if (controller == null || !controller.enabled) return;

        UpdateGlowVisuals();

        // --- MOUSE LOOK ---
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.1f;
            transform.Rotate(Vector3.up * mouseDelta.x);
            verticalRotation -= mouseDelta.y;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

            if (playerCam != null)
            {
                playerCam.transform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
            }
        }

        // --- MOVEMENT & GRAVITY ---
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = 0f, moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
        }

        IsMoving = (moveX != 0 || moveZ != 0);

        // --- STEALTH & COOLDOWN LOGIC ---
        bool isShiftPressed = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        if (!isShiftPressed)
        {
            canUseStealth = true;
        }

        bool isStealth = false;

        if (isStealthCoolingDown)
        {
            currentCooldownTime -= Time.deltaTime;
            if (currentCooldownTime <= 0f)
            {
                isStealthCoolingDown = false;
                currentStealthTime = maxStealthDuration;
                Debug.Log("Stealth ability reset and ready!");
            }
        }
        else
        {
            if (isShiftPressed && IsMoving && currentStealthTime > 0f && canUseStealth)
            {
                isStealth = true;
                currentStealthTime -= Time.deltaTime;

                if (currentStealthTime <= 0f)
                {
                    currentStealthTime = 0f;
                    isStealthCoolingDown = true;
                    currentCooldownTime = stealthCooldownTime;
                    canUseStealth = false;
                    Debug.Log("Stealth limit reached! Cooldown started for 10 seconds.");
                }
            }
            else
            {
                if (!isShiftPressed && currentStealthTime < maxStealthDuration && !isStealthCoolingDown)
                {
                    currentStealthTime += Time.deltaTime * 0.5f;
                    currentStealthTime = Mathf.Clamp(currentStealthTime, 0f, maxStealthDuration);
                }
            }
        }

        IsStealthActive = isStealth; // Update property value
        float currentSpeed = IsStealthActive ? stealthSpeed : normalSpeed;

        float maxGlowTime = (GameSettings.Instance != null) ? GameSettings.Instance.glowDuration.Value : 5f;

        if (IsMoving && !IsStealthActive)
        {
            BlindManController[] allBlindMen = FindObjectsByType<BlindManController>(FindObjectsSortMode.None);
            foreach (BlindManController blindMan in allBlindMen)
            {
                if (blindMan != null)
                {
                    blindMan.OnHearNoise(transform.position);
                }
            }
            currentGlowTimer = maxGlowTime;
        }
        else
        {
            if (currentGlowTimer > 0)
            {
                currentGlowTimer -= Time.deltaTime;
            }
        }

        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateGlowVisuals()
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
                        // Shader Graph ke backend reference names (underscore ke sath)
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

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class HiderController : MonoBehaviour
{
    private CharacterController controller;
    private Renderer hiderRenderer;

    public float normalSpeed = 6f;
    public float sprintSpeed = 10f; // C key sprint speed
    public float stealthSpeed = 2.5f;
    public float mouseSensitivity = 2f;
    public bool IsMoving { get; private set; }
    public bool IsStealthActive { get; private set; }

    private float verticalRotation = 0f;
    private float currentGlowTimer = 0f;

    private float gravity = -9.81f;
    private Vector3 velocity;

    private Material hiderMaterial;
    private Color originalBaseColor;
    private Camera playerCam;

    [Header("Stealth Limits (5s Active, 10s Cooldown)")]
    public float maxStealthDuration = 5f;
    public float stealthCooldownTime = 10f;
    private float currentStealthTime;
    private float currentCooldownTime;
    private bool isStealthCoolingDown = false;
    private bool canUseStealth = true;

    [Header("Glow Materials")]
    public Material normalMaterial;
    public Material glowMaterial;

    [HideInInspector] public bool isHiddenInLocker = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterHider(); // Yahan user player register ho jayega!
        }

        controller = GetComponent<CharacterController>();
        hiderRenderer = GetComponentInChildren<Renderer>();

        if (hiderRenderer != null)
        {
            hiderMaterial = hiderRenderer.material;
            originalBaseColor = hiderMaterial.color;
        }

        playerCam = GetComponentInChildren<Camera>();
        if (playerCam != null)
        {
            playerCam.enabled = true;
            playerCam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            playerCam.transform.localRotation = Quaternion.identity;
            playerCam.clearFlags = CameraClearFlags.Skybox;

            AudioListener listener = playerCam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        currentStealthTime = maxStealthDuration;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (controller == null) return;

        // --- MOUSE LOOK ---
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.1f;
            transform.Rotate(Vector3.up * mouseDelta.x);
            verticalRotation -= mouseDelta.y;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

            if (playerCam != null)
            {
                playerCam.transform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
            }
        }

        if (isHiddenInLocker) return;

        UpdateGlowVisuals();

        // --- MOVEMENT & GRAVITY ---
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = 0f, moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
        }

        IsMoving = (moveX != 0 || moveZ != 0);

        // --- STEALTH LOGIC (Left Shift) ---
        bool isShiftPressed = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        if (!isShiftPressed)
        {
            canUseStealth = true;
        }

        bool isStealth = false;

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
            if (isShiftPressed && IsMoving && currentStealthTime > 0f && canUseStealth)
            {
                isStealth = true;
                currentStealthTime -= Time.deltaTime;

                if (currentStealthTime <= 0f)
                {
                    currentStealthTime = 0f;
                    isStealthCoolingDown = true;
                    currentCooldownTime = stealthCooldownTime;
                    canUseStealth = false;
                }
            }
            else
            {
                if (!isShiftPressed && currentStealthTime < maxStealthDuration && !isStealthCoolingDown)
                {
                    currentStealthTime += Time.deltaTime * 0.5f;
                    currentStealthTime = Mathf.Clamp(currentStealthTime, 0f, maxStealthDuration);
                }
            }
        }

        IsStealthActive = isStealth;

        // --- SPRINT LOGIC (C Key) ---
        bool isSprintPressed = Keyboard.current != null && Keyboard.current.cKey.isPressed;
        bool isSprint = isSprintPressed && IsMoving && !IsStealthActive;

        // Speed selection
        float currentSpeed = normalSpeed;
        if (IsStealthActive)
        {
            currentSpeed = stealthSpeed;
        }
        else if (isSprint)
        {
            currentSpeed = sprintSpeed;
        }

        // --- NOISE & GLOW TRIGGER ---
        float maxGlowTime = (GameSettings.Instance != null) ? GameSettings.Instance.glowDuration.Value : 5f;

        if (IsMoving && !IsStealthActive)
        {
            BlindManController[] allBlindMen = FindObjectsByType<BlindManController>(FindObjectsSortMode.None);
            foreach (BlindManController blindMan in allBlindMen)
            {
                if (blindMan != null)
                {
                    blindMan.OnHearNoise(transform.position);
                }
            }
            currentGlowTimer = maxGlowTime;
        }
        else
        {
            if (currentGlowTimer > 0)
            {
                currentGlowTimer -= Time.deltaTime;
            }
        }

        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateGlowVisuals()
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
                        if (mats[i].HasProperty("_GlowStrength")) mats[i].SetFloat("_GlowStrength", pulseStrength);
                        if (mats[i].HasProperty("_EmissionColor")) mats[i].SetColor("_EmissionColor", Color.cyan * pulseStrength);
                    }
                    else
                    {
                        if (mats[i].HasProperty("_GlowStrength")) mats[i].SetFloat("_GlowStrength", 0f);
                        if (mats[i].HasProperty("_EmissionColor")) mats[i].SetColor("_EmissionColor", Color.black);
                    }
                }
                rend.materials = mats;
            }
        }
    }
}
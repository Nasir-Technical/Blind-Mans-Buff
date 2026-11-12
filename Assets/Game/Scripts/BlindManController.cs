/*using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class BlindManController : MonoBehaviour
{
    private CharacterController controller;
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    private Camera playerCam;
    private float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Catching Settings")]
    public float catchRange = 3f; // Hider ko pakre ki max distance
    public LayerMask hiderLayer;    // Hider ka layer (agar assign karna ho)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCam = GetComponentInChildren<Camera>();

        if (playerCam != null)
        {
            playerCam.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (controller == null) return;

        // Mouse Look
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

        // Movement & Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = 0f, moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;

            // 'E' Key press karne par Hider ko catch karne ki koshish karega
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryCatchHider();
            }
        }

        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void TryCatchHider()
    {
        // Scene mein jitne bhi AICharacters hain unhe dhund kar check karein ke kaun sa hider sabse kareeb hai
        AICharacterController[] allAI = FindObjectsByType<AICharacterController>(FindObjectsSortMode.None);

        foreach (var ai in allAI)
        {
            if (ai.aiRole == AICharacterController.Role.Hider)
            {
                float distance = Vector3.Distance(transform.position, ai.transform.position);

                // Agar distance 4 meter ya us se kam hai, toh hider pakra gaya!
                if (distance <= 4f)
                {
                   // Debug.Log("Hider Caught by Player Blind Man: " + ai.gameObject.name);

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.OnHiderCaught(); // GameManager ko batayein ke ek hider pakra gaya
                    }

                    ai.gameObject.SetActive(false);
                    return;
                }
            }
        }

        // Debug.Log("Koi Hider kareeb nahi hai!");
    }

    public void OnHearNoise(Vector3 noisePosition)
    {
        // Debug.Log("Blind Man heard noise at: " + noisePosition);
    }
}*/

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class BlindManController : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 85f; // Shift dabane par speed barh jayegi
    private float currentSpeed;

    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    private Camera playerCam;
    private float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Catching Settings")]
    public float catchRange = 3f;
    public LayerMask hiderLayer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCam = GetComponentInChildren<Camera>();

        if (playerCam != null)
        {
            playerCam.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (controller == null) return;

        // Mouse Look
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

        // Movement & Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = 0f, moveZ = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;

            // Shift Key check for Sprinting (Left Shift ya Right Shift)
            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            {
                currentSpeed = sprintSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }

            // 'E' Key press karne par Hider ko catch karne ki koshish karega
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryCatchHider();
            }
        }

        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void TryCatchHider()
    {
        AICharacterController[] allAI = FindObjectsByType<AICharacterController>(FindObjectsSortMode.None);

        foreach (var ai in allAI)
        {
            if (ai.aiRole == AICharacterController.Role.Hider)
            {
                float distance = Vector3.Distance(transform.position, ai.transform.position);

                if (distance <= 4f)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.OnHiderCaught();
                    }

                    ai.gameObject.SetActive(false);
                    return;
                }
            }
        }
    }

    public void OnHearNoise(Vector3 noisePosition)
    {
        // Debug.Log("Blind Man heard noise at: " + noisePosition);
    }
}
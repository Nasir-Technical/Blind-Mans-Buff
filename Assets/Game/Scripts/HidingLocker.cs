/*using UnityEngine;

public class HidingLocker : MonoBehaviour
{
    [Header("Locker Settings")]
    public Transform hidePosition; // Locker ke andar ki position jahan player hide hoga
    public float interactionDistance = 3.0f;

    private bool isPlayerInside = false;
    private GameObject currentPlayer;
    private CharacterController playerController;

    void Update()
    {
        if (currentPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, currentPlayer.transform.position);

            // Debug distance to see in console if you are within range
            // Debug.Log("Distance to locker: " + distance);

            if (distance <= interactionDistance && Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("'E' key pressed within range! Toggling hide.");
                ToggleHide();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered by: " + other.gameObject.name + " with Tag: " + other.tag);

        // Check if it's the player or hider
        if (other.CompareTag("Player") || other.name.Contains("Hider") || other.name.Contains("PLAYER"))
        {
            currentPlayer = other.gameObject;
            playerController = currentPlayer.GetComponent<CharacterController>();
            Debug.Log("Player successfully detected by locker! Press 'E' to hide.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentPlayer != null && other.gameObject == currentPlayer)
        {
            Debug.Log("Player exited locker trigger area.");
            if (isPlayerInside)
            {
                ExitLocker();
            }
            currentPlayer = null;
            playerController = null;
        }
    }

    void ToggleHide()
    {
        if (!isPlayerInside)
        {
            EnterLocker();
        }
        else
        {
            ExitLocker();
        }
    }

    void EnterLocker()
    {
        isPlayerInside = true;

        if (playerController != null) playerController.enabled = false;

        if (hidePosition != null)
        {
            currentPlayer.transform.position = hidePosition.position;
            Debug.Log("Moved player to HidePosition: " + hidePosition.position);
        }
        else
        {
            currentPlayer.transform.position = transform.position + new Vector3(0, 0, 0.5f);
            Debug.Log("Moved player to default offset position.");
        }

        // Hide player renderers
        Renderer[] renderers = currentPlayer.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        currentPlayer.tag = "HiddenPlayer";
        Debug.Log(">>> SUCCESS: Hider entered the locker and is now hidden! <<<");
    }

    void ExitLocker()
    {
        isPlayerInside = false;

        Renderer[] renderers = currentPlayer.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        if (playerController != null) playerController.enabled = true;

        currentPlayer.tag = "Player";
        Debug.Log(">>> Hider exited the locker. <<<");
    }
}*/

using UnityEngine;
using UnityEngine.InputSystem;

public class HidingLocker : MonoBehaviour
{
    [Header("Locker Settings")]
    public Transform hidePosition;   // Locker ke andar ki position
    public Transform exitPosition;   // Locker ke bahar aane ki position
    public float interactionDistance = 3.0f;

    private bool isPlayerInside = false;
    private GameObject currentPlayer;
    private CharacterController playerController;

    void Update()
    {
        if (currentPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, currentPlayer.transform.position);

            if (distance <= interactionDistance && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("'E' pressed! Toggling hide/exit.");
                ToggleHide();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Hider") || other.name.Contains("PLAYER") || other.name.Contains("Player"))
        {
            currentPlayer = other.gameObject;
            playerController = currentPlayer.GetComponent<CharacterController>();
            Debug.Log("SUCCESS: Player assigned to locker! Press 'E' to hide.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Trigger se bahar jaane par agar andar hai toh bahar nikal do
        if (currentPlayer != null && (other.gameObject == currentPlayer || other.gameObject.transform.root == currentPlayer.transform.root))
        {
            if (isPlayerInside)
            {
                ExitLocker();
            }
            currentPlayer = null;
            playerController = null;
        }
    }

    void ToggleHide()
    {
        if (!isPlayerInside)
        {
            EnterLocker();
        }
        else
        {
            ExitLocker();
        }
    }

    void EnterLocker()
    {
        isPlayerInside = true;

        if (playerController != null) playerController.enabled = false;

        if (hidePosition != null)
        {
            currentPlayer.transform.position = hidePosition.position;
        }
        else
        {
            currentPlayer.transform.position = transform.position + new Vector3(0, 0, 0.5f);
        }

        // Hide renderers
        Renderer[] renderers = currentPlayer.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        currentPlayer.tag = "HiddenPlayer";
        Debug.Log(">>> HIDER HIDDEN IN LOCKER! <<<");
    }

    void ExitLocker()
    {
        isPlayerInside = false;

        // Pehle character controller disable rakhein teleport karte waqt
        if (playerController != null) playerController.enabled = false;

        // Agar Exit Position set hai toh wahan bhejain, warna locker ke samne thoda aage
        if (exitPosition != null)
        {
            currentPlayer.transform.position = exitPosition.position;
        }
        else
        {
            currentPlayer.transform.position = transform.position + transform.forward * -1.5f + new Vector3(0, 0.2f, 0);
        }

        // Restore renderers
        Renderer[] renderers = currentPlayer.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        // Movement enable kar dein
        if (playerController != null) playerController.enabled = true;

        currentPlayer.tag = "Player";
        Debug.Log(">>> HIDER EXITED LOCKER SUCCESSFULLY! <<<");
    }
}
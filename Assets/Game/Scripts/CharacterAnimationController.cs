/*using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private HiderController hiderController;
    private Vector3 lastPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        hiderController = GetComponent<HiderController>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (animator == null) return;

        float speed = 0f;

        // 1. AI (NavMeshAgent) ki speed
        if (agent != null)
        {
            speed = agent.velocity.magnitude;
        }
        // 2. Player (Hider) ki speed (position change se)
        else
        {
            Vector3 move = transform.position - lastPosition;
            speed = move.magnitude / Time.deltaTime;
            lastPosition = transform.position;
        }

        // Animator mein speed pass kar rahe hain
        animator.SetFloat("Speed", speed);

        // Sound Play/Stop Logic with Stealth Check
        if (audioSource != null && audioSource.clip != null)
        {
            // Check karein ke kya player stealth mode mein hai ya nahi
            bool isStealth = false;
            if (hiderController != null)
            {
                // Agar aapke HiderController mein koi stealth variable hai toh aap yahan check kar sakte hain. 
                // Filhal hum speed threshold se check kar rahe hain: agar speed stealth speed (2.5) ya us se kam hai toh sound nahi bajegi.
                if (speed > 0.1f && speed <= 3.0f) // Yeh threshold aap adjust kar sakte hain
                {
                    isStealth = true;
                }
            }

            // Sound tabhi bajegi jab player tezi se bhaag raha ho (stealth / slow na ho) aur speed > 3.0f ho
            if (speed > 3.0f && !isStealth && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            // Agar player ruk gaya hai ya stealth mode (dhire chal raha hai) mein hai toh sound band ho jaye
            else if ((speed <= 3.0f) && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}*/




using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementThreshold = 0.15f;
    [SerializeField] private float runThreshold = 2.8f;

    [Header("Animation Settings")]
    [SerializeField] private float animationDampTime = 0.1f;

    [Header("Audio Settings")]
    [SerializeField] private float audioMovementThreshold = 3.0f;
    [SerializeField] private bool stealthMode = false;

    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;

    private Vector3 lastPosition;

    private float currentSpeed;
    private bool isMoving;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int IsRunningHash =
        Animator.StringToHash("IsRunning");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        lastPosition = transform.position;

        if (animator != null)
            animator.applyRootMotion = false;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.Stop();
        }
    }

    private void Update()
    {
        if (animator == null)
            return;

        // Calculate actual movement speed using Delta Position.
        currentSpeed = GetActualMovementSpeed();

        // Ignore tiny movements and micro drifts.
        if (currentSpeed < movementThreshold)
        {
            currentSpeed = 0f;
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }

        UpdateAnimation();
        UpdateAudio();
    }

    private float GetActualMovementSpeed()
    {
        Vector3 currentPosition = transform.position;

        // Actual horizontal movement.
        Vector3 deltaPosition = currentPosition - lastPosition;

        deltaPosition.y = 0f;

        // Delta Position / Delta Time.
        float speed = deltaPosition.magnitude /
                      Mathf.Max(Time.deltaTime, 0.0001f);

        lastPosition = currentPosition;

        // Safety check for abnormal position changes.
        if (float.IsNaN(speed) || float.IsInfinity(speed))
            speed = 0f;

        return speed;
    }

    private void UpdateAnimation()
    {
        if (!isMoving)
        {
            // Proper Idle state.
            animator.SetFloat(SpeedHash, 0f);
            animator.SetBool(IsRunningHash, false);
            return;
        }

        animator.SetFloat(
            SpeedHash,
            currentSpeed,
            animationDampTime,
            Time.deltaTime
        );

        animator.SetBool(
            IsRunningHash,
            currentSpeed >= runThreshold
        );
    }

    private void UpdateAudio()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        bool shouldPlay =
            isMoving &&
            currentSpeed >= audioMovementThreshold &&
            !stealthMode;

        if (shouldPlay)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    public void SetStealthMode(bool enabled)
    {
        stealthMode = enabled;

        if (stealthMode && audioSource != null)
            audioSource.Stop();
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    private void OnDisable()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}

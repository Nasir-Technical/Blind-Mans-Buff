using UnityEngine;
using Unity.Netcode;

public class GameSettings : NetworkBehaviour
{
    public static GameSettings Instance { get; private set; }

    [Header("Networked Game Rules")]
    public NetworkVariable<float> gameDuration = new NetworkVariable<float>(120f); // Default 2 mins
    public NetworkVariable<float> glowDuration = new NetworkVariable<float>(5f);   // Default 5 seconds
    public NetworkVariable<float> glowIntensity = new NetworkVariable<float>(3f);  // Default 3x intensity

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Host settings update karne ke liye helper methods
    public void SetGameDuration(float seconds)
    {
        if (IsServer || IsHost) gameDuration.Value = seconds;
    }

    public void SetGlowDuration(float seconds)
    {
        if (IsServer || IsHost) glowDuration.Value = seconds;
    }

    public void SetGlowIntensity(float intensity)
    {
        if (IsServer || IsHost) glowIntensity.Value = intensity;
    }
}
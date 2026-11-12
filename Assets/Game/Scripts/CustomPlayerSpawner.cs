/*using UnityEngine;
using UnityEngine.AI;

public class CustomPlayerSpawner : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject playerBlindManPrefab;
    [SerializeField] private GameObject playerHiderPrefab;

    [Header("AI Prefabs")]
    [SerializeField] private GameObject aiBlindManPrefab;
    [SerializeField] private GameObject aiHiderPrefab;

    [Header("Manual Spawn Points (Drag multiple empty objects here)")]
    [SerializeField] private Transform[] spawnPoints;

    void Start()
    {
        int selectedRole = PlayerPrefs.GetInt("SelectedRole", 1);
        int aiCount = PlayerPrefs.GetInt("AICount", 10);
        if (aiCount < 2) aiCount = 2;

        Debug.Log(">>> SPAWNER STARTED! Target AICount: " + aiCount);

        // 1. Spawn User's Active Player strictly at Index 0
        Vector3 playerSpawnPos = (spawnPoints != null && spawnPoints.Length > 0 && spawnPoints[0] != null)
            ? spawnPoints[0].position
            : transform.position;
        SpawnPlayer(selectedRole, playerSpawnPos);

        // 2. Spawn AI Opponents starting strictly from Index 1 onwards so they never overlap with Player!
        int spawnedCount = 0;
        for (int i = 0; i < aiCount; i++)
        {
            Vector3 spawnPos = transform.position;

            if (spawnPoints != null && spawnPoints.Length > 1)
            {
                // Index 1 se shuru hoga taake player (Index 0) ke sath overlap na ho
                int pointIndex = 1 + (i % (spawnPoints.Length - 1));
                if (spawnPoints[pointIndex] != null)
                {
                    spawnPos = spawnPoints[pointIndex].position;
                }
            }
            else
            {
                spawnPos = GetSafeNavMeshPosition(i + 5); // Thoda door spread karke
            }

            SpawnAI(selectedRole, spawnPos, i);
            spawnedCount++;
        }

        Debug.Log(">>> TOTAL AI SUCCESSFULLY SPAWNED: " + spawnedCount);
    }

    Vector3 GetSafeNavMeshPosition(int seedOffset)
    {
        // Map ke andar ground level par safe point dhoondne ke liye
        Vector3 randomDirection = Random.insideUnitSphere * 35f;
        randomDirection.y = 0; // Height ko zero rakhenge taake rooftop par na jaye
        randomDirection += transform.position;

        NavMeshHit hit;
        // Sample height ko restrict karenge taake zameen par hi snap ho
        if (NavMesh.SamplePosition(randomDirection, out hit, 15.0f, NavMesh.AllAreas))
        {
            return hit.position + Vector3.up * 0.2f; // Zameen se thoda upar
        }

        // Fallback ground position
        return transform.position + new Vector3(seedOffset * 2f, 0.2f, 0f);
    }

    private void SpawnPlayer(int role, Vector3 pos)
    {
        GameObject prefab = (role == 1) ? playerBlindManPrefab : playerHiderPrefab;
        if (prefab == null) return;

        GameObject player = Instantiate(prefab, pos + Vector3.up * 0.5f, Quaternion.identity);
        player.name = "PLAYER_" + (role == 1 ? "BlindMan" : "Hider");

        Camera cam = player.GetComponentInChildren<Camera>(true);
        if (cam != null)
        {
            cam.enabled = true;
            cam.tag = "MainCamera";
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener == null) player.AddComponent<AudioListener>();
            else listener.enabled = true;
        }
    }

    private void SpawnAI(int playerRole, Vector3 pos, int aiIndex)
    {
        GameObject prefab = (playerRole == 1) ? aiHiderPrefab : aiBlindManPrefab;
        if (prefab == null) return;

        // NavMeshAgent ko temporarily disable karke instantiate karenge taake placement error na aaye
        GameObject ai = Instantiate(prefab, pos, Quaternion.identity);
        ai.name = "AI_" + aiIndex + "_" + (playerRole == 1 ? "Hider" : "BlindMan");

        NavMeshAgent agent = ai.GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        ai.transform.position = pos;

        if (agent != null) agent.enabled = true;

        Camera cam = ai.GetComponentInChildren<Camera>(true);
        if (cam != null) cam.enabled = false;

        AudioListener listener = ai.GetComponentInChildren<AudioListener>(true);
        if (listener != null) listener.enabled = false;

        Debug.Log("AI [" + aiIndex + "] spawned at: " + pos);
    }
}*/

using UnityEngine;
using UnityEngine.AI;

public class CustomPlayerSpawner : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject playerBlindManPrefab;
    [SerializeField] private GameObject playerHiderPrefab;

    [Header("AI Prefabs")]
    [SerializeField] private GameObject aiBlindManPrefab;
    [SerializeField] private GameObject aiHiderPrefab;

    [Header("Separate Spawn Points Arrays")]
    [SerializeField] private Transform[] blindManSpawnPoints;
    [SerializeField] private Transform[] hiderSpawnPoints;

    void Start()
    {
        int selectedRole = PlayerPrefs.GetInt("SelectedRole", 1); // 1 = Blind Man, 2 = Hider

        // Ab hum dono counts ko alag read karenge (Default: 2 Blind Men, 3 Hiders)
        int blindManCount = PlayerPrefs.GetInt("BlindManCount", 2);
        int hiderCount = PlayerPrefs.GetInt("HiderCount", 3);

        Debug.Log($">>> SPAWNER STARTED! BlindMan Target: {blindManCount}, Hider Target: {hiderCount}");

        // --- 1. SPAWN BLIND MEN TEAM ---
        // Agar user khud Blind Man hai, toh 1 player aur baqi AI Blind Men honge
        int spawnedBlindMen = 0;
        for (int i = 0; i < blindManCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(blindManSpawnPoints, i, new Vector3(0, 0, 0));

            if (selectedRole == 1 && i == 0)
            {
                // User Player (Blind Man)
                SpawnPlayer(1, spawnPos);
            }
            else
            {
                // AI Blind Man
                SpawnAI(1, spawnPos, spawnedBlindMen);
            }
            spawnedBlindMen++;
        }

        // --- 2. SPAWN HIDERS TEAM ---
        int spawnedHiders = 0;
        for (int i = 0; i < hiderCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(hiderSpawnPoints, i, new Vector3(5, 0, 5));

            if (selectedRole == 2 && i == 0)
            {
                // User Player (Hider)
                SpawnPlayer(2, spawnPos);
            }
            else
            {
                // AI Hider
                SpawnAI(2, spawnPos, spawnedHiders);
            }
            spawnedHiders++;
        }

        Debug.Log($">>> SPAWN COMPLETE! Total BlindMen: {spawnedBlindMen}, Total Hiders: {spawnedHiders}");
    }

    Vector3 GetSpawnPosition(Transform[] points, int index, Vector3 fallbackOffset)
    {
        if (points != null && points.Length > 0)
        {
            Transform point = points[index % points.Length];
            if (point != null) return point.position;
        }
        return transform.position + fallbackOffset + new Vector3(index * 2f, 0, 0);
    }

    private void SpawnPlayer(int role, Vector3 pos)
    {
        GameObject prefab = (role == 1) ? playerBlindManPrefab : playerHiderPrefab;
        if (prefab == null) return;

        GameObject player = Instantiate(prefab, pos + Vector3.up * 0.5f, Quaternion.identity);
        player.name = "PLAYER_" + (role == 1 ? "BlindMan" : "Hider");

        Camera cam = player.GetComponentInChildren<Camera>(true);
        if (cam != null)
        {
            cam.enabled = true;
            cam.tag = "MainCamera";
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener == null) player.AddComponent<AudioListener>();
            else listener.enabled = true;
        }
        Debug.Log("User Player Spawned as: " + player.name);
    }

    private void SpawnAI(int role, Vector3 pos, int aiIndex)
    {
        // role 1 = BlindMan AI, role 2 = Hider AI
        GameObject prefab = (role == 1) ? aiBlindManPrefab : aiHiderPrefab;
        if (prefab == null) return;

        GameObject ai = Instantiate(prefab, pos, Quaternion.identity);
        ai.name = "AI_" + aiIndex + "_" + (role == 1 ? "BlindMan" : "Hider");

        NavMeshAgent agent = ai.GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        ai.transform.position = pos;

        if (agent != null) agent.enabled = true;

        Camera cam = ai.GetComponentInChildren<Camera>(true);
        if (cam != null) cam.enabled = false;

        AudioListener listener = ai.GetComponentInChildren<AudioListener>(true);
        if (listener != null) listener.enabled = false;

        Debug.Log("AI spawned: " + ai.name);
    }
}
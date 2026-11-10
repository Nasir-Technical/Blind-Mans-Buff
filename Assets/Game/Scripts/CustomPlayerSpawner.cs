
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

    [Header("Manual Spawn Points (Scene ke Empty GameObjects yahan drag karein)")]
    [SerializeField] private Transform[] spawnPoints;

    void Start()
    {
        int selectedRole = PlayerPrefs.GetInt("SelectedRole", 1);
        int aiCount = PlayerPrefs.GetInt("AICount", 2);

        Debug.Log("--- SPAWNER STARTED ---");

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("CustomPlayerSpawner: Inspector mein Spawn Points add karna bhool gaye hain!");
            return;
        }

        // Points ko randomize karne ke liye index array banayenge taake har bar alag jagah spawn ho
        int[] indices = new int[spawnPoints.Length];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;

        // Shuffle logic
        for (int i = 0; i < indices.Length; i++)
        {
            int rnd = Random.Range(0, indices.Length);
            int temp = indices[i];
            indices[i] = indices[rnd];
            indices[rnd] = temp;
        }

        // 1. Spawn User's Active Player
        SpawnPlayer(selectedRole, spawnPoints[indices[0]].position);

        // 2. Spawn AI Opponents
        for (int i = 0; i < aiCount && (i + 1) < indices.Length; i++)
        {
            SpawnAI(selectedRole, spawnPoints[indices[i + 1]].position, i);
        }
    }

    private void SpawnPlayer(int role, Vector3 pos)
    {
        GameObject prefab = (role == 1) ? playerBlindManPrefab : playerHiderPrefab;
        if (prefab == null)
        {
            Debug.LogError("Player Prefab missing in Inspector!");
            return;
        }

        GameObject player = Instantiate(prefab, pos, Quaternion.identity);
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
        Debug.Log("PLAYER SPAWNED AT: " + pos);
    }

    private void SpawnAI(int playerRole, Vector3 pos, int aiIndex)
    {
        GameObject prefab = (playerRole == 1) ? aiHiderPrefab : aiBlindManPrefab;
        if (prefab == null)
        {
            Debug.LogError("AI Prefab missing in Inspector!");
            return;
        }

        GameObject ai = Instantiate(prefab, pos, Quaternion.identity);
        ai.name = "AI_" + aiIndex + "_" + (playerRole == 1 ? "Hider" : "BlindMan");

        Camera cam = ai.GetComponentInChildren<Camera>(true);
        if (cam != null) cam.enabled = false;

        AudioListener listener = ai.GetComponentInChildren<AudioListener>(true);
        if (listener != null) listener.enabled = false;

        Debug.Log("AI SPAWNED AT: " + pos);
    }
}

*/

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

    [Header("Manual Spawn Points (Drag multiple empty objects here)")]
    [SerializeField] private Transform[] spawnPoints;

    /*void Start()
    {
        int selectedRole = PlayerPrefs.GetInt("SelectedRole", 1);
        // int aiCount = PlayerPrefs.GetInt("AICount", 2);
        int aiCount = 10;

        Debug.Log(">>> SPAWNER STARTED! Target AICount: " + aiCount);

        // 1. Spawn User's Active Player
        Vector3 playerSpawnPos = (spawnPoints != null && spawnPoints.Length > 0 && spawnPoints[0] != null)
            ? spawnPoints[0].position
            : transform.position;
        SpawnPlayer(selectedRole, playerSpawnPos);

        // 2. Spawn AI Opponents safely
        int spawnedCount = 0;
        for (int i = 0; i < aiCount; i++)
        {
            Vector3 spawnPos = Vector3.zero;

            // Agar manual spawn points available hain toh alag-alag point use karo taake overlap na ho
            if (spawnPoints != null && (i + 1) < spawnPoints.Length && spawnPoints[i + 1] != null)
            {
                spawnPos = spawnPoints[i + 1].position;
            }
            else
            {
                // Warna map par safe random NavMesh position lo (with height offset)
                spawnPos = GetSafeNavMeshPosition(i);
            }

            SpawnAI(selectedRole, spawnPos, i);
            spawnedCount++;
        }

        Debug.Log(">>> TOTAL AI SUCCESSFULLY SPAWNED: " + spawnedCount);
    }*/

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
}
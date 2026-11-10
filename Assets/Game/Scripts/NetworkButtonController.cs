using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkButtonController : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        // Host banne ke baad automatically GameScene load karne ke liye Netcode ka SceneManager use karein
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        Debug.Log("Host Started & Loading GameScene!");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client Joined!");
    }
}
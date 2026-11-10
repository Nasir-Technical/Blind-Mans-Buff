/*using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private int chosenRole = 1; // Default 1 (Blind Man)
    private int selectedAICount = 2;

    [Header("UI References (Optional)")]
    public TextMeshProUGUI aiCountText; // Agar text show karna ho

    // Yeh function Main Menu ke Buttons call karenge (Blind Men button par 1 dein, Hider button par 2)
    public void SetRole(int role)
    {
        chosenRole = role;
        PlayerPrefs.SetInt("SelectedRole", chosenRole);
        PlayerPrefs.Save();
        Debug.Log("Button Clicked! Role Saved to PlayerPrefs: " + chosenRole);
    }

    public void SetAICount(float count)
    {
        selectedAICount = Mathf.RoundToInt(count);
        PlayerPrefs.SetInt("AICount", selectedAICount);
        PlayerPrefs.Save();

        if (aiCountText != null)
        {
            aiCountText.text = "AI Count: " + selectedAICount;
        }

        Debug.Log("Selected AI Count Saved: " + selectedAICount);
    }

    public void PlayGame()
    {
        // Ensure current chosenRole is saved before loading
        PlayerPrefs.SetInt("SelectedRole", chosenRole);
        PlayerPrefs.Save();

        Debug.Log("PlayGame Clicked. Loading GameScene with Role: " + chosenRole + " and AI Count: " + selectedAICount);
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Debug.Log("Game is quitting...");
        Application.Quit();
    }
}*/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private int chosenRole = 1; // Default 1 (Blind Man)
    private int selectedAICount = 2;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;       // Pehla wala panel (Play, Option, Quit wala)
    public GameObject characterSelectPanel; // Character selection wala panel

    [Header("UI References")]
    public TextMeshProUGUI aiCountText;

    private void Start()
    {
        // Game shuru hote hi Main Menu on rakho aur Selection Panel band rakho
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
    }

    // 1. Jab user Main Menu ka "PLAY" button dabaye
    public void OnPlayButtonClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(true);
    }

    // 2. Agar wapas Main Menu ana ho (Back button ke liye)
    public void OnBackButtonClicked()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // 3. Role set karne ke liye (Blind Man = 1, Hider = 2) aur foran game play karne ke liye
    public void SelectRoleAndPlay(int role)
    {
        chosenRole = role;
        PlayerPrefs.SetInt("SelectedRole", chosenRole);
        PlayerPrefs.Save();

        Debug.Log("Role Selected: " + chosenRole + ". Loading GameScene...");
        SceneManager.LoadScene("GameScene");
    }

    public void SetAICount(float count)
    {
        selectedAICount = Mathf.RoundToInt(count);
        PlayerPrefs.SetInt("AICount", selectedAICount);
        PlayerPrefs.Save();

        if (aiCountText != null)
        {
            aiCountText.text = "AI Count: " + selectedAICount;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Game is quitting...");
        Application.Quit();
    }
}
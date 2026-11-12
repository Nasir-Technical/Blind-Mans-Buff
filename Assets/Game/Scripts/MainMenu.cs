/*using UnityEngine;
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
}*/


using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private int chosenRole = 1; // Default 1 (Blind Man)
    private int selectedBlindManCount = 2;
    private int selectedHiderCount = 3;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject characterSelectPanel;

    [Header("UI References")]
    public TextMeshProUGUI blindManCountText;
    public TextMeshProUGUI hiderCountText;

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
    }

    public void OnPlayButtonClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void SelectRoleAndPlay(int role)
    {
        chosenRole = role;
        PlayerPrefs.SetInt("SelectedRole", chosenRole);
        PlayerPrefs.Save();

        Debug.Log("Role Selected: " + chosenRole + ". Loading GameScene...");
        SceneManager.LoadScene("GameScene");
    }

    // Blind Man Slider ke liye function
    public void SetBlindManCount(float count)
    {
        selectedBlindManCount = Mathf.RoundToInt(count);
        PlayerPrefs.SetInt("BlindManCount", selectedBlindManCount);
        PlayerPrefs.Save();

        if (blindManCountText != null)
        {
            blindManCountText.text = "Blind Men: " + selectedBlindManCount;
        }
    }

    // Hider Slider ke liye function
    public void SetHiderCount(float count)
    {
        selectedHiderCount = Mathf.RoundToInt(count);
        PlayerPrefs.SetInt("HiderCount", selectedHiderCount);
        PlayerPrefs.Save();

        if (hiderCountText != null)
        {
            hiderCountText.text = "Hiders: " + selectedHiderCount;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Game is quitting...");
        Application.Quit();
    }
}
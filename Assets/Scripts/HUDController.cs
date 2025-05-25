using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Fragment Collection")]
    public float fragmentsCollected = 0;
    [SerializeField] private FragmentLogic[] allFragments;

    [Header("Script References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private InputManager inputManager;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI fragmentsCollectedText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI totalCollectionText;
    [SerializeField] private TextMeshProUGUI rankingText;

    [Header("Panel References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject hudPanel;

    [Header("Other References")]
    [SerializeField] private Slider slider;

    public float collectionPercentage;
    private string ranking;
    [SerializeField] private bool gamePaused = false;

    private void Awake()
    {
        Application.targetFrameRate = 240;
    }

    private void Start()
    {
        allFragments = GameObject.FindObjectsByType<FragmentLogic>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        playerHealthText.text = "Player Health: " + playerHealth.health + "/100";
        fragmentsCollectedText.text = "Fragments Collected: " + fragmentsCollected + "/" + allFragments.Length;

        if (inputManager.cancel && playerHealth.playerAlive && !gamePaused)
            PauseGame();
    }

    public void PauseGame()
    {
        if (!pausePanel.activeSelf)
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
            hudPanel.SetActive(false);
            Cursor.visible = true;
            gamePaused = true;
        }
    }

    public void ButtonResumeGame()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        hudPanel.SetActive(true);
        Cursor.visible = false;
        gamePaused = false;
    }

    public void RestartGame(string levelToLoad)
    {
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        deathPanel.SetActive(false);
        loadingPanel.SetActive(true);
        Time.timeScale = 1;
        StartCoroutine(LoadLevelASync(levelToLoad));
    }

    public void QuitGame(string levelToLoad)
    {
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        deathPanel.SetActive(false);
        loadingPanel.SetActive(true);
        Time.timeScale = 1;
        StartCoroutine(LoadLevelASync(levelToLoad));
    }

    public void CalculateFragmentsCollected()
    {
        collectionPercentage = fragmentsCollected / allFragments.Length;

        if (collectionPercentage >= 0.95)
            ranking = "S+";
        else if (collectionPercentage >= 0.85 && collectionPercentage < 0.95)
            ranking = "S";
        else if (collectionPercentage >= 0.75 && collectionPercentage < 0.85)
            ranking = "A";
        else if (collectionPercentage >= 0.60 && collectionPercentage < 0.75)
            ranking = "B";
        else if (collectionPercentage >= 0.40 && collectionPercentage < 0.60)
            ranking = "C";
        else if (collectionPercentage >= 0.20 && collectionPercentage < 0.40)
            ranking = "D";
        else
            ranking = "F";

        totalCollectionText.text = "Fragments Collected: " + fragmentsCollected + "/" + allFragments.Length;
        rankingText.text = "Ranking: " + ranking;

        hudPanel.SetActive(false);
        winPanel.SetActive(true);
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    IEnumerator LoadLevelASync(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            slider.value = progressValue;
            yield return null;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuMainController : MonoBehaviour
{
    [Header("Loading Screens")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadingScreen;
    [Header("Slider")]
    [SerializeField] private Slider slider;

    private void Start()
    {
        Cursor.visible = true;
        Application.targetFrameRate = 240;
    }

    public void EnterLevel1(string levelToLoad)
    {
        mainMenuPanel.SetActive(false);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadLevelASync(levelToLoad));
    }

    public void EnterLevel2(string levelToLoad)
    {
        mainMenuPanel.SetActive(false);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadLevelASync(levelToLoad));
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

    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _creditsPanel;

    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }
    void Start()
    {
        TurnOFFEverything();
        if (_menuPanel) _menuPanel.SetActive(true);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void TurnOFFEverything()
    {
        if (_menuPanel) _menuPanel.SetActive(false);
        if (_creditsPanel) _creditsPanel.SetActive(false);
    }

    public void CreditsPanel()
    {
        TurnOFFEverything();
        if (_creditsPanel) _creditsPanel.SetActive(true);

    }
    public void MainMenuPanel()
    {
        TurnOFFEverything();
        if (_menuPanel) _menuPanel.SetActive(true);

    }


}

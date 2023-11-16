using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private TMP_Text sizeText;
    private GameObject currentMenu;

    private void Awake()
    {
        currentMenu = mainMenu;
        SetMazeSize(9.0f);
    }

    public void SetMazeSize(float value)
    {
        PlayerPrefs.SetInt("MazeSize", Mathf.FloorToInt(value) * 3);
        sizeText.text = "Size : " + value * 3;
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        if (Application.isEditor)
        {
            
        }
        else Application.Quit();
    }

    public void MainMenu()
    {
        OpenMenu(mainMenu);
    }
    
    public void OptionsMenu()
    {
        OpenMenu(optionsMenu);
    }

    private void OpenMenu(GameObject menu)
    {
        currentMenu.SetActive(false);
        menu.SetActive(!menu.activeSelf);
        currentMenu = menu;
    }
}

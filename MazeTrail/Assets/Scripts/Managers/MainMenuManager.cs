using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
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
    }

    public void SetMazeSize(float value)
    {
        PlayerPrefs.SetInt("MazeSize", Mathf.FloorToInt(value));
        sizeText.text = "Size : " + value;
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

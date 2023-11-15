using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject endMenu;
    [SerializeField] private TMP_Text endText;

    public void PopEndMenu(string endText)
    {
        endMenu.SetActive(true);
        SetEndText(endText);
    }
    
    private void SetEndText(string text)
    {
        endText.text = text;
    }
}

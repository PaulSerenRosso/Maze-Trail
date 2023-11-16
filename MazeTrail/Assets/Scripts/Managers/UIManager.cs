using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject endMenu;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private TMP_Text timerText;
    
    public void SetTimerText(string text)
    {
        timerText.text = text;
    }
    
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

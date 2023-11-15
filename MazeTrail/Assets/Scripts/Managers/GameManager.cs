using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField] private UIManager uiManager;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (!instance) instance = this;
    }

    public static void EndGame(bool win)
    {
        instance.uiManager.PopEndMenu(win ? "You won !" : "You Lost..");
    }
    
    public static void ReturnToMenuFromGame()
    {
        
    }

    public static void Restart(bool newMap)
    {
        
    }
}

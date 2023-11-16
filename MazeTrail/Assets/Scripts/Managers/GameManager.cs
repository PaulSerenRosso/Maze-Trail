using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject player;
    [SerializeField] private Camera camera;
    
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
        if (!win) DestroyPlayer();
        else instance.camera.GetComponent<CameraController>().UnlinkTarget();
        instance.uiManager.PopEndMenu(win ? "You won !" : "You Lost..");
    }
    
    public static void ReturnToMenuFromGame()
    {
        SceneManager.LoadScene(0); //assume menu is scene 0
    }

    public static void Restart(bool newMap)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void DestroyPlayer()
    {
        instance.camera.GetComponent<CameraController>().UnlinkTarget();
        Destroy(instance.player);
    }
}

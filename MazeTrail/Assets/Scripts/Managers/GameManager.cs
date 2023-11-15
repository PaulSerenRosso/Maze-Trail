using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Camera camera;
    [SerializeField] private MazeManager mazeManager;

    private void Start()
    {
        var size = PlayerPrefs.GetInt("MazeSize", 10);
        mazeManager.GenerateMaze(size);
    }

    public void EndGame(bool win)
    {
        if (!win) DestroyPlayer();
        else camera.GetComponent<CameraController>().UnlinkTarget();
        uiManager.PopEndMenu(win ? "You won !" : "You Lost..");
    }
    
    public void ReturnToMenuFromGame()
    {
        SceneManager.LoadScene(0); //assume menu is scene 0
    }

    public void Restart(bool newMap)
    {
        
    }

    public void DestroyPlayer()
    {
        camera.GetComponent<CameraController>().UnlinkTarget();
        Destroy(mazeManager.Player.gameObject);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Camera camera;
    [SerializeField] private MazeManager mazeManager;
    [SerializeField] private float timer;

    private void Start()
    {
        var size = PlayerPrefs.GetInt("MazeSize", 10);
        mazeManager.GenerateMaze(size);
    }

    private void Update()
    {
        if (timer == 0) return;
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EndGame(false);
                timer = 0;
            }
        }

        var time = timer % 60;
        if (time < 10)
        {
            uiManager.SetTimerText(Mathf.FloorToInt(timer / 60) + ":" + "0" + Mathf.FloorToInt(time));
        }
        else
        {
            uiManager.SetTimerText(Mathf.FloorToInt(timer / 60) + ":" + Mathf.FloorToInt(time));
        }
    }

    public void SetTimer(float time)
    {
        timer = time;
    }

    public void EndGame(bool win)
    {
        if (!win) DestroyPlayer();
        else camera.GetComponent<CameraController>().UnlinkTarget();
        uiManager.PopEndMenu(win ? "You won !" : "You Lost..");
    }

    public void ReturnToMenuFromGame()
    {
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void DestroyPlayer()
    {
        camera.GetComponent<CameraController>().UnlinkTarget();
        Destroy(mazeManager.Player.gameObject);
    }
}
using UnityEngine;

public class Exit : MonoBehaviour
{
    private GameManager gameManager;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Win");
        gameManager.EndGame(true);
    }
}
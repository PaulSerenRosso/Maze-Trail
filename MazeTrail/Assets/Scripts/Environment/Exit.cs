using UnityEngine;

public class Exit : MonoBehaviour
{
    private GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Win");
        gameManager.EndGame(true);
    }
}
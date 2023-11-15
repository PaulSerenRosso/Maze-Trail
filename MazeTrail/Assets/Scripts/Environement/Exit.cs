using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Win");
        GameManager.EndGame(true);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accelerator : MonoBehaviour
{
    [SerializeField] private float accelerationValue; 
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<CharacterController>().Accelerate(accelerationValue);
    }
}

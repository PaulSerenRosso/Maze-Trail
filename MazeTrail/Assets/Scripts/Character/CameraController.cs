using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float offset;

    void Update()
    {
        float lerpT = 1 - Mathf.Exp(-5f * Time.deltaTime);
        Vector3 offsetVector = -target.forward * offset;
        offsetVector.y = -1.5f;
        transform.position = Vector3.Lerp(transform.position, (target.position - offsetVector), lerpT);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, lerpT);
    }
}

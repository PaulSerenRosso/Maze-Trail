using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float breakForce;
    
    private Rigidbody rb;
    private Vector3 direction = Vector3.forward;
    private Vector3 orientation = Vector3.forward;

    private bool accelerated = false;
    
    private PlayerInput inputSystem;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputSystem = new PlayerInput();
    }

    private void OnEnable()
    {
        inputSystem.Player.Forward.Enable();
        inputSystem.Player.Backward.Enable();
    }

    void Update()
    {
        GetInput();
        MovePlayer();
    }

    private void GetInput()
    {
        if (inputSystem.Player.Forward.IsPressed() && !accelerated)
        {
            rb.AddForce(acceleration * direction, ForceMode.Acceleration);
        }
        if (inputSystem.Player.Backward.IsPressed())
        {
            rb.AddForce(-breakForce * direction, ForceMode.Acceleration);
        }
        
    }

    private void MovePlayer()
    {
        //If speed is opposite to forward, set it to 0
        if(rb.velocity.normalized == -transform.forward)
            rb.velocity = Vector3.zero;
        
        //If wagon is accelerated : 
        //If speed went below maximum base speed, remove accelerated state
        //Else, make wagon decelerate each frame
        //If not accelerated, cap speed at topSpeed
        if (accelerated)
        {
            if(rb.velocity.magnitude < maxSpeed)
                accelerated = false;
            else
                rb.AddForce(-.05f * transform.forward, ForceMode.Acceleration);
        }
        else if (rb.velocity.magnitude > maxSpeed) 
            rb.velocity = rb.velocity.normalized * maxSpeed;
            
    }

    private void GetNextDirection(Intersection intersection)
    {
        switch (intersection.currentDirection)
        {
            case Intersection.Direction.Forward:
                direction = Vector3.forward;
                transform.forward = Vector3.forward;
                break;
            case Intersection.Direction.Backward:
                direction = Vector3.back;
                transform.forward = Vector3.back;
                break;
            case Intersection.Direction.Left:
                direction = Vector3.left;
                transform.forward = Vector3.left;
                break;
            case Intersection.Direction.Right:
                direction = Vector3.right;
                transform.forward = Vector3.right;
                break;
        }

        rb.velocity = rb.velocity.magnitude * direction;
    }

    public void Accelerate(float value)
    {
        rb.velocity += transform.forward * value;
        accelerated = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var intersection = other.GetComponent<Intersection>(); 
        if (intersection)
        {
            GetNextDirection(intersection);
            transform.position = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
        }
    }
}

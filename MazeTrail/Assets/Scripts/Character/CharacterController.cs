using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float breakForce;

    private Rigidbody rb;
    private Vector3 direction = Vector3.forward;
    private Vector3 orientation = Vector3.forward;

    private Direction nextDirection = Direction.Top;
    
    private bool accelerated = false;
    private bool lookBackwards = false;
    
    private PlayerInput inputSystem;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputSystem = new PlayerInput();
    }

    private void OnEnable()
    {
        inputSystem.Player.Accelerate.Enable();
        inputSystem.Player.Decelerate.Enable();
        inputSystem.Player.Forward.Enable();
        inputSystem.Player.Backward.Enable();
        inputSystem.Player.TurnLeft.Enable();
        inputSystem.Player.TurnRight.Enable();
        inputSystem.Player.TurnAround.Enable();
    }

    void Update()
    {
        GetInput();
        MovePlayer();
    }

    private void GetInput()
    {
        //Acceleration & deceleration
        if (inputSystem.Player.Accelerate.IsPressed() && !accelerated)
        {
            rb.AddForce(acceleration * (lookBackwards ? -1 : 1) * direction, ForceMode.Acceleration);
        }

        if (inputSystem.Player.Decelerate.IsPressed())
        {
            rb.AddForce(-breakForce * (lookBackwards ? -1 : 1) * direction, ForceMode.Acceleration);
        }
        
        //Direction switching
        var relativeDirection = DirectionLogic.GetRelativeDirection(direction);
        
        if (inputSystem.Player.Forward.triggered)
        {
            nextDirection = DirectionLogic.RelativeToAbsoluteDirection(relativeDirection, Direction.Top);
        }
        else if (inputSystem.Player.Backward.triggered)
        {
            nextDirection = DirectionLogic.RelativeToAbsoluteDirection(relativeDirection, Direction.Bottom);
        }
        else if (inputSystem.Player.TurnLeft.triggered)
        {
            nextDirection = DirectionLogic.RelativeToAbsoluteDirection(relativeDirection, Direction.Left);
        } 
        else if (inputSystem.Player.TurnRight.triggered)
        {
            nextDirection = DirectionLogic.RelativeToAbsoluteDirection(relativeDirection, Direction.Right);
        }
        
        //Look backwards ? 
        if(inputSystem.Player.TurnAround.triggered)
        lookBackwards = !lookBackwards;
    }
    
    private void MovePlayer()
    {
        //With input
        
        //If speed is opposite to forward, set it to 0
        if (rb.velocity.normalized == (lookBackwards ? 1 : -1) * transform.forward)
            rb.velocity = Vector3.zero;

        //If wagon is accelerated : 
        //If speed went below maximum base speed, remove accelerated state
        //Else, make wagon decelerate each frame
        //If not accelerated, cap speed at topSpeed
        if (accelerated)
        {
            if (rb.velocity.magnitude < maxSpeed)
                accelerated = false;
            else
                rb.AddForce(-.05f * transform.forward, ForceMode.Acceleration);
        }
        else if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
        
        
        //Without input
        /*if (accelerated)
        {
            if (rb.velocity.magnitude < maxSpeed)
                accelerated = false;
            else
                rb.AddForce(-.05f * transform.forward, ForceMode.Acceleration);
        }
        else
        {
            rb.velocity = transform.forward * maxSpeed;
        }*/
    }

    private void GetNextDirection(Direction nextDirection)
    {
        switch (nextDirection)
        {
            case Direction.Top:
                direction = Vector3.forward;
                transform.forward = Vector3.forward;
                break;
            case Direction.Bottom:
                direction = Vector3.back;
                transform.forward = Vector3.back;
                break;
            case Direction.Left:
                direction = Vector3.left;
                transform.forward = Vector3.left;
                break;
            case Direction.Right:
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

    public bool IsLookingBackwards()
    {
        return lookBackwards;
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var intersection = other.GetComponent<Intersection>();
        if (intersection)
        {
            if (intersection.MatchDirection(nextDirection) && nextDirection != DirectionLogic.GetOpposite(DirectionLogic.GetRelativeDirection(direction)))
            {
                Debug.Log($"Next direction : {nextDirection}");
                GetNextDirection(nextDirection);
            }
            else
            {
                Debug.LogError("Wagon crashed !");
                GameManager.EndGame(false);
            }
            
            transform.position = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
        }
    }
}
using System;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleratorMaxFactor = 1.0f;
    [SerializeField] private float breakForce;
    
    private float maxSpeedAccelerated;
    
    [SerializeField] private Transform directionIndicator;
    
    private Rigidbody rb;
    private Vector3 direction = Vector3.forward;

    private Direction nextDirection = Direction.Top;
    
    private bool accelerated;
    private bool lookBackwards;
    private bool lockControls;
   
    private Intersection nextIntersection;
    
    private PlayerInput inputSystem;
    private GameManager gameManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputSystem = new PlayerInput();
        maxSpeedAccelerated = maxSpeed * acceleratorMaxFactor;
    }

    private void OnEnable()
    {
        inputSystem.Player.Enable();
    }

    public void Init(Vector3 startDirection, GameManager manager)
    {
        direction = startDirection;
        nextDirection = DirectionLogic.GetRelativeDirection(startDirection);
        gameManager = manager;
    }

    void Update()
    {
        GetInput();
        HandleIntersection();
        MovePlayer();
    }

    private void GetInput()
    {
        if (!lockControls)
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
        }

        //Direction switching
        var relativeDirection = DirectionLogic.GetRelativeDirection(direction);

        if (inputSystem.Player.Forward.triggered)
        {
            nextDirection = DirectionLogic.RelativeToAbsoluteDirection(relativeDirection, Direction.Top);
            RotateIndicator(Direction.Top);
        }
        else if (inputSystem.Player.TurnLeft.triggered)
        {
            nextDirection = DirectionLogic.RelativeToAbsoluteDirection(relativeDirection, Direction.Left);
            RotateIndicator(Direction.Left);
        } 
        else if (inputSystem.Player.TurnRight.triggered)
        {
            nextDirection = DirectionLogic.RelativeToAbsoluteDirection(relativeDirection, Direction.Right);
            RotateIndicator(Direction.Right);
        }
        
        //Look backwards
        if (inputSystem.Player.TurnAround.triggered)
        {
            direction = -direction;
            //lookBackwards = !lookBackwards;
        }
    }

    private void MovePlayer()
    {
        if (transform.forward != direction) transform.forward = direction;
        
        //If speed is opposite to forward, set it to 0
        if (rb.velocity.normalized == (lookBackwards ? 1 : -1) * direction)
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
                rb.AddForce(-.05f * direction, ForceMode.Acceleration); //Natural friction to slow down the wagon
        }

        var contextMaxSpeed = (accelerated ? maxSpeedAccelerated : maxSpeed);
        
        if (rb.velocity.magnitude > contextMaxSpeed)
            rb.velocity = rb.velocity.normalized * contextMaxSpeed;
    }

    private void HandleIntersection()
    {
        if (!nextIntersection) return;
        
        var intersectionPos = nextIntersection.transform.position;
        
        if((intersectionPos - transform.position).magnitude > .1f) return;
        
        if (nextIntersection.MatchDirection(nextDirection) && nextDirection !=
            DirectionLogic.GetOpposite(DirectionLogic.GetRelativeDirection(direction)))
        {
            GetNextDirection(nextDirection);
            nextIntersection = null;
        }
        else 
        {
            gameManager.EndGame(false);
        }
            
        RotateIndicator(Direction.Top);
        
        transform.position = new Vector3(intersectionPos.x, transform.position.y, intersectionPos.z);
      
        
        
        
        rb.velocity = rb.velocity.magnitude * direction;
    }
    
    private void GetNextDirection(Direction nextDirection)
    {
        switch (nextDirection)
        {
            case Direction.Top:
                direction = Vector3.forward;
                break;
            case Direction.Bottom:
                direction = Vector3.back;
                break;
            case Direction.Left:
                direction = Vector3.left;
                break;
            case Direction.Right:
                direction = Vector3.right;
                break;
            default:
                break;
        }
    }

    private void RotateIndicator(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                directionIndicator.rotation = Quaternion.Euler(new Vector3(-45.0f, directionIndicator.eulerAngles.y, directionIndicator.eulerAngles.z));
                break;
            case Direction.Right:
                directionIndicator.rotation = Quaternion.Euler(new Vector3(45.0f, directionIndicator.eulerAngles.y, directionIndicator.eulerAngles.z));
                break;
            default:
                directionIndicator.rotation = Quaternion.Euler(new Vector3(0.0f, directionIndicator.eulerAngles.y, directionIndicator.eulerAngles.z));
                break;
        }
    }

    public void Accelerate(float value)
    {
        rb.velocity += direction * value;
        accelerated = true;
    }

    public bool IsLookingBackwards()
    {
        return lookBackwards;
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        var intersection = other.GetComponent<Intersection>();
        if (intersection && !nextIntersection)
            nextIntersection = intersection;
    }

    private void OnTriggerExit(Collider other)
    {
        if(nextIntersection)
            nextIntersection = null;
    }
}
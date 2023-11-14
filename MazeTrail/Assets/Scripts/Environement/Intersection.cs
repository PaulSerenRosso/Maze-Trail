using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    public Direction currentDirection = Direction.None;

    public void SwapDirection()
    {
        switch (currentDirection)
        {
            case Direction.Forward:
                currentDirection = Direction.Backward;
                break;
            case Direction.Backward:
                currentDirection = Direction.Forward;
                break;
            case Direction.Left:
                currentDirection = Direction.Right;
                break;
            case Direction.Right:
                currentDirection = Direction.Left;
                break;
            default:
                currentDirection = Direction.None;
                break;
        }
    }
    
    public enum Direction
    {
        None,
        Forward,
        Backward,
        Left,
        Right
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionLogic
{
 public static Direction GetRelativeDirection(Vector3 direction)
    {
        var relativeDirectionX = direction.x;
        var relativeDirectionZ = direction.z;
        var relativeDirection = Direction.None;
        
        //Going on horizontal axis
        if (relativeDirectionX != 0)
        {
            relativeDirection = relativeDirectionX < 0 ? Direction.Left : Direction.Right;
        }
        //Going on vertical axis
        else if(relativeDirectionZ != 0)
        {
            relativeDirection = relativeDirectionZ < 0 ? Direction.Bottom : Direction.Top;
        }

        return relativeDirection;
    }
    
    public static Direction RelativeToAbsoluteDirection(Direction relativeDirection, Direction inputDirection)
    {
        switch (relativeDirection)
        {
            case Direction.Top:
                return inputDirection;
            case Direction.Bottom:
                return GetOpposite(inputDirection);
            case Direction.Left:
                return GetDirectionRotated(inputDirection, false);
            case Direction.Right:
                return GetDirectionRotated(inputDirection, true);
            default:
                break;
        }

        return Direction.None;
    }
    
    public static Direction GetOpposite(Direction direction)
    {
        switch (direction)
        {
            case Direction.Top:
                return Direction.Bottom;
            case Direction.Bottom:
                return Direction.Top;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                break;
        }
        return Direction.None;
    }

    //If clockwise == true, rotate right, else rotate left
    public static Direction GetDirectionRotated(Direction direction, bool clockwise)
    {
        switch (direction)
        {
            case Direction.Top:
                return (clockwise ? Direction.Right : Direction.Left);
            case Direction.Bottom:
                return (clockwise ? Direction.Left : Direction.Right);
            case Direction.Left:
                return (clockwise ? Direction.Top : Direction.Bottom);
            case Direction.Right:
                return (clockwise ? Direction.Bottom : Direction.Top);
            default:
                break;
            }
        return Direction.None;
    }
    
}

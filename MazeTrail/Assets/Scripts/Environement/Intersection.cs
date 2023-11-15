using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    public List<Direction> availableDirection = new List<Direction>();

    public bool MatchDirection(Direction playerDirection)
    {
        return availableDirection.Contains(playerDirection);  
    }
    
    
}

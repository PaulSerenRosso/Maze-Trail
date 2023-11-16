using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Intersection : MonoBehaviour
{
    [FormerlySerializedAs("availableDirection")] public List<Direction> availableDirections = new List<Direction>();

    public bool MatchDirection(Direction playerDirection)
    {
        return availableDirections.Contains(playerDirection);  
    }
    
    
}

using System.Collections.Generic;
using UnityEngine;

public class WallCell : MonoBehaviour
{
    public List<WallCell> neighbours = new();
    public bool isClosingWall;
}
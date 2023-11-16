using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CellMaze : MonoBehaviour
{
    public List<Neighbour> DynamicNeighbours = new();
    public List<Neighbour> StaticNeighbours = new();
    public GameObject floor;
    public MeshRenderer floorMR;
    public MeshFilter floorMF;
    
    public SerializableDictionary<Direction, WallCell> walls = new();

    public RailShape GetRailShape()
    {
        var count = 0;
        foreach (var wall in walls)
        {
            if (wall.Value)
            {
                count++;
            }
        }

        switch (count)
        {
            case 0:
                return RailShape.ShapeX;
            case 1:
                return RailShape.ShapeT;
            case 2:
                if (walls[Direction.Top] && walls[Direction.Bottom] || walls[Direction.Left] && walls[Direction.Right])
                    return RailShape.ShapeI;
                return RailShape.ShapeL;
            case 3:
                return RailShape.ShapeU;
            default:
                throw new Exception();
        }
    }
    
    public void AddNeighbour(Direction direction, CellMaze cell)
    {
        DynamicNeighbours.Add(new Neighbour(direction, cell));
        StaticNeighbours.Add(new Neighbour(direction, cell));
    }

    public void RemoveDynamicNeighbour(CellMaze cell)
    {
        for (int i = DynamicNeighbours.Count - 1; i >= 0; i--)
        {
            if (DynamicNeighbours[i].cell != cell) continue;
            DynamicNeighbours.RemoveAt(i);
        }
    }

    public CellMaze GetNeighbourDynamic(Direction direction)
    {
        foreach (var neighbour in DynamicNeighbours)
        {
            if (neighbour.direction != direction) continue;
            return neighbour.cell;
        }

        throw new Exception();
    }
    
    public CellMaze GetNeighbourStatic(Direction direction)
    {
        foreach (var neighbour in StaticNeighbours)
        {
            if (neighbour.direction != direction) continue;
            return neighbour.cell;
        }

        throw new Exception();
    }
}
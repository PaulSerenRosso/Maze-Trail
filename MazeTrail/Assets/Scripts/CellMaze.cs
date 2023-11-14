using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CellMaze : MonoBehaviour
{
    public List<Neighbour> DynamicNeighbours = new();
    public List<Neighbour> StaticNeighbours = new();
    public MeshRenderer floorMR;

    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject bottomWall;

    public void AddWall(GameObject wall, Direction direction)
    {
        switch (direction)
        {
            case Direction.Top:
                topWall = wall;
                break;
            case Direction.Right:
                rightWall = wall;
                break;
            case Direction.Bottom:
                bottomWall = wall;
                break;
            case Direction.Left:
                leftWall = wall;
                break;
        }
    }

    public GameObject GetWall(Direction direction)
    {
        switch (direction)
        {
            case Direction.Top:
                return topWall;
            case Direction.Right:
                return rightWall;
            case Direction.Bottom:
                return bottomWall;
            case Direction.Left:
                return rightWall;
            default:
                throw new Exception();
        }
    }

    public void RemoveWall(Direction direction)
    {
        switch (direction)
        {
            case Direction.Top:
                topWall = null;
                break;
            case Direction.Right:
                rightWall = null;
                break;
            case Direction.Bottom:
                bottomWall = null;
                break;
            case Direction.Left:
                leftWall = null;
                break;
        }
    }

    public void DestroyWall(Direction direction)
    {
        switch (direction)
        {
            case Direction.Top:
                Destroy(topWall);
                topWall = null;
                break;
            case Direction.Right:
                Destroy(rightWall);
                rightWall = null;
                break;
            case Direction.Bottom:
                Destroy(bottomWall);
                bottomWall = null;
                break;
            case Direction.Left:
                Destroy(leftWall);
                leftWall = null;
                break;
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

    public CellMaze GetNeighbour(Direction direction)
    {
        foreach (var neighbour in DynamicNeighbours)
        {
            if (neighbour.direction != direction) continue;
            return neighbour.cell;
        }

        throw new Exception();
    }
}
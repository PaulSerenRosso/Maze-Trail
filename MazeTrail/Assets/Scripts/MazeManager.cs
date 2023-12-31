using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeManager : MonoBehaviour
{
    [SerializeField] private int xSize = 10;
    [SerializeField] private int ySize = 10;
    [SerializeField] private List<CellMaze> cells;
    [SerializeField] private CellMaze cellPrefab;
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wallsParent;
    [SerializeField] private List<CellMaze> recursivePathCells = new();

    private float cellSize;
    private int totalCells;

    private void Start()
    {
        GenerateGridMaze();
        var cell = cells[Random.Range(0, totalCells)];
        StartCoroutine(CreatePath(cell));
    }

    private void GenerateGridMaze()
    {
        cellSize = cellPrefab.transform.localScale.x;
        totalCells = xSize * ySize;

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                /* CELL INSTANTIATION */
                var cell = Instantiate(cellPrefab, new Vector3(x * cellSize, 0, y * cellSize), Quaternion.identity,
                    cellsParent.transform);
                cell.name = $"Cell {(y * xSize) + x}";

                /* WALLS INSTANTIATION */
                if (y == 0)
                {
                    var bottomWall = GenerateWall(x, y, Direction.Bottom);
                    cell.AddWall(bottomWall, Direction.Bottom);
                }
                else
                {
                    cells[(y - 1) * xSize + x].AddNeighbour(Direction.Top, cell);
                    cell.AddNeighbour(Direction.Bottom, cells[(y - 1) * xSize + x]);
                    cell.AddWall(cells[(y - 1) * xSize + x].GetWall(Direction.Top), Direction.Bottom);
                }

                if (x == 0)
                {
                    var leftWall = GenerateWall(x, y, Direction.Left);
                    cell.AddWall(leftWall, Direction.Left);
                }
                else
                {
                    cells[(y * xSize) + x - 1].AddNeighbour(Direction.Right, cell);
                    cell.AddNeighbour(Direction.Left, cells[(y * xSize) + x - 1]);
                    cell.AddWall(cells[(y * xSize) + x - 1].GetWall(Direction.Right), Direction.Left);
                }

                var rightWall = GenerateWall(x, y, Direction.Right);
                cell.AddWall(rightWall, Direction.Right);

                var topWall = GenerateWall(x, y, Direction.Top);
                cell.AddWall(topWall, Direction.Top);

                cells.Add(cell);
            }
        }
    }

    private GameObject GenerateWall(int x, int y, Direction direction)
    {
        var wall = Instantiate(wallPrefab, wallsParent.transform);
        wall.transform.localScale = new Vector3(cellSize, wall.transform.localScale.y,
            wall.transform.localScale.z);

        var tempPos = new Vector3(0, 0, 0);

        switch (direction)
        {
            case Direction.Top:
                tempPos = new Vector3(x * cellSize, 0, y * cellSize + (cellSize / 2));
                break;
            case Direction.Right:
                tempPos = new Vector3(x * cellSize + (cellSize / 2), 0, y * cellSize);
                wall.transform.Rotate(Vector3.up, 90);
                break;
            case Direction.Bottom:
                tempPos = new Vector3(x * cellSize, 0, y * cellSize - (cellSize / 2));
                break;
            case Direction.Left:
                tempPos = new Vector3(x * cellSize - (cellSize / 2), 0, y * cellSize);
                wall.transform.Rotate(Vector3.up, 90);
                break;
        }

        wall.transform.position = tempPos;

        return wall;
    }

    private void RecursivePathMaze(CellMaze recursiveCell)
    {
        var cell = recursiveCell;

        if (recursivePathCells.Count == 0)
        {
            return;
        }

        if (cell.DynamicNeighbours.Count == 0)
        {
            cell.floorMR.material.color = Color.blue;
            RemoveDynamicNeighbourFromStaticNeighbour(cell);
            var previousCell = recursivePathCells[^1];
            recursivePathCells.RemoveAt(recursivePathCells.Count - 1);
            RecursivePathMaze(previousCell);
            return;
        }

        StartCoroutine(CreatePath(cell));
    }

    private IEnumerator CreatePath(CellMaze cell)
    {
        cell.floorMR.material.color = Color.white;
        var neighbourCellTuple = cell.DynamicNeighbours[Random.Range(0, cell.DynamicNeighbours.Count)];

        cell.DestroyWall(neighbourCellTuple.direction);
        neighbourCellTuple.cell.RemoveWall(GetOppositeDirection(neighbourCellTuple.direction));
        foreach (var neighbour in cell.DynamicNeighbours)
        {
            neighbour.cell.RemoveDynamicNeighbour(cell);
        }

        cell.DynamicNeighbours.Remove(neighbourCellTuple);
        recursivePathCells.Add(cell);
        yield return new WaitForSeconds(1);
        RecursivePathMaze(neighbourCellTuple.cell);
    }

    public Direction GetOppositeDirection(Direction randomDirection)
    {
        switch (randomDirection)
        {
            case Direction.Top:
                return Direction.Bottom;
            case Direction.Right:
                return Direction.Left;
            case Direction.Bottom:
                return Direction.Top;
            case Direction.Left:
                return Direction.Right;
            default:
                throw new Exception();
        }
    }

    public void RemoveDynamicNeighbourFromStaticNeighbour(CellMaze cell)
    {
        foreach (var neighbour in cell.StaticNeighbours)
        {
            for (int i = neighbour.cell.DynamicNeighbours.Count - 1; i >= 0; i--)
            {
                if (neighbour.cell.DynamicNeighbours[i].cell != cell) continue;
                neighbour.cell.DynamicNeighbours.RemoveAt(i);
            }
        }
    }
}
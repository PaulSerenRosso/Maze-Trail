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
    [SerializeField] private WallCell wallPrefab;
    [SerializeField] private GameObject wallsParent;
    [SerializeField] private List<CellMaze> recursivePathCells = new();
    [SerializeField] private SerializableDictionary<RailShape, GameObject> rails;

    [SerializeField] private int maxLoopSize = 5;
    [SerializeField] private int minLoopSize = 1;

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
        cellSize = cellPrefab.floor.transform.localScale.x;
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
                    bottomWall.isClosingWall = true;
                    cell.walls.Add(Direction.Bottom, bottomWall);
                }
                else
                {
                    cells[(y - 1) * xSize + x].AddNeighbour(Direction.Top, cell);
                    cell.AddNeighbour(Direction.Bottom, cells[(y - 1) * xSize + x]);
                    cell.walls.Add(Direction.Bottom, cells[(y - 1) * xSize + x].walls[Direction.Top]);
                }

                if (x == 0)
                {
                    var leftWall = GenerateWall(x, y, Direction.Left);
                    leftWall.isClosingWall = true;
                    cell.walls.Add(Direction.Left, leftWall);
                }
                else
                {
                    cells[(y * xSize) + x - 1].AddNeighbour(Direction.Right, cell);
                    cell.AddNeighbour(Direction.Left, cells[(y * xSize) + x - 1]);
                    cell.walls.Add(Direction.Left, cells[(y * xSize) + x - 1].walls[Direction.Right]);
                }

                var rightWall = GenerateWall(x, y, Direction.Right);
                cell.walls.Add(Direction.Right, rightWall);
                if (x == xSize - 1) rightWall.isClosingWall = true;

                var topWall = GenerateWall(x, y, Direction.Top);
                cell.walls.Add(Direction.Top, topWall);
                if (y == ySize - 1) topWall.isClosingWall = true;

                cells.Add(cell);
            }
        }
    }

    private WallCell GenerateWall(int x, int y, Direction direction)
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
            StartCoroutine(GenerateCycle());
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

        Destroy(cell.walls[neighbourCellTuple.direction].gameObject);
        cell.walls[neighbourCellTuple.direction] = null;
        neighbourCellTuple.cell.walls[GetOppositeDirection(neighbourCellTuple.direction)] = null;
        foreach (var neighbour in cell.DynamicNeighbours)
        {
            neighbour.cell.RemoveDynamicNeighbour(cell);
        }

        cell.DynamicNeighbours.Remove(neighbourCellTuple);
        recursivePathCells.Add(cell);
        yield return new WaitForSeconds(0.2f);
        RecursivePathMaze(neighbourCellTuple.cell);
    }

    private IEnumerator GenerateCycle()
    {
        var cellsU = new List<CellMaze>();
        var currentMaxLoopSize = maxLoopSize;
        var currentMinLoopSize = maxLoopSize;
        foreach (var cell in cells)
        {
            var isValid = true;
            if (cell.GetRailShape() != RailShape.ShapeU) continue;
            foreach (var wall in cell.walls)
            {
                if (wall.Value == null)
                {
                    if (cell.walls[GetOppositeDirection(wall.Key)].isClosingWall)
                    {
                        isValid = false;
                    }
                    break;
                }
            }
            if (!isValid) continue; 
            cellsU.Add(cell);
        }

        if (cellsU.Count < maxLoopSize)
        {
            currentMaxLoopSize = cellsU.Count;
        }

        if (cellsU.Count < minLoopSize)
        {
            currentMinLoopSize = cellsU.Count;
        }

        var randomCount = Random.Range(currentMinLoopSize, currentMaxLoopSize);
        cellsU.Shuffle();

        var oppositeDirection = Direction.Bottom;
        for (int i = 0; i < randomCount; i++)
        {
            foreach (var wall in cellsU[i].walls)
            {
                if (wall.Value == null)
                {
                    oppositeDirection = GetOppositeDirection(wall.Key);
                    break;
                }
            }
            
            Destroy(cellsU[i].walls[oppositeDirection].gameObject);
            cellsU[i].walls[oppositeDirection] = null;
            var neighbourCell = cellsU[i].GetNeighbourStatic(oppositeDirection);
            neighbourCell.walls[GetOppositeDirection(oppositeDirection)] = null;
            yield return new WaitForSeconds(1.0f);
        }

        StartCoroutine(GenerateEntries());
    }

    private IEnumerator GenerateEntries()
    {
        var randomEntry = (Direction)Random.Range(0, 4);
        var randomExit = GetOppositeDirection(randomEntry);

        CreateEntry(randomEntry);
        yield return new WaitForSeconds(1.0f);
        CreateEntry(randomExit);
        yield return new WaitForSeconds(1.0f);
        
        StartCoroutine(GenerateRails());
    }

    private void CreateEntry(Direction randomEntry)
    {
        var randomX = Random.Range(0, xSize);
        var randomY = Random.Range(0, ySize);
        int index;
        switch (randomEntry)
        {
            case Direction.Top:
                index = ((ySize - 1) * xSize) + randomX;
                Destroy(cells[index].walls[Direction.Top].gameObject);
                cells[index].walls[Direction.Top] = null;
                break;
            case Direction.Right:
                index = (randomY) * xSize + xSize - 1;
                Destroy(cells[index].walls[Direction.Right].gameObject);
                cells[index].walls[Direction.Right] = null;
                break;
            case Direction.Bottom:
                Destroy(cells[randomX].walls[Direction.Bottom].gameObject);
                cells[randomX].walls[Direction.Bottom] = null;
                break;
            case Direction.Left:
                index = (randomY) * xSize;
                Destroy(cells[index].walls[Direction.Left].gameObject);
                cells[index].walls[Direction.Left] = null;
                break;
        }
    }

    private IEnumerator GenerateRails()
    {
        foreach (var cell in cells)
        {
            var shape = cell.GetRailShape();
            var rail = Instantiate(rails[shape], cell.transform.position, Quaternion.identity, cell.transform);
            switch (shape)
            {
                case RailShape.ShapeI:
                    if (cell.walls[Direction.Top])
                    {
                        rail.transform.Rotate(Vector3.up, 90);
                    }

                    break;

                case RailShape.ShapeL:
                    var directions = new List<Direction>();
                    foreach (var wall in cell.walls)
                    {
                        if (!wall.Value)
                        {
                            directions.Add(wall.Key);
                        }
                    }

                    if (directions[0] == Direction.Left && directions[1] == Direction.Top ||
                        directions[0] == Direction.Top && directions[1] == Direction.Left)
                    {
                        rail.transform.Rotate(Vector3.up, 90);
                    }

                    if (directions[0] == Direction.Top && directions[1] == Direction.Right ||
                        directions[0] == Direction.Right && directions[1] == Direction.Top)
                    {
                        rail.transform.Rotate(Vector3.up, 180);
                    }

                    if (directions[0] == Direction.Bottom && directions[1] == Direction.Right ||
                        directions[0] == Direction.Right && directions[1] == Direction.Bottom)
                    {
                        rail.transform.Rotate(Vector3.up, -90);
                    }

                    break;

                case RailShape.ShapeT:
                    foreach (var wall in cell.walls)
                    {
                        if (wall.Value)
                        {
                            switch (wall.Key)
                            {
                                case Direction.Right:
                                    rail.transform.Rotate(Vector3.up, 90);
                                    break;
                                case Direction.Bottom:
                                    rail.transform.Rotate(Vector3.up, 180);
                                    break;
                                case Direction.Left:
                                    rail.transform.Rotate(Vector3.up, -90);
                                    break;
                            }
                        }
                    }

                    break;

                case RailShape.ShapeU:
                    foreach (var wall in cell.walls)
                    {
                        if (!wall.Value)
                        {
                            switch (wall.Key)
                            {
                                case Direction.Right:
                                    rail.transform.Rotate(Vector3.up, 90);
                                    break;
                                case Direction.Bottom:
                                    rail.transform.Rotate(Vector3.up, 180);
                                    break;
                                case Direction.Left:
                                    rail.transform.Rotate(Vector3.up, -90);
                                    break;
                            }
                        }
                    }

                    break;

                case RailShape.ShapeX:
                    break;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    #region Utilities

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

    #endregion
}
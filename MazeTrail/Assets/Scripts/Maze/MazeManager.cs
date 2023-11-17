using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeManager : MonoBehaviour
{
    public CharacterController Player;

    [SerializeField] private int xSize = 10;
    [SerializeField] private int ySize = 10;
    [SerializeField] private List<CellMaze> cells;
    [SerializeField] private CellMaze cellPrefab;
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private WallCell wallPrefab;
    [SerializeField] private GameObject wallsParent;
    [SerializeField] private GameObject railsParent;
    [SerializeField] private GameObject propsParent;
    [SerializeField] private List<CellMaze> recursivePathCells = new();
    [SerializeField] private SerializableDictionary<RailShape, GameObject> rails;
    [SerializeField] private int maxLoopSize = 5;
    [SerializeField] private int minLoopSize = 1;
    [SerializeField] private int numberOfCellsEntries = 3;
    [SerializeField] private Intersection intersectionPrefab;
    [SerializeField] private CharacterController characterControllerPrefab;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Exit exitPrefab;
    [SerializeField] private GameObject acceleratorPrefab;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float timerPath = 0.01f;
    [SerializeField] private float timerCycle = 0.01f;
    [SerializeField] private float timerEntries = 0.01f;
    [SerializeField] private float timerRails = 0.01f;
    [SerializeField] private float timerBiome = 0.01f;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Biome[] biomes;
    [SerializeField] private MeshMergeManager meshMergeManagerWalls;
    [SerializeField] private MeshMergeManager meshMergeManagerFloors;
    [SerializeField] private MeshMergeManager meshMergeManagerProps;

    private float cellSize;
    private int totalCells;

    public void GenerateMaze(int size = 10)
    {
        xSize = size;
        ySize = size;
        maxLoopSize = size / 2;
        cellSize = cellPrefab.floor.transform.localScale.x;
        totalCells = xSize * ySize;
        cameraController.transform.position =
            new Vector3((xSize / 2) * cellSize, 10 * xSize / 2, ((xSize / 2) * cellSize) / 2);
        GenerateGridMaze();
        StartCoroutine(GenerateEntries());
    }

    private void GenerateGridMaze()
    {
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                /* CELL INSTANTIATION */
                var cell = Instantiate(cellPrefab, new Vector3(x * cellSize, 0, y * cellSize), Quaternion.identity,
                    cellsParent.transform);
                cell.name = $"Cell {(y * xSize) + x}";
                meshMergeManagerFloors.meshFilters.Add(cell.floorMF);
                meshMergeManagerFloors.meshRenderers.Add(cell.floorMR);

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
        meshMergeManagerWalls.meshFilters.Add(wall.meshFilter);
        meshMergeManagerWalls.meshRenderers.Add(wall.meshRenderer);

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

        if (cell.DynamicNeighbours.Count == 0 && recursivePathCells.Count != 0)
        {
            // cell.floorMR.material.color = Color.blue;
            RemoveDynamicNeighbourFromStaticNeighbour(cell);
            var previousCell = recursivePathCells[^1];
            recursivePathCells.RemoveAt(recursivePathCells.Count - 1);
            RecursivePathMaze(previousCell);
            return;
        }

        if (cell.DynamicNeighbours.Count != 0)
        {
            StartCoroutine(CreatePath(cell));
        }

        if (recursivePathCells.Count == 0)
        {
            StartCoroutine(GenerateCycle());
        }
    }

    private IEnumerator CreatePath(CellMaze cell)
    {
        // cell.floorMR.material.color = Color.red;
        var neighbourCellTuple = cell.DynamicNeighbours[Random.Range(0, cell.DynamicNeighbours.Count)];
        Destroy(cell.walls[neighbourCellTuple.direction].gameObject);
        meshMergeManagerWalls.meshFilters.Remove(cell.walls[neighbourCellTuple.direction].meshFilter);
        meshMergeManagerWalls.meshRenderers.Remove(cell.walls[neighbourCellTuple.direction].meshRenderer);
        AddInterpolatedRails(cell.transform.position, neighbourCellTuple.cell.transform.position);
        cell.walls[neighbourCellTuple.direction] = null;
        neighbourCellTuple.cell.walls[GetOppositeDirection(neighbourCellTuple.direction)] = null;
        foreach (var neighbour in cell.DynamicNeighbours)
        {
            neighbour.cell.RemoveDynamicNeighbour(cell);
        }

        cell.DynamicNeighbours.Remove(neighbourCellTuple);
        recursivePathCells.Add(cell);
        yield return new WaitForSeconds(timerPath);
        RecursivePathMaze(neighbourCellTuple.cell);
    }

    private IEnumerator GenerateCycle()
    {
        var cellsU = new List<CellMaze>();
        var wallsToDestroy = new List<WallCell>();
        var currentMaxLoopSize = maxLoopSize;
        var currentMinLoopSize = maxLoopSize;
        foreach (var cell in cells)
        {
            var isValid = true;
            WallCell wallToDestroy = null;
            if (cell.GetRailShape() != RailShape.ShapeU) continue;
            foreach (var wall in cell.walls)
            {
                if (wall.Value == null)
                {
                    wallToDestroy = cell.walls[GetOppositeDirection(wall.Key)];
                    if (wallToDestroy.isClosingWall)
                    {
                        isValid = false;
                    }
                    else
                    {
                        var neighbourCell = cell.GetNeighbourStatic(GetOppositeDirection(wall.Key));
                        if (cellsU.Contains(neighbourCell))
                        {
                            foreach (var wallToTest in wallsToDestroy)
                            {
                                if (wallToDestroy == wallToTest)
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                        }
                    }

                    break;
                }
            }

            if (!isValid) continue;
            cellsU.Add(cell);
            wallsToDestroy.Add(wallToDestroy);
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
            meshMergeManagerWalls.meshFilters.Remove(cellsU[i].walls[oppositeDirection].meshFilter);
            meshMergeManagerWalls.meshRenderers.Remove(cellsU[i].walls[oppositeDirection].meshRenderer);
            cellsU[i].walls[oppositeDirection] = null;
            var neighbourCell = cellsU[i].GetNeighbourStatic(oppositeDirection);
            AddInterpolatedRails(cellsU[i].transform.position, neighbourCell.transform.position);
            neighbourCell.walls[GetOppositeDirection(oppositeDirection)] = null;
            yield return new WaitForSeconds(timerCycle);
        }

        StartCoroutine(GenerateRails());
    }

    private IEnumerator GenerateEntries()
    {
        var randomExit = (Direction)Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            if (randomExit == (Direction)i) CreateEntry(randomExit, true);
            else CreateEntry((Direction)i);
            yield return new WaitForSeconds(timerEntries);
        }

        var cell = cells[Random.Range(0, totalCells)];
        StartCoroutine(CreatePath(cell));
    }

    private void CreateEntry(Direction randomEntry, bool isExit = false)
    {
        var randomX = Random.Range(0, xSize);
        var randomY = Random.Range(0, ySize);
        int index = 0;
        var pos = Vector2.zero;
        Vector3 direction = Vector3.zero;
        switch (randomEntry)
        {
            case Direction.Top:
                index = ((ySize - 1) * xSize) + randomX;
                direction = Vector3.forward;
                pos = new Vector2(randomX, ySize - 1);
                break;
            case Direction.Right:
                index = (randomY) * xSize + xSize - 1;
                direction = Vector3.right;
                pos = new Vector2((xSize - 1), randomY);
                break;
            case Direction.Bottom:
                index = randomX;
                direction = Vector3.back;
                pos = new Vector2(randomX, 0);
                break;
            case Direction.Left:
                index = (randomY) * xSize;
                direction = Vector3.left;
                pos = new Vector2(0, randomY);
                break;
        }

        for (int i = 0; i < numberOfCellsEntries; i++)
        {
            var position = cells[index].walls[randomEntry].transform.position +
                           direction * (i * cellSize + (cellSize / 2));
            var cell = Instantiate(cellPrefab, position, Quaternion.identity, cellsParent.transform);
            cell.name = $"CellEntry";
            meshMergeManagerFloors.meshFilters.Add(cell.floorMF);
            meshMergeManagerFloors.meshRenderers.Add(cell.floorMR);

            if (i == 0)
            {
                cells[index].AddNeighbour(randomEntry, cell);
                cell.AddNeighbour(GetOppositeDirection(randomEntry), cells[index]);
                GenerateWallForEntries(cell, GetOppositeDirection(randomEntry), cells[index].walls[randomEntry],
                    new Vector2(pos.x + direction.x * (i + 1), pos.y + direction.z * (i + 1)));
            }
            else
            {
                cells[^1].AddNeighbour(randomEntry, cell);
                cell.AddNeighbour(GetOppositeDirection(randomEntry), cells[^1]);
                GenerateWallForEntries(cell, GetOppositeDirection(randomEntry), cells[^1].walls[randomEntry],
                    new Vector2(pos.x + direction.x * (i + 1), pos.y + direction.z * (i + 1)));
            }

            cells.Add(cell);
        }

        var directionToLook = (cells[^1].transform.position - cells[^2].transform.position).normalized;
        if (isExit)
        {
            var exit = Instantiate(exitPrefab, cells[^2].transform.position, Quaternion.LookRotation(-directionToLook),
                railsParent.transform);
            exit.Initialize(gameManager);
        }
        else
        {
            Instantiate(entryPrefab, cells[^2].transform.position, Quaternion.LookRotation(directionToLook),
                railsParent.transform);
        }
    }

    private void GenerateWallForEntries(CellMaze cell, Direction directionWall, WallCell startWall, Vector2 position)
    {
        cell.walls.Add(directionWall, startWall);
        for (int j = 0; j < 4; j++)
        {
            if (j == (int)directionWall) continue;
            var wall = GenerateWall((int)position.x, (int)position.y, (Direction)j);
            cell.walls.Add((Direction)j, wall);
            wall.isClosingWall = true;
        }
    }

    private IEnumerator GenerateRails()
    {
        Intersection intersection;
        List<int> cellsX = new();
        List<int> cellsY = new();
        for (var index = 0; index < cells.Count; index++)
        {
            var cell = cells[index];
            var shape = cell.GetRailShape();
            GameObject rail = null;
            if (shape == RailShape.ShapeI)
            {
                if (Random.Range(0.0f, 1.0f) > 0.5f)
                {
                    var currentX = index % xSize;
                    var currentY = Mathf.FloorToInt((float)index / xSize);

                    if (!cellsX.Contains(currentX) && !cellsY.Contains(currentY))
                    {
                        rail = Instantiate(acceleratorPrefab, cell.transform.position, Quaternion.identity,
                            railsParent.transform);
                        cellsX.Add(currentX);
                        cellsY.Add(currentY);
                    }
                    else
                    {
                        rail = Instantiate(rails[shape], cell.transform.position, Quaternion.identity,
                            railsParent.transform);
                    }
                }

                else
                {
                    rail = Instantiate(rails[shape], cell.transform.position, Quaternion.identity,
                        railsParent.transform);
                }
            }
            else
            {
                rail = Instantiate(rails[shape], cell.transform.position, Quaternion.identity, railsParent.transform);
            }

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
                    intersection = Instantiate(intersectionPrefab, cell.transform.position, Quaternion.identity,
                        rail.transform);
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

                        intersection.availableDirections.Add(Direction.Left);
                        intersection.availableDirections.Add(Direction.Top);
                    }
                    else if (directions[0] == Direction.Top && directions[1] == Direction.Right ||
                             directions[0] == Direction.Right && directions[1] == Direction.Top)
                    {
                        rail.transform.Rotate(Vector3.up, 180);
                        intersection.availableDirections.Add(Direction.Right);
                        intersection.availableDirections.Add(Direction.Top);
                    }
                    else if (directions[0] == Direction.Bottom && directions[1] == Direction.Right ||
                             directions[0] == Direction.Right && directions[1] == Direction.Bottom)
                    {
                        rail.transform.Rotate(Vector3.up, -90);
                        intersection.availableDirections.Add(Direction.Right);
                        intersection.availableDirections.Add(Direction.Bottom);
                    }
                    else
                    {
                        intersection.availableDirections.Add(Direction.Left);
                        intersection.availableDirections.Add(Direction.Bottom);
                    }

                    break;

                case RailShape.ShapeT:
                    intersection = Instantiate(intersectionPrefab, cell.transform.position, Quaternion.identity,
                        rail.transform);
                    foreach (var wall in cell.walls)
                    {
                        if (wall.Value)
                        {
                            switch (wall.Key)
                            {
                                case Direction.Right:
                                    rail.transform.Rotate(Vector3.up, 90);
                                    intersection.availableDirections.Add(Direction.Top);
                                    intersection.availableDirections.Add(Direction.Bottom);
                                    intersection.availableDirections.Add(Direction.Left);
                                    break;
                                case Direction.Bottom:
                                    rail.transform.Rotate(Vector3.up, 180);
                                    intersection.availableDirections.Add(Direction.Right);
                                    intersection.availableDirections.Add(Direction.Top);
                                    intersection.availableDirections.Add(Direction.Left);
                                    break;
                                case Direction.Left:
                                    rail.transform.Rotate(Vector3.up, -90);
                                    intersection.availableDirections.Add(Direction.Top);
                                    intersection.availableDirections.Add(Direction.Bottom);
                                    intersection.availableDirections.Add(Direction.Right);
                                    break;
                                default:
                                    intersection.availableDirections.Add(Direction.Right);
                                    intersection.availableDirections.Add(Direction.Bottom);
                                    intersection.availableDirections.Add(Direction.Left);
                                    break;
                            }
                        }
                    }

                    break;

                case RailShape.ShapeU:
                    intersection = Instantiate(intersectionPrefab, cell.transform.position, Quaternion.identity,
                        rail.transform);
                    foreach (var wall in cell.walls)
                    {
                        if (!wall.Value)
                        {
                            switch (wall.Key)
                            {
                                case Direction.Right:
                                    rail.transform.Rotate(Vector3.up, -90);
                                    break;
                                case Direction.Top:
                                    rail.transform.Rotate(Vector3.up, 180);
                                    break;
                                case Direction.Left:
                                    rail.transform.Rotate(Vector3.up, 90);
                                    break;
                            }
                        }
                    }

                    break;

                case RailShape.ShapeX:
                    intersection = Instantiate(intersectionPrefab, cell.transform.position, Quaternion.identity,
                        rail.transform);
                    intersection.availableDirections.Add(Direction.Right);
                    intersection.availableDirections.Add(Direction.Bottom);
                    intersection.availableDirections.Add(Direction.Left);
                    intersection.availableDirections.Add(Direction.Top);
                    break;
            }

            yield return new WaitForSeconds(timerRails);
        }

        StartCoroutine(CreateBiome());
    }

    private void CreatePlayer()
    {
        var midCell = cells[Mathf.FloorToInt((totalCells - 12 + (xSize / 2)) / 2)];
        CellMaze neighbourCell = null;
        foreach (var wall in midCell.walls)
        {
            if (wall.Value != null) continue;
            neighbourCell = midCell.GetNeighbourStatic(wall.Key);
            break;
        }

        var direction = (neighbourCell.transform.position - midCell.transform.position).normalized;
        Player = Instantiate(characterControllerPrefab, midCell.transform.position + (direction * (cellSize / 2)),
            Quaternion.LookRotation(direction));
        Player.Init(direction, gameManager);
        cameraController.Initialize(Player.transform);
        var time = (30 / (1 + Mathf.Exp(-0.4f * ((xSize / 3) - 2.0f))) - 14.0f) * 60.0f;
        gameManager.SetTimer(time);

        meshMergeManagerProps.MergeMeshes();
        meshMergeManagerWalls.MergeMeshes();
        meshMergeManagerFloors.MergeMeshes();

        Destroy(propsParent);
        Destroy(wallsParent);
        Destroy(cellsParent);
    }

    private IEnumerator CreateBiome()
    {
        var biomeSize = xSize / 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var biome = biomes[j + i * 3];
                for (int y = i * biomeSize; y < biomeSize + (i * biomeSize); y++)
                {
                    for (int x = j * biomeSize; x < biomeSize + j * biomeSize; x++)
                    {
                        var listWalls = cells[(y * xSize) + x].walls.Values.ToList();
                        listWalls.Shuffle();
                        foreach (var wall in listWalls)
                        {
                            if (wall == null) continue;
                            var direction = (cells[y * xSize + x].transform.position - wall.transform.position)
                                .normalized;
                            var prop = Instantiate(biome.prefabs[Random.Range(0, biome.prefabs.Count)],
                                wall.transform.position + direction * (cellSize / 4),
                                Quaternion.LookRotation(direction), propsParent.transform);
                            prop.transform.parent = wall.transform;
                            meshMergeManagerProps.meshRenderers.Add(prop.meshRenderer);
                            meshMergeManagerProps.meshFilters.Add(prop.meshFilter);
                            yield return new WaitForSeconds(timerBiome);
                            break;
                        }
                    }
                }
            }
        }

        CreatePlayer();
    }

    private void AddInterpolatedRails(Vector3 pos, Vector3 pos2)
    {
        var direction = (pos2 - pos).normalized;
        var countInterpolatedRails = cellSize - 1;
        for (int i = 0; i < countInterpolatedRails; i++)
        {
            var interpolatedPos = pos + direction * (i + 1);
            var interpolatedRail = Instantiate(rails[RailShape.ShapeI], interpolatedPos, Quaternion.identity,
                railsParent.transform);
            interpolatedRail.transform.LookAt(pos2);
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
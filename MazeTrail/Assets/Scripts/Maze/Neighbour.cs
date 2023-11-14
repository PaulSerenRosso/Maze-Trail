using System;

[Serializable]
public class Neighbour
{
    public Direction direction;
    public CellMaze cell;

    public Neighbour(Direction direction, CellMaze cell)
    {
        this.direction = direction;
        this.cell = cell;
    }
}
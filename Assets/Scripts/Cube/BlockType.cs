using UnityEngine;

[System.Serializable]
public struct Coordinate
{
    public int row;
    public int column;
}

[System.Serializable]
public struct BlockType
{
    public string name;
    public bool isSolid;
    public Coordinate top;
    public Coordinate side;
    public Coordinate bottom;
}
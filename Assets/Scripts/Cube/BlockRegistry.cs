using UnityEngine;

public static class BlockRegistry
{
    public static readonly BlockType[] table = new BlockType[]
    {
        new BlockType
        {
            name = "air",
            isSolid = false,
        },
        new BlockType
        {
            name = "stone",
            isSolid = true,
            top    = new Coordinate { row = 3, column = 0 },
            side   = new Coordinate { row = 3, column = 0 },
            bottom = new Coordinate { row = 3, column = 0 },
        },

        new BlockType
        {
            name = "dirt",
            isSolid = true,
            top    = new Coordinate { row = 3, column = 1 },
            side   = new Coordinate { row = 3, column = 1 },
            bottom = new Coordinate { row = 3, column = 1 },
        },

        new BlockType
        {
            name = "grass",
            isSolid = true,
            top    = new Coordinate { row = 2, column = 3 }, 
            side   = new Coordinate { row = 3, column = 1 },
            bottom = new Coordinate { row = 3, column = 1 }, 
        },
    };
    public static byte GetId(string name)
    {
        for (byte i = 0; i < table.Length; i++)
            if (table[i].name == name) return i;
        Debug.LogError($"Unknown block: {name}");
        return 0;
    }
}
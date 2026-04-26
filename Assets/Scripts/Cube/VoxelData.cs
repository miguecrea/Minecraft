using UnityEngine;

public static class VoxelData
{
    // (position, UV, normal, etc.)

    //Flat shading(each face has its own normal)
    //Per-face lighting
    //Face culling(for chunks)
    //allows different textures per 

    public static readonly Vector3[,] faceVertices = new Vector3[6, 4]
{
    // 0 = Front  (+Z)
    { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) },
    // 1 = Back   (-Z)
    { new Vector3(1,0,0), new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0) },
    // 2 = Left   (-X)
    { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0) },
    // 3 = Right  (+X)
    { new Vector3(1,0,1), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1) },
    // 4 = Top    (+Y)
    { new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(0,1,0) },
    // 5 = Bottom (-Y)
    { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
};

    // Local indices into the 4 vertices of a face — same for every face
    public static readonly int[] faceTriangles = { 0, 1, 2, 0, 2, 3 };

    public enum FACE
    {
        FRONT,
        BACK,
        LEFT,
        RIGHT,
        TOP,
        BOTTOM
    }

    // Direction to the neighbour block for each face — used for culling
    public static readonly Vector3Int[] faceChecks = new Vector3Int[6]
    {
    new Vector3Int( 0,  0,  1), // Front
    new Vector3Int( 0,  0, -1), // Back
    new Vector3Int(-1,  0,  0), // Left
    new Vector3Int( 1,  0,  0), // Right
    new Vector3Int( 0,  1,  0), // Top
    new Vector3Int( 0, -1,  0), // Bottom
    };


}

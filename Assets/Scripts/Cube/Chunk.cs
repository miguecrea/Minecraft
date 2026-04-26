
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using static VoxelData;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{

    // Owner reference
    private World m_World;

    //Material Reference
    private Material m_AtlasMaterial;

    private MeshRenderer m_Renderer;
    private MeshFilter m_MeshFilter;

    public const int Width = 16;
    public const int Height = 50;
    public const int Depth = 16;

    // should be const 
    static float tileSize = 1f / 4f;  // 0.25

    public int ChunkX { get; private set; }
    public int ChunkZ { get; private set; }

    private byte[,,] blocks = new byte[Width, Height, Depth];
    //16 × 64 × 16 = 16,384 bytes of memory
    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_MeshFilter = GetComponent<MeshFilter>();
    }
    private void BuildMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int x = 0; x < Width; x++)//Right Axis
            for (int y = 0; y < Height; y++) //Up Axis
                for (int z = 0; z < Depth; z++) //Forward Axis
                {
                    if (!IsSolidLocal(x, y, z)) continue;

                    Vector3 blockPos = new Vector3(x, y, z);
                    for (int face = 0; face < 6; face++)
                    {
                        Vector3Int neighbour = new Vector3Int(x, y, z) + VoxelData.faceChecks[face];

                        if (IsSolid(neighbour.x,neighbour.y,neighbour.z)) continue;
                        //index before you
                        int vertStart = vertices.Count;
                        for (int v = 0; v < 4; v++)
                        {
                            vertices.Add(VoxelData.faceVertices[face, v] + blockPos);
                        }
                        //To be improved 

                        AddFaces(triangles, vertStart);

                        byte blockId = blocks[x, y, z];
                        AddTextures(uvs, blockId, face);

                    }
                }
        //Unity already takes into account the mesh Pos
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        m_MeshFilter.sharedMesh = mesh;
        m_Renderer.sharedMaterial = m_AtlasMaterial;
    }

    public void InitialiseTerrain(int chunkX, int chunkZ, Material atlasMaterial,World world)
    {
        m_World = world;
        ChunkX = chunkX;
        ChunkZ = chunkZ;
        m_AtlasMaterial = atlasMaterial;

        // Position this chunk in the world
        transform.position = new Vector3(chunkX * Width, 0, chunkZ * Depth);

        GenerateTerrain();
    }

    public void BuildMeshFaces()
    {
        BuildMesh();
    }

    private static void AddTextures(List<Vector2> uvs, byte blockId, int face)
    {

        BlockType type = BlockRegistry.table[blockId];

        Coordinate coord;
        if (face == (int)FACE.TOP) coord = type.top;
        else if (face == (int)FACE.BOTTOM) coord = type.bottom;
        else coord = type.side;

        float uMin = coord.column * tileSize;
        float vMin = coord.row * tileSize;
        float uMax = uMin + tileSize;
        float vMax = vMin + tileSize;

        uvs.Add(new Vector2(uMin, vMin));  // bottom-left
        uvs.Add(new Vector2(uMax, vMin));  // bottom-right
        uvs.Add(new Vector2(uMax, vMax));  // top-right
        uvs.Add(new Vector2(uMin, vMax));  // top-left
    }

    private static void AddFaces(List<int> triangles, int vertStart)
    {
        //this could be an array 
        // Two triangles per face, indexing into the 4 verts we just added
        triangles.Add(vertStart + 0);
        triangles.Add(vertStart + 1);
        triangles.Add(vertStart + 2);

        triangles.Add(vertStart + 0);
        triangles.Add(vertStart + 2);
        triangles.Add(vertStart + 3);
    }

    public bool IsSolid(int x, int y, int z)
    {
        // Inside this chunk's local bounds → fast path, just look up the array
        if (x >= 0 && x < Width &&
            y >= 0 && y < Height &&
            z >= 0 && z < Depth)
        {
            byte id = blocks[x, y, z];
            return BlockRegistry.table[id].isSolid;
        }

        // Outside local bounds → ask the world (which finds the right neighbour chunk)
        int globalX = ChunkX * Width + x;
        int globalZ = ChunkZ * Depth + z;

        return m_World.IsSolidAtGlobal(globalX, y, globalZ);

    }

    public bool IsSolidLocal(int x, int y, int z)
    {
        byte id = blocks[x, y, z];
        return BlockRegistry.table[id].isSolid;
    }

    void GenerateTerrain()
    {
        // How "zoomed in" the noise is. Smaller = wider, smoother hills.
        // Larger = bumpier, more chaotic terrain.
        float noiseScale = 0.05f;

        // The vertical range the surface can occupy.
        int baseHeight = 16;   // minimum surface level
        int hillHeight = 30;   // how much the surface can vary on top of base

        for (int x = 0; x < Width; x++)
            for (int z = 0; z < Depth; z++)
            {
                // Sample 2D noise at this column's (x, z)
               // float noise = Mathf.PerlinNoise(x * noiseScale, z * noiseScale);

                float globalX = ChunkX * Width + x + m_World.NoiseOffset.x;
                float globalZ = ChunkZ * Depth + z + m_World.NoiseOffset.y;

              //  float noise = Mathf.PerlinNoise(globalX * noiseScale, globalZ * noiseScale);


                float noise = SampleHeightNoise(globalX, globalZ);

                // Convert noise to a surface height for this column
                int surfaceHeight = baseHeight + Mathf.FloorToInt(noise * hillHeight);

                for (int y = 0; y < Height; y++)
                {
                    byte type;
                    byte air = BlockRegistry.GetId("air");
                    byte stone = BlockRegistry.GetId("stone");
                    byte dirt = BlockRegistry.GetId("dirt");
                    byte grass = BlockRegistry.GetId("grass");

                    if (y > surfaceHeight) type = air;
                    else if (y == surfaceHeight) type = grass;
                    else if (y > surfaceHeight - 4) type = dirt;
                    else type = stone;

                    blocks[x, y, z] = type;
                }
            }
    }

    float SampleHeightNoise(float x, float z)
    {
        float frequency = 0.05f;   // starting "zoom level"
        float amplitude = 1f;      // starting strength
        float total = 0f;
        float maxValue = 0f;      // for normalising back to [0, 1]

        int octaves = 6;     // how many layers to stack
        float persistence = 0.4f;  // each octave is half as strong as the last
        float lacunarity = 2f;    // each octave is twice as zoomed-in as the last

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;   // result is in [0, 1]
    }


//Octaves — how many layers to stack.More octaves = more detail, more cost.Three or four is plenty for terrain; eight starts to look like static.
//Persistence — how much weaker each successive octave is. 0.5 (each octave half as strong as the last) is the sweet spot for terrain.Lower(0.3) → smoother, big-shapes-dominate look.Higher(0.7) → noisier, small-features-dominate look.
//Lacunarity — how much more zoomed-in each successive octave is. 2.0 is standard (each octave doubles in frequency). Mess with this last; the default is almost always right.
//Normalising — without it, four octaves of noise would sum to values up to 4.0, and your Mathf.FloorToInt(noise* hillHeight) would explode.Dividing by maxValue(which is the sum of all amplitudes) keeps the output in [0, 1] regardless of how many octaves you use.
}
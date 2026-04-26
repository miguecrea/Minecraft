using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private Material m_AtlasMaterial;
    [SerializeField] private GameObject m_ChunkPrefab;

    [SerializeField] private bool m_UseRandomSeed = true;
    [SerializeField] private int m_Seed = 0;
    public Vector2 NoiseOffset { get; private set; }

    [SerializeField] private int m_WorldSizeInChunks = 20;
    private Chunk[,] m_Chunks;
    private void Awake()
    {
        if (!m_AtlasMaterial || !m_ChunkPrefab) Debug.LogError("Material or prefab missing on the editor");

        //Ramdom Seed 
        int seed = m_UseRandomSeed ? System.DateTime.Now.GetHashCode() : m_Seed;
        Random.InitState(seed);

        NoiseOffset = new Vector2(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));
    }
    private void Start()
    {
        //5 x 5 = 25 Chunks
        m_Chunks = new Chunk[m_WorldSizeInChunks, m_WorldSizeInChunks];

        for (int x = 0; x < m_WorldSizeInChunks; x++)
            for (int z = 0; z < m_WorldSizeInChunks; z++)
            {
                CreateChunk(x, z);
            }

        for (int x = 0; x < m_WorldSizeInChunks; x++)
            for (int z = 0; z < m_WorldSizeInChunks; z++)
            {
                m_Chunks[x, z].BuildMeshFaces();
            }
    }

    void CreateChunk(int XPos, int ZPos)
    {
        GameObject chunkObj = Instantiate(m_ChunkPrefab, transform);

        Chunk chunk = chunkObj.GetComponent<Chunk>();
        chunk.InitialiseTerrain(XPos, ZPos, m_AtlasMaterial, this);

        m_Chunks[XPos, ZPos] = chunk;
    }

    public bool IsSolidAtGlobal(int globalX, int globalY, int globalZ)
    {
        // Vertical out-of-bounds: above or below the world is always air
        if (globalY < 0 || globalY >= Chunk.Height)
            return false;

        // Convert global X/Z to chunk coordinates and local-within-chunk coordinates
        int chunkX = Mathf.FloorToInt(globalX / (float)Chunk.Width);
        int chunkZ = Mathf.FloorToInt(globalZ / (float)Chunk.Depth);

        // Outside the loaded world entirely? Treat as air (so the world has visible edges)
        if (chunkX < 0 || chunkX >= m_WorldSizeInChunks ||
            chunkZ < 0 || chunkZ >= m_WorldSizeInChunks)
            return false;

        int localX = globalX - chunkX * Chunk.Width;
        int localZ = globalZ - chunkZ * Chunk.Depth;

        Chunk neighbourChunk = m_Chunks[chunkX, chunkZ];
        if (neighbourChunk == null) return false;   // not loaded yet

        return neighbourChunk.IsSolidLocal(localX, globalY, localZ);
    }






}

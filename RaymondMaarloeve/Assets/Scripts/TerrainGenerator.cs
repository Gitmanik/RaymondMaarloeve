using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance { get; private set; }
    
    private int width = 100, height = 100;
    // public int sqrtLayerResolution = 10;

    public Terrain terrain;
    
    // public Texture2D terrainTexture;

    public int buildingArea = 10;
    public GameObject[] Buildings;

    void Awake()
    {
        Instance = this;
    }
    
    public void GenerateMap()
    {
        Debug.Log("Generating map");
        width = (int) Math.Floor(terrain.terrainData.size.x);
        height = (int) Math.Floor(terrain.terrainData.size.z);

        var terrainData = terrain.terrainData;
        
        // TerrainData terrainData = new TerrainData
        // {
        //     heightmapResolution = width + 1,
        //     size = new Vector3(width, 1, height)
        // };
        //
        // float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        // terrainData.SetHeights(0, 0, heights);
        //
        // terrain.terrainData   = terrainData;
        //
        // if (terrainTexture)
        // {
        //     TerrainLayer[] layers = new TerrainLayer[sqrtLayerResolution * sqrtLayerResolution];
        //
        //     for (int i = 0; i < sqrtLayerResolution * sqrtLayerResolution; i++)
        //     {
        //         layers[i] = new TerrainLayer
        //         {
        //             diffuseTexture = terrainTexture,
        //             tileSize = new Vector2(width / sqrtLayerResolution, height / sqrtLayerResolution) // Dzielenie na 9 kwadrat�w
        //         };
        //     }
        //
        //     terrainData.terrainLayers = layers;
        //     
        //     float[,,] alphaMap = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, sqrtLayerResolution * sqrtLayerResolution];
        //     int cellSizeX = terrainData.alphamapWidth / sqrtLayerResolution;
        //     int cellSizeY = terrainData.alphamapHeight / sqrtLayerResolution;
        //
        //     for (int x = 0; x < terrainData.alphamapWidth; x++)
        //     {
        //         for (int y = 0; y < terrainData.alphamapHeight; y++)
        //         {
        //             int textureIndex = (x / cellSizeX) + (y / cellSizeY) * sqrtLayerResolution;
        //             for (int i = 0; i < sqrtLayerResolution * sqrtLayerResolution; i++)
        //             {
        //                 alphaMap[x, y, i] = (i == textureIndex) ? 1f : 0f; // Ka�dy kwadrat trawy ma swoj� warstw�
        //             }
        //         }
        //     }
        //
        //     terrainData.SetAlphamaps(0, 0, alphaMap);
        // }
        // else
        // {
        //     Debug.LogError("Musisz przypisa� tekstur� do 'terrainTexture'!");
        // }
        //

        for (int x = 0; x < terrainData.size.x; x += buildingArea)
        {
            for (int z = 0; z < terrainData.size.z; z += buildingArea)
            {
                Vector3 position = new Vector3(transform.position.x -width/2 + x + buildingArea / 2, 0, transform.position.z*2 -height/2 + z + buildingArea / 2);

                int randomIndex = Random.Range(0, 100); // potrzebny lepszy system losowanie z wieloma warunkami
                GameObject result = randomIndex switch
                {
                    int n when (n >= 0 && n <= 3) => Instantiate(Buildings[0], position, Quaternion.identity, terrain.transform),
                    int n when (n >= 4 && n <= 13) => Instantiate(Buildings[1], position, Quaternion.identity, terrain.transform),
                    _ => null
                };
                
            }
        }
    }
}

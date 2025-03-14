using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Game Manager starting");
        Application.targetFrameRate = 60;
        
        TerrainGenerator.Instance.GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

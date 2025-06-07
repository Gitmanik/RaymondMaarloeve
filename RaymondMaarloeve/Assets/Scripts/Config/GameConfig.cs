using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class GameConfig 
{   
    public int Revision; 
    public string LlmServerApi;     
    public bool FullScreen; 
    public int GameWindowWidth; 
    public int GameWindowHeight; 
    public List<ModelConfig> Models; 
    public List<NpcConfig> Npcs;

    public int NarratorModelId;
    
    private GameConfig() { }
    
    public static GameConfig LoadGameConfig(string configPath)
    {   
        var gameConfig = new GameConfig();
        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            gameConfig = JsonUtility.FromJson<GameConfig>(json);

            if (gameConfig.Revision == 1)
            {
                Debug.Log("Game configuration loaded successfully.");
                return gameConfig;
            }
            Debug.LogWarning("Unsupported Configuration file. Make sure you are using the latest version of the Game and Launcher.");
        }
        else
        {
            Debug.LogError($"Configuration file not found at path: {configPath}");
        }
        
        gameConfig = new GameConfig
        {
            Revision = 1,
            LlmServerApi = "http://127.0.0.1:5000/",
            FullScreen = false,
            GameWindowWidth = 1920,
            GameWindowHeight = 1080,
            Models = new List<ModelConfig>
            {
            },
            Npcs = new List<NpcConfig>
            {
                new NpcConfig { Id = 1, ModelId = -1 },
                new NpcConfig { Id = 2, ModelId = -1 },
                new NpcConfig { Id = 3, ModelId = -1 },
                new NpcConfig { Id = 4, ModelId = -1 },
                new NpcConfig { Id = 5, ModelId = -1 },
                new NpcConfig { Id = 6, ModelId = -1 },
                new NpcConfig { Id = 7, ModelId = -1 },
            },
            NarratorModelId = -1
        };

        return gameConfig;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Represents the configuration for the game, including settings for the LLM server, window dimensions, models, and NPCs.
/// This configuration is provided by the game launcher.
/// </summary>
[Serializable]
public class GameConfig 
{   
    /// <summary>
    /// Revision number of the configuration file.
    /// </summary>
    public int Revision; 
    /// <summary>
    /// API endpoint for the LLM server.
    /// </summary>
    public string LlmServerApi;     
    /// <summary>
    /// Indicates whether the game should run in fullscreen mode.
    /// </summary>
    public bool FullScreen; 
    /// <summary>
    /// Width of the game window.
    /// </summary>
    public int GameWindowWidth; 
    /// <summary>
    /// Height of the game window.
    /// </summary>
    public int GameWindowHeight; 
    /// <summary>
    /// List of model configurations used in the game.
    /// </summary>
    public List<ModelConfig> Models; 
    /// <summary>
    /// List of NPC configurations used in the game.
    /// </summary>
    public List<NpcConfig> Npcs;

    /// <summary>
    /// ID of the model used by the narrator.
    /// </summary>
    public int NarratorModelId;
    
    /// <summary>
    /// Private constructor to prevent direct instantiation.
    /// </summary>
    private GameConfig() { }
    
    /// <summary>
    /// Loads the game configuration from a JSON file at the specified path.
    /// If the file does not exist or is invalid, a default configuration is returned.
    /// </summary>
    /// <param name="configPath">Path to the configuration file.</param>
    /// <returns>A GameConfig object containing the loaded or default configuration.</returns>
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
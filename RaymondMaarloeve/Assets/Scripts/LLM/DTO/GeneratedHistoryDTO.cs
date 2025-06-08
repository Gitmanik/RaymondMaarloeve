using System;
using System.Collections.Generic;

[Serializable]
/// <summary>
/// DTO class storing generated story with characters from Narrator LLM derived from <see cref="GameManager::GenerateHistory"/> prompt
/// </summary>
public class GeneratedHistoryDTO
{
    /// <summary>
    /// Generated story
    /// </summary>
    public string story;
    
    /// <summary>
    /// List of generated characters stored in <see cref="CharacterDTO"/> objects
    /// </summary>
    public List<CharacterDTO> characters;

    /// <summary>
    /// ToString override used for pretty printing
    /// </summary>
    /// <returns>Pretty print of the object containing all information about the Story</returns>
    public override string ToString() => $"Generated History:\nStory: {story}\nCharacters:\n{string.Join('\n', characters)}";
}

[Serializable]
/// <summary>
/// DTO class used to describe generated Character in <see cref="GeneratedHistoryDTO"/> object.
/// </summary>
public class CharacterDTO
{
    /// <summary>
    /// Name of the Character
    /// </summary>
    public string name;
    
    /// <summary>
    /// LLM Model used by Character 
    /// </summary>
    public string archetype;
    
    /// <summary>
    /// Age of the Character
    /// </summary>
    public int age;
    
    /// <summary>
    /// System prompt of the Character
    /// </summary>
    public string description;

    /// <summary>
    /// ToString override used for pretty printing
    /// </summary>
    /// <returns>Pretty print of the object containing all information about the Character</returns>
    public override string ToString() => $"Character: Name: '{name}',\nArchetype: '{archetype}',\nAge: '{age}',\nDescription: '{description}'";
}

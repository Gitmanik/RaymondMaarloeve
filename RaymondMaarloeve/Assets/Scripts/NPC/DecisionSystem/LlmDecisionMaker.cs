using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Decision-making system for NPCs that uses a Large Language Model (LLM) to determine the next action based on the current environment and NPC state.
/// This system interacts with the LLMServer to request decisions and processes the responses to guide NPC behavior.
/// </summary>
public class LlmDecisionMaker : IDecisionSystem
{
    /// <summary>
    /// The NPC that this decision-making system is associated with.
    /// </summary>
    private NPC npc;
    
    /// <summary>
    /// Represents the response from the LLM that is currently being processed.
    /// This response contains the NPC's next action as determined by the LLM based on the current environment and NPC state.
    /// </summary>
    private ChatResponseDTO waitingResponse = null;

    /// <summary>
    /// Represents the current environment that the NPC is aware of.
    /// This list is populated with objects that the NPC can interact with or consider when making decisions.
    /// </summary>
    private List<CurrentEnvironment> currentEnvironment;

    /// <summary>
    /// Sets up the decision-making system with the provided NPC.
    /// </summary>
    /// <param name="npc">The NPC that will use this decision-making system.</param>

    public void Setup(NPC npc)
    {
        this.npc = npc;
    }

    /// <summary>
    /// Decides the NPC's next action based on the current state and LLM responses.
    /// </summary>
    /// <returns>An implementation of <see cref="IDecision"/> representing the NPC's next action.</returns>
    public IDecision Decide()
    {
      if (!GameManager.Instance.LlmServerReady)
        return new WaitForLLMReadyDecision();

      if (waitingResponse != null)
      {
        var decision = ParseDecision(waitingResponse);
        waitingResponse = null;
        return decision;
      }
      else
      {
        RequestResponse();
        return new WaitForLLMDecision();
      }
    }

    /// <summary>
    /// Requests a response from the LLMServer to determine the NPC's next action.
    /// </summary>
    private void RequestResponse()
    {
      var currentConversation = new List<Message>();
        
      var dto = new IdleDTO();
      dto.core_memories = npc.SystemPrompt.Split('.').ToList().ConvertAll(x => x.Trim());
      
      dto.needs = new List<NeedDTO>()
      {
        new NeedDTO { need = "hunger", weight = (int) npc.Hunger },
        new NeedDTO { need = "thirst", weight = (int) npc.Thirst }
      };
      dto.stopped_action = npc.StoppedDecision == null ? IdleDecision.RandomPrettyName : npc.StoppedDecision.PrettyName;

      currentEnvironment = npc.GetCurrentEnvironment();
      List<CurrentEnvironmentDTO> currentEnvironmentDtos = currentEnvironment.ConvertAll(x => x.ToDTO(npc));
      dto.current_environment = currentEnvironmentDtos;
      
      dto.obtained_memories = npc.ObtainedMemories.ConvertAll(x => x.ToDTO());;

      string prompt =
        $"It it currently {DayNightCycle.Instance.GetCurrentTimeText()}, day {DayNightCycle.Instance.GetCurrentDay()}.\n" +
        $"Take needs into account.\n" +
        $"What should {npc.NpcName} do now? Choose from CurrentEnvironment.\n" +
        $"Respond ONLY with the action index (1-{dto.current_environment.Count}).";
      
      var content = JsonUtility.ToJson(dto);
      currentConversation.Add(new Message { role = "system", content = content});
      currentConversation.Add(new Message { role = "user", content = prompt});
      
      LlmManager.Instance.Chat(
        npc.ModelID,
        currentConversation,
        (response) =>
        {
          waitingResponse = response;
          (npc.GetCurrentDecision() as WaitForLLMDecision).Ready = true;
        },
        OnChatError
      );
    }

    /// <summary>
    /// Parses the decision received from the LLM and maps it to an appropriate NPC action.
    /// </summary>
    /// <param name="chatResponseDto">The response from the LLM.</param>
    /// <returns>An implementation of <see cref="IDecision"/> representing the parsed action.</returns>
    private IDecision ParseDecision(ChatResponseDTO chatResponseDto)
    {
      string result = Regex.Replace(chatResponseDto.response, @"\D*(\d+)\D*", "$1");


      if (int.TryParse(result, out int response) == false)
      {
        Debug.LogError($"{npc.NpcName}: Idle response error: invalid response: {chatResponseDto.response}");
        return new IdleDecision();
      }

      if (response < 0 || response > currentEnvironment.Count)
      {
        Debug.LogError($"{npc.NpcName}: Idle response error: index out of bounds: {chatResponseDto.response}");
        return new IdleDecision();
      }
      
      var action = currentEnvironment[response - 1];
      Debug.Log($"{npc.NpcName}: Idle response, selected action: {action.decision.DebugInfo()} (result: {result})");

      return action.decision;
    }
    
    /// <summary>
    /// Logs an error in case of a failure during the chat process with the LLM.
    /// </summary>
    /// <param name="error">The error message.</param>
    private void OnChatError(string error)
    {
      Debug.LogError($"Idle error: {error}");
    }

    /// <summary>
    /// Requests a relevance value from the NPC's LLM model.
    /// </summary>
    /// <param name="newMemory">New obtained memory to consider</param>
    /// <param name="relevanceFunc">Delegate which will be called when the value is calculated.</param>
    public void CalculateRelevance(string newMemory, Action<int> relevanceFunc)
    {
      string prompt = @"
You are a memory analysis model in a mystery narrative game.
Your task is to assign a Relevance score (1–10) to a newly obtained memory.

Definitions:
""core_memories"": Permanent, unchanging truths or world context. Always available.
""obtained_memories"": Episodic memories discovered over time

Relevance (last value in the weight triplet) measures how strongly the new memory connects to the central mystery and existing information. It is not a measure of importance in isolation — that is covered by the Importance score.

Instructions:

1. Carefully analyze the new_memory.
2. Compare its content with:
   - core_memories
   - obtained_memories
3. If the new memory connects directly or indirectly to one or more existing memories or core truths (same symbols, items, locations, actions, or implications), assign a higher Relevance.
   - Strong connections (shared specific items, rituals, locations, etc.): Relevance 7–10.
   - Moderate thematic or suggestive links: Relevance 4–6.
   - Weak or no clear links to existing data: Relevance 1–3.

If the new_memory clearly connects to one or more obtained_memories, increase the Relevance of those memories accordingly.

Updated Relevance values must never exceed 10.

Output format must be **EXACTLY AND ONLY** an integer. Do not explain your reasoning. Do not include anything else.
";
      
      var currentConversation = new List<Message>();
        
      var dto = new CalculateRelevanceDTO();
      dto.core_memories = npc.SystemPrompt.Split('.').ToList().ConvertAll(x => x.Trim());
      dto.obtained_memories = npc.ObtainedMemories.ConvertAll(x => x.ToDTO());
      dto.new_memory = newMemory;
      
      var dtoJson = JsonUtility.ToJson(dto);
      
      currentConversation.Add(new Message { role = "system", content = prompt});
      currentConversation.Add(new Message { role = "user", content = dtoJson});
      
      // Debug.Log($"Calculating relevance:\n{dtoJson}");
      
      LlmManager.Instance.Chat(
        npc.ModelID,
        currentConversation,
        (response) =>
        {
          string result = Regex.Replace(response.response, @"\D*(\d+)\D*", "$1");
          Debug.Log($"{npc.NpcName}: Relevance response: '{result}'");
          
          int relevance = 5;
          if (!int.TryParse(result, out relevance))
            Debug.LogWarning($"Wrong relevance response: '{response.response}'");
          relevanceFunc(relevance);
        },
        OnChatError
      );
      
      
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LlmDecisionMaker : IDecisionSystem
{
    private NPC npc;
    
    private ChatResponseDTO waitingResponse = null;

    private IdleDTO idleDto = null;

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
      dto.stopped_action = "";
      dto.current_environment = npc.GetCurrentEnvironment();
      dto.obtained_memories = npc.ObtainedMemories;
      
      idleDto = dto;

      var content = JsonUtility.ToJson(dto);
      currentConversation.Add(new Message { role = "system", content = content});
      
      // TODO: Read options from key -> Decision system
      currentConversation.Add(new Message { role = "user", content = $"What should {npc.NpcName} do now? Choose from CurrentEnvironment. Respond ONLY with the action index."});
      
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
      Debug.Log($"Idle response, content: ({chatResponseDto.response})");

      if (int.TryParse(chatResponseDto.response, out int response) == false)
      {
        Debug.LogError($"Idle response, invalid response: {chatResponseDto.response}");
        return new IdleDecision();
      }
      
      var action = idleDto.current_environment[response - 1].action;
      
      Debug.Log($"Idle response, action: {action}");

      if (action.Contains("idle"))
        return new IdleDecision();
      if (action.Contains("walk"))
        return new WalkDecision();
      if (action.Contains("buy goods"))
        return new PrayDecision(); // TODO: Change to BuyGoodsDecision when Market building is added
      if (action.Contains("get water"))
        return new GetWaterDecision();
      if (action.Contains("pray"))
        return new PrayDecision();
      if (action.Contains("get ale"))
        return new GetAleDecision();
      
      return new IdleDecision();
    }
    
    /// <summary>
    /// Logs an error in case of a failure during the chat process with the LLM.
    /// </summary>
    /// <param name="error">The error message.</param>
    private void OnChatError(string error)
    {
      Debug.LogError($"Idle error: {error}");
    }

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
      dto.obtained_memories = npc.ObtainedMemories;
      dto.new_memory = newMemory;
      
      var dtoJson = JsonUtility.ToJson(dto);
      
      currentConversation.Add(new Message { role = "system", content = prompt});
      currentConversation.Add(new Message { role = "user", content = dtoJson});
      
      Debug.Log($"Calculating relevance:\n{dtoJson}");
      
      LlmManager.Instance.Chat(
        npc.ModelID,
        currentConversation,
        (response) =>
        {
          Debug.Log($"Relevance response: '{response.response}'");
          int relevance = 5;
          if (!int.TryParse(response.response, out relevance))
            Debug.LogWarning($"Wrong relevance response: '{response.response}'");
          relevanceFunc(relevance);
        },
        OnChatError
      );
      
      
    }
}
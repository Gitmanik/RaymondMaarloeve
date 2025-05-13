using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LlmDecisionMaker : IDecisionSystem
{
    private NPC npc;
    
    private ChatResponseDTO waitingResponse = null;

    private IdleDTO idleDto = null;

    public void Setup(NPC npc)
    {
        this.npc = npc;
    }

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
    public string GetNPCName()
    {
        return npc.NpcName;
    }

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
      dto.current_environment = new List<CurrentEnvironmentDTO>()
      {
        new CurrentEnvironmentDTO("Stand by the chapel steps, unmoving (pray)", 2),
        new CurrentEnvironmentDTO("Sit upright beneath a broken statue (walk)", 1),
        new CurrentEnvironmentDTO("Gaze at the river without blinking (idle)", 3),
        new CurrentEnvironmentDTO("Walk slowly through the square without speaking (walk)", 2),
        new CurrentEnvironmentDTO("Trace the burned symbol on his ring (walk)", 1),
        new CurrentEnvironmentDTO("Watch birds scatter in the market (buy goods)", 2)
      };
      dto.obtained_memories = new List<ObtainedMemoryDTO>()
      {
        new ObtainedMemoryDTO
        {
          memory = "A child asked what the sigil on my ring was. I told him it meant 'nothing'.",
          weight = 21
        },
        new ObtainedMemoryDTO
        {
          memory = "A merchant mentioned my house name, then spat. I didn’t blink.",
          weight = 20
        },
        new ObtainedMemoryDTO
        {
          memory = "Saw my old banner being used to wrap fish. I said nothing. But I watched.",
          weight = 23
        },
        new ObtainedMemoryDTO
        {
          memory = "Heard someone say I should’ve been executed too. He’s not wrong.",
          weight = 22
        },
        new ObtainedMemoryDTO
        {
          memory = "A drunken man bowed to me by mistake. For a moment, I let him.",
          weight = 18
        },
        new ObtainedMemoryDTO
        {
          memory = "Someone asked if I’d return home. I said ‘Home is gone.’",
          weight = 19
        }
      };
      
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
    
    private void OnChatError(string error)
    {
      Debug.LogError($"Idle error: {error}");
    }
}
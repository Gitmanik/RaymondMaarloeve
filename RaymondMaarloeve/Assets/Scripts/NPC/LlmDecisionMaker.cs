using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class LlmDecisionMaker : IDecisionSystem
{
    private NPC npc;
    private IDecision cachedDecision = new IdleDecision();
    private string npcName = "LLM NPC";
    private string serverUrl = "http://localhost:5000";
    private string modelId = "npc_model";

    public void Setup(NPC npc)
    {
        this.npc = npc;
        npcName = npc.name;

        npc.StartCoroutine(UpdateDecisionLoop());
    }

    public IDecision Decide()
    {
        return cachedDecision;
    }

    public string GetNPCName()
    {
        return npcName;
    }

    private IEnumerator UpdateDecisionLoop()
    {
        while (true)
        {
            yield return UpdateDecision();
            yield return new WaitForSeconds(5f);
        }
    }

    private IEnumerator UpdateDecision()
    {
        string prompt = $"What should {npcName} do now? Options: idle, walk, talk to another npc, talk to player, buy goods, get water, pray, get ale.";

        var requestBody = new
        {
            model_id = modelId,
            prompt = prompt,
            max_tokens = 50,
            temperature = 0.7,
            top_p = 0.9
        };

        string jsonData = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest($"{serverUrl}/predict", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($"LLM decision error: {request.error}");
            }
            else
            {
                var responseText = request.downloadHandler.text;
                PredictResponse response = JsonUtility.FromJson<PredictResponse>(responseText);

                cachedDecision = ParseDecision(response.response);
            }
        }
    }

    private IDecision ParseDecision(string response)
    {
        response = response.ToLower();

        if (response.Contains("idle"))
            return new IdleDecision();
        if (response.Contains("walk"))
            return new WalkDecision();
        if (response.Contains("talk to another npc"))
            return new NPCConversationDecision();
        if (response.Contains("talk to player"))
            return new PlayerConversationDecision();
        if (response.Contains("buy goods"))
            return new BuyGoodsDecision();
        if (response.Contains("get water"))
            return new GetWaterDecision();
        if (response.Contains("pray"))
            return new PrayDecision();
        if (response.Contains("get ale"))
            return new GetAleDecision();

        return new IdleDecision();
    }

    [System.Serializable]
    private class PredictResponse
    {
        public string response;
        public object raw;
    }
}

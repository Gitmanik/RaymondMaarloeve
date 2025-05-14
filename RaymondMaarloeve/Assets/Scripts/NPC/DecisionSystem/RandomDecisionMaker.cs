using UnityEngine;

public class RandomDecisionMaker : IDecisionSystem
{
    private NPC npc;

    public void Setup(NPC npc)
    {
        this.npc = npc;
    }

    public IDecision Decide()
    {
        int decision = Random.Range(0, 7);

        switch (decision)
        {
            case 0:
                return new IdleDecision();
            case 1:
                return new WalkDecision();
            case 2:
                return new NPCConversationDecision();
            case 3:
                return new PlayerConversationDecision();
            case 4:
                return new BuyGoodsDecision();
            case 5:
                return new GetWaterDecision();
            case 6:
                return new PrayDecision();
            case 7:
                return new GetAleDecision();
            default:
                return new IdleDecision();
        }
    }

    public string GetNPCName()
    {
        return "Random NPC name";
    }

    public NPC GetNPC()
    {
        return npc;
    }
}

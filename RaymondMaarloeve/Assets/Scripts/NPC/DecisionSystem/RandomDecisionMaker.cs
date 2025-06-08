/**
 * @file RandomDecisionMaker.cs
 * @brief Implements a decision system that picks a random action for an NPC.
 */

using UnityEngine;

/**
 * @class RandomDecisionMaker
 * @brief Chooses an NPC's next action at random from a set of available decisions.
 * 
 * Implements the IDecisionSystem interface.
 */
public class RandomDecisionMaker : IDecisionSystem
{
    /** @brief Reference to the NPC using this system. */
    private NPC npc;

    /**
     * @brief Initializes the decision system with the given NPC.
     * @param npc The NPC that will use this decision-maker.
     */
    public void Setup(NPC npc)
    {
        this.npc = npc;
    }

    /**
     * @brief Selects and returns a random decision for the NPC.
     * @returns An object implementing IDecision representing the chosen action.
     */
    public IDecision Decide()
    {
        int decision = Random.Range(0, 8);  // 0–7 inclusive

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
}

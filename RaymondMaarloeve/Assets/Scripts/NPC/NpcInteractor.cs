using UnityEngine;
using TMPro;

/// <summary>
/// Allows the player to interact with NPCs by displaying a prompt and starting a conversation.
/// Shows a message when near an NPC and hides it when talking or walking away.
/// </summary>
public class NpcInteractor : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the player is in range to talk to an NPC.
    /// </summary>
    private bool canTalk = false;

    /// <summary>
    /// The name of the currently targeted NPC.
    /// </summary>
    private string npcName = "";

    /// <summary>
    /// The currently targeted NPC GameObject.
    /// </summary>
    private GameObject currentNpc;

    /// <summary>
    /// Reference to the UI GameObject that displays the interaction text.
    /// </summary>
    public GameObject interactText;

    /// <summary>
    /// Reference to the TextMeshProUGUI component that holds the interaction message.
    /// </summary>
    public TextMeshProUGUI interactTextComponent;

    /// <summary>
    /// Indicates whether the player is currently in a conversation with an NPC.
    /// </summary>
    private bool isTalking = false;

    /// <summary>
    /// Called every frame to check for interaction input (E) and exit input (Escape).
    /// </summary>
    void Update()
    {
        if (canTalk && !isTalking && Input.GetKeyDown(KeyCode.E))
        {
            isTalking = true;
            interactText.SetActive(false);
        }

        if (isTalking && Input.GetKeyDown(KeyCode.Escape))
        {
            isTalking = false;
            if (canTalk)
                interactText.SetActive(true);
        }
    }

    /// <summary>
    /// Triggered when the player enters the collider of an NPC.
    /// Displays the interaction prompt with the NPC's name.
    /// </summary>
    /// <param name="other">The collider the player entered.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            var id = other.GetComponent<NpcIdentity>();
            if (id != null)
            {
                npcName = id.npcName;
                currentNpc = other.gameObject;
                canTalk = true;

                if (!isTalking)
                {
                    interactTextComponent.text = $"Press E to talk to {npcName}";
                    interactText.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// Triggered when the player exits the collider of an NPC.
    /// Hides the interaction prompt and clears references.
    /// </summary>
    /// <param name="other">The collider the player exited.</param>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC") && other.gameObject == currentNpc)
        {
            canTalk = false;
            currentNpc = null;
            interactText.SetActive(false);
        }
    }
}

using UnityEngine;
using TMPro;

public class NpcInteractor : MonoBehaviour
{
    private bool canTalk = false;
    private string npcName = "";
    private GameObject currentNpc;

    public GameObject interactText;
    public TextMeshProUGUI interactTextComponent;

    private bool isTalking = false;

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

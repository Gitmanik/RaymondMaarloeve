using UnityEngine;

[RequireComponent(typeof(NPC))]
public class NpcIdentity : MonoBehaviour
{
    public string npcName = "Unnamed";

    void Start()
    {
        NPC npc = GetComponent<NPC>();
        if (npc != null)
        {
            npcName = npc.NpcName;
        }
        else
        {
            Debug.LogWarning("NpcIdentity: Brak komponentu NPC przy obiekcie " + gameObject.name);
        }
    }
}

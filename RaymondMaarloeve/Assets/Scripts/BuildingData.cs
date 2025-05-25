using UnityEngine;

public class BuildingData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Tile HisTile = null;
    public NPC HisNPC = null;
    public BuildingType HisType = BuildingType.None;
    public enum BuildingType
    {
        None,
        House,
        Church,
        Well,
        Blacksmith,
        Tavern,
        Scaffold,
        Other
    }
}

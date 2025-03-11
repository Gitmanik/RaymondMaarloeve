using Gitmanik.BaseCode;
using TMPro;
using UnityEngine;

public class BuildInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text buildInfoText;
    
    void Start()
    {
        buildInfoText.text = BuildInfo.Instance.BuildDate;
    }
}

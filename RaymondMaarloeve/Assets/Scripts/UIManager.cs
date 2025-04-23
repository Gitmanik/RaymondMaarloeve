using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Dialog Box")]
    [SerializeField] private GameObject dialogBoxParent;

    [Header("Dialog Box Text Input Field")]
    [SerializeField] private TMP_InputField dialogInputField;

    private void Awake()
    {
        // singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ensure it is hidden at the start
        HideDialogBox();
    }

    public void ShowDialogBox()
    {
        dialogBoxParent.SetActive(true);
        dialogInputField.ActivateInputField();
    }

    public void HideDialogBox()
    {
        dialogBoxParent.SetActive(false);
    }

    private void OnEnable()
    {
        // subscribe to the callback
        dialogInputField.onEndEdit.AddListener(OnDialogInputEndEdit);
    }

    private void OnDisable()
    {
        // unsubscribe to avoid memory leaks
        dialogInputField.onEndEdit.RemoveListener(OnDialogInputEndEdit);
    }

    private void OnDialogInputEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessDialogInput(text);

            dialogInputField.text = "";
            dialogInputField.ActivateInputField();
        }
    }

    private void ProcessDialogInput(string input)
    {
        Debug.Log("Player entered and confirmed: " + input);

        // Place for processing the text
        // e.g., pass the text to an NPC / LLM / PlayerController, etc.:
        // PlayerController.Instance.SendMessageToNPC(input);
    }
}

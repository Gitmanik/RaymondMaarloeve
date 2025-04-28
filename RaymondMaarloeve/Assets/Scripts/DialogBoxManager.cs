using UnityEngine;
using TMPro;

public class DialogBoxManager : MonoBehaviour
{
    public static DialogBoxManager Instance { get; private set; }



    [Header("Dialog Box")]
    [SerializeField] private GameObject dialogBoxParent;

    [Header("Dialog Box Text Input Field")]
    [SerializeField] private TMP_InputField dialogInputField;

    [Header("Dialog Box Text Output Field")]
    [SerializeField] private TMP_Text npcResponseText;

    [Header("Dialog Box Npc Name Field")]
    [SerializeField] private TMP_Text npcNameText;

    private bool waitingForNpcDismiss = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        dialogBoxParent.SetActive(false);
    }

    private void OnEnable()
    {
        dialogInputField.onSubmit.AddListener(OnDialogInputSubmit);
    }

    private void OnDisable()
    {
        dialogInputField.onSubmit.RemoveListener(OnDialogInputSubmit);
    }

    private void OnDialogInputSubmit(string text)
    {
        ProcessDialogInput(text);
    }

    private void ProcessDialogInput(string input)
    {
        Debug.Log("Player entered and confirmed: " + input);
        dialogInputField.gameObject.SetActive(false);
        npcResponseText.text = GenerateResponse(input);
        npcResponseText.gameObject.SetActive(true);
        StartCoroutine(WaitForDismiss());
    }
    private System.Collections.IEnumerator WaitForDismiss()
    {
        // Wait for a short time before allowing the player to dismiss the dialog box
        // Without this, system dismisses the dialog box immediately after the NPC response is shown
        yield return new WaitForSeconds(0.5f);
        waitingForNpcDismiss = true;
    }

    private string GenerateResponse(string input)
    {
        // Here you can implement your logic to generate a response based on the input.
        // For now, we will just echo the input back to the player.
        return"You said " + input + " and I agree!" + "\nPress Enter to continue...";
    }

    private void Update()
    {
        if (waitingForNpcDismiss && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Dismissed the dialog box.");
            npcResponseText.gameObject.SetActive(false);

            dialogInputField.text = "";
            dialogInputField.gameObject.SetActive(true);
            dialogInputField.ActivateInputField();

            waitingForNpcDismiss = false;
        }
    }


    public void ShowDialogBox()
    {
        dialogBoxParent.SetActive(true);
        npcResponseText.gameObject.SetActive(false);

        dialogInputField.text = "";

        // Check if PlayerController.Instance and currentlyInteractingNPC are not null
        if (PlayerController.Instance != null && PlayerController.Instance.currentlyInteractingNPC != null)
        {
            npcNameText.text = PlayerController.Instance.currentlyInteractingNPC.npcName;
        }
        else
        {
            Debug.LogError("PlayerController.Instance or currentlyInteractingNPC is null!");
            npcNameText.text = "Unknown NPC"; // Fallback text
        }

        dialogInputField.gameObject.SetActive(true);
        dialogInputField.ActivateInputField();
    }

    public void HideDialogBox()
    {
        dialogBoxParent.SetActive(false);
        waitingForNpcDismiss = false;
    }
}

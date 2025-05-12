using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogBoxManager : MonoBehaviour
{
    public static DialogBoxManager Instance { get; private set; }
    private List<Message> currentConversation = new List<Message>();



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

        // Add player's message to conversation
        currentConversation.Add(new Message { role = "user", content = input });

        // Send to LLM and get response
        if (GameManager.Instance.LlmServerReady)
        {
            Debug.Log("Sending to LLM...");
            LlmManager.Instance.Chat(
                PlayerController.Instance.currentlyInteractingNPC.ModelID,
                currentConversation,
                OnChatResponse,
                OnChatError
            );
        }
        else
        {
            Debug.LogError("LLM Manager is not connected!");
            npcResponseText.text = "Sorry, I cannot respond right now.\nPress Enter to continue...";
            npcResponseText.gameObject.SetActive(true);
            StartCoroutine(WaitForDismiss());
        }
    }
    

    private void OnChatResponse(ChatResponseDTO response)
    {
        // Add AI's response to conversation history
        currentConversation.Add(new Message { role = "assistant", content = response.response });
        
        Debug.Log($"Chat response, took {response.generation_time}, total_tokens: {response.total_tokens}: {response.response}");
        
        npcResponseText.text = response.response + "\nPress Enter to continue...";
        npcResponseText.gameObject.SetActive(true);
        StartCoroutine(WaitForDismiss());
    }

    private void OnChatError(string error)
    {
        Debug.LogError($"Chat error: {error}");
        npcResponseText.text = "Sorry, I'm having trouble responding.\nPress Enter to continue...";
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

        // Clear previous conversation and add system prompt
        currentConversation.Clear();
        currentConversation.Add(new Message { role = "system", content = PlayerController.Instance.currentlyInteractingNPC.SystemPrompt });

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
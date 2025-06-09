using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the dialog box UI for player interactions with NPCs.
/// Handles input, output, and communication with the LLM server.
/// </summary>
public class DialogBoxManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the DialogBoxManager class.
    /// </summary>
    public static DialogBoxManager Instance { get; private set; }

    /// <summary>
    /// List of messages forming the current conversation.
    /// </summary>
    private List<Message> currentConversation = new List<Message>();

    /// <summary>
    /// Parent GameObject for the dialog box UI.
    /// </summary>
    [Header("Dialog Box")]
    [SerializeField] private GameObject dialogBoxParent;

    /// <summary>
    /// Input field for player dialog text.
    /// </summary>
    [Header("Dialog Box Text Input Field")]
    [SerializeField] private TMP_InputField dialogInputField;

    /// <summary>
    /// Text field for displaying NPC responses.
    /// </summary>
    [Header("Dialog Box Text Output Field")]
    [SerializeField] private TMP_Text npcResponseText;

    /// <summary>
    /// Text field for displaying the NPC's name.
    /// </summary>
    [Header("Dialog Box Npc Name Field")]
    [SerializeField] private TMP_Text npcNameText;

    /// <summary>
    /// Indicates whether the dialog box is waiting for the player to dismiss the NPC's response.
    /// </summary>
    private bool waitingForNpcDismiss = false;

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    /// <summary>
    /// Sets the dialog box to inactive at the start.
    /// </summary>
    private void Start()
    {
        dialogBoxParent.SetActive(false);
    }

    /// <summary>
    /// Adds a listener for dialog input submission when the dialog box is enabled.
    /// </summary>
    private void OnEnable()
    {
        dialogInputField.onSubmit.AddListener(OnDialogInputSubmit);
    }

    /// <summary>
    /// Removes the listener for dialog input submission when the dialog box is disabled.
    /// </summary>
    private void OnDisable()
    {
        dialogInputField.onSubmit.RemoveListener(OnDialogInputSubmit);
    }

    /// <summary>
    /// Handles dialog input submission.
    /// </summary>
    /// <param name="text">The text entered by the player.</param>
    private void OnDialogInputSubmit(string text)
    {
        ProcessDialogInput(text);
    }

    /// <summary>
    /// Processes the player's dialog input and sends it to the LLM server.
    /// </summary>
    /// <param name="input">The text entered by the player.</param>
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
    

    /// <summary>
    /// Handles the response from the LLM server.
    /// Updates the NPC response text and waits for player dismissal.
    /// </summary>
    /// <param name="response">The response from the LLM server.</param>
    private void OnChatResponse(ChatResponseDTO response)
    {
        // Add AI's response to conversation history
        currentConversation.Add(new Message { role = "assistant", content = response.response });
        
        Debug.Log($"Chat response, took {response.generation_time}, total_tokens: {response.total_tokens}: {response.response}");
        
        npcResponseText.text = response.response + "\nPress Enter to continue...";
        npcResponseText.gameObject.SetActive(true);
        StartCoroutine(WaitForDismiss());
    }

    /// <summary>
    /// Handles errors from the LLM server.
    /// Displays an error message in the NPC response text.
    /// </summary>
    /// <param name="error">The error message from the LLM server.</param>
    private void OnChatError(string error)
    {
        Debug.LogError($"Chat error: {error}");
        npcResponseText.text = "Sorry, I'm having trouble responding.\nPress Enter to continue...";
        npcResponseText.gameObject.SetActive(true);
        StartCoroutine(WaitForDismiss());
    }

    /// <summary>
    /// Waits for a short time before allowing the player to dismiss the dialog box.
    /// </summary>
    /// <returns>An enumerator for coroutine execution.</returns>
    private System.Collections.IEnumerator WaitForDismiss()
    {
        // Wait for a short time before allowing the player to dismiss the dialog box
        // Without this, system dismisses the dialog box immediately after the NPC response is shown
        yield return new WaitForSeconds(0.5f);
        waitingForNpcDismiss = true;
    }
    
    
    /// <summary>
    /// Updates the dialog box state every frame.
    /// Handles player dismissal of the NPC's response.
    /// </summary>
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


    /// <summary>
    /// Activates the dialog box and initializes the conversation.
    /// </summary>
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
            npcNameText.text = PlayerController.Instance.currentlyInteractingNPC.NpcName;
        }
        else
        {
            Debug.LogError("PlayerController.Instance or currentlyInteractingNPC is null!");
            npcNameText.text = "Unknown NPC"; // Fallback text
        }

        dialogInputField.gameObject.SetActive(true);
        dialogInputField.ActivateInputField();
    }

    /// <summary>
    /// Deactivates the dialog box and resets its state.
    /// </summary>
    public void HideDialogBox()
    {
        dialogBoxParent.SetActive(false);
        waitingForNpcDismiss = false;
    }
}
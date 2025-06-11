using UnityEngine;

/// <summary>
/// Handles activation of a minigame when the player approaches a trigger and presses 'E'.
/// Displays an interaction prompt and toggles the minigame UI panel.
/// </summary>
public class MinigameActivator : MonoBehaviour
{
    /// <summary>
    /// Determines if the player is within range to activate the minigame.
    /// </summary>
    private bool canActivate = false;

    /// <summary>
    /// Reference to the UI text GameObject that displays "Press E".
    /// </summary>
    [Tooltip("UI text 'Press E'")]
    public GameObject interactText;

    /// <summary>
    /// Reference to the minigame UI panel to be shown when the game starts.
    /// </summary>
    [Tooltip("Minigame panel")]
    public GameObject miniGamePanel;

    /// <summary>
    /// Called once per frame. 
    /// Checks if the player can activate the minigame and has pressed the 'E' key.
    /// </summary>
    void Update()
    {
        if (canActivate && Input.GetKeyDown(KeyCode.E))
        {
            if (interactText != null)
                interactText.SetActive(false);

            if (miniGamePanel != null)
                miniGamePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Called when another collider enters this object's trigger.
    /// Activates interaction prompt if the collider is tagged "StartMinigame".
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StartMinigame"))
        {
            canActivate = true;

            if (interactText != null)
                interactText.SetActive(true);
        }
    }

    /// <summary>
    /// Called when another collider exits this object's trigger.
    /// Hides the interaction prompt if the collider was tagged "StartMinigame".
    /// </summary>
    /// <param name="other">The collider that exited the trigger.</param>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StartMinigame"))
        {
            canActivate = false;

            if (interactText != null)
                interactText.SetActive(false);
        }
    }
}

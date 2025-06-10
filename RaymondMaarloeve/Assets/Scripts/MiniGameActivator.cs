using UnityEngine;

public class MinigameActivator : MonoBehaviour
{
    private bool canActivate = false;

    [Tooltip("UI tekst 'Naci�nij E'")]
    public GameObject interactText;

    [Tooltip("Panel z minigierk�")]
    public GameObject miniGamePanel;

    void Update()
    {
        if (canActivate && Input.GetKeyDown(KeyCode.E))
        {
            if (interactText != null)
                interactText.SetActive(false);

            if (miniGamePanel != null)
                miniGamePanel.SetActive(true);

            // Mo�esz doda� tu te� np. pauz� gry: Time.timeScale = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StartMinigame"))
        {
            canActivate = true;
            if (interactText != null)
                interactText.SetActive(true);
        }
    }

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

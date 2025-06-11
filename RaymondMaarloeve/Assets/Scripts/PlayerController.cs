using System.Linq;
using Gitmanik.Console;
using UnityEngine;

public enum PlayerState
{
    Moving,     // Gracz moze sie poruszac
    Interacting // Gracz jest w interakcji z NPC
}

/// <summary>
/// Manages player movement, interaction with NPCs, and state transitions.
/// Handles gravity, animations, and camera behavior during interactions.
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the PlayerController class.
    /// </summary>
    public static PlayerController Instance;

    /// <summary>
    /// Speed at which the player moves.
    /// </summary>
    public float moveSpeed = 5f;
    /// <summary>
    /// Gravity applied to the player.
    /// </summary>
    public float gravity = 9.81f;
    /// <summary>
    /// Reference to the CharacterController component.
    /// </summary>
    private CharacterController characterController;
    /// <summary>
    /// Direction of player movement.
    /// </summary>
    private Vector3 moveDirection;

    /// <summary>
    /// Current state of the player (e.g., Moving or Interacting).
    /// </summary>
    private PlayerState currentState = PlayerState.Moving;
    /// <summary>
    /// Transform of the NPC the player is targeting for interaction.
    /// </summary>
    private Transform targetNPC = null;
    /// <summary>
    /// Reference to the player's SkinnedMeshRenderer.
    /// </summary>
    private SkinnedMeshRenderer characterMesh;

    /// <summary>
    /// Reference to the NPC the player is currently interacting with.
    /// </summary>
    public NPC currentlyInteractingNPC = null;

    /// <summary>
    /// Reference to the Animator component for player animations.
    /// </summary>
    private Animator animator;


    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Initializes components and sets the camera target to the player.
    /// </summary>
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        CameraFollow.Instance.SetTarget(transform, false);
        characterMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        animator = GetComponentInChildren<Animator>();

    }

    /// <summary>
    /// Updates player movement and interaction logic every frame.
    /// </summary>
    void Update()
    {
        if (GitmanikConsole.Visible)
            return;
        
        if (currentState == PlayerState.Moving)
        {
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentState == PlayerState.Moving && targetNPC != null)
            {
                StartInteraction(targetNPC.GetComponent<NPC>());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && currentState == PlayerState.Interacting)
        {
            EndInteraction();
        }
    }

    /// <summary>
    /// Handles player movement, including gravity and animations.
    /// </summary>
    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(moveX, 0, moveZ).normalized * moveSpeed;

        if (moveDirection.magnitude > 0)
        {
            transform.forward = new Vector3(moveX, 0, moveZ);
        }
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }


        characterController.Move(moveDirection * Time.deltaTime);
        if (animator != null)
        {
            Vector3 horizontalMove = moveDirection;
            horizontalMove.y = 0f; 
            animator.SetFloat("Speed", horizontalMove.magnitude);
        }

    }

    /// <summary>
    /// Starts interaction with the specified NPC.
    /// Adjusts camera and player state for interaction.
    /// </summary>
    /// <param name="npc">The NPC to interact with.</param>
    public void StartInteraction(NPC npc)
    {
        currentState = PlayerState.Interacting;
        moveDirection = Vector3.zero; // Zatrzymanie gracza

        characterMesh.enabled = false;
        GameManager.Instance.MinimapGameObject.SetActive(false);

        npc.OnInteraction();
        
        currentlyInteractingNPC = npc;
        
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.SetTarget(npc.transform, true); // Kamera przybliza sie do NPC
        }
        else
        {
            Debug.LogError("CameraFollow.Instance is NULL!");
        }

        if (DialogBoxManager.Instance != null)
        {
            DialogBoxManager.Instance.ShowDialogBox(); // Pokazanie okna dialogowego
        }
        else
        {
            Debug.LogError("DialogBoxManager.Instance is NULL!");
        }

        Debug.Log("Started interaction with: " + npc.name);
    }

    /// <summary>
    /// Ends interaction with the currently interacting NPC.
    /// Resets camera and player state.
    /// </summary>
    public void EndInteraction()
    {
        currentState = PlayerState.Moving;

        characterMesh.enabled = true;
        GameManager.Instance.MinimapGameObject.SetActive(true);
        currentlyInteractingNPC.LookAt(null);

        var conv = DialogBoxManager.Instance.currentConversation.ToList();
        
        StartCoroutine(
            Instance.currentlyInteractingNPC?.DrawConclusions(conv));
        
        Instance.currentlyInteractingNPC = null;
        
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.SetTarget(transform, false); // Kamera wraca do gracza
        }
        else
        {
            Debug.LogError("CameraFollow.Instance is NULL!");
        }

        if (DialogBoxManager.Instance != null)
        {
            DialogBoxManager.Instance.HideDialogBox(); // Ukrycie okna dialogowego
        }
        else
        {
            Debug.LogError("DialogBoxManager.Instance is NULL!");
        }

        Debug.Log("Ended interaction");
    }

    /// <summary>
    /// Detects when the player enters the trigger zone of an NPC.
    /// </summary>
    /// <param name="other">The collider of the NPC.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            targetNPC = other.transform;
        }
    }

    /// <summary>
    /// Detects when the player exits the trigger zone of an NPC.
    /// </summary>
    /// <param name="other">The collider of the NPC.</param>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            targetNPC = null;
        }
    }
}

using Gitmanik.Console;
using UnityEngine;

public enum PlayerState
{
    Moving,     // Gracz moze sie poruszac
    Interacting // Gracz jest w interakcji z NPC
}


public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public float moveSpeed = 5f;
    public float gravity = 9.81f;
    private CharacterController characterController;
    private Vector3 moveDirection;

    private PlayerState currentState = PlayerState.Moving;
    private Transform targetNPC = null;
    private SkinnedMeshRenderer characterMesh;
    
    public NPC currentlyInteractingNPC = null;

    private Animator animator;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        CameraFollow.Instance.SetTarget(transform, false);
        characterMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        animator = GetComponentInChildren<Animator>();

    }

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
            // else if (currentState == PlayerState.Interacting)
            // {
            //     EndInteraction();
            // }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && currentState == PlayerState.Interacting)
        {
            EndInteraction();
        }
    }

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

    public void StartInteraction(NPC npc)
    {
        currentState = PlayerState.Interacting;
        moveDirection = Vector3.zero; // Zatrzymanie gracza

        characterMesh.enabled = false;

        npc.LookAt(CameraFollow.Instance.transform);
        
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

        Debug.Log("Rozpoczeto interakcje z NPC: " + npc.name);
    }

    public void EndInteraction()
    {
        currentState = PlayerState.Moving;

        characterMesh.enabled = true;

        currentlyInteractingNPC.LookAt(null);
        
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

        Debug.Log("Zakonczono interakcje");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            targetNPC = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            targetNPC = null;
        }
    }
}

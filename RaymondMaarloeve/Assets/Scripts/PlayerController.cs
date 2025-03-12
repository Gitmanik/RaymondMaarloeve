using UnityEngine;

public enum PlayerState
{
    Moving,     // Gracz mo¿e siê poruszaæ
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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        CameraFollow.Instance.SetTarget(transform, false);
        characterMesh = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    void Update()
    {
        if (currentState == PlayerState.Moving)
        {
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentState == PlayerState.Moving && targetNPC != null)
            {
                StartInteraction();
            }
            else if (currentState == PlayerState.Interacting)
            {
                EndInteraction();
            }
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
    }

    void StartInteraction()
    {
        currentState = PlayerState.Interacting;
        moveDirection = Vector3.zero; // Zatrzymanie gracza

        characterMesh.enabled = false;

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.SetTarget(targetNPC, true); // Kamera przybli¿a siê do NPC
        }
        else
        {
            Debug.LogError("CameraFollow.Instance is NULL!");
        }

        Debug.Log("Rozpoczêto interakcjê z NPC: " + targetNPC.name);
    }

    public void EndInteraction()
    {
        currentState = PlayerState.Moving;

        characterMesh.enabled = true;

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.SetTarget(transform, false); // Kamera wraca do gracza
        }
        else
        {
            Debug.LogError("CameraFollow.Instance is NULL!");
        }

        Debug.Log("Zakoñczono interakcjê");
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

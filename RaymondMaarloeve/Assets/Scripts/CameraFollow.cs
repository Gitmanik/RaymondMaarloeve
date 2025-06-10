using UnityEngine;

/// <summary>
/// Handles the camera movement and positioning to follow a target.
/// Supports smooth transitions and interaction-specific camera behavior.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the CameraFollow class.
    /// </summary>
    public static CameraFollow Instance;
    /// <summary>
    /// Transform of the target the camera should follow.
    /// </summary>
    public Transform target;
    /// <summary>
    /// Speed of the camera's smooth movement.
    /// </summary>
    public float smoothSpeed = 5f;

    /// <summary>
    /// Normal position offset of the camera.
    /// </summary>
    public Vector3 normalOffset = new Vector3(0, -10, 0);
    /// <summary>
    /// Interaction-specific position offset of the camera.
    /// </summary>
    public Vector3 interactionOffset = new Vector3(0, 1.7f, 1.5f);
    /// <summary>
    /// Interaction-specific rotation of the camera.
    /// </summary>
    public Vector3 interactionRotation = new Vector3(0, 0, 0);

    /// <summary>
    /// Indicates whether the camera is in interaction mode.
    /// </summary>
    private bool isInteracting = false;

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Updates the camera position and rotation every frame.
    /// Smoothly transitions between normal and interaction modes.
    /// </summary>
    void LateUpdate()
    {
        if (target != null)
        {
            // Choose the appropriate offset: normal or interaction-specific
            Vector3 desiredOffset = isInteracting ? interactionOffset : normalOffset;
            Vector3 desiredPosition = target.position + desiredOffset;

            // Smoothly move the camera
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // Point the camera directly at the NPC's face during interaction
            if (isInteracting)
            {
                Vector3 npcFacePosition = target.position + new Vector3(0, 1.7f, 0); // Eye level of the NPC
                transform.LookAt(npcFacePosition);
                transform.rotation = Quaternion.Euler(interactionRotation); // Set rotation
            }
            else
            {
                transform.LookAt(target.position);
            }
        }
    }

    /// <summary>
    /// Sets the target for the camera to follow and specifies whether interaction mode is active.
    /// </summary>
    /// <param name="newTarget">The new target for the camera.</param>
    /// <param name="interacting">True if interaction mode is active, false otherwise.</param>
    public void SetTarget(Transform newTarget, bool interacting)
    {
        target = newTarget;
        isInteracting = interacting;
    }
}

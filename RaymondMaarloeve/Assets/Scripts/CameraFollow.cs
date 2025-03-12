using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    public Transform target;
    public float smoothSpeed = 5f;

    // Pozycje kamery
    public Vector3 normalOffset = new Vector3(0, -10, 0); // Normalna pozycja kamery
    public Vector3 interactionOffset = new Vector3(0, 1.7f, 1.5f); // Przybli¿ona kamera na NPC
    public Vector3 interactionRotation = new Vector3(0,0,0);

    private bool isInteracting = false;

    void Awake()
    {
        Instance = this;
    }


    void LateUpdate()
    {
        if (target != null)
        {
            // Wybierz odpowiedni offset: normalny lub interakcyjny
            Vector3 desiredOffset = isInteracting ? interactionOffset : normalOffset;
            Vector3 desiredPosition = target.position + desiredOffset;

            // P³ynne przesuwanie kamery
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // Skieruj kamerê prosto na twarz NPC
            if (isInteracting)
            {
                Vector3 npcFacePosition = target.position + new Vector3(0, 1.7f, 0); // Poziom oczu NPC
                transform.LookAt(npcFacePosition);
                transform.rotation = Quaternion.Euler(interactionRotation); // Ustawienie rotacji
            }
            else
            {
                transform.LookAt(target.position);
            }
        }
    }

    public void SetTarget(Transform newTarget, bool interacting)
    {
        target = newTarget;
        isInteracting = interacting;
    }
}

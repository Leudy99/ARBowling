using UnityEngine;

public class BowlingCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform ballTarget;       // Padre de la bola
    public Transform lookTarget;       // Centro de los pines o un empty al fondo de la pista

    [Header("Follow Settings")]
    public float distanceBehindBall = 4.5f;
    public float height = 2.2f;
    public float sideOffset = 0f;
    public float positionSmooth = 8f;
    public float rotationSmooth = 10f;

    [Header("Look Settings")]
    public bool useLookTarget = true;
    public float forwardLookDistance = 12f;
    public Vector3 lookOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Behavior")]
    public bool lockXPosition = false;
    public float lockedX = 0f;

    private void LateUpdate()
    {
        if (ballTarget == null)
            return;

        // Dirección hacia adelante según la pista / bola
        Vector3 forward = ballTarget.forward;

        // Evita problemas si el target tiene inclinación rara
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        // Posición clásica detrás de la bola
        Vector3 desiredPosition =
            ballTarget.position
            - forward * distanceBehindBall
            + Vector3.up * height
            + right * sideOffset;

        if (lockXPosition)
            desiredPosition.x = lockedX;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmooth * Time.deltaTime
        );

        // Hacia dónde mirar
        Vector3 targetLookPoint;

        if (useLookTarget && lookTarget != null)
        {
            targetLookPoint = lookTarget.position + lookOffset;
        }
        else
        {
            targetLookPoint = ballTarget.position + forward * forwardLookDistance + lookOffset;
        }

        Vector3 lookDir = targetLookPoint - transform.position;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmooth * Time.deltaTime
            );
        }
    }
}
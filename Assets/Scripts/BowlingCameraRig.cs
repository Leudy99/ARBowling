using UnityEngine;

public class BowlingClassicCameraRig : MonoBehaviour
{
    [Header("Rig References")]
    public Transform cameraTransform;
    public Transform ballTarget;
    public Transform lookTarget;

    [Header("Desired Camera View")]
    public float distanceBehindBall = 5.2f;
    public float height = 1.9f;
    public float sideOffset = 0f;
    public float moveSmooth = 8f;
    public float rotateSmooth = 10f;

    [Header("Look")]
    public Vector3 lookOffset = new Vector3(0f, 0.7f, 0f);

    private void LateUpdate()
    {
        if (cameraTransform == null || ballTarget == null)
            return;

        Vector3 forward = ballTarget.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3 desiredCameraWorldPos =
            ballTarget.position
            - forward * distanceBehindBall
            + Vector3.up * height
            + right * sideOffset;

        Vector3 cameraOffsetFromRig = cameraTransform.position - transform.position;
        Vector3 desiredRigPos = desiredCameraWorldPos - cameraOffsetFromRig;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredRigPos,
            moveSmooth * Time.deltaTime
        );

        Vector3 targetLookPoint = lookTarget != null
            ? lookTarget.position + lookOffset
            : ballTarget.position + forward * 12f + lookOffset;

        Vector3 flatLookDir = Vector3.ProjectOnPlane(targetLookPoint - cameraTransform.position, Vector3.up);

        if (flatLookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRigRotation = Quaternion.LookRotation(flatLookDir.normalized, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRigRotation,
                rotateSmooth * Time.deltaTime
            );
        }
    }
}
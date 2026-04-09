using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("References")]
    public Transform spawnPoint;

    [Header("Stop Detection")]
    public float minLinearStopSpeed = 0.05f;
    public float minAngularStopSpeed = 0.05f;

    private Rigidbody rb;
    private bool launched = false;

    public bool HasLaunched()
    {
        return launched;
    }

    public bool IsBallStopped()
    {
        if (rb == null) return true;

        bool linearStopped = rb.linearVelocity.magnitude <= minLinearStopSpeed;
        bool angularStopped = rb.angularVelocity.magnitude <= minAngularStopSpeed;

        return linearStopped && angularStopped;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void LaunchBall(Vector3 force)
    {
        if (rb == null || launched) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();

        rb.AddForce(force, ForceMode.Impulse);
        launched = true;
    }

    public void ResetBall()
    {
        launched = false;

        if (spawnPoint == null)
        {
            Debug.LogWarning("BallLauncher: falta asignar spawnPoint.");
            return;
        }

        if (rb == null)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            return;
        }

        rb.isKinematic = true;
        rb.detectCollisions = false;

        transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

        Physics.SyncTransforms();

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }
}
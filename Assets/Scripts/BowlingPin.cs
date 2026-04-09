using UnityEngine;

public class BowlingPin : MonoBehaviour
{
    public float fallAngle = 20f;

    private bool fallen = false;
    private bool removed = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (removed || !gameObject.activeSelf)
            return;

        if (!fallen)
        {
            float x = NormalizeAngle(transform.eulerAngles.x);
            float z = NormalizeAngle(transform.eulerAngles.z);

            if (Mathf.Abs(x) > fallAngle || Mathf.Abs(z) > fallAngle)
            {
                fallen = true;
            }
        }
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    public bool IsFallen()
    {
        return fallen;
    }

    public bool IsRemoved()
    {
        return removed;
    }

    public void RemovePin()
    {
        removed = true;
        gameObject.SetActive(false);
    }

    public void ResetPin()
    {
        fallen = false;
        removed = false;

        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.isKinematic = true;
            transform.SetPositionAndRotation(startPosition, startRotation);
            Physics.SyncTransforms();
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
        else
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
        }
    }
}
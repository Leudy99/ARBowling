using UnityEngine;

public class BallAimController : MonoBehaviour
{
    [Header("References")]
    public BallLauncher ballLauncher;
    public Camera mainCamera;

    [Header("Arrow Objects")]
    public GameObject rightArrow;
    public GameObject leftArrow;
    public GameObject aimArrow;

    [Header("Aim Arrow Base Settings")]
    public Vector3 aimArrowBaseRotation = new Vector3(-100f, 90f, 0f);
    public Vector3 aimArrowBaseScale = new Vector3(0.18f, 0.18f, 0.18f);
    public Vector3 aimArrowBaseOffset = new Vector3(0f, 0.12f, 0.45f);

    [Header("Aim Arrow Power Visual")]
    public float arrowMinScaleMultiplier = 0.8f;
    public float arrowMaxScaleMultiplier = 1.8f;

    [Header("Side Arrow Settings")]
    public Vector3 rightArrowOffset = new Vector3(0.18f, 0.12f, 0f);
    public Vector3 leftArrowOffset = new Vector3(-0.18f, 0.12f, 0f);

    public Vector3 rightArrowRotation = new Vector3(0f, 0f, 0f);
    public Vector3 leftArrowRotation = new Vector3(0f, 0f, 0f);

    public Vector3 rightArrowScale = new Vector3(0.08f, 0.08f, 0.08f);
    public Vector3 leftArrowScale = new Vector3(0.08f, 0.08f, 0.08f);

    public float sideArrowShowThreshold = 0.05f;

    [Header("Launch Settings")]
    public float maxForwardForce = 12f;
    public float sideForceMultiplier = 2.5f;
    public float laneHalfWidth = 0.28f;

    [Header("Drag Sensitivity")]
    public float dragSensitivityX = 300f;
    public float dragSensitivityY = 350f;

    private bool isDragging = false;
    private Vector2 dragStart;
    private Vector2 dragCurrent;

    private float originalBallX;
    private float currentBallX;
    private float currentPower = 0f;
    private float currentHorizontal = 0f;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (ballLauncher != null)
        {
            originalBallX = ballLauncher.transform.position.x;
            currentBallX = originalBallX;
        }

        HideAim();
    }

    void Update()
    {
        if (ballLauncher == null || ballLauncher.HasLaunched())
            return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchOnBall(Input.mousePosition))
                StartDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsTouchOnBall(touch.position))
                    StartDrag(touch.position);
            }
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isDragging)
            {
                UpdateDrag(touch.position);
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isDragging)
            {
                EndDrag();
            }
        }
    }

    bool IsTouchOnBall(Vector2 screenPosition)
    {
        if (mainCamera == null || ballLauncher == null)
            return false;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            return hit.collider != null && hit.collider.gameObject == ballLauncher.gameObject;

        return false;
    }

    void StartDrag(Vector2 position)
    {
        isDragging = true;
        dragStart = position;
        dragCurrent = position;

        originalBallX = ballLauncher.transform.position.x;
        currentBallX = originalBallX;
        currentPower = 0f;
        currentHorizontal = 0f;

        ShowAim();
        UpdateAimVisuals();
    }

    void UpdateDrag(Vector2 position)
    {
        dragCurrent = position;
        Vector2 dragDelta = dragCurrent - dragStart;

        currentHorizontal = Mathf.Clamp(dragDelta.x / dragSensitivityX, -1f, 1f);

        currentBallX = Mathf.Clamp(
            originalBallX + currentHorizontal * laneHalfWidth,
            -laneHalfWidth,
            laneHalfWidth
        );

        Vector3 ballPos = ballLauncher.transform.position;
        ballLauncher.transform.position = new Vector3(currentBallX, ballPos.y, ballPos.z);

        currentPower = Mathf.Clamp((-dragDelta.y) / dragSensitivityY, 0f, 1f);

        UpdateAimVisuals();
    }

    void EndDrag()
    {
        isDragging = false;

        float finalPower = Mathf.Clamp(currentPower, 0.15f, 1f);

        Vector3 launchForce = new Vector3(
            currentHorizontal * sideForceMultiplier,
            0f,
            finalPower * maxForwardForce
        );

        ballLauncher.LaunchBall(launchForce);

        HideAim();
    }

    void UpdateAimVisuals()
    {
        if (ballLauncher == null || aimArrow == null)
            return;

        Vector3 direction = new Vector3(currentHorizontal * 0.35f, 0f, 1f).normalized;

        UpdateAimArrow(direction);
        UpdateSideArrows();
    }

    void UpdateAimArrow(Vector3 direction)
    {
        if (aimArrow == null || ballLauncher == null)
            return;

        aimArrow.SetActive(true);

        Vector3 ballPos = ballLauncher.transform.position;
        aimArrow.transform.position = ballPos + aimArrowBaseOffset;

        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion baseRot = Quaternion.Euler(aimArrowBaseRotation);
            aimArrow.transform.rotation = lookRot * baseRot;
        }

        float visualScaleMultiplier = Mathf.Lerp(
            arrowMinScaleMultiplier,
            arrowMaxScaleMultiplier,
            currentPower
        );

        aimArrow.transform.localScale = aimArrowBaseScale * visualScaleMultiplier;
    }

    void UpdateSideArrows()
    {
        if (rightArrow != null)
        {
            bool showRight = currentHorizontal > sideArrowShowThreshold;
            rightArrow.SetActive(showRight);

            if (showRight)
            {
                rightArrow.transform.position = ballLauncher.transform.position + rightArrowOffset;
                rightArrow.transform.rotation = Quaternion.Euler(rightArrowRotation);
                rightArrow.transform.localScale = rightArrowScale;
            }
        }

        if (leftArrow != null)
        {
            bool showLeft = currentHorizontal < -sideArrowShowThreshold;
            leftArrow.SetActive(showLeft);

            if (showLeft)
            {
                leftArrow.transform.position = ballLauncher.transform.position + leftArrowOffset;
                leftArrow.transform.rotation = Quaternion.Euler(leftArrowRotation);
                leftArrow.transform.localScale = leftArrowScale;
            }
        }
    }

    void ShowAim()
    {
        if (aimArrow != null)
            aimArrow.SetActive(true);
    }

    void HideAim()
    {
        if (aimArrow != null)
            aimArrow.SetActive(false);

        if (rightArrow != null)
            rightArrow.SetActive(false);

        if (leftArrow != null)
            leftArrow.SetActive(false);
    }
}
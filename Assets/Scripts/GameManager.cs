using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public BowlingPin[] pins;
    public BallLauncher ball;

    public TextMeshProUGUI pinsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI turnText;

    [Header("Game Settings")]
    public int totalRounds = 5;
    public int throwsPerRound = 2;

    [Header("Timing")]
    public float ballStopDelay = 0.5f;
    public float resetDelay = 1.25f;
    public float maxThrowTime = 4.0f;

    [Header("Lane End Detection")]
    public float endLaneZ = 8.5f;
    public float sideOutLimitX = 1.2f;

    private int score = 0;
    private int currentRound = 1;
    private int currentThrow = 1;
    private int pinsRemaining = 10;

    private bool counted = false;
    private bool waitingForReset = false;

    private float stopTimer = 0f;
    private float throwTimer = 0f;

    void Start()
    {
        ResetWholeGame();
    }

    void Update()
    {
        if (ball == null)
            return;

        if (ball.HasLaunched() && !counted && !waitingForReset)
        {
            throwTimer += Time.deltaTime;

            bool shouldCount = false;

            // 1. The ball stopped
            if (ball.IsBallStopped())
            {
                stopTimer += Time.deltaTime;

                if (stopTimer >= ballStopDelay)
                    shouldCount = true;
            }
            else
            {
                stopTimer = 0f;
            }

            // 2. The ball reached the end of the lane
            if (ball.transform.position.z >= endLaneZ)
                shouldCount = true;

            // 3. The ball went too far sideways
            if (Mathf.Abs(ball.transform.position.x) >= sideOutLimitX)
                shouldCount = true;

            // 4. The ball took too long
            if (throwTimer >= maxThrowTime)
                shouldCount = true;

            if (shouldCount)
            {
                CountPinsForCurrentThrow();
            }
        }
    }

    void CountPinsForCurrentThrow()
    {
        counted = true;

        int fallenThisThrow = 0;

        foreach (BowlingPin pin in pins)
        {
            if (pin.IsFallen() && !pin.IsRemoved())
            {
                fallenThisThrow++;
                pin.RemovePin();
            }
        }

        score += fallenThisThrow;
        pinsRemaining = CountStandingPins();

        pinsText.text = "Pins: " + fallenThisThrow;
        scoreText.text = "Score: " + score;

        if (pinsRemaining == 0)
        {
            if (currentThrow == 1 && throwsPerRound >= 2)
                infoText.text = "STRIKE!";
            else if (throwsPerRound == 2)
                infoText.text = "SPARE!";
            else
                infoText.text = "All pins cleared!";

            StartNextRoundSequence();
            return;
        }

        if (currentThrow < throwsPerRound)
        {
            infoText.text = "Throw " + (currentThrow + 1);
            PrepareNextThrow();
            return;
        }

        if (fallenThisThrow == 0)
            infoText.text = "No pins knocked down";
        else
            infoText.text = "End of round";

        StartNextRoundSequence();
    }

    void PrepareNextThrow()
    {
        currentThrow++;

        counted = false;
        waitingForReset = false;
        stopTimer = 0f;
        throwTimer = 0f;

        if (ball != null)
            ball.ResetBall();

        UpdateUI();
    }

    void StartNextRoundSequence()
    {
        waitingForReset = true;
        CancelInvoke(nameof(AdvanceRound));
        Invoke(nameof(AdvanceRound), resetDelay);
    }

    void AdvanceRound()
    {
        if (currentRound >= totalRounds)
        {
            ResetWholeGame();
            return;
        }

        currentRound++;
        currentThrow = 1;

        ResetAllPins();

        if (ball != null)
            ball.ResetBall();

        pinsRemaining = CountStandingPins();

        counted = false;
        waitingForReset = false;
        stopTimer = 0f;
        throwTimer = 0f;

        infoText.text = "New round";
        pinsText.text = "Pins: 0";
        UpdateUI();
    }

    void ResetWholeGame()
    {
        if (throwsPerRound < 1)
            throwsPerRound = 1;

        if (totalRounds < 1)
            totalRounds = 1;

        score = 0;
        currentRound = 1;
        currentThrow = 1;

        counted = false;
        waitingForReset = false;
        stopTimer = 0f;
        throwTimer = 0f;

        ResetAllPins();

        if (ball != null)
            ball.ResetBall();

        pinsRemaining = CountStandingPins();

        pinsText.text = "Pins: 0";
        infoText.text = "Swipe to launch";
        UpdateUI();
    }

    void ResetAllPins()
    {
        foreach (BowlingPin pin in pins)
        {
            pin.ResetPin();
        }
    }

    int CountStandingPins()
    {
        int count = 0;

        foreach (BowlingPin pin in pins)
        {
            if (!pin.IsRemoved())
                count++;
        }

        return count;
    }

    void UpdateUI()
    {
        turnText.text = "Round: " + currentRound + " | Throw: " + currentThrow + "/" + throwsPerRound;
        scoreText.text = "Score: " + score;
    }
}
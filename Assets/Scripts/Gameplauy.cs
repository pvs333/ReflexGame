using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // <-- Add this

public class Gameplauy : MonoBehaviour
{
    public TMP_Text messageText; // Main message
    public TMP_Text player1PointsText; // Player 1 points
    public TMP_Text player2PointsText; // Player 2 points
    public TMP_Text tugOfWarTimerText; // Tug of War timer
    public RectTransform tugOfWarObject; // Assign in Inspector
    public float tugOfWarRange = 400f;   // Max distance from center in pixels
    public Animator animator; // Assign in Inspector or via code

    private enum MiniGame { Shootout, Count, TugOfWar, JustInTime, SimonSays } // <-- Add SimonSays
    private MiniGame currentGame;

    private int player1Points = 0;
    private int player2Points = 0;

    private List<MiniGame> unplayedGames = new List<MiniGame>();

    public TMP_Text player1RuleText; // Player 1 rule text
    public TMP_Text player2RuleText; // Player 2 rule text

    // Add these public fields to assign in Inspector
    public RectTransform player1FallingObject;
    public RectTransform player2FallingObject;
    public RectTransform player1Marker;
    public RectTransform player2Marker;
    public float fallingSpeed = 1200f; // pixels per second, 3x faster

    private Coroutine gameCoroutine;

    void Start()
    {
        UpdatePointsDisplay();
        gameCoroutine = StartCoroutine(StartRandomMiniGame());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // <-- Reloads the scene
    }

    void UpdatePointsDisplay()
    {
        player1PointsText.text = $"{player1Points}";
        player2PointsText.text = $"{player2Points}";
    }

    IEnumerator StartRandomMiniGame()
    {
        while (true)
        {
            // If all minigames have been played, reset the list
            if (unplayedGames.Count == 0)
            {
                unplayedGames = new List<MiniGame> { MiniGame.Shootout, MiniGame.Count, MiniGame.TugOfWar, MiniGame.JustInTime, MiniGame.SimonSays }; // <-- Add SimonSays
            }

            // Pick a random unplayed minigame
            int idx = Random.Range(0, unplayedGames.Count);
            currentGame = unplayedGames[idx];
            unplayedGames.RemoveAt(idx);

            yield return StartCoroutine(PlayMiniGame(currentGame));

            // Hide rule texts while loading next minigame
            player1RuleText.text = "";
            player2RuleText.text = "";

            for (int i = 5; i > 0; i--)
            {
                messageText.text = $"Next Round in {i}s";
                yield return new WaitForSeconds(1f);
            }
        }
    }

    IEnumerator PlayMiniGame(MiniGame game)
    {
        tugOfWarTimerText.gameObject.SetActive(false);
        tugOfWarObject.gameObject.SetActive(false);

        // Hide falling objects and markers before starting
        player1FallingObject.gameObject.SetActive(false);
        player2FallingObject.gameObject.SetActive(false);
        player1Marker.gameObject.SetActive(false);
        player2Marker.gameObject.SetActive(false);

        // Show rules only for the minigame, clear before starting
        player1RuleText.text = "";
        player2RuleText.text = "";

        switch (game)
        {
            case MiniGame.Shootout:
                player1RuleText.text = "Press LShift when you see 'Shoot!'";
                player2RuleText.text = "Press RShift when you see 'Shoot!'";
                yield return StartCoroutine(PlayShootout());
                break;
            case MiniGame.Count:
                player1RuleText.text = "Press LShift";
                player2RuleText.text = "Press RShift";
                yield return StartCoroutine(PlayCount());
                break;
            case MiniGame.TugOfWar:
                player1RuleText.text = "Mash LShift!";
                player2RuleText.text = "Mash RShift!";
                yield return StartCoroutine(PlayTugOfWar());
                break;
            case MiniGame.JustInTime:
                player1RuleText.text = "Press LShift when the object is near your marker!";
                player2RuleText.text = "Press RShift when the object is near your marker!";
                yield return StartCoroutine(PlayJustInTime());
                break;
            case MiniGame.SimonSays:
                player1RuleText.text = "Follow the sequence!";
                player2RuleText.text = "Follow the sequence!";
                yield return StartCoroutine(PlaySimonSays());
                break;
        }

        // Clear rule texts after minigame ends/results announced
        player1RuleText.text = "";
        player2RuleText.text = "";
    }

    IEnumerator PlayShootout()
    {
        float[] intervals = { Random.Range(0.5f, 2f), Random.Range(0.5f, 2f), Random.Range(0.5f, 2f), Random.Range(0.5f, 2f) };
        string[] countdownTexts = { "Get Ready!", "3", "2", "1" };

        // Early press detection during countdown
        for (int i = 0; i < countdownTexts.Length; i++)
        {
            float timer = 0f;
            messageText.text = countdownTexts[i];
            while (timer < intervals[i])
            {
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    messageText.text = "Player 1 pressed early! Disqualified.\nPlayer 2 wins!";
                    player2Points++;
                    OnPlayer2Win();
                    UpdatePointsDisplay();
                    yield return new WaitForSeconds(2f);
                    yield break;
                }
                if (Input.GetKeyDown(KeyCode.RightShift))
                {
                    messageText.text = "Player 2 pressed early! Disqualified.\nPlayer 1 wins!";
                    player1Points++;
                    OnPlayer1Win();
                    UpdatePointsDisplay();
                    yield return new WaitForSeconds(2f);
                    yield break;
                }
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // Random delay before "Shoot!"
        float shootDelay = Random.Range(0.5f, 2f);
        float shootTimer = 0f;
        while (shootTimer < shootDelay)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                messageText.text = "Player 1 pressed early! Disqualified.\nPlayer 2 wins!";
                player2Points++;
                OnPlayer2Win();
                UpdatePointsDisplay();
                yield return new WaitForSeconds(2f);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                messageText.text = "Player 2 pressed early! Disqualified.\nPlayer 1 wins!";
                player1Points++;
                OnPlayer1Win();
                UpdatePointsDisplay();
                yield return new WaitForSeconds(2f);
                yield break;
            }
            shootTimer += Time.deltaTime;
            yield return null;
        }

        // Now allow shooting
        messageText.text = "Shoot!";
        bool gameEnded = false;
        while (!gameEnded)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                messageText.text = "Player 1 wins!";
                player1Points++;
                OnPlayer1Win();
                gameEnded = true;
            }
            else if (Input.GetKeyDown(KeyCode.RightShift))
            {
                messageText.text = "Player 2 wins!";
                player2Points++;
                OnPlayer2Win();
                gameEnded = true;
            }
            yield return null;
        }
        UpdatePointsDisplay();

        // Hide rule texts when showing results
        player1RuleText.text = "";
        player2RuleText.text = "";

        yield return new WaitForSeconds(2f);
    }

    IEnumerator PlayCount()
    {
        int targetTime = Random.Range(10, 21);
        messageText.text = $"Press your button at exactly {targetTime} seconds!";

        float startTime = Time.time;
        bool p1Pressed = false, p2Pressed = false;
        float p1Time = 0, p2Time = 0;

        while (!p1Pressed || !p2Pressed)
        {
            if (!p1Pressed && Input.GetKeyDown(KeyCode.LeftShift))
            {
                p1Pressed = true;
                p1Time = Time.time - startTime;
                player1RuleText.text = $"You have pressed!";
            }
            if (!p2Pressed && Input.GetKeyDown(KeyCode.RightShift))
            {
                p2Pressed = true;
                p2Time = Time.time - startTime;
                player2RuleText.text = $"You have pressed!";
            }
            yield return null;
        }

        // Reveal actual times and winner
        float p1Diff = Mathf.Abs(p1Time - targetTime);
        float p2Diff = Mathf.Abs(p2Time - targetTime);

        messageText.text = $"Target: {targetTime}s\nPlayer 1: {p1Time:F2}s\nPlayer 2: {p2Time:F2}s";

        // Hide rule texts when showing results
        player1RuleText.text = "";
        player2RuleText.text = "";

        yield return new WaitForSeconds(2f);

        if (p1Diff < p2Diff)
        {
            messageText.text = "Player 1 wins!";
            player1Points++;
            OnPlayer1Win(); // <-- Add here
        }
        else if (p2Diff < p1Diff)
        {
            messageText.text = "Player 2 wins!";
            player2Points++;
            OnPlayer2Win(); // <-- Add here
        }
        else
        {
            messageText.text = "It's a tie!";
        }
        UpdatePointsDisplay();
        yield return new WaitForSeconds(2f);
    }

    IEnumerator PlayTugOfWar()
    {
        // Show timer and object
        tugOfWarTimerText.gameObject.SetActive(true);
        tugOfWarObject.gameObject.SetActive(true);

        messageText.text = "Tug of War!";
        float duration = 30f;
        float timer = 0f;
        float position = 0f; // 0 = center, positive = right, negative = left

        // Reset object to center
        tugOfWarObject.anchoredPosition = new Vector2(0, tugOfWarObject.anchoredPosition.y);

        while (timer < duration)
        {
            tugOfWarTimerText.text = $"{(duration - timer):F0}s";
            if (Input.GetKeyDown(KeyCode.LeftShift))
                position += 1f;
            if (Input.GetKeyDown(KeyCode.RightShift))
                position -= 1f;

            // Move object visually
            float normalized = Mathf.Clamp(position / 50f, -1f, 1f); // Adjust denominator for game feel
            tugOfWarObject.anchoredPosition = new Vector2(normalized * tugOfWarRange, tugOfWarObject.anchoredPosition.y);

            timer += Time.deltaTime;
            yield return null;
        }

        tugOfWarTimerText.text = ""; // Clear timer after game

        // Hide timer and object
        tugOfWarTimerText.gameObject.SetActive(false);
        tugOfWarObject.gameObject.SetActive(false);

        if (position > 0)
        {
            messageText.text = "Player 1 wins!";
            player1Points++;
            OnPlayer1Win();
        }
        else if (position < 0)
        {
            messageText.text = "Player 2 wins!";
            player2Points++;
            OnPlayer2Win();
        }
        else
        {
            messageText.text = "It's a tie!";
        }

        // Hide rule texts when showing results
        player1RuleText.text = "";
        player2RuleText.text = "";

        UpdatePointsDisplay();
        yield return new WaitForSeconds(2f);
    }

    IEnumerator PlayJustInTime()
    {
        // Show objects and markers
        player1FallingObject.gameObject.SetActive(true);
        player2FallingObject.gameObject.SetActive(true);
        player1Marker.gameObject.SetActive(true);
        player2Marker.gameObject.SetActive(true);

        // Set objects at top
        float startY = 2000f; // adjust as needed for your canvas
        player1FallingObject.anchoredPosition = new Vector2(player1Marker.anchoredPosition.x, startY);
        player2FallingObject.anchoredPosition = new Vector2(player2Marker.anchoredPosition.x, startY);

        messageText.text = "Just In Time!";

        bool p1Pressed = false, p2Pressed = false;
        float p1Diff = float.MaxValue, p2Diff = float.MaxValue;
        float p1ObjectY = startY, p2ObjectY = startY;

        while (!p1Pressed || !p2Pressed)
        {
            float deltaY = fallingSpeed * Time.deltaTime;
            if (!p1Pressed)
            {
                p1ObjectY -= deltaY;
                player1FallingObject.anchoredPosition = new Vector2(player1Marker.anchoredPosition.x, p1ObjectY);
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    p1Pressed = true;
                    p1Diff = Mathf.Abs(p1ObjectY - player1Marker.anchoredPosition.y);
                    player1FallingObject.gameObject.SetActive(false);
                    player1RuleText.text = $"Pressed! Diff: {p1Diff:F1}";
                }
                else if (p1ObjectY <= player1Marker.anchoredPosition.y - 100f) // missed window
                {
                    p1Pressed = true;
                    p1Diff = float.MaxValue;
                    player1FallingObject.gameObject.SetActive(false);
                    player1RuleText.text = $"Missed!";
                }
            }
            if (!p2Pressed)
            {
                p2ObjectY -= deltaY;
                player2FallingObject.anchoredPosition = new Vector2(player2Marker.anchoredPosition.x, p2ObjectY);
                if (Input.GetKeyDown(KeyCode.RightShift))
                {
                    p2Pressed = true;
                    p2Diff = Mathf.Abs(p2ObjectY - player2Marker.anchoredPosition.y);
                    player2FallingObject.gameObject.SetActive(false);
                    player2RuleText.text = $"Pressed! Diff: {p2Diff:F1}";
                }
                else if (p2ObjectY <= player2Marker.anchoredPosition.y - 100f)
                {
                    p2Pressed = true;
                    p2Diff = float.MaxValue;
                    player2FallingObject.gameObject.SetActive(false);
                    player2RuleText.text = $"Missed!";
                }
            }
            yield return null;
        }

        // Hide markers
        player1Marker.gameObject.SetActive(false);
        player2Marker.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        // Decide winner
        if (p1Diff < p2Diff)
        {
            messageText.text = "Player 1 wins!";
            player1Points++;
            OnPlayer1Win();
        }
        else if (p2Diff < p1Diff)
        {
            messageText.text = "Player 2 wins!";
            player2Points++;
            OnPlayer2Win();
        }
        else
        {
            messageText.text = "It's a tie!";
        }

        player1RuleText.text = "";
        player2RuleText.text = "";

        UpdatePointsDisplay();
        yield return new WaitForSeconds(2f);
    }

    IEnumerator PlaySimonSays()
    {
        var actions = new List<string> { "Tap Once", "Tap Twice", "Tap Thrice", "Hold" };
        bool roundEnded = false;
        bool lastWasHold = false;

        // Countdown before Simon Says starts
        messageText.text = "Get Ready for Simon Says!";
        yield return new WaitForSeconds(0.7f);
        messageText.text = "3";
        yield return new WaitForSeconds(0.7f);
        messageText.text = "2";
        yield return new WaitForSeconds(0.7f);
        messageText.text = "1";
        yield return new WaitForSeconds(0.7f);

        while (!roundEnded)
        {
            bool p1Failed = false, p2Failed = false;
            bool simonSays = Random.value > 0.25f;
            string action;

            action = actions[Random.Range(0, actions.Count)];
            lastWasHold = action == "Hold";

            string instruction = (simonSays ? "Simon Says " : "") + action;

            player1RuleText.text = instruction;
            player2RuleText.text = instruction;
            messageText.text = instruction;

            yield return new WaitForSeconds(0.5f);

            // Reset state
            bool p1HoldStarted = false, p2HoldStarted = false;
            float timer = 0f;
            float graceTime = 1.5f;

            if (action.StartsWith("Tap"))
            {
                int requiredTaps = action == "Tap Once" ? 1 : action == "Tap Twice" ? 2 : 3;
                int p1Taps = 0, p2Taps = 0;

                while (timer < graceTime)
                {
                    timer += Time.deltaTime;
                    if (Input.GetKeyDown(KeyCode.LeftShift)) p1Taps++;
                    if (Input.GetKeyDown(KeyCode.RightShift)) p2Taps++;
                    yield return null;
                }

                if (simonSays)
                {
                    if (p1Taps != requiredTaps) p1Failed = true;
                    if (p2Taps != requiredTaps) p2Failed = true;
                }
                else
                {
                    if (p1Taps > 0) p1Failed = true;
                    if (p2Taps > 0) p2Failed = true;
                }
            }
            else if (action == "Hold")
            {
                timer = 0f;
                while (timer < graceTime)
                {
                    timer += Time.deltaTime;
                    if (Input.GetKey(KeyCode.LeftShift)) p1HoldStarted = true;
                    if (Input.GetKey(KeyCode.RightShift)) p2HoldStarted = true;
                    yield return null;
                }
                if (simonSays)
                {
                    if (!p1HoldStarted) p1Failed = true;
                    if (!p2HoldStarted) p2Failed = true;
                }
                else
                {
                    if (p1HoldStarted) p1Failed = true;
                    if (p2HoldStarted) p2Failed = true;
                }
            }

            // Feedback and win condition
            if (p1Failed && p2Failed)
            {
                messageText.text = $"Both failed! No points.";
                roundEnded = true;
            }
            else if (p1Failed)
            {
                messageText.text = $"Player 1 failed! Player 2 wins!";
                player2Points++;
                OnPlayer2Win();
                roundEnded = true;
            }
            else if (p2Failed)
            {
                messageText.text = $"Player 2 failed! Player 1 wins!";
                player1Points++;
                OnPlayer1Win();
                roundEnded = true;
            }
            else
            {
                messageText.text = $"Both succeeded!";
            }

            UpdatePointsDisplay();
            yield return new WaitForSeconds(1f);
        }

        player1RuleText.text = "";
        player2RuleText.text = "";
        yield return new WaitForSeconds(1f);
    }

    void OnPlayer1Win()
    {
        animator.SetTrigger("p1win");
    }

    void OnPlayer2Win()
    {
        animator.SetTrigger("p2win");
    }
}

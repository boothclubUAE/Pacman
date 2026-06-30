using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[HasTabField]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text highScoreText;
    public int Round = 0;
    [TabField]
    public float gameOverTimeout = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip powerPelletClip;
    [SerializeField] private AudioClip ghostEatenClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip gameStartClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip wakaSound1; // First "waka" sound (wa)
    [SerializeField] private AudioClip wakaSound2; // Second "waka" sound (ka)

    public int score { get; private set; } = 0;
    [TabField]
    public int lives = 3;
    private int highScore = 0;

    private int ghostMultiplier = 1;
    private bool isGameStarted = false;
    private bool useWaka1 = true; // Toggle between wakaSound1 and wakaSound2
    private float lastWakaTime = -1f;
    private float wakaCooldown = 0.1f; // Reduced to match faster pellet-eating speed

    private string HighScoreFilePath => Path.Combine(Application.persistentDataPath, "highscore.json");

    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private TMP_Text finalScoreText;

    [System.Serializable]
    private class HighScoreData
    {
        public int highScore;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }

        LoadHighScore();
        UpdateHighScoreText();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        SetScore(0);
        SetLives(3);
        IdleState();
        SerialManager.Instance.OnMessageReceived.AddListener(OnSerialMessageReceived);
    }

    private void OnSerialMessageReceived(string message)
    {
        var direction = message switch
        {
            "UP" => Vector2.up,
            "DOWN" => Vector2.down,
            "LEFT" => Vector2.left,
            "RIGHT" => Vector2.right,
            _ => Vector2.zero
        };
        if (direction != Vector2.zero)
            pacman.SetExternalDirection(direction);
    }

    private void Update()
    {
        if (showingGameOver)
            return;

        if (!isGameStarted && IsKeyboardKeyDown())
        {
            StartGame();
        }
        else if (lives <= 0 && IsKeyboardKeyDown())
        {
            NewGame();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            ClearHighScore();
        }
    }

    [TabButton]
    private void ClearHighScore()
    {
        highScore = 0;
        SaveHighScore();
        UpdateHighScoreText();
        Debug.Log("High score reset to 0.");
    }

    private bool IsKeyboardKeyDown()
    {
        if (!string.IsNullOrEmpty(Input.inputString))
        {
            return true;
        }

        return Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.KeypadEnter) ||
               Input.GetKeyDown(KeyCode.UpArrow) ||
               Input.GetKeyDown(KeyCode.DownArrow) ||
               Input.GetKeyDown(KeyCode.LeftArrow) ||
               Input.GetKeyDown(KeyCode.RightArrow) ||
               Input.GetKeyDown(KeyCode.Escape) ||
               Input.GetKeyDown(KeyCode.Tab) ||
               Input.GetKeyDown(KeyCode.Backspace);
    }

    private void IdleState()
    {
        gameOverText.enabled = true;
        gameOverText.text = "READY!";

        foreach (var ghost in ghosts)
        {
            ghost.Idle();
            ghost.movement.ResetPosition();
            ghost.movement.canMove = false;
            ghost.movement.speedMultiplier = 1;
        }
        pacman.Idle();
        pacman.movement.ResetPosition();
        pacman.movement.canMove = false;

        ResetPellets();
    }

    public void StartGame()
    {
        if (isGameStarted) return;
        isGameStarted = true;

        gameOverText.enabled = true;
        gameOverText.text = "READY!";
        if (gameStartClip != null)
        {
            audioSource.PlayOneShot(gameStartClip);
        }
        DOVirtual.DelayedCall(gameStartClip != null ? gameStartClip.length : 0f, () =>
        {
            gameOverText.enabled = false;
        }).OnComplete(() =>
        {
            NewRound();
        });
    }

    private void NewGame()
    {
        isGameStarted = false;
        SetScore(0);
        SetLives(3);
        IdleState();
        Round = 0;
    }

    private void NewRound()
    {
        ResetPellets();
        ResetState();
    }

    private void ResetState()
    {
        foreach (var ghost in ghosts)
        {
            ghost.ResetState();
            ghost.movement.canMove = true;
        }
        pacman.ResetState();
        pacman.movement.canMove = true;
    }

    private void GameOver()
    {
        gameOverText.enabled = false;

        foreach (var ghost in ghosts)
            ghost.gameObject.SetActive(false);

        pacman.gameObject.SetActive(false);

        foreach (Transform pellet in pellets)
            pellet.gameObject.SetActive(false);

        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
            UpdateHighScoreText();
        }

        if (gameOverClip != null)
        {
            audioSource.PlayOneShot(gameOverClip);
        }

        ShowGameOverScreen();
    }

    public bool showingGameOver = false;
    private void ShowGameOverScreen()
    {
        gameOverCanvasGroup.alpha = 0;
        gameOverCanvasGroup.gameObject.SetActive(true);
        showingGameOver = true;
        string paddedScore = score.ToString().PadLeft(4, '0');
        finalScoreText.text = $"YOUR SCORE <color=#ffff00>{paddedScore}</color>";

        gameOverCanvasGroup.DOFade(1f, 1f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => DOVirtual.DelayedCall(gameOverTimeout, HideGameOverScreen));
    }

    private void HideGameOverScreen()
    {
        gameOverCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            gameOverCanvasGroup.gameObject.SetActive(false);
            showingGameOver = false;
            IdleState();
        });
    }

    private void TryPlayWaka()
    {
        if (wakaSound1 == null || wakaSound2 == null) return;

        if (Time.time - lastWakaTime >= wakaCooldown)
        {
            if (useWaka1)
            {
                audioSource.PlayOneShot(wakaSound1, 0.8f);
            }
            else
            {
                audioSource.PlayOneShot(wakaSound2, 0.8f);
            }
            useWaka1 = !useWaka1;
            lastWakaTime = Time.time;
        }
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = $"YOUR SCORE {score.ToString().PadLeft(4, '0')}";

        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
            UpdateHighScoreText();
        }
    }

    private void UpdateHighScoreText()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"HIGH SCORE {highScore.ToString().PadLeft(4, '0')}";
        }
    }

    private void LoadHighScore()
    {
        try
        {
            if (File.Exists(HighScoreFilePath))
            {
                string json = File.ReadAllText(HighScoreFilePath);
                HighScoreData data = JsonUtility.FromJson<HighScoreData>(json);
                highScore = data.highScore;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to load high score: {e.Message}");
            highScore = 0;
        }
    }

    private void SaveHighScore()
    {
        try
        {
            HighScoreData data = new HighScoreData { highScore = highScore };
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(HighScoreFilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to save high score: {e.Message}");
        }
    }

    public void PacmanEaten()
    {
        pacman.DeathSequence();
        SetLives(lives - 1);

        if (deathClip != null)
        {
            audioSource.PlayOneShot(deathClip);
        }

        if (lives > 0)
        {
            Invoke(nameof(ResetState), 3f);
        }
        else
        {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);
        ghostMultiplier++;

        if (ghostEatenClip != null)
        {
            audioSource.PlayOneShot(ghostEatenClip);
        }
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        SetScore(score + pellet.points);

        if (!(pellet is PowerPellet))
        {
            TryPlayWaka();
        }

        if (!HasRemainingPellets())
        {
            pacman.gameObject.SetActive(false);
            DOVirtual.DelayedCall(3, () =>
            {
                NewRound();
                Round++;
            });

        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        foreach (var ghost in ghosts)
        {
            ghost.frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);

        if (powerPelletClip != null)
        {
            audioSource.PlayOneShot(powerPelletClip);
        }
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

    private void ResetPellets()
    {
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MinigameController : MonoBehaviour
{
    private enum GameState { Menu, Playing, GameOver }
    private GameState currentState = GameState.Menu;

    [Header("Core UI")]
    public GameObject minigameTabObject;
    public GameObject startMenu;
    public GameObject gameOverMenu;
    public GameObject tutorialPanel; 
    
    [Header("Audio")]
    public AudioSource minigameMusic;

    [Header("Text Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameOverTimeText;
    public TextMeshProUGUI highScoreText;

    [Header("Road Scrolling")]
    public RectTransform road1;
    public RectTransform road2;
    public float baseScrollSpeed = 300f;
    private float currentScrollSpeed;
    private float roadHeight;
    private float scrollOffset; 

    [Header("Player Movement")]
    public RectTransform playerBike;
    public float[] lanePositions = { -229f, -74f, 74f, 229f }; 
    private int currentLane = 1;

    [Header("Obstacles")]
    public RectTransform obstacleSpawnPoint;
    public GameObject[] obstaclePrefabs;
    public float baseObstacleSpeed = 400f;
    public float baseSpawnInterval = 1.5f;
    
    [Header("Difficulty Milestones")]
    public float timeToLevelUp = 10f;
    public float speedBump = 50f;
    public float spawnRateBump = 0.2f;
    public float minimumSpawnInterval = 0.4f;

    private float nextMilestone;
    private float currentObstacleSpeed;
    private float currentSpawnInterval;
    private float spawnTimer;
    private List<RectTransform> activeObstacles = new List<RectTransform>();

    private float timeSurvived = 0f;
    private float highScore = 0f;

    private void Start()
    {
        if (road1 != null)
        {
            roadHeight = road1.rect.height;
        }
        
        highScore = PlayerPrefs.GetFloat("BikerHighScore", 0f);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        ShowMenu();
    }

    private void Update()
    {
        if (currentState != GameState.Playing) return;

        HandleInput();
        SmoothMoveBike();
        UpdateTimerAndDifficulty();
        ScrollRoadSeamlessly();
        HandleObstacles();
        CheckCollisions();
    }

    public void StartGame()
    {
        if (startMenu != null) startMenu.SetActive(false);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        
        timeSurvived = 0f;
        currentLane = 1;
        scrollOffset = 0f;
        UpdatePlayerPosition();

        currentScrollSpeed = baseScrollSpeed;
        currentObstacleSpeed = baseObstacleSpeed;
        currentSpawnInterval = baseSpawnInterval;
        nextMilestone = timeToLevelUp;

        foreach (var obs in activeObstacles) 
        { 
            if (obs != null) Destroy(obs.gameObject); 
        }
        activeObstacles.Clear();
        spawnTimer = currentSpawnInterval;

        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource src in allAudioSources)
        {
            if (src != null && src != minigameMusic && src.isPlaying)
            {
                src.Pause();
            }
        }

        if (minigameMusic != null)
        {
            minigameMusic.Play();
        }

        currentState = GameState.Playing;
    }

    private void HandleInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (currentLane > 0) currentLane--;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (currentLane < lanePositions.Length - 1) currentLane++;
        }
    }

    private void SmoothMoveBike()
    {
        if (playerBike == null) return;
        
        Vector2 currentPos = playerBike.anchoredPosition;
        float targetX = lanePositions[currentLane];
        
        currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * 15f);
        playerBike.anchoredPosition = currentPos;
    }

    private void UpdatePlayerPosition()
    {
    }

    private void UpdateTimerAndDifficulty()
    {
        timeSurvived += Time.deltaTime;
        if (timerText != null) timerText.text = timeSurvived.ToString("00.00");

        if (timeSurvived >= nextMilestone)
        {
            currentScrollSpeed += speedBump;
            currentObstacleSpeed += speedBump;
            
            currentSpawnInterval -= spawnRateBump;
            if (currentSpawnInterval < minimumSpawnInterval) currentSpawnInterval = minimumSpawnInterval;

            nextMilestone += timeToLevelUp;
        }
    }

    private void ScrollRoadSeamlessly()
    {
        if (road1 == null || road2 == null) return;

        scrollOffset += currentScrollSpeed * Time.deltaTime;
        scrollOffset %= roadHeight; 

        road1.anchoredPosition = new Vector2(road1.anchoredPosition.x, -scrollOffset);
        road2.anchoredPosition = new Vector2(road2.anchoredPosition.x, roadHeight - scrollOffset);
    }

    private void HandleObstacles()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnObstacle();
            spawnTimer = currentSpawnInterval;
        }

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            RectTransform obs = activeObstacles[i];
            
            if (obs == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            obs.anchoredPosition += Vector2.down * currentObstacleSpeed * Time.deltaTime;

            if (obs.anchoredPosition.y < -roadHeight)
            {
                Destroy(obs.gameObject);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    private void SpawnObstacle()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0 || obstacleSpawnPoint == null) return;

        GameObject prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        int randomLane = Random.Range(0, lanePositions.Length);

        GameObject newObstacle = Instantiate(prefabToSpawn, obstacleSpawnPoint.parent);
        
        newObstacle.transform.SetSiblingIndex(obstacleSpawnPoint.GetSiblingIndex());

        RectTransform obsRect = newObstacle.GetComponent<RectTransform>();
        obsRect.anchoredPosition = new Vector2(lanePositions[randomLane], obstacleSpawnPoint.anchoredPosition.y);
        activeObstacles.Add(obsRect);
    }

    private void CheckCollisions()
    {
        if (playerBike == null) return;

        Vector3[] playerCorners = new Vector3[4];
        playerBike.GetWorldCorners(playerCorners);
        Rect playerRect = new Rect(playerCorners[0].x, playerCorners[0].y, playerCorners[2].x - playerCorners[0].x, playerCorners[2].y - playerCorners[0].y);

        playerRect.xMin += 5f; playerRect.xMax -= 5f;
        playerRect.yMin += 5f; playerRect.yMax -= 5f;

        foreach (var obs in activeObstacles)
        {
            if (obs == null) continue;

            Vector3[] obsCorners = new Vector3[4];
            obs.GetWorldCorners(obsCorners);
            Rect obsRect = new Rect(obsCorners[0].x, obsCorners[0].y, obsCorners[2].x - obsCorners[0].x, obsCorners[2].y - obsCorners[0].y);

            if (playerRect.Overlaps(obsRect))
            {
                TriggerGameOver();
                break;
            }
        }
    }

    private void TriggerGameOver()
    {
        currentState = GameState.GameOver;
        
        if (minigameMusic != null)
        {
            minigameMusic.Stop();
        }

        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource src in allAudioSources)
        {
            if (src != null && src != minigameMusic && !src.isPlaying)
            {
                src.UnPause();
            }
        }

        if (timeSurvived > highScore)
        {
            highScore = timeSurvived;
            PlayerPrefs.SetFloat("BikerHighScore", highScore);
            PlayerPrefs.Save();
        }

        if (gameOverTimeText != null) gameOverTimeText.text = "Time: " + timeSurvived.ToString("00.00");
        if (highScoreText != null) highScoreText.text = "Highest Time: " + highScore.ToString("00.00");
        
        if (gameOverMenu != null) gameOverMenu.SetActive(true);
    }

    public void ShowMenu()
    {
        currentState = GameState.Menu;
        if (startMenu != null) startMenu.SetActive(true);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public void OpenTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public void QuitMinigame()
    {
        if (minigameMusic != null)
        {
            minigameMusic.Stop();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        CyberdeckController cyberdeck = FindObjectOfType<CyberdeckController>();
        if (cyberdeck != null)
        {
            cyberdeck.selectTab(0); 
        }

        if (minigameTabObject != null)
        {
            minigameTabObject.SetActive(false);
        }
    }
}

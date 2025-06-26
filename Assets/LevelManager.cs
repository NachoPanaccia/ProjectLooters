using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Timer Settings")]
    [SerializeField] private float levelDuration = 60f; // duración del nivel en segundos
    private float timer;
    private bool timerRunning = false;
    private int permaDeadCount = 0;

    [Header("Money Counter")]
    private int moneyCounter = 0;
    private int objmoney = 300;

    [Header("Events")]
    public UnityEvent<bool> CopWon;
    public UnityEvent<bool> LooterWon;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        StartTimer();
        GameManager.instance.onLooterPermaDied.AddListener(PermaDeadCounter);
    }

    private void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                timerRunning = false;
                CopWon?.Invoke(true);
            }
        }
    }

    public void StartTimer()
    {
        timer = levelDuration;
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public float GetTimeRemaining()
    {
        return timer;
    }

    public bool IsTimerRunning()
    {
        return timerRunning;
    }

    private void PermaDeadCounter()
    {
        permaDeadCount++;
        Debug.Log("Looter perma muerto. Total: " + permaDeadCount);
        if (permaDeadCount >= GameManager.instance.getplayingLooters() - 1)
        {
            Debug.Log("Todos los looters han sido eliminados. El guardia gana.");
            CopWon?.Invoke(true); 
        }
    }
    public void LooterDeposited(int lootAmount)
    {
        moneyCounter += lootAmount;
        if (moneyCounter >= objmoney)
        {
            LooterWon?.Invoke(false);
            Debug.Log("Los looters han ganado.");
        }
    }
}


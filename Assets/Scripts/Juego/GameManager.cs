using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private int playingLooters;
    [SerializeField] private int initialRespawns;
    private int currentRespawns;

    [Header("Events")]
    public UnityEvent onLooterPermaDied;


    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        currentRespawns = initialRespawns;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CopWon.AddListener(GameEnd);
            LevelManager.Instance.LooterWon.AddListener(GameEnd);
        }
    }

    public void LooterDied()
    {
        currentRespawns--;
    }

    public void LooterInitialized()
    {
        playingLooters++;
    }

    public void LooterPermaDied()
    {
        playingLooters--;
        if (playingLooters == 0)
        {
            Debug.Log("Gan� el guardia!");
            onLooterPermaDied?.Invoke();
            //ac� la condicion de victoria: guardia mat� todos los looters
        }
    }

    public bool CanRespawn()
    {
        return (currentRespawns > 0);
    }

    private void GameEnd(bool winner)
    {
        Debug.Log("El tiempo del nivel termin�. El GameManager recibi� el evento.");
        if (winner)
        {
            Debug.Log("El guardia gan�.");
        }
        else
        {
            Debug.Log("Los looters ganaron.");
        }
    }

    public int getplayingLooters()
    {
        return playingLooters;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int playingLooters;
    [SerializeField] private int initialRespawns;
    private int currentRespawns;

    [SerializeField] String looterswinscene;
    [SerializeField] String policewinscene;
    public wincondition actualwin;


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
            LevelManager.Instance.con.AddListener(GameEnd);
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
            Debug.Log("Ganó el guardia!");
            onLooterPermaDied?.Invoke();
            //acá la condicion de victoria: guardia mató todos los looters
        }
    }

    public bool CanRespawn()
    {
        return (currentRespawns > 0);
    }

    [PunRPC]
    private void GameEnd( wincondition winid)
    {
        actualwin = winid;
        switch (winid)
        {
            case wincondition.tiempo:
                SceneManager.LoadScene(policewinscene);
                Debug.Log("El guardia ganó por tiempo");
                break;
            case wincondition.lootpermadead:
                SceneManager.LoadScene(policewinscene);
                Debug.Log("El guardia gano porque muerieron los looters.");
                break;
            case wincondition.copdead:
                SceneManager.LoadScene(looterswinscene);
                Debug.Log("Los looters ganaron porque murio el policia");
                break;
            case wincondition.quota:
                SceneManager.LoadScene(looterswinscene);
                Debug.Log("Los looters llegaron al dinero requeriod");
                break;
            default :
                Debug.Log("ERROR ID NO RECONOCIDO");
                break;
        }
    }

    public int getplayingLooters()
    {
        return playingLooters;
    }
}

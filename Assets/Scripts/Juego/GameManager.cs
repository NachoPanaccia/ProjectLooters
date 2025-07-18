using System;
using System.Collections.Generic;
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

    public string policeName;
    public bool[] connected_player = new bool[4];
    public UnityEvent forcePause = new UnityEvent();
    public UnityEvent forceUnPause = new UnityEvent();
    public UnityEvent forceLotersWin = new UnityEvent();

    private UIManager _uiManager;
    
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
        _uiManager = UIManager.instance;
        _uiManager.UpdateRespawns(currentRespawns);
    }

    public void LooterDied()
    {
        currentRespawns--;
        _uiManager.UpdateRespawns(currentRespawns);
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

    public void PoliceDied()
    {
        GameEnd(wincondition.copdead);
    }

    [PunRPC]
    private void GameEnd( wincondition winid)
    {
        actualwin = winid;
        switch (winid)
        {
            case wincondition.tiempo:
                SceneManager.LoadScene(policewinscene);
                Debug.Log("El guardia gan� por tiempo");
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

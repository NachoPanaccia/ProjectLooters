
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviourPunCallbacks
{
    public static LevelManager Instance { get; private set; }

    [Header("Timer Settings")]
    [SerializeField] private float levelDuration = 60f; // duración del nivel en segundos
    private float timer;
    private bool timerRunning = false;
    private int permaDeadCount = 0;

    [Header("Money Counter")]
    private int moneyCounter = 0;
    [SerializeField] private int objmoney = 300;

    [Header("Events")]
    public UnityEvent<wincondition> con;

    private UIManager _uiManager;

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
        _uiManager = UIManager.instance;
        StartTimer();
        GameManager.instance.onLooterPermaDied.AddListener(PermaDeadCounter);
        _uiManager.UpdateQuotaMax(objmoney);
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
                photonView.RPC("OnConditionMet",RpcTarget.All,wincondition.tiempo);
            }
        }
    }

    public void StartTimer()
    {
        timer = levelDuration;
        timerRunning = true;
        _uiManager.UpdateTime(Mathf.RoundToInt(timer));
        _uiManager.SetTimerState(true);
    }

    public void StopTimer()
    {
        timerRunning = false;
        _uiManager.SetTimerState(false);
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
            photonView.RPC("OnConditionMet", RpcTarget.All,wincondition.lootpermadead); 
        }
    }

    public void CopDied()
    {
        photonView.RPC("OnConditionMet", RpcTarget.All,wincondition.copdead);
    }
    
    public void LooterDeposited(int lootAmount)
    {
        photonView.RPC("RPC_Deposited", RpcTarget.All, lootAmount);
    }

    [PunRPC]
    private void RPC_Deposited(int amount)
    {
        moneyCounter += amount;
        _uiManager.UpdateQuota(moneyCounter);
        if (moneyCounter >= objmoney)
        {
            photonView.RPC("OnConditionMet", RpcTarget.All,wincondition.quota);
            Debug.Log("Los looters han ganado.");
        }
    }

    // WIN ID = ( 1 = Tiempo , 2 = LootersPermaDead , 3 = Cop Died , 4 = Quota Reached)
    [PunRPC]
    public void OnConditionMet( wincondition winid)
    {
        con?.Invoke(winid);
    }
}

public enum wincondition
{
    tiempo,
    lootpermadead,
    copdead,
    quota
}
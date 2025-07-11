using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI walletText;
    [SerializeField] private TextMeshProUGUI quotaText;
    [SerializeField] private TextMeshProUGUI quotaMaxText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI respawnsText;
    [SerializeField] private TextMeshProUGUI maxBulletsText;
    [SerializeField] private TextMeshProUGUI bulletsText;
    [SerializeField] private GameObject bullets;
    
    private float time;
    private int minutes;
    private int seconds;
    private bool timerRunning = false;

    public static UIManager instance;
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

    private void Update()
    {
        time -= Time.deltaTime;
        minutes = Mathf.FloorToInt(time / 60);
        seconds = Mathf.FloorToInt(time % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateWallet(int amount)
    {
        walletText.text = amount.ToString();
    }

    public void UpdateQuota(int amount)
    {
        quotaText.text = amount.ToString();
    }
    
    public void UpdateQuotaMax(int amount)
    {
        quotaMaxText.text = amount.ToString();
    }
    
    public void UpdateTime(int amount)
    {
        time = amount;
    }

    public void SetTimerState(bool state)
    {
        timerRunning = state;
    }
    
    public void UpdateRespawns(int amount)
    {
        respawnsText.text = amount.ToString();
    }

    public void UpdateMaxBullets(int amount)
    {
        maxBulletsText.text = amount.ToString();
    }

    public void UpdateBullets(int amount)
    {
        bulletsText.text = amount.ToString();
    }

    public void HideBulletCount()
    {
        bullets.SetActive(false);
    }

    public void ShowBulletCount()
    {
        bullets.SetActive(true);
    }
}

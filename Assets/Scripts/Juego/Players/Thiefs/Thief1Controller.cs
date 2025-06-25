using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView), typeof(LooterMovementController))]
public class Thief1Controller : MonoBehaviourPunCallbacks, IRobber
{
    [Header("Respawn")]
    [SerializeField] private float respawnTime = 3f;
    private float respawnTimer;
    private bool isAlive = true;
    private Vector3 spawnPosition;

    [Header("Melee")]
    [SerializeField] private float meleeCooldown = 2f;
    [SerializeField] private Collider2D meleeCollider;
    [SerializeField] private ContactFilter2D contactFilter;
    private float meleeTimer;

    [Header("Stun")]
    [SerializeField] private float stunTime = 1.5f;
    [SerializeField] private float stunCooldownTime = 3f;
    private bool canBeStunned = true;
    private float stunCooldownTimer;

    [Header("Loot")]
    [SerializeField] private int actual_loot;

    [Header("Events")]
    public UnityEvent<int> DepositedLoot;

    private Rigidbody2D rb;
    private GameManager gameManager;
    private LooterMovementController moveCtrl;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveCtrl = GetComponent<LooterMovementController>();
        gameManager = GameManager.instance;
        spawnPosition = transform.position;

        gameManager.LooterInitialized();

        if (photonView.IsMine)
            FindObjectOfType<CameraFollow>().SetTarget(transform);
    }
    

    private void Update()
    {
        if (!isAlive)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime) isAlive = true;
            return;
        }

        if (!photonView.IsMine) return;
        
        meleeTimer = Mathf.Min(meleeTimer + Time.deltaTime, meleeCooldown);
        stunCooldownTimer = Mathf.Min(stunCooldownTimer + Time.deltaTime, stunCooldownTime);
        if (stunCooldownTimer >= stunCooldownTime) canBeStunned = true;

        // Input Melee
        if ((Input.GetKeyDown(KeyCode.V) || Input.GetMouseButtonDown(0)) && meleeTimer >= meleeCooldown)
            MeleeAttack();
    }

    
    private void MeleeAttack()
    {
        meleeTimer = 0f;

        var hits = new List<Collider2D>();
        meleeCollider.OverlapCollider(contactFilter, hits);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PoliceController>(out var guard))
            {
                guard.Stunned();
                Debug.Log("Wham!");
            }
        }
    }
    
    public void GetLoot(int value) => actual_loot = value;

    public void DepositLoot()
    {
        LevelManager.Instance.LooterDeposited(actual_loot, 1);
        actual_loot = 0;
    }
    
    public void Hit() => photonView.RPC(nameof(RPC_Hit), RpcTarget.All);
    public void Stunned() => photonView.RPC(nameof(RPC_Stunned), RpcTarget.All);

    [PunRPC]
    private void RPC_Hit()
    {
        if (gameManager.CanRespawn())
        {
            gameManager.LooterDied();
            isAlive = false;
            respawnTimer = 0;
            Respawn();
        }
        else
        {
            gameManager.LooterPermaDied();
            isAlive = false;
            Destroy(gameObject);
        }
    }

    [PunRPC]
    private void RPC_Stunned()
    {
        if (!canBeStunned) return;

        canBeStunned = false;
        stunCooldownTimer = 0f;
        moveCtrl.SetStunned(stunTime);
    }
    
    private void Respawn() => transform.position = spawnPosition;
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class Thief1Controller : MonoBehaviourPunCallbacks, IRobber, IShopClient
{
    [SerializeField] private float moveSpeed = 5f;
    private float initialMoveSpeed;
    [SerializeField] private float respawnTime = 3f;
    private float respawnTimer;
    private bool isAlive;

    //private SpriteRenderer _spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 movement;
    private GameManager _gameManager;
    private Vector3 _spawnPosition;

    [Header("Melee Parameters")]
    [SerializeField] private float _meleeTime = 2f;
    private float _meleeTimer;
    [SerializeField] private Collider2D _meleeCollider;
    [SerializeField] ContactFilter2D contactFilter2D;
    [SerializeField] private float stunTime = 1.5f;
    private float stunTimer;
    private bool isStunned;
    [SerializeField] private float stunCooldownTime = 3f;
    private float stunCooldownTimer;
    private bool canBeStunned;
    [Header("Loot Parameters")]
    [SerializeField] int actual_loot;
    [SerializeField] int total_loot;
    [Header("Events")]
    public UnityEvent<int> DepositedLoot;
    private void Awake()
    {
        //_spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        _gameManager = GameManager.instance;
        _spawnPosition = transform.position;
        _gameManager.LooterInitialized();
        initialMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        if (!isAlive)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                isAlive = true;
                //_spriteRenderer.enabled = true;
            }
            return;
        }
        
        if (!photonView.IsMine) return;

        if (_meleeTimer < _meleeTime)
        {
            _meleeTimer += Time.deltaTime;
        }
        
        if (isStunned)
        {
            stunTimer += Time.deltaTime;
            if (stunTimer >= stunTime)
            {
                isStunned = false;
                moveSpeed = initialMoveSpeed;
            }
        }

        if (!canBeStunned)
        {
            stunCooldownTimer += Time.deltaTime;
            if (stunCooldownTimer >= stunCooldownTime)
            {
                canBeStunned = true;
            }
        }
        
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePosition - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        if ((Input.GetKeyDown("v") || Input.GetMouseButtonDown(0)) && _meleeTimer >= _meleeTime)
        {
            MeleeAttack();
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void GetLoot(int value)
    {
        actual_loot = value;
    }

    public void DepositLoot()
    {
        total_loot += actual_loot;
        LevelManager.Instance.LooterDeposited(actual_loot, 1);
        actual_loot = 0;
    }

    private void MeleeAttack()
    {
        List<Collider2D> meleeHits = new List<Collider2D>();
        _meleeCollider.OverlapCollider(contactFilter2D, meleeHits);

        foreach (var hit in meleeHits)
        {
            PoliceController guard = hit.transform.gameObject.GetComponent<PoliceController>();
            if (guard != null)
            {
                Debug.Log("Wham!");
                guard.Stunned();
            }
        }
    }

    public void Hit()
    {
        //Debug.Log("Thief 1 hit!");
        photonView.RPC("RPC_Hit", RpcTarget.All);
    }

    public void Stunned()
    {
        //Debug.Log("Thief 1 whacked!");
        photonView.RPC("RPC_Stunned", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Hit()
    {
        if (photonView.IsMine)
        {
            //Debug.Log("I'm hit!");
        }
        if (_gameManager.CanRespawn())
        {
            _gameManager.LooterDied();
            isAlive = false;
            //_spriteRenderer.enabled = false;
            respawnTimer = 0;
            Respawn();
        }
        else
        {
            _gameManager.LooterPermaDied();
            isAlive = false;
            Destroy(gameObject);
        }
    }
    [PunRPC]
    private void RPC_Stunned()
    {
        if (photonView.IsMine)
        {
            Debug.Log("I'm whacked!");
        }
        if (canBeStunned)
        {
            canBeStunned = false;
            isStunned = true;
            stunTimer = 0f;
            stunCooldownTimer = 0f;
            moveSpeed = initialMoveSpeed / 2f;
        }
    }

    private void Respawn()
    {
        transform.position = _spawnPosition;
    }

    private readonly List<UpgradeData> misUpgrades = new List<UpgradeData>();
    public bool Pagar(int costo)
    {
        if (total_loot < costo) return false;
        total_loot -= costo;
        return true;
    }
    public void AñadirUpgrade(UpgradeData upg) => misUpgrades.Add(upg);
}
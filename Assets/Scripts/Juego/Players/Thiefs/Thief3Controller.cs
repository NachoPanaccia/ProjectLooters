using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class Thief3Controller : MonoBehaviourPunCallbacks, IRobber, IMovementProvider
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private AudioClip killed;
    private float initialMoveSpeed;
    private float respawnTimer;
    private bool isAlive;
    private Rigidbody2D rb;
    private AudioSource _audioSource;
    private Vector2 movement;
    private GameManager _gameManager;
    private Vector3 _spawnPosition;
    public Vector2 UltimaDireccion => movement;

    [Header("Melee")]
    [SerializeField] private float _meleeTime = 2f;
    [SerializeField] private Collider2D _meleeCollider;
    [SerializeField] ContactFilter2D contactFilter2D;
    private float _meleeTimer;
    [SerializeField] private AudioClip meleeHit;
    [SerializeField] private AudioClip meleeMiss;
    [SerializeField] private float meleeSoundRange = 15f;

    [SerializeField] private float stunTime = 1.5f;
    [SerializeField] private float stunCooldownTime = 3f;
    private float stunTimer;
    private bool isStunned;
    private float stunCooldownTimer;
    private bool canBeStunned;
    
    [Header("Loot Parameters")]
    [SerializeField] int actual_loot;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        _gameManager = GameManager.instance;
        _spawnPosition = transform.position;
        _gameManager.LooterInitialized();
        initialMoveSpeed = moveSpeed;

        if (photonView.IsMine)
        {
            FindObjectOfType<CameraFollow>().SetTarget(transform);
        }
    }

    private void Update()
    {
        if (!isAlive)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                isAlive = true;
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
        //total_loot = actual_loot;
        actual_loot = 0;
    }
    
    private void MeleeAttack()
    {
        List<Collider2D> meleeHits = new List<Collider2D>();
        _meleeCollider.OverlapCollider(contactFilter2D, meleeHits);

        if (meleeHits.Count == 0)
        {
            photonView.RPC("RPC_MeleeMiss", RpcTarget.All);
        }
        else
        {
            photonView.RPC("RPC_MeleeHit", RpcTarget.All);
        }
        
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
        photonView.RPC("RPC_Hit", RpcTarget.All);
    }
    
    public void Stunned()
    {
        photonView.RPC("RPC_Stunned", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Hit()
    {
        _audioSource.maxDistance = meleeSoundRange;
        _audioSource.PlayOneShot(killed, 0.7f);
        if (photonView.IsMine)
        {
            
        }
        if (_gameManager.CanRespawn())
        {
            _gameManager.LooterDied();
            isAlive = false;
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
    
    [PunRPC]
    private void RPC_MeleeMiss()
    {
        _audioSource.maxDistance = meleeSoundRange;
        _audioSource.PlayOneShot(meleeMiss, 0.7f);
    }
    
    [PunRPC]
    private void RPC_MeleeHit()
    {
        _audioSource.maxDistance = meleeSoundRange;
        _audioSource.PlayOneShot(meleeHit, 0.7f);
    }

    private void Respawn()
    {
        transform.position = _spawnPosition;
    }
}
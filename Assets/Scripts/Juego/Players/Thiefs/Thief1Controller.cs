using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView), typeof(LooterMovementController))]
public class Thief1Controller : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("Respawn")]
    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private AudioClip killed;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float respawnTimer;
    private bool isAlive = true;
    private Vector3 spawnPosition;

    [Header("Melee")]
    [SerializeField] private float meleeCooldown = 2f;
    [SerializeField] private Collider2D meleeCollider;
    [SerializeField] private ContactFilter2D contactFilter;
    private float meleeTimer;
    [SerializeField] private AudioClip meleeHit;
    [SerializeField] private AudioClip meleeMiss;
    [SerializeField] private float meleeSoundRange = 15f;

    [Header("Stun")]
    [SerializeField] private float stunTime = 1.5f;
    [SerializeField] private float stunCooldownTime = 3f;
    private bool canBeStunned = true;
    private float stunCooldownTimer;


    [Header("Events")]
    public UnityEvent<int> DepositedLoot;

    private Rigidbody2D rb;
    private AudioSource _audioSource;
    private GameManager gameManager;
    private LooterMovementController moveCtrl;

    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
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
            if (respawnTimer >= respawnTime)
            {
                Respawn();
                isAlive = true;
                photonView.RPC("RPC_Respawn", RpcTarget.All);
            }
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
        
        if (hits.Count == 0)
        {
            photonView.RPC("RPC_MeleeMiss", RpcTarget.All);
        }
        else
        {
            photonView.RPC("RPC_MeleeHit", RpcTarget.All);
        }

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PoliceController>(out var guard))
            {
                guard.Stunned();
                Debug.Log("Wham!");
            }
        }
    }
    
    
    
    public void Hit() => photonView.RPC(nameof(RPC_Hit), RpcTarget.All);
    public void Stunned() => photonView.RPC(nameof(RPC_Stunned), RpcTarget.All);

    [PunRPC]
    private void RPC_Hit()
    {
        if (!isAlive) return;
        isAlive = false;
        spriteRenderer.color = new Color(255, 0, 0, 140);
        
        _audioSource.maxDistance = meleeSoundRange;
        _audioSource.PlayOneShot(killed, 0.7f);
        if (gameManager.CanRespawn())
        {
            gameManager.LooterDied();
            GetComponent<IRobber>().LoseLoot();
            respawnTimer = 0;
        }
        else
        {
            gameManager.LooterPermaDied();
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

    [PunRPC]
    private void RPC_Respawn()
    {
        spriteRenderer.color = new Color(255, 255, 255, 255);
    }
    
    private void Respawn() => transform.position = spawnPosition;
}
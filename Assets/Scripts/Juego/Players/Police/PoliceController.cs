using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class PoliceController : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("Gun")] 
    [SerializeField] private LayerMask _noCopMask;
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip gunReloaded;
    [SerializeField] private float gunCooldownTime = 2.5f;
    [SerializeField] private float gunCooldownTimer;
    [SerializeField] private float gunSoundRange = 45f;
    private bool canFire;

    [Header("Melee")]
    [SerializeField] private float _meleeTime = 2f;
    [SerializeField] private Collider2D _meleeCollider;
    [SerializeField] ContactFilter2D contactFilter2D;
    private float _meleeTimer;
    [SerializeField] private AudioClip meleeHit;
    [SerializeField] private AudioClip meleeMiss;
    [SerializeField] private float meleeSoundRange = 15f;
    
    [Header("Stun")]
    [SerializeField] private float stunTime = 1.5f;
    [SerializeField] private float stunCooldownTime = 3f;
    private float stunCooldownTimer;
    private bool canBeStunned;
    private float stunTimer;
    private bool isStunned;
    
    [SerializeField] private float moveSpeed = 5f;
    private float initialMoveSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        initialMoveSpeed = moveSpeed;

        if (photonView.IsMine)
        {
            FindObjectOfType<CameraFollow>().SetTarget(transform);
        }
    }

    private void Update()
    {
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

        if (!canFire)
        {
            gunCooldownTimer += Time.deltaTime;
            if (gunCooldownTimer >= gunCooldownTime)
            {
                canFire = true;
                photonView.RPC("RPC_GunReloaded", RpcTarget.All);
            }
        }
        
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePosition - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Input.GetMouseButtonDown(0) && !isStunned && canFire)
        {
            Shoot(lookDir);
        }

        if (Input.GetKeyDown("v") && _meleeTimer >= _meleeTime)
        {
            MeleeAttack();
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void Shoot(Vector3 mousePos)
    {
        canFire = false;
        gunCooldownTimer = 0f;
        photonView.RPC("RPC_ShotFired", RpcTarget.All);
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);
        Vector2 pos2d = new Vector2(transform.position.x, transform.position.y);
        
        RaycastHit2D hit = Physics2D.Raycast(pos2d, mousePos2d, Mathf.Infinity,_noCopMask);
        
        if (hit != false)
        {
            IDamageable robber = hit.transform.gameObject.GetComponent<IDamageable>();
            if (robber != null)
            {
                Debug.Log("I got him!");
                robber.Hit();
            }
        }
        
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
            IDamageable robber = hit.transform.gameObject.GetComponent<IDamageable>();
            if (robber != null)
            {
                robber.Stunned();
            }
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
    private void RPC_ShotFired()
    {
        _audioSource.maxDistance = gunSoundRange;
        _audioSource.PlayOneShot(gunShot, 0.7f);
    }
    
    [PunRPC]
    private void RPC_GunReloaded()
    {
        _audioSource.maxDistance = meleeSoundRange;
        _audioSource.PlayOneShot(gunReloaded, 0.7f);
    }

    public void Hit()
    {

    }

    public void Stunned()
    {
        photonView.RPC("RPC_Stunned", RpcTarget.All);
    }
}
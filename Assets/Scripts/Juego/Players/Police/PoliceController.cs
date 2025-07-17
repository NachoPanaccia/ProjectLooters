using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class PoliceController : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("Gun")] 
    [SerializeField] private LayerMask _noCopMask;
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip gunReloaded;
    [SerializeField] private float gunSoundRange = 45f;
    [SerializeField] private GameObject smokeEffect;
    private float _gunCooldownTimer;
    private bool _canFire;
    private int _loadedBullets = 1;

    [SerializeField] private WeaponUpgradeData currentWeapon;
    public WeaponUpgradeData CurrentWeapon
    {
        get { return currentWeapon; }
        set
        {
            _uiManager.UpdateMaxBullets(value.magazineSize);
            currentWeapon = value;
        }
    }

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
    private float _stunCooldownTimer;
    private bool _canBeStunned;
    private float _stunTimer;
    private bool _isStunned;

    [Header("General")]
    [SerializeField] private bool isAlive = true;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private AudioClip killed;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float _initialMoveSpeed;
    private Rigidbody2D _rb;
    private Vector2 _movement;
    private AudioSource _audioSource;
    private UIManager _uiManager;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody2D>();
        _initialMoveSpeed = moveSpeed;
        _uiManager = UIManager.instance;

        if (photonView.IsMine)
        {
            FindObjectOfType<CameraFollow>().SetTarget(transform);
        }
        else
        {
            _uiManager.HideBulletCount();
            _uiManager.ShowLootInventory();
        }
    }

    private void Update()
    {
        if (!photonView.IsMine || !isAlive) return;

        if (_meleeTimer < _meleeTime)
        {
            _meleeTimer += Time.deltaTime;
        }
        
        if (_isStunned)
        {
            _stunTimer += Time.deltaTime;
            if (_stunTimer >= stunTime)
            {
                _isStunned = false;
                moveSpeed = _initialMoveSpeed;
            }
        }

        if (!_canBeStunned)
        {
            _stunCooldownTimer += Time.deltaTime;
            if (_stunCooldownTimer >= stunCooldownTime)
            {
                _canBeStunned = true;
            }
        }

        if (!_canFire)
        {
            _gunCooldownTimer += Time.deltaTime;
            if (_loadedBullets == 0)
            {
                if (_gunCooldownTimer >= currentWeapon.reloadTime)
                {
                    _canFire = true;
                    _loadedBullets = currentWeapon.magazineSize;
                    photonView.RPC("RPC_GunReloaded", RpcTarget.All);
                    _uiManager.UpdateBullets(_loadedBullets);
                }
            }
            else
            {
                if (_gunCooldownTimer >= 1 / currentWeapon.fireRate)
                {
                    _canFire = true;
                    photonView.RPC("RPC_GunReloaded", RpcTarget.All);
                }
            }
            /*gunCooldownTimer += Time.deltaTime;
            if (gunCooldownTimer >= gunCooldownTime)
            {
                canFire = true;
                photonView.RPC("RPC_GunReloaded", RpcTarget.All);
            }*/
        }
        
        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePosition - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Input.GetMouseButtonDown(0) && !_isStunned && _canFire)
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

        _rb.MovePosition(_rb.position + _movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void Shoot(Vector3 mouseDir)
    {
        _canFire = false;
        _loadedBullets--;
        _uiManager.UpdateBullets(_loadedBullets);
        _gunCooldownTimer = 0f;
        
        Vector2 pos2d = new Vector2(transform.position.x, transform.position.y);
        Vector2 mouseDir2d = new Vector2(mouseDir.x, mouseDir.y).normalized;

        Vector2[] hitPoints = new Vector2[currentWeapon.projectileNumber];

        for (int i = 0; i < currentWeapon.projectileNumber; i++)
        {
            float spreadAngle = Random.Range(-currentWeapon.spread / 2f, currentWeapon.spread / 2f);
            Vector2 shotDir = Quaternion.AngleAxis(spreadAngle, Vector3.forward) * mouseDir2d;
        
            RaycastHit2D hit = Physics2D.Raycast(pos2d, shotDir, Mathf.Infinity,_noCopMask);
            //Debug.DrawLine(pos2d, hit.point, Color.red, 10.0f);
        
            if (hit != false)
            {
                hitPoints[i] = hit.point;
                //Instantiate(smokeEffect, hit.point, transform.rotation);
                IDamageable robber = hit.transform.gameObject.GetComponent<IDamageable>();
                if (robber != null)
                {
                    Debug.Log("I got him!");
                    robber.Hit();
                }
            }
        }
        
        photonView.RPC("RPC_ShotFired", RpcTarget.All, hitPoints as Vector2[]);
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
        if (_canBeStunned)
        {
            _canBeStunned = false;
            _isStunned = true;
            _stunTimer = 0f;
            _stunCooldownTimer = 0f;
            moveSpeed = _initialMoveSpeed / 2f;
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
    private void RPC_ShotFired(Vector2[] hitPoints)
    {
        _audioSource.maxDistance = gunSoundRange;
        _audioSource.PlayOneShot(gunShot, 0.7f);
        foreach(Vector2 hit in hitPoints)
        {
            Instantiate(smokeEffect, hit, transform.rotation);
        }
    }
    
    [PunRPC]
    private void RPC_GunReloaded()
    {
        _audioSource.maxDistance = meleeSoundRange;
        _audioSource.PlayOneShot(gunReloaded, 0.7f);
    }

    public void Hit() => photonView.RPC(nameof(RPC_Hit), RpcTarget.All);

    [PunRPC]
    private void RPC_Hit()
    {
        float gameoverDelay = 2f;
        isAlive = false;
        _audioSource.maxDistance = meleeSoundRange;
        _audioSource.PlayOneShot(killed, 0.7f);
        spriteRenderer.color = new Color(255, 0, 0, 140);
        Debug.Log("Mataron al guardia!");
        StartCoroutine(PoliceDeath(gameoverDelay));
    }
    
    IEnumerator PoliceDeath(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        GameManager.instance.PoliceDied();
    }

    public void Stunned()
    {
        photonView.RPC("RPC_Stunned", RpcTarget.All);
    }
}
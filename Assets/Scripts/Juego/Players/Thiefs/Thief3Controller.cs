using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView), typeof(LooterMovementController))]
public class Thief3Controller : MonoBehaviourPunCallbacks, IDamageable
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
    
    [Header("Gun")] 
    [SerializeField] private LayerMask _noLooterMask;
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private float gunSoundRange = 45f;
    [SerializeField] private GameObject smokeEffect;
    [SerializeField] private float spread;
    [SerializeField] private bool canFire;


    [Header("Events")]
    public UnityEvent<int> DepositedLoot;

    private Rigidbody2D rb;
    private AudioSource _audioSource;
    private UIManager _uiManager;
    private GameManager gameManager;
    private LooterMovementController moveCtrl;
    
    //ELIMINAR ESTA LINEA CUANDO HAYA UN SOLO THIEFCONTROLLER
    private LooterUpgradeHandler _upgradeHandler;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        moveCtrl = GetComponent<LooterMovementController>();
        _uiManager = UIManager.instance;
        gameManager = GameManager.instance;
        spawnPosition = transform.position;
        
        //ELIMINAR ESTAS DOS LINEAS CUANDO HAYA UN SOLO THIEFCONTROLLER
        _upgradeHandler = GetComponent<LooterUpgradeHandler>();
        _upgradeHandler._thief3Controller = this;

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
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePosition - transform.position;

        // Input Melee
        if ((Input.GetKeyDown(KeyCode.V) || (Input.GetMouseButtonDown(0) && !canFire)) && meleeTimer >= meleeCooldown)
            MeleeAttack();
        if (Input.GetMouseButtonDown(0) && canFire) Shoot(lookDir);
    }

    private void Shoot(Vector3 mouseDir)
    {
        canFire = false;
        _uiManager.HideBulletCount();
        //_uiManager.UpdateBullets(_loadedBullets);
        //_gunCooldownTimer = 0f;

        Vector2 pos2d = new Vector2(transform.position.x, transform.position.y);
        Vector2 mouseDir2d = new Vector2(mouseDir.x, mouseDir.y).normalized;

        Vector2 hitPoint = new Vector2(); 
        float spreadAngle = Random.Range(-spread / 2f, spread / 2f);
        Vector2 shotDir = Quaternion.AngleAxis(spreadAngle, Vector3.forward) * mouseDir2d;

        RaycastHit2D hit = Physics2D.Raycast(pos2d, shotDir, Mathf.Infinity, _noLooterMask);
        //Debug.DrawLine(pos2d, hit.point, Color.red, 10.0f);

        if (hit != false)
        {
            hitPoint = hit.point;
            //Instantiate(smokeEffect, hit.point, transform.rotation);
            IDamageable guard = hit.transform.gameObject.GetComponent<IDamageable>();
            if (guard != null) 
            {
                Debug.Log("I got him!"); 
                guard.Hit();
            }
        }
        
        photonView.RPC("RPC_ShotFired", RpcTarget.All, hitPoint);
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

    public void EnableFirearm()
    {
        canFire = true;
        _uiManager.UpdateBullets(1);
        _uiManager.UpdateMaxBullets(0);
        _uiManager.ShowBulletCount();
    }

    public void Hit() => photonView.RPC(nameof(RPC_Hit), RpcTarget.All);
    public void Stunned() => photonView.RPC(nameof(RPC_Stunned), RpcTarget.All);
    
    [PunRPC]
    private void RPC_ShotFired(Vector2 hitPoint)
    {
        _audioSource.maxDistance = gunSoundRange;
        _audioSource.PlayOneShot(gunShot, 0.7f);
        Instantiate(smokeEffect, hitPoint, transform.rotation);
    }

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
            isAlive = false;
            respawnTimer = 0f;
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
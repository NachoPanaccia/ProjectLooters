using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class PoliceController : MonoBehaviourPunCallbacks, IShopClient
{
    [SerializeField] private float moveSpeed = 5f;
    private float initialMoveSpeed;

    [SerializeField] private int currentMoney = 0;
    private readonly List<UpgradeData> misUpgrades = new List<UpgradeData>();

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Gun Parameters")] 
    [SerializeField] private LayerMask _noCopMask;

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
    
    private void Awake()
    {
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
        
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePosition - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Input.GetMouseButtonDown(0) && !isStunned)
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
        //Debug.Log("Bang!");
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);
        Vector2 pos2d = new Vector2(transform.position.x, transform.position.y);

        RaycastHit2D hit = Physics2D.Raycast(pos2d, mousePos2d, Mathf.Infinity,_noCopMask);
        
        if (hit != false)
        {
            //Debug.Log("Hit: " + hit.transform.gameObject.name);
            IRobber robber = hit.transform.gameObject.GetComponent<IRobber>();
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

        foreach (var hit in meleeHits)
        {
            IRobber robber = hit.transform.gameObject.GetComponent<IRobber>();
            if (robber != null)
            {
                //Debug.Log("Wham!");
                robber.Stunned();
            }
        }
    }

    public void Stunned()
    {
        //Debug.Log("Guard whacked!");
        photonView.RPC("RPC_Stunned", RpcTarget.All);
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

    public int Dinero => currentMoney;
    public bool Pagar(int costo)
    {
        if (currentMoney < costo) return false;
        currentMoney -= costo;
        return true;
    }
    public void AñadirUpgrade(UpgradeData upg) => misUpgrades.Add(upg);
}
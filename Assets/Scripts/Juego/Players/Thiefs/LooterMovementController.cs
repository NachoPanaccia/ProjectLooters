using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class LooterMovementController : MonoBehaviourPun, IMovementProvider
{
    [Header("Defaults")]
    [SerializeField] private float defaultSpeed = 5f;
    [SerializeField] private bool defaultCanDash = false;
    [SerializeField] private float defaultDashCD = 6f;
    [SerializeField] private float defaultDashForce = 7f;
    [SerializeField] private float dashDuration = 0.18f;
    private float baseSpeed;
    private bool canDash;
    private float dashCooldown;
    private float dashForce;
    private float nextDashTime;
    private bool isDashing;
    private float dashTimer;
    private Vector2 dashDir;


    private Vector2 moveInput;
    private Rigidbody2D rb;

    
    private bool isStunned;
    private float stunTimer;
    private float stunDuration;

    
    public Vector2 UltimaDireccion { get; private set; } = Vector2.up;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetMovementDefaults();
    }

    
    private void Update()
    {
        if (!photonView.IsMine) return;

        
        if (isStunned)
        {
            stunTimer += Time.deltaTime;
            if (stunTimer >= stunDuration) isStunned = false;
            return; // sin input mientras está aturdido
        }

        
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (moveInput != Vector2.zero) UltimaDireccion = moveInput;


        if (canDash && !isDashing && Time.time >= nextDashTime && Input.GetKeyDown(KeyCode.Space))
            StartDash();


        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }


    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (isStunned)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            dashTimer += Time.fixedDeltaTime;
            rb.velocity = dashDir * dashForce;

            if (dashTimer >= dashDuration)
                isDashing = false;
        }
        else
        {
            rb.velocity = moveInput * baseSpeed;
        }
    }

    public void ConfigureDash(bool enabled, float cooldown, float fuerza)
    {
        canDash = enabled;
        dashCooldown = cooldown;
        dashForce = fuerza;
    }

    
    public void SetSpeed(float speed) => baseSpeed = speed;

    
    public void ResetMovementDefaults()
    {
        baseSpeed = defaultSpeed;
        ConfigureDash(defaultCanDash, defaultDashCD, defaultDashForce);
    }

    public void SetStunned(float duration)
    {
        isStunned = true;
        stunDuration = duration;
        stunTimer = 0f;

        // cancela dash en curso y frena al jugador
        isDashing = false;
        rb.velocity = Vector2.zero;
    }

    private void StartDash()
    {
        dashDir = UltimaDireccion == Vector2.zero ? Vector2.up : UltimaDireccion;
        isDashing = true;
        dashTimer = 0f;
        nextDashTime = Time.time + dashCooldown;
    }
}

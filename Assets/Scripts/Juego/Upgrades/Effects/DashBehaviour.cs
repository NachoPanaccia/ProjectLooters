using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class DashBehaviour : MonoBehaviourPunCallbacks
{
    [HideInInspector] public float cooldown = 6f;
    [HideInInspector] public float impulso = 7f;

    private float proxDash = 0f;
    private Rigidbody2D rb;
    private IMovementProvider input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<IMovementProvider>();
    }

    private void Update()
    {
        if (!photonView.IsMine || !enabled) return;

        if (Time.time >= proxDash && Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 dir = input.UltimaDireccion.normalized;
            if (dir == Vector2.zero) dir = Vector2.up;

            Debug.Log($"Dash -> dir={dir}  fuerza={impulso}");

            rb.AddForce(dir * impulso, ForceMode2D.Impulse);
            proxDash = Time.time + cooldown;
        }
    }
}

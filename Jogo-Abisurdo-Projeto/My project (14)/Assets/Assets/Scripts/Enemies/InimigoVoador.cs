using System.Collections;
using UnityEngine;

public class InimigoVoador : MonoBehaviour
{
    public Transform player;
    public float deteccaoRange = 10f;
    public float velocidadeVoo = 3f;
    public float velocidadeAtaque = 12f;
    public float velocidadeRetorno = 4f;

    [Header("Vida")]
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    [Header("Chão")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private bool isGrounded;

    [Header("Ataque")]
    public float tempoDeAviso = 0.6f;
    public float turnSpeed = 5f;
    public float tempoMaximoDive = 1.5f;
    private float tempoDiveAtual = 0f;

    public float cooldownAtaque = 1.5f;
    private float tempoCooldown = 0f;

    private Vector2 startPosition;
    private Vector2 direcaoAtaque;

    private bool isDiving = false;
    private bool retornando = false;
    private bool preparandoAtaque = false;

    private Rigidbody2D rb;

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();

        float distancia = Vector2.Distance(transform.position, player.position);

        tempoCooldown -= Time.deltaTime;

        if (!isDiving && !retornando && !preparandoAtaque)
        {
            Patrulhar();

            if (distancia <= deteccaoRange && tempoCooldown <= 0f)
            {
                StartCoroutine(PrepararAtaque());
            }
        }

        if (isDiving) Dive();
        if (retornando) ReturnToStart();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isGrounded && rb.velocity.y < 0)
            rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    void CheckGround()
    {
        if (groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void Patrulhar()
    {
        float movimentoX = Mathf.Sin(Time.time) * velocidadeVoo;
        rb.velocity = new Vector2(movimentoX, 0);
    }

    IEnumerator PrepararAtaque()
    {
        if (preparandoAtaque || isDead) yield break;

        preparandoAtaque = true;

        direcaoAtaque = (player.position - transform.position).normalized;

        yield return new WaitForSeconds(tempoDeAviso);

        isDiving = true;
        preparandoAtaque = false;
        tempoDiveAtual = 0f;
    }

    void Dive()
    {
        tempoDiveAtual += Time.deltaTime;

        Vector2 direcaoDesejada = ((Vector2)player.position - rb.position).normalized;

        float maxTurn = turnSpeed * Time.deltaTime;

        direcaoAtaque = Vector2.Lerp(direcaoAtaque, direcaoDesejada, maxTurn).normalized;

        Vector2 novaVelocidade = direcaoAtaque * velocidadeAtaque;

        if (isGrounded && novaVelocidade.y < 0)
            novaVelocidade.y = 0;

        rb.velocity = novaVelocidade;

        if (Vector2.Distance(rb.position, player.position) < 0.5f)
            FinalizarDive();

        if (tempoDiveAtual >= tempoMaximoDive)
            FinalizarDive();
    }

    void ReturnToStart()
    {
        Vector2 direcao = (startPosition - rb.position).normalized;
        Vector2 vel = direcao * velocidadeRetorno;

        if (isGrounded && vel.y < 0)
            vel.y = 0;

        rb.velocity = vel;

        if (Vector2.Distance(rb.position, startPosition) < 0.1f)
        {
            retornando = false;
            rb.velocity = Vector2.zero;
        }
    }

    void FinalizarDive()
    {
        isDiving = false;
        retornando = true;
        tempoDiveAtual = 0f;
        tempoCooldown = cooldownAtaque;
    }


    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        rb.velocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StopAllCoroutines();

        Destroy(gameObject);
    }
}
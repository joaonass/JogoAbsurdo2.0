using System.Collections;
using UnityEngine;

public class InimigoVoador : MonoBehaviour
{

    [Header("Detecção e Voo")]
    public Transform player;
    public float deteccaoRange    = 10f;
    public float velocidadeVoo    = 3f;
    public float velocidadeAtaque = 12f;
    public float velocidadeRetorno = 4f;

    [Header("Ataque")]
    public float tempoDeAviso   = 0.6f;
    public float turnSpeed      = 5f;
    public float tempoMaximoDive = 1.5f;
    public float cooldownAtaque  = 1.5f;

    float tempoDiveAtual = 0f;
    float tempoCooldown  = 0f;

    Vector2 startPosition;
    Vector2 direcaoAtaque;

    bool isDiving         = false;
    bool retornando       = false;
    bool preparandoAtaque = false;

    [Header("Vida")]
    public int maxHealth    = 50;
    int currentHealth;
    bool isDead = false;

    [Header("Chão")]
    public LayerMask  groundLayer;
    public Transform  groundCheck;
    public float      groundCheckRadius = 0.2f;

    bool isGrounded;

        Rigidbody2D rb;

        void Start()
    {
        rb            = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();

        tempoCooldown -= Time.deltaTime;

        float distancia = Vector2.Distance(transform.position, player.position);

        if (!isDiving && !retornando && !preparandoAtaque)
        {
            Patrulhar();

            if (distancia <= deteccaoRange && tempoCooldown <= 0f)
                StartCoroutine(PrepararAtaque());
        }

        if (isDiving)    Dive();
        if (retornando)  ReturnToStart();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Impede que o inimigo atravesse o chão enquanto toma velocidade negativa
        if (isGrounded && rb.velocity.y < 0f)
            rb.velocity = new Vector2(rb.velocity.x, 0f);
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
        rb.velocity = new Vector2(movimentoX, 0f);
    }

    IEnumerator PrepararAtaque()
    {
        if (preparandoAtaque || isDead) yield break;

        preparandoAtaque = true;
        direcaoAtaque    = (player.position - transform.position).normalized;

        yield return new WaitForSeconds(tempoDeAviso);

        if (isDead) yield break;   // morreu durante o aviso

        isDiving         = true;
        preparandoAtaque = false;
        tempoDiveAtual   = 0f;
    }

    void Dive()
    {
        tempoDiveAtual += Time.deltaTime;

        Vector2 direcaoDesejada = ((Vector2)player.position - rb.position).normalized;
        direcaoAtaque = Vector2.Lerp(direcaoAtaque, direcaoDesejada, turnSpeed * Time.deltaTime).normalized;

        Vector2 novaVelocidade = direcaoAtaque * velocidadeAtaque;

        // Não empurra contra o chão
        if (isGrounded && novaVelocidade.y < 0f)
            novaVelocidade.y = 0f;

        rb.velocity = novaVelocidade;

        if (Vector2.Distance(rb.position, player.position) < 0.5f || tempoDiveAtual >= tempoMaximoDive)
            FinalizarDive();
    }

    void ReturnToStart()
    {
        Vector2 direcao = (startPosition - rb.position).normalized;
        Vector2 vel     = direcao * velocidadeRetorno;

        if (isGrounded && vel.y < 0f)
            vel.y = 0f;

        rb.velocity = vel;

        if (Vector2.Distance(rb.position, startPosition) < 0.1f)
        {
            retornando  = false;
            rb.velocity = Vector2.zero;
        }
    }

    void FinalizarDive()
    {
        isDiving       = false;
        retornando     = true;
        tempoDiveAtual = 0f;
        tempoCooldown  = cooldownAtaque;
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
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        rb.velocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Range de detecção
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deteccaoRange);

        // Checagem de chão
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
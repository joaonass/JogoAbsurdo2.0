using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    enum PlayerState
    {
        Normal,
        Attack,
        Defense,
        Knockback,
        Dead
    }

    [Header("Movimento")]
    public float speed = 6f;
    public float jumpForce = 10f;
    public float glideFallSpeed = 1.2f;
    public float forcaKnockback = 12f;

    [Header("Knockback Avançado")]
    public float forcaVertical = 6f;
    public float multiplicadorAr = 1.0f;
    public float tempoKnockback = 0.3f;

    [Header("Pulo Responsivo")]
    public float coyoteTime = 0.15f;
    float coyoteTimeCounter;

    float jumpBufferCounter;
    public float jumpBufferTime = 0.15f;

    [Header("Checagem de Chão")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public Vector2 groundBoxSize = new Vector2(0.7f, 0.1f);
    public bool isGrounded;

    [Header("Combate")]
    public float attackDuration = 0.3f;
    public float defendDuration = 0.5f;
    public int health = 5;

    [Header("Ataque")]
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask enemyLayer;
    public int attackDamage = 1;

    [Header("Invencibilidade")]
    public float tempoInvencibilidade = 1.5f;

    [Header("Componentes")]
    public Animator animator;
    public Animator animacao;
    public Rigidbody2D rb;

    PlayerCombat combat;

    PlayerState currentState = PlayerState.Normal;

    bool faceRight = true;
    float inputX;

    bool isAttacking = false;
    public bool isDefending = false;
    bool levandoKnockback = false;
    bool podeTomarDano = true;

    public int limitador_de_pulo = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animacao = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
    }

    void Start()
    {
        rb.gravityScale = 3f;
    }

    void Update()
    {
        if (levandoKnockback)
        {
            return; // ou bloqueia movimento
        }

        if (currentState == PlayerState.Dead)
            return;

        VerificarChao();
        AtualizarTimers();
        LerInput();
        Movimento();
        Pulo();
        Glide();
        Defesa();
        Ataque();
        AtualizarAnimacoes();
        VerificarMorte();
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Dead)
            return;

        if (currentState == PlayerState.Normal)
        {
            rb.velocity = new Vector2(inputX * speed, rb.velocity.y);
        }
    }

    void LerInput()
    {
        if (currentState == PlayerState.Normal && (combat == null || !combat.estaAtordoado))
            inputX = Input.GetAxisRaw("Horizontal");
        else
            inputX = 0;

        if (inputX > 0 && !faceRight) Flip();
        else if (inputX < 0 && faceRight) Flip();

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
    }

    void Movimento()
    {
        if (currentState == PlayerState.Defense)
            rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void VerificarChao()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        animator.SetBool("isGrounded", isGrounded);
        animacao.SetBool("isGrounded", isGrounded);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            limitador_de_pulo = 1;
        }
    }

    void AtualizarTimers()
    {
        coyoteTimeCounter -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }

    void Pulo()
    {
        if (jumpBufferCounter > 0 &&
            (coyoteTimeCounter > 0 || isGrounded) &&
            limitador_de_pulo > 0 &&
            currentState != PlayerState.Knockback &&
            (combat == null || !combat.estaAtordoado))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0;
            limitador_de_pulo--;
        }
    }

    void Glide()
    {
        if (!isGrounded && Input.GetKey(KeyCode.Space) && rb.velocity.y < 0)
            rb.velocity = new Vector2(rb.velocity.x, -glideFallSpeed);
    }

    void Ataque()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isDefending)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        currentState = PlayerState.Attack;

        animator.SetBool("atacando", true);
        animacao.SetBool("atacando", true);

        yield return new WaitForSeconds(attackDuration);

        animator.SetBool("atacando", false);
        animacao.SetBool("atacando", false);

        isAttacking = false;
        currentState = PlayerState.Normal;
    }

    public void DarDano()
    {
        Collider2D[] inimigos = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D inimigo in inimigos)
        {
            InimigoVoador script = inimigo.GetComponent<InimigoVoador>();

            if (script != null)
            {
                script.TakeDamage(attackDamage);
            }
        }
    }




    void Defesa()
    {
        if (Input.GetMouseButtonDown(1) && isGrounded && !isAttacking)
        {
            isDefending = true;
            currentState = PlayerState.Defense;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDefending = false;
            currentState = PlayerState.Normal;
        }
    }

    void AtualizarAnimacoes()
    {
        animator.SetBool("correr", inputX != 0);
        animacao.SetBool("correr", inputX != 0);

        animator.SetBool("pulando", rb.velocity.y > 0.1f && !isGrounded);
        animacao.SetBool("pulando", rb.velocity.y > 0.1f && !isGrounded);

        animator.SetBool("caindo", rb.velocity.y < -0.1f && !isGrounded);
        animacao.SetBool("caindo", rb.velocity.y < -0.1f && !isGrounded);

        animator.SetBool("defendendo", isDefending);
        animacao.SetBool("defendendo", isDefending);
    }

    public void TakeDamage(int dano, Transform inimigo)
    {
        if (!podeTomarDano || currentState == PlayerState.Dead)
            return;

        if (isDefending)
        {
            Debug.Log("Defendeu!");
            return;
        }

        if (inimigo == null)
            return;

        health -= dano;

        StartCoroutine(InvencibilidadeCoroutine());

        float direcao = (transform.position.x - inimigo.position.x) >= 0 ? 1 : -1;

        float forcaFinal = forcaKnockback;
        if (!isGrounded)
            forcaFinal *= multiplicadorAr;

        rb.velocity = Vector2.zero;

        rb.AddForce(
            new Vector2(direcao * forcaFinal, forcaVertical),
            ForceMode2D.Impulse
        );

        StartCoroutine(KnockbackCoroutine());
        StartCoroutine(HitStop(0.05f));
    }

    IEnumerator InvencibilidadeCoroutine()
    {
        podeTomarDano = false;
        yield return new WaitForSeconds(tempoInvencibilidade);
        podeTomarDano = true;
    }

    IEnumerator KnockbackCoroutine()
    {
        currentState = PlayerState.Knockback;
        levandoKnockback = true;

        yield return new WaitForSeconds(tempoKnockback);

        levandoKnockback = false;
        currentState = PlayerState.Normal;
    }

    IEnumerator HitStop(float tempo)
    {
        float original = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(tempo);

        Time.timeScale = original;
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void VerificarMorte()
    {
        if (health <= 0)
        {
            currentState = PlayerState.Dead;
            SceneManager.LoadScene(0);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Inimigo"))
        {
            TakeDamage(1, col.transform);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundBoxSize);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
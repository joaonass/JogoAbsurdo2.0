using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 6f;
    public float jumpForce = 10f;
    public float glideFallSpeed = 1.2f;
    public float forcaKnockback = 12f;

    [Header("Knockback Avançado")]
    public float forcaVertical = 6f;
    public float multiplicadorAr = 1.3f;
    public float tempoKnockback = 0.3f;

    [Header("Pulo Responsivo")]
    public float coyoteTime = 0.15f;
    float coyoteTimeCounter;

    [Header("Checagem de Chão")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public bool isGrounded;

    [Header("Combate")]
    public float attackDuration = 0.3f;
    public float defendDuration = 0.5f;
    public int health = 5;
    public Transform attackPoint;
    public float attackRange = 1f;
    public LayerMask enemyLayers;

    public int attackDamage = 1;

    public GameObject gameOverPanel;

    public Animator animator;
    public Rigidbody2D rb;

    PlayerCombat combat;

    bool faceRight = true;
    float inputX;
    public Animator animacao;

    bool isAttacking = false;
    public bool isDefending = false;
    bool levandoKnockback = false;
    bool podeTomarDano = true;
    bool morreu = false;
    BoxCollider2D box;


    public int limitador_de_pulo = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        combat = GetComponent<PlayerCombat>();
        animacao = GetComponent<Animator>();
        rb.gravityScale = 3f;

        box = GetComponent<BoxCollider2D>();

    }

    void Update()
    {
        if (morreu)
            return;

        if (!isDefending && !combat.estaAtordoado && !levandoKnockback)
        {
            inputX = Input.GetAxisRaw("Horizontal");
        }
        else if (isDefending)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (inputX > 0 && !faceRight) Flip();
        else if (inputX < 0 && faceRight) Flip();

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animacao.SetBool("isGrounded", isGrounded);

        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && limitador_de_pulo > 0 && !combat.estaAtordoado)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            limitador_de_pulo--;
        }

        if (!isGrounded && Input.GetKey(KeyCode.Space) && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, -glideFallSpeed);
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking && !isDefending && isGrounded)
        {
            StartCoroutine(Attack());
        }

        if (Input.GetMouseButtonDown(1) && isGrounded && !isAttacking)
        {
            isDefending = true;
            inputX = 0;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDefending = false;
            inputX = 0;
        }

        animacao.SetBool("correr", inputX != 0);
        animacao.SetBool("pulando", rb.velocity.y > 0.1f && !isGrounded);
        animacao.SetBool("caindo", rb.velocity.y < -0.1f && !isGrounded);
        animacao.SetBool("defendendo", isDefending);
    }

    void FixedUpdate()
    {
        if (!isAttacking && !isDefending && !combat.estaAtordoado && !levandoKnockback)
        {
            rb.velocity = new Vector2(inputX * speed, rb.velocity.y);
        }
        else if (isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (morreu)
            return;

        morreu = true;

        isAttacking = false;
        isDefending = false;

        rb.velocity = Vector2.zero;

        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        Vector2 tamanhoAtual = box.size;

        box.size = new Vector2(tamanhoAtual.x, 0.5f);
        animacao.SetBool("morreu", true);

        StartCoroutine(GameOverDelay());

    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(0.5f);

        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("atacando", true);
        }

        yield return null;
    }
    public void EndAttack()
    {
        isAttacking = false;

        if (animator != null)
        {
            animator.SetBool("atacando", false);
        }
    }

    public void Hit()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>().TakeDamage(attackDamage);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Inimigo"))
        {
            TakeDamage(1, col.transform);
        }
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.CompareTag("Chao"))
        {
            limitador_de_pulo = 1;
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int dano, Transform inimigo)
    {
        if (!podeTomarDano) return;

        if (isDefending)
        {
            Debug.Log("Defendeu o ataque!");
            return;
        }

        podeTomarDano = false;

        health -= dano;
        Debug.Log("Vida atual: " + health);

        float direcao = Mathf.Sign(transform.position.x - inimigo.position.x);

        float forcaFinal = forcaKnockback;

        if (!isGrounded)
            forcaFinal *= multiplicadorAr;

        rb.velocity = Vector2.zero;

        rb.AddForce(new Vector2(direcao * forcaFinal, forcaVertical), ForceMode2D.Impulse);

        StartCoroutine(KnockbackCoroutine());
        StartCoroutine(HitStop(0.05f)); // 💥 impacto
    }

    IEnumerator KnockbackCoroutine()
    {
        levandoKnockback = true;

        yield return new WaitForSeconds(tempoKnockback);

        levandoKnockback = false;
        podeTomarDano = true;
    }

    IEnumerator HitStop(float tempo)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(tempo);
        Time.timeScale = 1f;
    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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

    public Rigidbody2D rb;

    PlayerCombat combat;

    bool faceRight = true;
    float inputX;
    public Animator animacao;

    bool isAttacking = false;
    public bool isDefending = false;
    bool levandoKnockback = false;
    bool podeTomarDano = true;

    public int limitador_de_pulo = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        combat = GetComponent<PlayerCombat>();
        animacao = GetComponent<Animator>();

        rb.gravityScale = 3f;
    }

    void Update()
    {
        if (!isAttacking && !isDefending && !combat.estaAtordoado && !levandoKnockback)
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

        if (Input.GetMouseButtonDown(0) && !isAttacking && !isDefending)
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

        if (health <= 0)
        {
            SceneManager.LoadScene(0);
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
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
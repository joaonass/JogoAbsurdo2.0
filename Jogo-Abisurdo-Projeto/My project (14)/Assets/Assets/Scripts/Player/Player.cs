using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    enum PlayerState { Normal, Attack, Defense, Knockback, Dead }
    PlayerState currentState = PlayerState.Normal;


    [Header("Movimento")]
    public float speed = 6f;
    public float jumpForce = 10f;
    public float glideFallSpeed = 1.2f;

    [Header("Pulo Responsivo")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    float coyoteTimeCounter;
    float jumpBufferCounter;
    public int limitador_de_pulo = 1;

    [Header("Checagem de Chão")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public float voidHeight = -6.5f;
    public bool isGrounded;

    [Header("Knockback")]
    public float forcaKnockback = 12f;
    public float forcaVertical = 6f;
    public float multiplicadorAr = 1.0f;
    public float tempoKnockback = 0.3f;

    [Header("Combate")]
    public int health = 5;
    public float attackDuration = 0.3f;

    [Header("LifeBar")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;


    [Header("Ataque")]
    public Transform attackPoint;
    public float     attackRange  = 1f;
    public LayerMask enemyLayers;
    public int       attackDamage = 1;

    [Header("Invencibilidade")]
    public float tempoInvencibilidade = 1.5f;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    [Header("Componentes")]
    public Animator      animator;   // referência principal (usada em Animation Events)
    public Rigidbody2D   rb;

    [Header("Ataque de Longe")]
    public bool temSapato = false;
    public float cooldownSapato = 0.7f;
    private float timerSapato = 0f;

    public GameObject sapatoPrefab;
    public Transform pontoDisparo;

    PlayerCombat   combat;
    BoxCollider2D  box;

    bool faceRight       = true;
    float inputX;

    bool isAttacking     = false;
    public bool isDefending = false;
    bool podeTomarDano   = true;
    bool morreu          = false;
    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        combat   = GetComponent<PlayerCombat>();
        box      = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        rb.gravityScale = 3f;
    }

    void Update()
    {
        if (transform.position.y < voidHeight && !morreu)
        {
            health = 0;
            UpdateHearts();
            Die();
            return;
        }

        if (morreu)
            return;

        if (currentState == PlayerState.Dead)
            return;

        if (timerSapato > 0f)
        {
            timerSapato -= Time.deltaTime;
        }

        VerificarChao();
        AtualizarTimers();
        LerInput();
        Movimento();
        Pulo();
        Glide();
        Defesa();
        Ataque();
        Sapato();
        AtualizarAnimacoes();
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Dead)
            return;

        if (currentState == PlayerState.Normal)
            rb.velocity = new Vector2(inputX * speed, rb.velocity.y);
        else if (currentState == PlayerState.Attack)
            rb.velocity = new Vector2(0f, rb.velocity.y);
        else if (currentState == PlayerState.Defense)
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    void VerificarChao()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            limitador_de_pulo = 1;
        }
    }

    void AtualizarTimers()
    {
        if (!isGrounded)
            coyoteTimeCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    void LerInput()
    {
        bool bloqueado = currentState == PlayerState.Knockback
                      || currentState == PlayerState.Attack
                      || currentState == PlayerState.Defense
                      || (combat != null && combat.estaAtordoado);

        inputX = bloqueado ? 0f : Input.GetAxisRaw("Horizontal");

        if (inputX > 0f && !faceRight) Flip();
        else if (inputX < 0f &&  faceRight) Flip();

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
    }
    void Movimento()
    {
        // Movimento horizontal controlado em FixedUpdate
    }

    void Pulo()
    {
        bool podeUsar = jumpBufferCounter > 0f
                     && coyoteTimeCounter > 0f
                     && limitador_de_pulo > 0
                     && currentState != PlayerState.Knockback
                     && currentState != PlayerState.Dead
                     && (combat == null || !combat.estaAtordoado);

        if (podeUsar)
        {
            rb.velocity       = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
            limitador_de_pulo--;
        }
    }

    void Glide()
    {
        if (!isGrounded && Input.GetKey(KeyCode.Space) && rb.velocity.y < 0f)
            rb.velocity = new Vector2(rb.velocity.x, -glideFallSpeed);
    }

    void Ataque()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isDefending)
            StartCoroutine(AttackCoroutine());
    }

    void Sapato()
    {
        if (!temSapato)
            return;

        if (timerSapato > 0f)
            return;

        if (Input.GetKeyDown(KeyCode.C)
            && !isAttacking
            && !isDefending
            && currentState == PlayerState.Normal)
        {
            AtirarSapato();

            timerSapato = cooldownSapato;
        }
    }

    void AtirarSapato()
    {
        if (sapatoPrefab == null || pontoDisparo == null)
        {
            Debug.LogWarning("SapatoPrefab ou PontoDisparo não foi configurado no Player.");
            return;
        }

        GameObject novoSapato = Instantiate(
            sapatoPrefab,
            pontoDisparo.position,
            Quaternion.identity
        );

        Sapato sapato = novoSapato.GetComponent<Sapato>();

        if (sapato == null)
        {
            Debug.LogWarning("O SapatoPrefab não possui o componente Sapato.");
            Destroy(novoSapato);
            return;
        }

        float direcaoHorizontal = faceRight ? 1f : -1f;

        sapato.Lancar(direcaoHorizontal);
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < health)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }

    }

    public void Heal(int amount)
    {
        health += amount;

        if (health > 5)
        {
            health = 5;
        }

        UpdateHearts();
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking  = true;
        currentState = PlayerState.Attack;

        animator.SetBool("atacando", true);

        yield return new WaitForSeconds(attackDuration);

        animator.SetBool("atacando", false);
        isAttacking  = false;
        currentState = PlayerState.Normal;
    }

    public void Hit()     => AplicarDanoAtaque();
    public void DarDano() => AplicarDanoAtaque();

    void AplicarDanoAtaque()
    {
        if (attackPoint == null) return;

        Collider2D[] atingidos = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D col in atingidos)
        {
            // Tenta EnemyHealth (inimigo genérico)
            EnemyHealth eh = col.GetComponent<EnemyHealth>();
            if (eh != null) { eh.TakeDamage(attackDamage); continue; }

            // Tenta InimigoVoador
            InimigoVoador iv = col.GetComponent<InimigoVoador>();
            if (iv != null) { iv.TakeDamage(attackDamage); }
        }
    }

    public void EndAttack()
    {
        animator.SetBool("atacando", false);
        isAttacking  = false;
        currentState = PlayerState.Normal;
    }

    void Defesa()
    {
        if (Input.GetMouseButtonDown(1) && isGrounded && !isAttacking)
        {
            isDefending  = true;
            currentState = PlayerState.Defense;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDefending  = false;
            if (currentState == PlayerState.Defense)
                currentState = PlayerState.Normal;
        }
    }

    public void TakeDamage(int dano, Transform inimigo)
    {
        if (!podeTomarDano || currentState == PlayerState.Dead) return;

        if (isDefending)
        {
            Debug.Log("Defendeu o ataque!");
            return;
        }

        health -= dano;
        UpdateHearts();
        Debug.Log($"Vida atual: {health}");

        StartCoroutine(InvencibilidadeCoroutine());

        float direcao   = (transform.position.x - inimigo.position.x) >= 0f ? 1f : -1f;
        float forcaFinal = forcaKnockback * (!isGrounded ? multiplicadorAr : 1f);

        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(direcao * forcaFinal, forcaVertical), ForceMode2D.Impulse);

        StartCoroutine(KnockbackCoroutine());
        StartCoroutine(HitStop(0.05f));

        if (health <= 0)
            Die();
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
        yield return new WaitForSeconds(tempoKnockback);
        if (currentState == PlayerState.Knockback)
            currentState = PlayerState.Normal;
    }

    IEnumerator HitStop(float tempo)
    {
        float original  = Time.timeScale;
        Time.timeScale  = 0f;
        yield return new WaitForSecondsRealtime(tempo);
        Time.timeScale  = original;
    }

    void Die()
    {
        if (morreu) return;
        morreu       = true;
        currentState = PlayerState.Dead;

        isAttacking  = false;
        isDefending  = false;
        rb.velocity  = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        if (box != null)
            box.size = new Vector2(box.size.x, 0.5f);

        animator.SetBool("morreu", true);

        StartCoroutine(GameOverDelay());
    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            SceneManager.LoadScene(0);   // fallback se não houver painel
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void AtualizarAnimacoes()
    {
        animator.SetBool("correr",    inputX != 0f);
        animator.SetBool("pulando",   rb.velocity.y >  0.1f && !isGrounded);
        animator.SetBool("caindo",    rb.velocity.y < -0.1f && !isGrounded);
        animator.SetBool("defendendo", isDefending);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Inimigo"))
            TakeDamage(1, col.transform);
    }

    // OnCollisionEnter2D mantido para compatibilidade com plataformas tagueadas como "Chao"
    void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.CompareTag("Chao"))
            limitador_de_pulo = 1;
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
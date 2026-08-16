using UnityEngine;
using System.Collections;

public class InimigoCobra : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public SpriteRenderer spriteRenderer;

    [Header("Movimento")]
    public float velocidadeMovimento = 2.5f;

    [Header("Ataque")]
    public int dano = 1;
    public float tempoDeGrab = 2f;
    public float cooldownGrab = 2f;
    public float distanciaMaximaPerseguicao = 8f;

    [Header("Recuo")]
    public float forcaRecuo = 5f;
    public float tempoRecuo = 0.3f;

    [Header("Pausa pós-ataque")]
    public float tempoPausaPosAtaque = 1f;

    private Rigidbody2D rb;
    private Collider2D col;

    private bool estaAgarrando = false;
    private bool podeAgarrar = true;
    private bool estaRecuando = false;
    private bool estaEmPausa = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.freezeRotation = true;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject jogador = GameObject.FindGameObjectWithTag("Player");

            if (jogador != null)
                player = jogador.transform;
        }
    }

    void Update()
{
    if (player == null)
        return;

    OlharParaJogador();

    if (!estaAgarrando && !estaRecuando && !estaEmPausa)
    {
        SeguirJogador();
    }
    else
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
}

    void SeguirJogador()
{
    float distancia = Mathf.Abs(
        player.position.x - transform.position.x
    );

    // Jogador está longe demais
    if (distancia > distanciaMaximaPerseguicao)
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        return;
    }

    // Para quando estiver perto o suficiente
    if (distancia < 0.5f)
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        return;
    }

    float direcao = Mathf.Sign(
        player.position.x - transform.position.x
    );

    rb.velocity = new Vector2(
        direcao * velocidadeMovimento,
        rb.velocity.y
    );
}
    void OlharParaJogador()
{
    if (player == null || spriteRenderer == null)
        return;

    if (player.position.x > transform.position.x)
    {
        // Jogador está à direita
        spriteRenderer.flipX = true;
    }
    else if (player.position.x < transform.position.x)
    {
        // Jogador está à esquerda
        spriteRenderer.flipX = false;
    }
}

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!podeAgarrar || estaAgarrando)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Player playerScript =
                collision.gameObject.GetComponent<Player>();

            PlayerCombat combat =
                collision.gameObject.GetComponent<PlayerCombat>();

            if (playerScript != null && combat != null)
            {
                StartCoroutine(
                    ExecutarGrab(
                        playerScript,
                        combat,
                        collision.collider
                    )
                );
            }
        }
    }

    IEnumerator ExecutarGrab(
        Player playerScript,
        PlayerCombat combat,
        Collider2D playerCollider)
    {
        estaAgarrando = true;
        podeAgarrar = false;

        rb.velocity = Vector2.zero;

        Physics2D.IgnoreCollision(
            col,
            playerCollider,
            true
        );

        if (playerScript.isDefending)
        {
            yield return StartCoroutine(Recuar());
        }
        else
        {
            combat.Stun(tempoDeGrab);

            yield return new WaitForSeconds(tempoDeGrab);

            playerScript.TakeDamage(
                dano,
                transform
            );
        }

        Physics2D.IgnoreCollision(
            col,
            playerCollider,
            false
        );

        estaAgarrando = false;

        yield return StartCoroutine(PausaPosAtaque());

        yield return new WaitForSeconds(cooldownGrab);

        podeAgarrar = true;
    }

    IEnumerator Recuar()
    {
        estaRecuando = true;

        float direcao = Mathf.Sign(
            transform.position.x - player.position.x
        );

        rb.velocity = new Vector2(
            direcao * forcaRecuo,
            rb.velocity.y
        );

        yield return new WaitForSeconds(tempoRecuo);

        rb.velocity = Vector2.zero;

        estaRecuando = false;
    }

    IEnumerator PausaPosAtaque()
    {
        estaEmPausa = true;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(
            tempoPausaPosAtaque
        );

        estaEmPausa = false;
    }
}
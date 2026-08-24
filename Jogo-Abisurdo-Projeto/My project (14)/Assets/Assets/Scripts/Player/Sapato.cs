using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sapato : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidadeHorizontal = 8f;
    public float forcaVertical = 7f;
    public float gravidade = 2.5f;

    [Header("Combate")]
    public int damage = 1;

    [Header("Tempo de vida")]
    public float tempoDeVida = 3f;

    [Header("Rotação")]
    public float velocidadeRotacao = 720f;

    [Header("Tamanho")]
    public float escalaX = 1.5f;
    public float escalaY = 1f;

    private Rigidbody2D rb;
    private bool foiLancado = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Define o tamanho do sapato
        transform.localScale = new Vector3(escalaX, escalaY, 1f);

        if (rb == null)
        {
            Debug.LogError("O Sapato precisa de um Rigidbody2D.");
        }
    }

    public void Lancar(float direcaoHorizontal)
    {
        if (rb == null)
            return;

        foiLancado = true;

        // Configura a gravidade do sapato
        rb.gravityScale = gravidade;

        // Define a velocidade inicial:
        // horizontal + impulso para cima
        rb.velocity = new Vector2(
            direcaoHorizontal * velocidadeHorizontal,
            forcaVertical
        );

        // Destrói automaticamente depois de alguns segundos
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        if (!foiLancado)
            return;

        // Faz o sapato girar durante o voo
        transform.Rotate(
            Vector3.forward,
            velocidadeRotacao * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Inimigo comum
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Inimigo voador
        InimigoVoador inimigoVoador = other.GetComponent<InimigoVoador>();

        if (inimigoVoador != null)
        {
            inimigoVoador.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Se acertar o chão, desaparece
        if (other.CompareTag("Chao"))
        {
            Destroy(gameObject);
        }
    }
}

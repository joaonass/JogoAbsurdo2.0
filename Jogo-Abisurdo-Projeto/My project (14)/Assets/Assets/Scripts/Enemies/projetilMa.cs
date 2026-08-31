using UnityEngine;

public class ProjetilMa : MonoBehaviour
{
    public float velocidade = 10f;
    public float tempoDeVida = 5f;
    public int dano = 1;

    [Header("Rotação")]
    public float velocidadeRotacao = 720f;

    [Header("Tamanho")]
    public float escalaX = 1f;
    public float escalaY = 1f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        transform.localScale = new Vector3(escalaX, escalaY, 1f);

        if (rb == null)
        {
            Debug.LogError("Projétil sem Rigidbody2D!");
        }

        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        transform.Rotate(
            Vector3.forward,
            velocidadeRotacao * Time.deltaTime
        );
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return;

        Destroy(gameObject);
    }
}
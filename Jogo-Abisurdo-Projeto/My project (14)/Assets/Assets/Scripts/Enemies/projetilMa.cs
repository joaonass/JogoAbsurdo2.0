using UnityEngine;

public class ProjetilMa : MonoBehaviour
{
    public float velocidade = 10f;
    public float tempoDeVida = 5f;
    public int dano = 1;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Projétil sem Rigidbody2D!");
        }

        Destroy(gameObject, tempoDeVida);
    }

    void OnTriggerEnter2D(Collider2D collision)
{
    Destroy(gameObject);
}
}
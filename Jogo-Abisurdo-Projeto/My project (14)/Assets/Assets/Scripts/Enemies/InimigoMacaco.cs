using UnityEngine;

public class InimigoMacaco : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public Transform pontoDeDisparo;
    public GameObject projetilPrefab;

    [Header("Configurações")]
    public float alcance = 8f;
    public float tempoEntreAtaques = 2f;
    public float velocidadeProjetil = 12f;

    private float tempoProximoAtaque;

    void Update()
    {
        if (player == null || pontoDeDisparo == null || projetilPrefab == null)
            return;

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= alcance && Time.time >= tempoProximoAtaque)
        {
            Atacar();
            tempoProximoAtaque = Time.time + tempoEntreAtaques;
        }
    }

    void Atacar()
    {
        GameObject proj = Instantiate(projetilPrefab, pontoDeDisparo.position, Quaternion.identity);

        Vector2 direcao = (player.position - pontoDeDisparo.position).normalized;

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("O projétil está sem Rigidbody2D!");
            return;
        }

        rb.velocity = direcao * velocidadeProjetil;
    }
}
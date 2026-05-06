using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Estados")]
    public bool defending;
    public bool estaAtordoado;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Exemplo simples de defesa (segurando botão)
        defending = Input.GetMouseButton(1); // botão direito
    }

    public void Stun(float tempo)
    {
        if (!estaAtordoado)
        {
            StartCoroutine(StunCoroutine(tempo));
        }
    }

    IEnumerator StunCoroutine(float tempo)
    {
        estaAtordoado = true;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(tempo);

        estaAtordoado = false;
    }
}
   
using UnityEngine;

public class BauPowerUp : MonoBehaviour
{
    private bool coletado = false;

    private Animator animator;

    public SapatoHUD sapatoHUD;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado)
            return;

        Player player = other.GetComponent<Player>();

        if (player != null)
        {
            player.temSapato = true;

            coletado = true;

            if (animator != null)
            {
                animator.SetTrigger("Abrir");
            }

            if (sapatoHUD != null)
            {
                sapatoHUD.MostrarSapato();
            }

            Debug.Log("Power-up do sapato adquirido!");
        }
    }
}
using UnityEngine;

public class BauPowerUp : MonoBehaviour
{
    private bool coletado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado)
            return;

        Player player = other.GetComponent<Player>();

        if (player != null)
        {
            player.temSapato = true;

            coletado = true;

            Debug.Log("Power-up do sapato adquirido!");

            Destroy(gameObject);
        }
    }
}
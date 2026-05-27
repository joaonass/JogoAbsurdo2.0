using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;

    public void TakeDamage(int damage)
    {
        health -= damage;

        Debug.Log("Inimigo tomou dano!");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("POOF!");

        Destroy(gameObject);
    }
}
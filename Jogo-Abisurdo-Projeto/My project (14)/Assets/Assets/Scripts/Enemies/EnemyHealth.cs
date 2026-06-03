using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    public GameObject heartDrop;

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

        DropHeart();

        Destroy(gameObject);
    }

    void DropHeart()
    {
        int chance = Random.Range(0, 100);

        if (chance < 100)
        {
            Instantiate(
                heartDrop,
                transform.position + Vector3.up * 1f,
                Quaternion.identity
            );
        }
    }
}
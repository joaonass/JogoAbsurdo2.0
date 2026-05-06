using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Referências")]
    public GameManager gameManager;
    public PlayerCombat combat;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("K foi pressionado");
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (combat != null && combat.defending)
        {
            damage /= 2;
        }

        currentHealth -= damage;

        Debug.Log("Vida atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("A princesa morreu!");

        gameManager.GameOver();

        Destroy(gameObject);
    }
}
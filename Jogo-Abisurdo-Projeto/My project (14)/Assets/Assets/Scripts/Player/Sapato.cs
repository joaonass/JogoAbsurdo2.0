using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sapato : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    public Vector2 direction;


    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

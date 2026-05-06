using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animacoes : MonoBehaviour
{
    public Animator animacao;
    public Player player;

    void Start()
    {
        animacao = GetComponent<Animator>();
        player = GameObject.Find("Princesa").GetComponent<Player>();
    }

    void Update()
    {
        float velocidadeX = Mathf.Abs(player.rb.velocity.x);
        float velocidadeY = player.rb.velocity.y;

        bool estaNoChao = player.isGrounded; // vem do Player

        animacao.SetBool("correr", velocidadeX > 0.1f && estaNoChao);
        animacao.SetBool("pulando", velocidadeY > 0.1f && !estaNoChao);
        animacao.SetBool("caindo", velocidadeY < -0.1f && !estaNoChao);
        animacao.SetBool("defendendo", player.isDefending);
    }
}

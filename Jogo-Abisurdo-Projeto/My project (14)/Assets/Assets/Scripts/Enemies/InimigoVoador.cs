using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InimigoVoador : MonoBehaviour
{
    public Transform player;
    public float deteccaoRange = 10f;
    public float velocidadeVoo = 3f;
    public float velocidadeAtaque = 12f;
    public float velocidadeRetorno = 4f;

    public float tempoDeAviso = 0.6f;
    public float turnSpeed = 5f;

    public float tempoMaximoDive = 1.5f; 
    private float tempoDiveAtual = 0f;

    public float cooldownAtaque = 1.5f;
    private float tempoCooldown = 0f;

    private Vector2 startPosition;
    private Vector2 direcaoAtaque;

    private bool isDiving = false;
    private bool retornando = false;
    private bool preparandoAtaque = false;

    private Rigidbody2D rb;

    void Start(){
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update(){
        float distancia = Vector2.Distance(transform.position, player.position);

        tempoCooldown -= Time.deltaTime;

        if(!isDiving && !retornando && !preparandoAtaque){
            Patrulhar();

            if(distancia <= deteccaoRange && tempoCooldown <= 0f){
                StartCoroutine(PrepararAtaque());
            }
        }

        if(isDiving){
            Dive();
        }

        if(retornando){
            ReturnToStart();
        }
    }

    void Patrulhar(){
        float movimentoX = Mathf.Sin(Time.time) * velocidadeVoo;
        rb.velocity = new Vector2(movimentoX, 0);
    }

    IEnumerator PrepararAtaque(){
        if(preparandoAtaque) yield break;

        preparandoAtaque = true;

        direcaoAtaque = (player.position - transform.position).normalized;

        yield return new WaitForSeconds(tempoDeAviso);

        isDiving = true;
        preparandoAtaque = false;
    }

    void Dive(){
        tempoDiveAtual += Time.deltaTime;

        Vector2 direcaoDesejada = ((Vector2)player.position - rb.position).normalized;

        float maxTurn = turnSpeed * Time.deltaTime;

        direcaoAtaque = Vector2.Lerp(direcaoAtaque, direcaoDesejada, maxTurn).normalized;

        rb.velocity = direcaoAtaque * velocidadeAtaque;

        if(Vector2.Distance(rb.position, player.position) < 0.5f){
            FinalizarDive();
        }

        if(tempoDiveAtual >= tempoMaximoDive){
            FinalizarDive();
        }
    }

    void ReturnToStart(){
        Vector2 direcao = (startPosition - rb.position).normalized;
        rb.velocity = direcao * velocidadeRetorno;

        if (Vector2.Distance(rb.position, startPosition) < 0.1f){
            retornando = false;
            rb.velocity = Vector2.zero;
        }
    }

    void FinalizarDive(){
        isDiving = false;
        retornando = true;
        tempoDiveAtual = 0f;
        tempoCooldown = cooldownAtaque;
    }
}
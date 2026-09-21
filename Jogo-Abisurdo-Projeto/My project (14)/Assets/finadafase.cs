using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalDaFase : MonoBehaviour
{
    [Header("Tela de fase concluída")]
    public GameObject painelFaseConcluida;

    private bool faseConcluida = false;

    private void Start()
    {
        // Garante que o painel começa escondido
        painelFaseConcluida.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se quem chegou ao final foi o jogador
        if (other.CompareTag("Player") && !faseConcluida)
        {
            ConcluirFase();
        }
    }

    private void ConcluirFase()
    {
        faseConcluida = true;

        // Mostra a tela de fase concluída
        painelFaseConcluida.SetActive(true);

        // Para o jogo
        Time.timeScale = 0f;
    }

    public void ProximaFase()
    {
        // Retoma o tempo
        Time.timeScale = 1f;

        // Carrega a próxima cena
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex + 1
        );
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;

        // Troque "Menu" pelo nome da sua cena de menu
        SceneManager.LoadScene("Menu");
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverScreen;

    public void GameOver(){
        Debug.Log("GAME OVER");

        Time.timeScale = 0f;

        if (gameOverScreen != null){
            gameOverScreen.SetActive(true);
        }
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class botao : MonoBehaviour
{
   public void playGame()
   {
       SceneManager.LoadSceneAsync(1);
    }
}

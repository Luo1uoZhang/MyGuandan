using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButtonScript : MonoBehaviour{
    public void Start(){

    }
    public void Update(){

    }
    public void OnButtonClickDown(){
        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
    }
}

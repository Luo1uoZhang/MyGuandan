using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingCanvasManager : MonoBehaviour{
    public Canvas canvas;
    public event Action OnBackMenu;
    public void Start(){
        
    }
    public void Update(){

    }
    public void BackGame(){
        LeanTween.moveX(canvas.gameObject, -32, 0.25f).setEaseInOutSine();
        // canvas.gameObject.SetActive(false);
    }

    public void BackMenu(){
        CardObjectPool.Instance.ReturnAllCards();
        SceneManager.LoadSceneAsync("Menu Scene");
    }
}

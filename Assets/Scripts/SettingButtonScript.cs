using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingButtonScript : MonoBehaviour
{
    public Canvas menu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnButtonClickDown(){
        Debug.Log("OnButtonClickDown");
        LeanTween.moveX(menu.gameObject, 0f, 0.25f).setEaseInOutSine();
    }
}

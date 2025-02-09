using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{   
    SpriteRenderer sr;
    private bool isPicked = false;
    Color pickColor = new(0.2f,0.2f,0.2f,1);
    Color unpickColor = new(1, 1, 1, 1);
    private bool canPick = true;
    // Start is called before the first frame update
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void OnPointerDown(PointerEventData eventData){
        if (!canPick)
        {
            return;
        }
        isPicked = ! isPicked;
        ChangeColor();
    }

    public void OnPointerUp(PointerEventData eventData){
        if (!canPick)
        {
            return;
        }
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (!canPick)
        {
            return;
        }
        if (Input.GetMouseButton(0)){
            isPicked = ! isPicked;
            ChangeColor();
        }
    }

    public void SetCanPick(bool canPick)
    {
        this.canPick = canPick;
    }
    
    private void ChangeColor()
    {
        if (isPicked)
        {
            sr.color = pickColor;
        }
        else
        {
            sr.color = unpickColor;
        }
    }

    public bool IsPicked()
    {
        return isPicked;
    }

    public void ResetCard()
    {
        isPicked = false;
        ChangeColor();
    }

    public void SetPickState(bool state)
    {
        isPicked = state;
        ChangeColor();
    }
}
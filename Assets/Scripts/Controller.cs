using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class CardSelector : MonoBehaviour{
    private List<GameObject> selectedCards = new List<GameObject>();
    private GameObject initialCard = null;

    public void Update(){
        if (Input.GetMouseButtonDown(0)){
            Debug.Log("Mouse Down.");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log("Create a ray, position" + ray.origin + ", direction " + ray.direction);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                initialCard = hit.collider.gameObject;
                selectedCards.Clear();
                selectedCards.Add(initialCard);
            }
        }
        if (Input.GetMouseButton(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject currentCard = hit.collider.gameObject;
                if (currentCard!= initialCard &&!selectedCards.Contains(currentCard))
                {
                    selectedCards.Add(currentCard);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Mouse Up");
            initialCard = null;
        }
    }
}
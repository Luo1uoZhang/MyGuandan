using System.Collections.Generic;
using UnityEngine;



public class Card {
    public int suit;
    public int rank;
    public int priority;
    public GameObject cardObject;
    public CardBehaviour cardBehaviour;
    private readonly Dictionary<int, string> SuitMapper = new() {
        {0, "spades"}, {1, "hearts"}, {2, "clubs"}, {3, "diamonds"}
    };

    public Card(int suit, int rank, GameObject cardObject) {
        this.suit = suit;
        this.rank = rank;
        // priority = rank;

        SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
        string cardName = SuitMapper[suit] + "_" + rank;
        Sprite cardSprite = Resources.Load<Sprite>("Images/" + cardName);
        cardObject.name = cardName;
        spriteRenderer.sprite = cardSprite;
        cardObject.SetActive(false);
        cardBehaviour = cardObject.GetComponent<CardBehaviour>();

        this.cardObject = cardObject;
    }

    public void SetPriority(int priority) {
        this.priority = priority;
    }
}
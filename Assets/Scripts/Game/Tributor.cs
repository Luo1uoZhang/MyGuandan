

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

class TributeManager : MonoBehaviour
{
    private int _needTributeCount;
    private int needTributeCount;
    private readonly Dictionary<int, Card> tributeCards = new();
    private readonly Dictionary<int, Card> backCards = new();
    public bool isResist = false;

    public bool sameTribute;
    public int whoTributeGreatest;
    private int greatestPriority;
    private List<HoldArea> whoWins;
    private int jokerCount = 0;

    private List<HoldArea> whoShouldBack;

    private Controller controller;

    public void Initialize(Controller controller)
    {
        this.controller = controller;
    }
    public void Tribute(HoldArea player, List<Card> cards)
    {
        Card tributeCard = cards[0];
        Debug.Log("Got Player " + player.pos + " tribute " + tributeCard.suit + ", " + tributeCard.rank);
        if (tributeCard.rank == 17) jokerCount++;

        if (tributeCard.priority > greatestPriority)
        {
            greatestPriority = tributeCard.priority;
            whoTributeGreatest = player.pos;
        }
        else if (tributeCard.priority == greatestPriority)
        {
            sameTribute = true;
        }
        tributeCards.Add(player.pos, tributeCard);
        needTributeCount--;

        if (needTributeCount == 0)
        {
            Debug.Log("All tributes received");

            if (jokerCount == 2)
            {
                isResist = true;
                Debug.Log("Resist, jokers will be returned to respective players.");
                //TODO: Wait 1 second and return all cards to theirself.

                StartCoroutine(ReturnCards());
            }
            else
            {
                foreach (var playerShouldBack in whoShouldBack)
                {
                    playerShouldBack.BackButton.SetActive(true);
                }
            }
        }
    }

    private IEnumerator<WaitForSeconds> ReturnCards()
    {
        yield return new WaitForSeconds(1);

        foreach (var pair in tributeCards)
        {
            var player = controller.players[pair.Key];
            player.handArea.ClearHandAtTributePeriod();
            player.AddCardToHold(pair.Value);
        }

        controller.ReadyToStart();
    }

    public void Back(HoldArea player, List<Card> cards)
    {
        Debug.Log("Player " + player.pos + " backs " + cards[0].suit + ", " + cards[0].rank);
        backCards.Add(player.pos, cards[0]);

        if (backCards.Count == _needTributeCount)
        {
            StartCoroutine(FinishTributeStage());
        }
    }

    private IEnumerator<WaitForSeconds> FinishTributeStage()
    {
        yield return new WaitForSeconds(0.5f);
        switch (_needTributeCount)
        {
            case 1:
                // actually backCards now have only 1 pair.
                foreach (var pair in backCards)
                {
                    controller.players[pair.Key].handArea.ClearHandAtTributePeriod();
                    controller.players[pair.Key].AddCardToHold(tributeCards.First().Value);
                    
                    // StartCoroutine(CommonUtil.DelayMakeUp(controller.players[pair.Key]));
                    controller.players[tributeCards.First().Key].handArea.ClearHandAtTributePeriod();
                    controller.players[tributeCards.First().Key].AddCardToHold(pair.Value);
                    // StartCoroutine(CommonUtil.DelayMakeUp(controller.players[tributeCards.First().Key]));
                }
                break;
            case 2:
                {
                    foreach (var pair in backCards)
                    {
                        var player = controller.players[pair.Key];
                        HoldArea playerToGive;
                        if (sameTribute)
                        {
                            playerToGive = controller.players[(pair.Key + 1) % 4];
                        }
                        else
                        {
                            if (pair.Key == whoWins[0].pos)
                            {
                                playerToGive = controller.players[whoTributeGreatest];
                            }
                            else
                            {
                                playerToGive = controller.players[(whoTributeGreatest + 2) % 4];
                            }
                        }
                        playerToGive.handArea.ClearHandAtTributePeriod();
                        playerToGive.AddCardToHold(pair.Value);
                        // StartCoroutine(CommonUtil.DelayMakeUp(playerToGive));
                        player.handArea.ClearHandAtTributePeriod();
                        player.AddCardToHold(tributeCards[playerToGive.pos]);
                        // StartCoroutine(CommonUtil.DelayMakeUp(player));
                        Debug.Log("Player " + player.pos + " gives " + pair.Value.suit + ", " + pair.Value.rank + " to " + playerToGive.pos);
                    }
                }
                break;
            default:
                throw new Exception("Invalid needTributeCount: " + _needTributeCount);
        }
        
        controller.ReadyToStart();
    }

    public void ResetManager(List<HoldArea> whoWins, int needTributeCount = 1)
    {
        this.whoWins = whoWins;
        isResist = false;
        _needTributeCount = needTributeCount;
        this.needTributeCount = needTributeCount;
        tributeCards.Clear();
        whoTributeGreatest = -1;
        sameTribute = false;
        greatestPriority = -1;
        jokerCount = 0;
        backCards.Clear();
    }

    public void SetWhoShouldBack(List<HoldArea> whoShouldBack)
    {
        this.whoShouldBack = whoShouldBack;
    }
}
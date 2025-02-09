using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;


public partial class Controller : MonoBehaviour
{
    public Dictionary<int, string> PointToPoint = new(){
        {2, "2"},
        {3, "3"},
        {4, "4"},
        {5, "5"},
        {6, "6"},
        {7, "7"},
        {8, "8"},
        {9, "9"},
        {10, "10"},
        {11, "J"},
        {12, "Q"},
        {13, "K"},
        {14, "A"}
    };
    public List<HoldArea> players = new();

    public int CampAOverPlayerCount = 0;
    public int CampBOverPlayerCount = 0;

    private TextMeshProUGUI campARank;
    private int campARankValue = 2;
    private TextMeshProUGUI campBRank;
    private int campBRankValue = 2;

    private TributeManager tributeManager;
    private int round = 1;

    private List<HoldArea> WhoWins = new();
    private GameObject readyButton;

    private int currentRank = 2;

    void Awake()
    {
        tributeManager = gameObject.AddComponent<TributeManager>();
        tributeManager.Initialize(this);
        // CardObjectPool.Instance.Init();
        players.Add(new DownHoldArea(this));
        players.Add(new RightHoldArea(this));
        players.Add(new UpHoldArea(this));
        players.Add(new LeftHoldArea(this));

        CardObjectPool.Instance.SetHoldAreas(players);
        CardObjectPool.Instance.Init();

        GameObject go = GameObject.Find("Button");
        if (go != null)
        {
            readyButton = go;
            go.SetActive(true);
        }

        campARank = GameObject.Find("CampARank").GetComponent<TextMeshProUGUI>();
        campBRank = GameObject.Find("CampBRank").GetComponent<TextMeshProUGUI>();

        GameRankManager.Instance.SetGameRank(2);
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // Debug.Log("Get Mouse Button Up.");
            foreach (var player in players)
            {
                if (player.IsMyTurn)
                {
                    player.ShowHandInfo();
                    break;
                }
            }
        }
    }

    private void SetReadyButton(bool isReady)
    {
        readyButton.SetActive(isReady);
    }

    // This method should be renamed as OnGameStartButtonClick
    // So, once we start the game, we first deal all the cards.
    // After dealing, we check tribute.
    public void StartGame()
    {
        DealCard();
        SetReadyButton(false);
    }

    public void Tribute(HoldArea player, List<Card> cards)
    {
        tributeManager.Tribute(player, cards);
    }

    public void DealCard()
    {
        List<Card> cards = CardObjectPool.Instance.GetCards();
        // Debug.Log(cards.Count);
        CardShuffler.Shuffle(cards);
        for (int i = 0; i < cards.Count; i++)
        {
            players[i % 4].Initialize(cards[i]);
        }
        foreach (var player in players)
        {
            /// Actually, once the card is dealt, an event "CardDealt" should be triggered
            /// Then players' card will be made up by themselves through listening the event
            player.MakeUp();
        }

        #region Debug Tribute

        WhoWins.Clear();

        HashSet<int> appearedPlayers = new();
        for (int i = 0; i < 3; i++)
        {
            while (true)
            {
                int selectedPlayer = UnityEngine.Random.Range(0, 4);
                if (!appearedPlayers.Contains(selectedPlayer))
                {
                    appearedPlayers.Add(selectedPlayer);
                    WhoWins.Add(players[selectedPlayer]);
                    break;
                }
            }
        }
        Debug.Log("Generated WhoWins: " + string.Join(", ", WhoWins.ConvertAll(p => p.pos)));
        if ((WhoWins[0].pos + WhoWins[1].pos) % 2 == 0)
        {
            WhoWins.RemoveAt(2);
        }

        Debug.Log("Processed WhoWins: " + string.Join(", ", WhoWins.ConvertAll(p => p.pos)));

        #endregion

        // players[0].AllowIssue();

        #region Tribute Stage
        bool isResist = false;
        List<HoldArea> whoShouldBack = new();
        if (WhoWins.Count == 0)
        {
            Debug.Log("The very first round.");
            // TODO: At the very first round, we randomly choose one player to start the game.
        }
        else
        {
            Debug.Log("Someone has to tribute.");
            int numberOfWinners = WhoWins.Count;
            switch (numberOfWinners)
            {
                case 2:
                    tributeManager.ResetManager(WhoWins, 2);
                    foreach (var player in players)
                    {
                        if (player.isCampA != WhoWins[0].isCampA && player.SoloResist())
                        {
                            isResist = true;
                            break;
                        }
                    }
                    if (isResist)
                    {
                        Debug.Log("Resist.");
                        break;
                    }
                    foreach (var player in players)
                    {
                        if (player.isCampA != WhoWins[0].isCampA)
                        {
                            Debug.Log("12, " + player.pos + " needs to tribute.");
                            player.NeedToTribute();
                        }
                        else
                        {
                            whoShouldBack.Add(player);
                        }
                    }
                    break;
                case 3:
                    tributeManager.ResetManager(WhoWins, 1);
                    whoShouldBack.Add(WhoWins[0]);
                    if (WhoWins[0].isCampA != WhoWins[2].isCampA) // 14
                    {
                        foreach (var player in players)
                        {
                            if (player.isCampA != WhoWins[0].isCampA || player == WhoWins[0])
                            {
                                
                                continue;
                            }
                            if (player.SoloResist())
                            {
                                isResist = true;
                                Debug.Log("Resist.");
                            }
                            else
                            {
                                Debug.Log("14, " + player.pos + " needs to tribute.");
                                player.NeedToTribute();
                            }
                        }
                    }
                    else // 13
                    {
                        
                        foreach (var player in players)
                        {
                            if (player.isCampA == WhoWins[0].isCampA || player == WhoWins[1])
                            {
                                continue;
                            }
                            if (player.SoloResist())
                            {
                                isResist = true;
                                Debug.Log("Resist.");
                            }
                            else
                            {
                                Debug.Log("13, " + player.pos + " needs to tribute.");
                                player.NeedToTribute();
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("Invalid number of winners.");
            }
        }

        #endregion

        if (isResist)
        {
            tributeManager.isResist = true;
            ReadyToStart();
        }
        else
        {
            tributeManager.SetWhoShouldBack(whoShouldBack);
        }
    }

    public void ReadyToStart()
    {
        if (tributeManager.isResist)
        {
            WhoWins[0].AllowIssue();
        }
        else
        {
            switch (WhoWins.Count)
            {
                case 0:
                    // TODO: At the very first round, how to choose one player to start the game?
                    break;
                case 2:
                    if (tributeManager.sameTribute)
                    {
                        players[WhoWins[0].pos + 1].AllowIssue();
                    }
                    else
                    {
                        players[tributeManager.whoTributeGreatest].AllowIssue();
                    }
                    break;
                case 3:
                    HashSet<int> allPlayers = new() { 0, 1, 2, 3 };
                    foreach (var player in WhoWins)
                    {
                        allPlayers.Remove(player.pos);
                    }
                    int leaderPos = allPlayers.First();
                    players[leaderPos].AllowIssue();
                    break;
            }
        }

        // foreach (var player in players)
        // {
        //     player.handArea.ClearHandAtTributePeriod();
        // }
        WhoWins.Clear();
    }

    public void ReturnAllCards()
    {
        foreach (var player in players)
        {
            player.ReturnAllCards();
        }
    }

    public void OnIssue(HoldArea HoldArea, HandInfo handInfo)
    {
        if (HoldArea.cards.Count == 0)
        {
            if (HoldArea.isCampA)
            {
                CampAOverPlayerCount++;
            }
            else
            {
                CampBOverPlayerCount++;
            }
            WhoWins.Add(HoldArea);
        }

        /// TODO: We can use delegate to make the code more elegant.
        if (CampAOverPlayerCount == 2 || CampBOverPlayerCount == 2)
        {
            bool gameOver = false;
            HoldArea winner = WhoWins[0];
            if (winner.isCampA)
            {
                if (campARankValue == 14)
                {
                    gameOver = true;
                }
                else
                {
                    int score = 3;
                    for (int i = 1; i < WhoWins.Count; i++)
                    {
                        if (!WhoWins[i].isCampA)
                        {
                            score -= 1;
                        }
                    }
                    campARankValue = Math.Min(14, campARankValue + score);
                    GameRankManager.Instance.SetGameRank(campARankValue);
                }
            }
            else
            {
                if (campBRankValue == 14)
                {
                    gameOver = true;
                }
                else
                {
                    int score = 3;
                    for (int i = 1; i < WhoWins.Count; i++)
                    {
                        if (WhoWins[i].isCampA)
                        {
                            score -= 1;
                        }
                    }
                    campBRankValue = Math.Min(14, campBRankValue + score);
                    GameRankManager.Instance.SetGameRank(campBRankValue);
                }
            }

            ReturnAllCards();
            if (gameOver)
            {
                campARankValue = 2;
                campBRankValue = 2;
            }
            NewGame();
        }
        else
        {
            Board board = Board.Instance;
            int index = (players.FindIndex(p => p == HoldArea) + 1) % 4;

            // Actually it is simulating that it is the player's turn but he is holding nothing and he have to choose pass
            // This means that we should still send a signal to the player that he is playing and we'll check that 
            // if he is holding nothing, he can only pass
            // But if we are simulating this, we can not determine whether his ally is the leader.
            // I GOT AN IDEA.
            while (players[index].cards.Count == 0)
            {
                board.UpdateHandInfo(new HandInfo(HandType.NONE, -1));
                index = (index + 1) % 4;
            }
            if (board.GetNumOfPasses() > 3)
            {
                index = (index + 1) % 4;
            }

            if (board.GetNumOfPasses() >= 3)
            {
                board.Reset();
            }
            players[index].AllowIssue();
        }
    }

    private void NewGame()
    {
        campARank.text = PointToPoint[campARankValue];
        campBRank.text = PointToPoint[campBRankValue];

        CampAOverPlayerCount = 0;
        CampBOverPlayerCount = 0;


        SetReadyButton(true);
    }

    public void ShowHandInfo(HandInfo handInfo)
    {
        Debug.Log("You play type: " + handInfo.Type + " with rank: " + handInfo.Rank);
    }

    public int GetGameRank()
    {
        return currentRank;
    }

    public void ResetGame()
    {
        ReturnAllCards();
        foreach (var player in players)
        {
            player.AllowIssue(false);
            player.DisableAllButtons();
        }
        Board.Instance.Reset();
        NewGame();
    }

    enum GameState
    {
        WaitingForPlayers,
        Playing,
        GameOver
    }

    public void Back(HoldArea player, List<Card> cards)
    {
        tributeManager.Back(player, cards);
    }
}
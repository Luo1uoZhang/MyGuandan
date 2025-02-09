using System;
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

#region Abstract HoldArea
public abstract class HoldArea
{
    public float distance;
    public List<Card> cards = new();
    public abstract GameObject issueButton { get; }
    public GameObject passButton;
    public event Action<HoldArea, HandInfo> IssueEvent;
    public event Action<HoldArea, List<Card>> OnTribute;
    public event Action<HoldArea, List<Card>> OnBack;
    public event Action<HandInfo> TestEvent;
    private Controller controller;

    public bool isCampA = false;
    public int pos = 0;

    public bool IsMyTurn = false;

    public abstract GameObject TributeButton { get; }
    public abstract GameObject BackButton { get; }

    public abstract void MakeUp();
    public abstract void Initialize(Card cards);

    public abstract HandArea handArea { get; set; }

    public HoldArea(Controller controller)
    {
        this.controller = controller;
        IssueEvent += controller.OnIssue;
        OnTribute += controller.Tribute;
        OnBack += controller.Back;
    }

    public void NeedToTribute()
    {
        try
        {
            Debug.Log("Player " + pos + " need to tribute.");
            TributeButton.SetActive(true);

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        // First Check if resist
        // If not resist, show the button bind to Tribute().
        // Resist should be checked once the cards are dealt.
    }

    public bool SoloResist()
    {
        int count = 0;
        foreach (Card card in cards)
        {
            if (card.rank == 17) count++;
        }
        return count == 2;
    }

    public void Tribute()
    {
        (List<Card> picked, List<Card> unpicked) = PickedCards();
        int maxPriority = MaxPriorityHold();
        if (picked.Count != 1)
        {
            Debug.Log("Invalid tribute, please select only one card.");
        }
        else
        {
            if (picked[0].priority != maxPriority)
            {
                Debug.Log("Invalid tribute, the selected card's priority is not the highest.");
            }
            else
            {
                cards = unpicked;
                handArea.PushHand(picked);

                MakeUp();
                OnTribute.Invoke(this, picked);
                TributeButton.SetActive(false);
            }
        }
    }

    public int MaxPriorityHold()
    {
        int maxPriority = 0;
        List<Card> cardsWithOutWild = HandUtil.RemoveWildCards(cards, 2);
        foreach (Card card in cardsWithOutWild)
        {
            if (card.priority > maxPriority)
            {
                maxPriority = card.priority;
            }
        }
        return maxPriority;
    }

    public int CountWildCards()
    {
        int gameRank = GameRankManager.Instance.GetGameRank();
        int count = 0;
        foreach (Card card in cards)
        {
            if (card.rank == gameRank && card.suit == 1)
                count++;
        }
        return count;
    }

    private (List<Card>, List<Card>) PickedCards()
    {
        List<Card> picked = new();
        List<Card> unpicked = new();
        foreach (Card card in cards)
        {
            CardBehaviour cb = card.cardObject.GetComponent<CardBehaviour>();
            if (cb.IsPicked())
            {
                picked.Add(card);
            }
            else
            {
                unpicked.Add(card);
            }
        }
        return (picked, unpicked);
    }

    public void Issue()
    {
        (List<Card> toBeReturned, List<Card> newHand) = PickedCards();
        HandInfo lastHandInfo = Board.Instance.GetLastHandInfo();

        HandInfo handInfo = HandTools.GetHandInfo(toBeReturned);

        if (toBeReturned.Count == 0)
        {
            if (lastHandInfo.Type == HandType.NONE)
            {
                Debug.Log("You choose to pass, but you are the leader.");
            }
            else
            {
                Debug.Log("You choose to pass, because lastHandInfo is " + lastHandInfo.Type + ", it is valid.");

                Board.Instance.UpdateHandInfo(handInfo);
                IssueEvent?.Invoke(this, handInfo);
                AllowIssue(false);

                handArea.PushHand(toBeReturned);
            }
        }
        else
        {
            if (!HandComparator.IsGreater(handInfo, lastHandInfo))
            {
                Debug.Log("You are tring to play " + handInfo.Type + " with rank " + handInfo.Rank + " but it is not greater than lastHandInfo " + lastHandInfo.Type + " with rank " + lastHandInfo.Rank);
            }
            else
            {
                Debug.Log("You play a hand of " + handInfo.Type + " with rank " + handInfo.Rank);
                TestEvent?.Invoke(handInfo);
                cards = newHand;
                AllowIssue(false);
                Board.Instance.UpdateHandInfo(handInfo);
                IssueEvent?.Invoke(this, handInfo);
                handArea.PushHand(toBeReturned);

                MakeUp();
            }
        }


    }

    public virtual void AllowIssue(bool allow = true)
    {
        handArea.ClearHand();
        issueButton.SetActive(allow);
        IsMyTurn = allow;
    }

    public void SortCards()
    {
        cards.Sort((c1, c2) =>
        {
            if (c1.priority == c2.priority)
            {
                return c2.suit.CompareTo(c1.suit);
            }
            return c2.priority.CompareTo(c1.priority);
        });
    }

    public int HowManyDifferentRankCards()
    {
        int i = 0;
        int preValue = -1;
        foreach (Card card in cards)
        {
            if (card.rank != preValue)
            {
                i++;
                preValue = card.rank;
            }
        }
        return i;
    }

    public void ReturnAllCards()
    {
        foreach (Card card in cards)
        {
            CardObjectPool.Instance.ReturnCard(card);
        }
        handArea.ClearHand();
        cards.Clear();
    }

    public void ShowHandInfo()
    {
        List<Card> pickedCards = new();
        foreach (Card card in cards)
        {
            CardBehaviour cb = card.cardObject.GetComponent<CardBehaviour>();
            if (cb.IsPicked())
            {
                pickedCards.Add(card);
            }
        }
        HandInfo handInfo = HandTools.GetHandInfo(pickedCards);

        GameObject tmp = GameObject.Find("HandInfo");
        if (tmp == null)
        {
            Debug.Log("Can not find GameObject HandInfo.");
        }
        else
        {
            TextMeshProUGUI tmpText = tmp.GetComponent<TextMeshProUGUI>();
            if (tmpText == null)
            {
                Debug.Log("Can not find TextMeshPro component in GameObject HandInfo.");
            }
            else
            {
                tmpText.text = "牌型：" + handInfo.Type + "，等级：" + handInfo.Rank;
            }

        }
    }

    public abstract void DisableAllButtons();

    public void Back()
    {
        (List<Card> picked, List<Card> unpicked) = PickedCards();
        if (picked.Count != 1)
        {
            Debug.Log("Invalid back, please select only one card.");
        }
        else
        {
            if (picked[0].priority > 10)
            {
                Debug.Log("Invalid back, the selected card's priority should be less than or equal to 10.");
            }
            else
            {
                handArea.PushHand(picked);
                cards = unpicked;
                MakeUp();
                OnBack.Invoke(this, picked);
                BackButton.SetActive(false);
            }
        }
    }

    public void AddCardToHold(Card card)
    {
        card.cardBehaviour.SetCanPick(true);
        cards.Add(card);
        MakeUp();
    }
}
#endregion

#region DownHoldArea
public class DownHoldArea : HoldArea
{
    float currentCardPositionX = Constants.DownHoldAreaCenterX;
    float currentCardPositionZ = -1f;

    private GameObject IssueButton;
    private GameObject HintButton;
    public override GameObject issueButton { get { return IssueButton; } }
    public override HandArea handArea { get; set; }

    private readonly GameObject tributeButton;
    public override GameObject TributeButton { get { return tributeButton; } }

    private readonly GameObject backButton;
    public override GameObject BackButton { get { return backButton; } }

    private int hintIndex = 0;
    private Hint hinter;
    private List<int[][]> hints = new();


    public DownHoldArea(Controller controller) : base(controller)
    {
        pos = 0;
        currentCardPositionX = Constants.DownHoldAreaCenterX;
        distance = Constants.UpDownHoldAreaDistance;
        isCampA = true;
        Canvas canvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();

        #region Prepare Buttons

        IssueButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/IssueButton"));
        issueButton.transform.SetParent(canvas.transform, false);

        HintButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/HintButton"));
        HintButton.transform.SetParent(canvas.transform, false);

        tributeButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/TributeButton"));
        tributeButton.transform.SetParent(canvas.transform, false);
        Vector3 tributeButtonPosition = issueButton.transform.position;
        tributeButton.transform.localPosition = tributeButtonPosition;

        backButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ReturnButton"));
        backButton.transform.SetParent(canvas.transform, false);
        Vector3 backButtonPosition = issueButton.transform.position;
        backButton.transform.localPosition = backButtonPosition;

        SetButtonOnClick();

        #endregion


        handArea = new HandArea(new Vector3(0f, -2.5f, -3f), 0);

        hinter = new(this);
    }

    public void SetButtonOnClick()
    {
        IssueButton.GetComponent<Button>().onClick.AddListener(Issue);
        IssueButton.SetActive(false);

        HintButton.GetComponent<Button>().onClick.AddListener(Hint);
        HintButton.SetActive(false);

        tributeButton.GetComponent<Button>().onClick.AddListener(Tribute);
        tributeButton.SetActive(false);

        backButton.GetComponent<Button>().onClick.AddListener(Back);
        backButton.SetActive(false);
    }

    public override void AllowIssue(bool allow = true)
    {
        IssueButton.SetActive(allow);
        AllowHint(allow);
        IsMyTurn = allow;
    }

    public void AllowHint(bool allow = true)
    {
        HintButton.SetActive(allow);
        if (allow)
        {
            hinter.GenerateHints(Board.Instance.GetLastHandInfo());
        }
    }

    private void Hint()
    {
        /// This is a temporary solution, we will implement a more efficient algorithm later.
        List<SimpleCard> hint = hinter.NextHint();
        if (hint == null || hint.Count == 0)
        {
            return;
        }
        foreach (Card card in cards)
        {
            card.cardBehaviour.SetPickState(false);
        }

        foreach (SimpleCard simpleCard in hint)
        {
            Card card = cards.Find(c => (c.rank == simpleCard.rank || c.priority == simpleCard.priority) && c.suit == simpleCard.suit && !c.cardBehaviour.IsPicked());
            if (card == null)
            {
                Debug.Log("Can not find card with rank " + simpleCard.rank + " and suit " + simpleCard.suit);
                return;
            }
            card.cardBehaviour.SetPickState(true);
        }
    }

    public override void MakeUp()
    {
        SortCards();
        int i = HowManyDifferentRankCards();
        int preValue = -1;
        currentCardPositionX = -((i + 1) / 2f) * distance;
        float currentCardPositionY = Constants.DownHoldAreaCenterY;
        currentCardPositionZ = -4f;
        float offset = Constants.HoldAreaOffsetZ;
        foreach (Card card in cards)
        {
            if (card.rank != preValue)
            {
                currentCardPositionX += distance;
                // offset = Constants.HoldAreaOffsetZ;
                currentCardPositionY = Constants.DownHoldAreaCenterY;
                card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ);
            }
            else
            {
                currentCardPositionY += Constants.DownHoldAreaOffsetY;
                card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ);
                // offset += Constants.HoldAreaOffsetZ;
            }
            preValue = card.rank;
            currentCardPositionZ += Constants.HoldAreaOffsetZ;
            card.cardObject.GetComponent<CardBehaviour>().ResetCard();
        }

    }

    public override void Initialize(Card card)
    {
        card.cardBehaviour.SetCanPick(true);
        card.cardObject.transform.position = new Vector3(currentCardPositionX, -6f, currentCardPositionZ);
        currentCardPositionX += distance;
        currentCardPositionZ -= 0.01f;
        card.cardObject.SetActive(true);

        cards.Add(card);
    }

    public override void DisableAllButtons()
    {
        TributeButton.SetActive(false);
        BackButton.SetActive(false);
    }
}
#endregion

#region UpHoldArea
class UpHoldArea : HoldArea
{
    float currentCardPositionX = Constants.DownHoldAreaCenterX;
    float currentCardPositionZ = -1f;

    private GameObject IssueButton;
    private readonly GameObject tributeButton;
    private readonly GameObject backButton;
    public override GameObject issueButton { get { return IssueButton; } }
    public override HandArea handArea { get; set; }
    public override GameObject TributeButton { get { return tributeButton; } }
    public override GameObject BackButton { get { return backButton; } }


    public UpHoldArea(Controller controller) : base(controller)
    {
        pos = 2;
        isCampA = true;
        distance = Constants.UpDownHoldAreaDistance;
        IssueButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/IssueButton"));

        Canvas canvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();
        IssueButton.transform.SetParent(canvas.transform, false);
        IssueButton.transform.localPosition = new Vector3(-5f, 3f, -3f);

        tributeButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/TributeButton"));
        tributeButton.transform.SetParent(canvas.transform, false);
        Vector3 tributeButtonPosition = issueButton.transform.position;
        tributeButton.transform.localPosition = tributeButtonPosition;

        backButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ReturnButton"));
        backButton.transform.SetParent(canvas.transform, false);
        Vector3 backButtonPosition = issueButton.transform.position;
        backButton.transform.localPosition = backButtonPosition;

        SetButtonOnClick();
        handArea = new HandArea(new Vector3(0f, 2.5f, -3f), 0);
    }

    private void SetButtonOnClick()
    {
        IssueButton.GetComponent<Button>().onClick.AddListener(Issue);
        IssueButton.SetActive(false);

        tributeButton.GetComponent<Button>().onClick.AddListener(Tribute);
        tributeButton.SetActive(false);

        backButton.GetComponent<Button>().onClick.AddListener(Back);
        backButton.SetActive(false);
    }


    public override void MakeUp()
    {
        SortCards();
        int i = HowManyDifferentRankCards();
        int preValue = -1;
        currentCardPositionX = -((i + 1) / 2f) * distance;
        float currentCardPositionY = Constants.UpHoldAreaCenterY;
        currentCardPositionZ = -3f;
        float offset = -Constants.HoldAreaOffsetZ;
        foreach (Card card in cards)
        {
            if (card.rank != preValue)
            {
                currentCardPositionX += distance;
                offset = -Constants.HoldAreaOffsetZ;
                currentCardPositionY = Constants.UpHoldAreaCenterY;
                card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ);
            }
            else
            {
                currentCardPositionY += Constants.UpHoldAreaOffsetY;
                card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ + offset);
                offset -= Constants.HoldAreaOffsetZ;
            }
            preValue = card.rank;
            currentCardPositionZ -= 0.16f;
            card.cardObject.GetComponent<CardBehaviour>().ResetCard();
        }
    }
    public override void Initialize(Card card)
    {
        card.cardBehaviour.SetCanPick(true);
        card.cardObject.transform.position = new Vector3(currentCardPositionX, 6f, currentCardPositionZ);
        currentCardPositionX += distance;
        currentCardPositionZ -= 0.01f;
        card.cardObject.SetActive(true);

        cards.Add(card);
    }

    public override void DisableAllButtons()
    {
        TributeButton.SetActive(false);
        BackButton.SetActive(false);
    }
}
#endregion

#region LeftHoldArea
class LeftHoldArea : HoldArea
{
    private float currentCardPositionX = Constants.LeftHoldAreaCenterX;
    private float currentCardPositionY = 0f;
    private float currentCardPositionZ = -3f;

    private GameObject IssueButton;
    private GameObject tributeButton;
    private readonly GameObject backButton;
    public override GameObject issueButton { get { return IssueButton; } }
    public override HandArea handArea { get; set; }
    public override GameObject TributeButton { get { return tributeButton; } }
    public override GameObject BackButton { get { return backButton; } }


    public LeftHoldArea(Controller controller) : base(controller)
    {
        pos = 3;
        distance = Constants.LeftRightHoldAreaDistance;

        IssueButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/IssueButton"));

        Canvas canvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();
        IssueButton.transform.SetParent(canvas.transform, false);
        IssueButton.transform.localPosition = new Vector3(-8.6f, -1.6f, -3f);

        tributeButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/TributeButton"));
        tributeButton.transform.SetParent(canvas.transform, false);
        Vector3 tributeButtonPosition = issueButton.transform.position;
        tributeButton.transform.localPosition = tributeButtonPosition;

        backButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ReturnButton"));
        backButton.transform.SetParent(canvas.transform, false);
        Vector3 backButtonPosition = issueButton.transform.position;
        backButton.transform.localPosition = backButtonPosition;

        SetButtonOnClick();
        handArea = new HandArea(new Vector3(-5.8f, 0f, -3f), 0);
    }

    public void SetButtonOnClick()
    {
        IssueButton.GetComponent<Button>().onClick.AddListener(Issue);
        IssueButton.SetActive(false);

        TributeButton.GetComponent<Button>().onClick.AddListener(Tribute);
        TributeButton.SetActive(false);

        backButton.GetComponent<Button>().onClick.AddListener(Back);
        backButton.SetActive(false);
    }

    public override void MakeUp()
    {
        SortCards();
        int i = HowManyDifferentRankCards();
        int preValue = -1;
        currentCardPositionY = (i + 1) / 2f * distance;
        currentCardPositionZ = -3f;
        // float offset = - Constants.HoldAreaOffsetZ;
        foreach (Card card in cards)
        {
            if (card.rank != preValue)
            {
                currentCardPositionY -= distance;
                // offset = - Constants.HoldAreaOffsetZ;
                currentCardPositionX = Constants.LeftHoldAreaCenterX;
                card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ);
            }
            else
            {
                currentCardPositionX += Constants.LeftHoldAreaOffsetX;
                card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ);
                // offset -= Constants.HoldAreaOffsetZ;
            }
            preValue = card.rank;
            currentCardPositionZ -= 0.01f;
            card.cardObject.GetComponent<CardBehaviour>().ResetCard();
        }
    }

    public override void Initialize(Card card)
    {
        card.cardBehaviour.SetCanPick(true);
        card.cardObject.transform.position = new Vector3(-6f, 0f, 0f);
        card.cardObject.SetActive(true);

        cards.Add(card);
    }

    public override void DisableAllButtons()
    {
        TributeButton.SetActive(false);
        BackButton.SetActive(false);
    }
}
#endregion

#region RightHoldArea
public partial class RightHoldArea : HoldArea
{
    private float currentCardPositionX = Constants.RightHoldAreaCenterX;
    private float currentCardPositionY = 0f;
    private float currentCardPositionZ = -3f;

    private GameObject IssueButton;
    private GameObject tributeButton;
    private readonly GameObject backButton;

    public override GameObject issueButton { get { return IssueButton; } }
    public override HandArea handArea { get; set; }

    public override GameObject TributeButton { get { return tributeButton; } }
    public override GameObject BackButton { get { return backButton; } }

    public RightHoldArea(Controller controller) : base(controller)
    {
        pos = 1;
        distance = Constants.LeftRightHoldAreaDistance;

        IssueButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/IssueButton"));

        Canvas canvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();
        IssueButton.transform.SetParent(canvas.transform, false);
        IssueButton.transform.localPosition = new Vector3(9.4f, 2.6f, -3f);

        tributeButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/TributeButton"));
        tributeButton.transform.SetParent(canvas.transform, false);
        Vector3 tributeButtonPosition = issueButton.transform.position;
        tributeButton.transform.localPosition = tributeButtonPosition;

        backButton = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/ReturnButton"));
        backButton.transform.SetParent(canvas.transform, false);
        Vector3 backButtonPosition = issueButton.transform.position;
        backButton.transform.localPosition = backButtonPosition;

        SetButtonOnClick();
        handArea = new HandArea(new Vector3(5.8f, 0f, -3f), 0);
    }

    public void SetButtonOnClick()
    {
        IssueButton.GetComponent<Button>().onClick.AddListener(Issue);
        IssueButton.SetActive(false);

        TributeButton.GetComponent<Button>().onClick.AddListener(Tribute);
        TributeButton.SetActive(false);

        backButton.GetComponent<Button>().onClick.AddListener(Back);
        backButton.SetActive(false);
    }

    public override void MakeUp()
    {
        SortCards();
        int i = HowManyDifferentRankCards();
        int preValue = -1;
        currentCardPositionY = (i + 1) / 2f * distance;
        currentCardPositionZ = -2.91f;
        float offset = 0.01f;
        foreach (Card card in cards)
        {
            if (card.rank != preValue)
            {
                currentCardPositionZ -= 0.09f;
                currentCardPositionY -= distance;
                offset = 0.01f;
                currentCardPositionX = Constants.RightHoldAreaCenterX;
                // card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ);
                LeanTween.move(card.cardObject, new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ), 0.3f);
            }
            else
            {
                currentCardPositionX += Constants.RightHoldAreaOffsetX;
                // card.cardObject.transform.position = new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ + offset);
                LeanTween.move(card.cardObject, new Vector3(currentCardPositionX, currentCardPositionY, currentCardPositionZ + offset), 0.3f);
                offset += 0.01f;
            }
            preValue = card.rank;

            card.cardObject.GetComponent<CardBehaviour>().ResetCard();
        }
    }



    public override void Initialize(Card card)
    {
        card.cardBehaviour.SetCanPick(true);
        card.cardObject.transform.position = new Vector3(-6f, 0f, 0f);
        card.cardObject.SetActive(true);

        cards.Add(card);
    }

    public override void DisableAllButtons()
    {
        TributeButton.SetActive(false);
        BackButton.SetActive(false);
    }
}
#endregion
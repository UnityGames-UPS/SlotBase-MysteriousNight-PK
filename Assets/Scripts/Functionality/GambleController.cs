using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GambleController : MonoBehaviour
{
    // UI and Components References
    [Header("UI and Components")]
    [SerializeField] private GameObject gamble_game; // The main gamble game object
    [SerializeField] private Button doubleButton; // Button for doubling the bet
    [SerializeField] private SocketIOManager socketManager; // Reference to the SocketIO Manager
    [SerializeField] private AudioController audioController; // Reference to the Audio Controller
    [SerializeField] internal List<CardFlip> allcards = new List<CardFlip>(); // List of all card flip objects
    [SerializeField] private TMP_Text winamount; // Text to display the win amount
    [SerializeField] private SlotBehaviour slotController; // Reference to the Slot Controller
    [SerializeField] private Sprite[] HeartSpriteList; // List of heart suit sprites
    [SerializeField] private Sprite[] ClubSpriteList; // List of club suit sprites
    [SerializeField] private Sprite[] SpadeSpriteList; // List of spade suit sprites
    [SerializeField] private Sprite[] DiamondSpriteList; // List of diamond suit sprites
    [SerializeField] private Sprite cardCover; // Default card cover sprite
    [SerializeField] private CardFlip DealerCard_Script; // Reference to the dealer's card flip script

    // Gamble Section References
    [Header("Gamble Section References")]
    [SerializeField] private GameObject GambleEnd_Object; // Object to display when gamble ends
    [SerializeField] private Button m_Collect_Button; // Button for collecting winnings
    [SerializeField] private Button m_Double_Button; // Button for doubling winnings

    // Loading Screen References
    [Header("Loading Screen References")]
    [SerializeField] private GameObject loadingScreen; // Loading screen game object
    [SerializeField] private Transform progressbar; // Slider for loading screen
    [SerializeField] private GameObject[] progressDots;

    // Internal Variables
    private Sprite highcard_Sprite; // Sprite for the high card
    private Sprite lowcard_Sprite; // Sprite for the low card
    private Sprite spare1card_Sprite; // Sprite for the first spare card
    private Sprite spare2card_Sprite; // Sprite for the second spare card
    internal bool gambleStart = false; // Indicates if the gamble has started
    internal bool isResult = false; // Indicates if the result has been received

    private Tweener Gamble_Tween_Scale = null; // Tweener for scaling the double button
    internal double gambleAmount;

    [SerializeField] internal bool isAutoSpinOn;
    private string[] cardSuits = new string[] { "Hearts", "Diamonds", "Clubs", "Spades" };
    private cardStruct dealerCard = new cardStruct();
    private cardStruct playerCard = new cardStruct();
    private cardStruct spare1Card = new cardStruct();
    private cardStruct spare2Card = new cardStruct();

    [SerializeField] private Button[] cardsButtons;

    #region Initialization

    private void Start()
    {
        // Setup event listeners for buttons
        if (doubleButton)
        {
            doubleButton.onClick.RemoveAllListeners();
            doubleButton.onClick.AddListener(delegate
            {
                audioController.PlayButtonAudio();
                StartGamblegame(false);
            });
        }

        // Collect Button Setup
        if (m_Collect_Button)
        {
            m_Collect_Button.onClick.RemoveAllListeners();
            m_Collect_Button.onClick.AddListener(() =>
            {
                audioController.PlayButtonAudio();
                OnReset();
            });
        }

        // Double Button Setup
        if (m_Double_Button)
        {
            m_Double_Button.onClick.RemoveAllListeners();
            m_Double_Button.onClick.AddListener(delegate
            {
                audioController.PlayButtonAudio();
                NormalCollectFunction();
                StartGamblegame(true);
            });
        }

        toggleDoubleButton(false); // Disable double button at start
    }

    #endregion

    #region Button Toggle

    // Toggles the interactability of the double button
    internal void toggleDoubleButton(bool toggle)
    {
        doubleButton.interactable = toggle;
    }

    #endregion

    #region Gamble Game

    // Starts the gamble game
    void StartGamblegame(bool isRepeat = false)
    {
        if (GambleEnd_Object) GambleEnd_Object.SetActive(false);
        GambleTweeningAnim(false);
        slotController.DeactivateGamble();
        if (!isRepeat)
        {
            winamount.text = "0";
        }
        if (audioController) audioController.PlayButtonAudio();
        if (gamble_game) gamble_game.SetActive(true);
        SetCardButtonsInteractable(true);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadingRoutine());
        if (!isRepeat) socketManager.OnGamble();
        //  StartCoroutine(GambleCoroutine(isRepeat));
        // StartCoroutine(GambleCoroutine()); // Start gamble coroutine
    }

    // Resets the game and collects winnings
    private void OnReset()
    {
        if (slotController) slotController.GambleCollect(); // Collect winnings
        socketManager.resultData.payload.winAmount = socketManager.GambleData.payload.winAmount;
        socketManager.playerdata.balance = socketManager.GambleData.player.balance;
        slotController.updateBalance();
        if (isAutoSpinOn)
        {
            isAutoSpinOn = false;
            slotController.AutoSpin();
        }
        NormalCollectFunction(); // Reset the gamble game
    }

    // Normal collect function
    private void NormalCollectFunction()
    {
        gambleStart = false; // End gamble
        slotController.updateBalance(); // Update player balance

        if (gamble_game) gamble_game.SetActive(false); // Hide gamble game

        // Reset all card flip objects
        allcards.ForEach((element) =>
        {
            element.Card_Button.image.sprite = cardCover;
            element.Reset();
        });

        // Reset dealer's card
        DealerCard_Script.Card_Button.image.sprite = cardCover;
        DealerCard_Script.once = false;
        winamount.text = "0";
        toggleDoubleButton(false); // Disable double button
    }

    #endregion

    #region Card Handling

    // Compute the card sprites based on the received message
    private void ComputeCards()
    {
        // highcard_Sprite = CardSet(socketManager.myMessage.highCard.suit, socketManager.myMessage.highCard.value);
        // lowcard_Sprite = CardSet(socketManager.myMessage.lowCard.suit, socketManager.myMessage.lowCard.value);
        // spare1card_Sprite = CardSet(socketManager.myMessage.exCards[0].suit, socketManager.myMessage.exCards[0].value);
        // spare2card_Sprite = CardSet(socketManager.myMessage.exCards[1].suit, socketManager.myMessage.exCards[1].value);


        dealerCard = ChoseARandomeCard(socketManager.GambleData.payload.cards.dealerCard - 1);
        playerCard = ChoseARandomeCard(socketManager.GambleData.payload.cards.playerCard - 1);
        spare1Card = FindUniqueCard();
        spare2Card = FindUniqueCard();
        Debug.Log($" Dealer :" + socketManager.GambleData.payload.cards.dealerCard + "Player  " + socketManager.GambleData.payload.cards.playerCard);


        highcard_Sprite = CardSet(dealerCard.suit, dealerCard.value);
        lowcard_Sprite = CardSet(playerCard.suit, playerCard.value);
        spare1card_Sprite = CardSet(spare1Card.suit, spare1Card.value);
        spare2card_Sprite = CardSet(spare2Card.suit, spare2Card.value);
    }

    private Sprite CardSet(string suit, int value)
    {

        Sprite tempSprite = null;
        switch (suit.ToUpper())
        {
            case "HEARTS":
                tempSprite = HeartSpriteList[value];
                break;
            case "DIAMONDS":
                tempSprite = DiamondSpriteList[value];
                break;
            case "CLUBS":
                tempSprite = ClubSpriteList[value];
                break;
            case "SPADES":
                tempSprite = SpadeSpriteList[value];
                break;
            default:
                Debug.LogError("Invalid Suit: " + suit);
                break;
        }
        return tempSprite;
    }

    private cardStruct ChoseARandomeCard(int val = -1)
    {
        cardStruct cardx = new cardStruct();
        string suit;
        int value;

        int index = UnityEngine.Random.Range(0, cardSuits.Length);
        suit = cardSuits[index];

        if (val == -1)
        {
            value = UnityEngine.Random.Range(0, 13);

        }
        else
        {
            value = val;
        }
        cardx.suit = suit;
        cardx.value = value;
        Debug.Log($"ChoseARandomeCard: {cardx.suit} {cardx.value}");
        return cardx;
    }
    private cardStruct FindUniqueCard()
    {
        cardStruct newCard = null;
        newCard = ChoseARandomeCard();

        if (newCard == dealerCard && newCard == playerCard)
        {
            return FindUniqueCard();
        }
        else
        {
            return newCard;
        }
    }

    // Helper function to get the correct sprite from a sprite list based on value
    private Sprite GetCardSprite(Sprite[] spriteList, string value)
    {
        switch (value.ToUpper())
        {
            case "A": return spriteList[0];
            case "K": return spriteList[12];
            case "Q": return spriteList[11];
            case "J": return spriteList[10];
            default:
                int myval = int.Parse(value);
                return spriteList[myval - 1];
        }
    }

    #endregion

    #region Coroutines

    // Main coroutine for handling the gamble process
    public IEnumerator GambleCoroutine(bool isRepeate = false)
    {
        for (int i = 0; i < allcards.Count; i++)
        {
            allcards[i].once = false;
        }

        if (isRepeate) socketManager.GambleDraw();
        else socketManager.OnGamble(); // Send gamble request 
        Debug.Log($"Gamble coroutine styarted ");
        yield return new WaitUntil(() => socketManager.isResultdone);
        ComputeCards();
        gambleStart = true;
    }
    // Coroutine for handling the loading screen
    //IEnumerator loadingRoutine()
    //{
    //    float fillAmount = 1;
    //    while (fillAmount > 0.1)
    //    {
    //        fillAmount -= Time.deltaTime;
    //        slider.fillAmount = fillAmount;
    //        if (fillAmount == 0.1) yield break;
    //        yield return null;
    //    }
    //    yield return new WaitUntil(() => gambleStart);
    //    slider.fillAmount = 0;
    //    yield return new WaitForSeconds(1f);
    //    loadingScreen.SetActive(false);
    //}

    IEnumerator LoadingRoutine()
    {
        foreach (var i in progressDots)
        {
            i.SetActive(true);
            yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(.2f);
        //  yield return new WaitUntil(() => gambleStart);
        loadingScreen.SetActive(false);
        ResetProgressDots();
    }

    private void ResetProgressDots()
    {
        foreach (var i in progressDots)
        {
            i.SetActive(false);
        }
    }

    // Coroutine for collecting winnings
    private IEnumerator NewCollectRoutine()
    {
        isResult = false;
        socketManager.OnCollect(); // Send collect request

        yield return new WaitUntil(() => socketManager.isResultdone); // Wait for result
        isResult = true; // Mark result as received
    }

    // Coroutine for resetting the game after collection
    IEnumerator Collectroutine()
    {
        yield return new WaitForSeconds(2f);
        gambleStart = false;
        yield return new WaitForSeconds(1f);
        socketManager.resultData.payload.winAmount = socketManager.GambleData.payload.winAmount;
        socketManager.playerdata.balance = socketManager.GambleData.player.balance;
        slotController.updateBalance();
        if (gamble_game) gamble_game.SetActive(false);
        allcards.ForEach((element) =>
        {
            element.Card_Button.image.sprite = cardCover;
            element.Reset();
        });
        DealerCard_Script.Card_Button.image.sprite = cardCover;
        DealerCard_Script.once = false;
        toggleDoubleButton(false);

        if (isAutoSpinOn)
        {
            slotController.AutoSpin();
        }
    }

    #endregion

    #region Gamble Actions

    // Get the correct card sprite based on the player's result
    internal Sprite GetCard()
    {
        // if (socketManager.myMessage.playerWon)
        // {
        //     if (DealerCard_Script) DealerCard_Script.cardImage = lowcard_Sprite;
        //     return highcard_Sprite;
        // }
        // else
        // {
        //     if (DealerCard_Script) DealerCard_Script.cardImage = highcard_Sprite;
        //     return lowcard_Sprite;
        // }
        if (DealerCard_Script) DealerCard_Script.cardImage = highcard_Sprite;
        return lowcard_Sprite;
    }

    // Flip all the cards when the game ends
    internal void FlipAllCard()
    {
        int cardVal = 0;
        for (int i = 0; i < allcards.Count; i++)
        {
            if (allcards[i].once) continue;

            allcards[i].Card_Button.interactable = false;
            if (cardVal == 0)
            {
                allcards[i].cardImage = spare1card_Sprite;
                cardVal++;
            }
            else
            {
                allcards[i].cardImage = spare2card_Sprite;
            }
            allcards[i].FlipMyObject();
            allcards[i].Card_Button.interactable = false;
        }

        if (DealerCard_Script) DealerCard_Script.FlipMyObject();

        if (socketManager.GambleData.payload.playerWon)
        {
            winamount.text = "YOU WIN\n" + socketManager.GambleData.payload.winAmount.ToString();
            if (GambleEnd_Object) GambleEnd_Object.SetActive(true);
        }
        else
        {
            winamount.text = "YOU LOSE\n0";
            StartCoroutine(Collectroutine());
        }
    }

    // Starts the coroutine for collecting winnings
    internal void RunOnCollect()
    {
        StartCoroutine(NewCollectRoutine());
    }

    // Coroutine to handle the game over situation
    void OnGameOver()
    {
        StartCoroutine(Collectroutine());
    }

    #endregion

    #region Tweening Animations

    // Controls the scaling animation for the double button
    internal void GambleTweeningAnim(bool IsStart)
    {
        if (IsStart)
        {
            Gamble_Tween_Scale = doubleButton.gameObject.GetComponent<RectTransform>()
                .DOScale(new Vector2(1.18f, 1.18f), 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(0);
        }
        else
        {
            Gamble_Tween_Scale.Kill();
            doubleButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    internal void SetCardButtonsInteractable(bool interactable)
    {
        foreach (Button btn in cardsButtons)
        {
            btn.interactable = interactable;
        }
    }


    #endregion
}
public class cardStruct
{
    public string suit;
    public int value;
}

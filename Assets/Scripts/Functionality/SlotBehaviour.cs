using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
    [SerializeField]
    private List<SlotImage> Tempimages;

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button TBetPlus_Button;
    [SerializeField]
    private Button TBetMinus_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button Turbo_Button;
    [SerializeField]
    private Button StopSpin_Button;

    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button Double_button;
    [SerializeField] private Button _buttonBetone;


    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] TurboToggleSprites;

    [SerializeField]
    private Sprite[] Symbol1;
    [SerializeField]
    private Sprite[] Symbol2;
    [SerializeField]
    private Sprite[] Symbol3;
    [SerializeField]
    private Sprite[] Symbol4;
    [SerializeField]
    private Sprite[] Symbol5;
    [SerializeField]
    private Sprite[] Symbol6;
    [SerializeField]
    private Sprite[] Symbol7;
    [SerializeField]
    private Sprite[] Symbol8;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private int[] Lines_num;
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private Image Lines_Image;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private TMP_Text Lines_text;
    [SerializeField] TMP_Text BetperLine_text;

    [SerializeField]
    private Sprite[] Lines_Sprites;
    private int LineCounter = 0;


    int tweenHeight = 0;

    [SerializeField]
    private GameObject Image_Prefab;
    [SerializeField]
    private GameObject Gamble;

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    [SerializeField]
    private List<ImageAnimation> TempList;

    [SerializeField]
    private int IconSizeFactor = 100;
    [SerializeField] private int SpacingFactor;

    private int numberOfSlots = 5;

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;
    [SerializeField]
    private BonusController _bonusManager;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private GambleController gambleController;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine tweenroutine = null;
    private Tween BalanceTween;
    private bool StopSpinToggle;
    private float SpinDelay = 0.2f;
    private bool IsTurboOn;
    internal bool WasAutoSpinOn;
    internal bool IsAutoSpin = false;
    private bool IsSpinning = false;
    internal bool CheckPopups = false;
    private bool CheckSpinAudio = false;
    public int BetCounter = 0;
    private Coroutine FreeSpinRoutine = null;
    private bool IsFreeSpin = false;
    private double currentTotalBet = 0;
    private double currentBalance = 0;

    internal List<int> dynamicLinesIndex = new List<int>();


    [SerializeField] private AudioController audioController;

    private void Start()
    {

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        //if (LinePlus_Button) LinePlus_Button.onClick.RemoveAllListeners();
        //if (LinePlus_Button) LinePlus_Button.onClick.AddListener(delegate { ToggleLine(true); });

        //if (LineMinus_Button) LineMinus_Button.onClick.RemoveAllListeners();
        //if (LineMinus_Button) LineMinus_Button.onClick.AddListener(delegate { ToggleLine(false); });

        if (Lines_Image) Lines_Image.sprite = Lines_Sprites[8];
        LineCounter = 0;

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(delegate
        {
            audioController.PlayButtonAudio();
            StopAutoSpin();
            gambleController.isAutoSpinOn = false;
        });

        //if (_buttonBetone) _buttonBetone.onClick.RemoveAllListeners();
        //if (_buttonBetone) _buttonBetone.onClick.AddListener(OnBetChange);

        if (TBetPlus_Button) TBetPlus_Button.onClick.RemoveAllListeners();
        if (TBetPlus_Button) TBetPlus_Button.onClick.AddListener(delegate { OnBetChange(true); });

        if (TBetMinus_Button) TBetMinus_Button.onClick.RemoveAllListeners();
        if (TBetMinus_Button) TBetMinus_Button.onClick.AddListener(delegate { OnBetChange(false); });

        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() =>
        {
            audioController.PlayButtonAudio();
            StopSpinToggle = true;
            StopSpin_Button.gameObject.SetActive(false);
            SlotStart_Button.gameObject.SetActive(true);
        });

        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(delegate
        {
            audioController.PlayButtonAudio();
            TurboToggle();
        });

        tweenHeight = (8 * IconSizeFactor) - 280;
    }

    internal void AutoSpin()
    {
        if (!IsAutoSpin && !IsSpinning)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    void TurboToggle()
    {

        if (IsTurboOn)
        {

            IsTurboOn = false;

            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();

            Turbo_Button.image.sprite = TurboToggleSprites[0];

            Turbo_Button.image.color = new Color(0.86f, 0.86f, 0.86f, 1);

        }

        else
        {

            IsTurboOn = true;

            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();

            Turbo_Button.image.color = new Color(1, 1, 1, 1);

        }

    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        LineCounter = SocketManager.initialData.lines.Count - 1;
        if (Lines_text) Lines_text.text = SocketManager.initialData.lines.Count.ToString();
        //  PayCalculator.SetButtonActive(SocketManager.initialData.LinesCount[LineCounter]);
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * SocketManager.initialData.lines.Count).ToString();
        // if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("F3");
        if (Balance_text) Balance_text.text = SocketManager.playerdata.balance.ToString("F3");
        if (BetperLine_text) BetperLine_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        currentBalance = SocketManager.playerdata.balance;
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * SocketManager.initialData.lines.Count;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.paylines);
    }

    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    private IEnumerator AutoSpinCoroutine()
    {

        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
        }
        Debug.Log("Was Auto Spin Is False Now...");
        WasAutoSpinOn = false;
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            //uiManager.FreeSpins--;
            //StartSlots(IsAutoSpin);
            //if (FSnum_text) FSnum_text.text = (spinchances - i).ToString();
            //if (FSnum_text) FSnum_text.text = uiManager.FreeSpins.ToString();
            StartSlots();
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            i++;
        }
        if (WasAutoSpinOn)
        {
            AutoSpin();
        }
        else
        {
            ToggleButtonGrp(true);
        }
        IsFreeSpin = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.bets.Count - 1;
        if (BetperLine_text) BetperLine_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * SocketManager.initialData.lines.Count).ToString();
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * SocketManager.initialData.lines.Count;


    }

    void OnBetChange(bool inc)
    {
        //if (audioController) audioController.PlayButtonAudio();

        //if (inc && (BetCounter < SocketManager.initialData.Bets.Count - 1))
        //    BetCounter++;
        //else if (!inc && (BetCounter > 0))
        //    BetCounter--;

        if (audioController) audioController.PlayButtonAudio();
        if (inc)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.bets.Count - 1; // Loop to the last bet
            }
        }

        if (BetperLine_text) BetperLine_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * SocketManager.initialData.lines.Count).ToString();
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * SocketManager.initialData.lines.Count;

    }

    private void ToggleLine(bool inc)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (inc)
        {
            LineCounter++;
            if (LineCounter == Lines_num.Length)
            {
                LineCounter = 0;
            }
        }
        else
        {
            LineCounter--;
            if (LineCounter == -1)
            {
                LineCounter = Lines_num.Length - 1;
            }
        }
        if (Lines_Image) Lines_Image.sprite = Lines_Sprites[LineCounter];
        PayCalculator.DontDestroy.Clear();
        PayCalculator.ResetLines();
        PayCalculator.GeneratePayoutLinesBackend(-1);
        PayCalculator.SetButtonActive(SocketManager.initialData.LinesCount[LineCounter]);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
    //    {
    //        StartSlots();
    //    }
    //}

    //internal void PopulateInitalSlots(int number, List<int> myvalues)
    //{
    //    PopulateSlot(myvalues, number);
    //}

    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }


    //private void PopulateSlot(List<int> values, int number)
    //{
    //    if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);

    //    for (int i = 0; i < values.Count; i++)
    //    {
    //        GameObject myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(myImg.GetComponent<Image>());
    //        images[number].slotImages[i].sprite = myImages[values[i]];
    //    }
    //    for (int k = 0; k < 2; k++)
    //    {
    //        GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(mylastImg.GetComponent<Image>());
    //        images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
    //    }
    //    if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
    //    tweenHeight = (values.Count * IconSizeFactor) - 280;
    //    GenerateMatrix(number);
    //}

    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        animScript.AnimationSpeed = 30;

        switch (val)
        {
            case 0:
                for (int i = 0; i < Symbol1.Length; i++)
                {
                    animScript.textureArray.Add(Symbol1[i]);
                }
                break;
            case 1:
                for (int i = 0; i < Symbol2.Length; i++)
                {
                    animScript.textureArray.Add(Symbol2[i]);
                }
                break;
            case 2:
                for (int i = 0; i < Symbol3.Length; i++)
                {
                    animScript.textureArray.Add(Symbol3[i]);
                }
                break;
            case 3:
                for (int i = 0; i < Symbol4.Length; i++)
                {
                    animScript.textureArray.Add(Symbol4[i]);
                }
                break;
            case 4:
                for (int i = 0; i < Symbol5.Length; i++)
                {
                    animScript.textureArray.Add(Symbol5[i]);
                }
                break;
            case 5:
                for (int i = 0; i < Symbol6.Length; i++)
                {
                    animScript.textureArray.Add(Symbol6[i]);
                }
                break;
            case 6:
                for (int i = 0; i < Symbol7.Length; i++)
                {
                    animScript.textureArray.Add(Symbol7[i]);
                }
                break;
            case 7:
                for (int i = 0; i < Symbol8.Length; i++)
                {
                    animScript.textureArray.Add(Symbol8[i]);
                    animScript.AnimationSpeed = 18;
                }
                break;
        }
    }
    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }
    private void StartSlots(bool autoSpin = false)
    {
        gambleController.GambleTweeningAnim(false);

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        if (audioController) audioController.PlayButtonAudio("spin");

        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    private IEnumerator TweenRoutine()
    {
        TotalWin_text.text = "0.00";
        dynamicLinesIndex.Clear();
        gambleController.GambleTweeningAnim(false);
        if (currentBalance < currentTotalBet)
        {
            CompareBalance();
            if (IsAutoSpin)
            {

                StopAutoSpin();
                yield return new WaitForSeconds(1);
            }
            ToggleButtonGrp(true);
            yield break;
        }

        yield return new WaitForSeconds(0.3f);
        if (audioController) audioController.PlaySpinBonusAudio();
        CheckSpinAudio = true;
        IsSpinning = true;
        ToggleButtonGrp(false);
        gambleController.toggleDoubleButton(false);

        //HACK: Modification For Turbo
        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
            SlotStart_Button.gameObject.SetActive(false);
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        double initAmount = balance;
        balance = balance - (bet);

        BalanceTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f3");
        });

        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int resultNum = int.Parse(SocketManager.resultData.matrix[i][j]);
                // print("resultNum: " + resultNum);
                // print("image loc: " + j + " " + i);
                PopulateAnimationSprites(Tempimages[j].slotImages[i].GetComponent<ImageAnimation>(), resultNum);
                Tempimages[j].slotImages[i].sprite = myImages[resultNum];
            }
        }

        //yield return new WaitForSeconds(0.5f);

        // if (IsTurboOn || IsFreeSpin)
        // {
        //     yield return new WaitForSeconds(0.1f);
        // }
        if (!(IsTurboOn || IsFreeSpin))
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.35f);
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
            SlotStart_Button.gameObject.SetActive(true);
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            //yield return StopTweening(5, Slot_Transform[i], i);
            yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
        }

        StopSpinToggle = false;
        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();

        yield return new WaitForSeconds(0.1f);
        if (SocketManager.resultData.payload.winAmount > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.2f;
        }
        if (SocketManager.resultData.payload.winAmount > 0)
        {
            List<int> winLine = new();
            foreach (var win in SocketManager.resultData.payload.wins)
            {
                winLine.Add(win.line);
            }
            CheckPopups = true;
            StartCoroutine(CheckPayoutLineBackend(winLine));
            yield return new WaitUntil(() => !CheckPopups);

        }
        CheckForFeaturesAnimation();
        // CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);

        if (audioController) audioController.StopApinBonusAudio();

        CheckPopups = true;

        BalanceTween?.Kill();
        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("f3");
        if (Balance_text) Balance_text.text = SocketManager.resultData.player.balance.ToString("f3");
        if (SocketManager.resultData.bonus.isTriggered)
        {
            yield return new WaitForSeconds(0.5f);
            _bonusManager.ChestOpen(); //SocketManager.resultData.BonusResult, SocketManager.initialData.bets[BetCounter]

        }
        else if (SocketManager.resultData.payload.winAmount >= bet * 10 && SocketManager.resultData.payload.winAmount < bet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.payload.winAmount);
        }
        else if (SocketManager.resultData.payload.winAmount >= bet * 15 && SocketManager.resultData.payload.winAmount < bet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.payload.winAmount);
        }
        else if (SocketManager.resultData.payload.winAmount >= bet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.payload.winAmount);
        }
        else
        {
            CheckPopups = false;
        }


        yield return new WaitUntil(() => !CheckPopups);
        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("f3");
        if (Balance_text) Balance_text.text = SocketManager.resultData.player.balance.ToString("f3");
        print("checkpopups, " + CheckPopups);
        if (!IsAutoSpin)
        {
            ActivateGamble();
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            // ActivateGamble();
            //yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }

    }

    private void ActivateGamble()
    {
        if (SocketManager.resultData.payload.winAmount > 0)
        {
            gambleController.gambleAmount = SocketManager.resultData.payload.winAmount;
            gambleController.GambleTweeningAnim(true);
            gambleController.toggleDoubleButton(true);
        }
    }

    internal void DeactivateGamble()
    {
        if (IsAutoSpin)
        {
            StopAutoSpin();
        }
        ToggleButtonGrp(true);
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseWebSocket();
    }

    //internal void CheckBonusGame()
    //{
    //    if (SocketManager.resultData.isBonus)
    //    {
    //        _bonusManager.ChestOpen(SocketManager.resultData.BonusResult);
    //    }
    //    else
    //    {
    //        CheckPopups = false;
    //    }
    //}

    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        // if (LinePlus_Button) LinePlus_Button.interactable = toggle;
        // if (LineMinus_Button) LineMinus_Button.interactable = toggle;
        if (_buttonBetone) _buttonBetone.interactable = toggle;
        if (TBetPlus_Button) TBetPlus_Button.interactable = toggle;
        if (TBetMinus_Button) TBetMinus_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            //if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            //if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        //else
        //{
        //    if (AutoSpin_Button) AutoSpin_Button.interactable = true;
        //    if (SlotStart_Button) SlotStart_Button.interactable = true;
        //}
    }


    internal void updateBalance()
    {
        if (Balance_text) Balance_text.text = SocketManager.playerdata.balance.ToString();
        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString();
    }

    private void StartGameAnimation(GameObject animObjects)
    {

        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();

        temp.StartAnimation();
        TempList.Add(temp);
    }

    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }

    }

    private IEnumerator CheckPayoutLineBackend(List<int> LineId)
    {
        List<int> points_anim = null;

        List<KeyValuePair<int, int>> Totalcoords = new();
        for (int i = 0; i < LineId.Count; i++)
        {
            List<KeyValuePair<int, int>> coords = new();

            Debug.Log("line come " + LineId[i]);
            PayCalculator.DontDestroyLines.Add(LineId[i]);
            PayCalculator.GeneratePayoutLinesBackend(LineId[i]);
            dynamicLinesIndex.Add(LineId[i]);

            for (int k = 0; k < SocketManager.resultData.payload.wins[i].positions.Count; k++)
            {
                int rowIndex = SocketManager.initialData.lines[LineId[i]][SocketManager.resultData.payload.wins[i].positions[k]];
                int columnIndex = SocketManager.resultData.payload.wins[i].positions[k];
                coords.Add(new KeyValuePair<int, int>(rowIndex, columnIndex));
                Totalcoords.Add(new KeyValuePair<int, int>(rowIndex, columnIndex));
            }

            foreach (var coord in coords)
            {
                int rowIndex = coord.Key;
                int columnIndex = coord.Value;
                StartGameAnimation(Tempimages[columnIndex].slotImages[rowIndex].gameObject);
            }
            // yield return new WaitForSeconds(1f);
            // foreach (var coord in coords)
            // {

            //     int rowIndex = coord.Key;
            //     int columnIndex = coord.Value;
            //     Tempimages[columnIndex].slotImages[rowIndex].gameObject.GetComponent<ImageAnimation>().StopAnimation();

            // }
            //PayoutLines[LineId[i]].SetActive(false);
            yield return null;
        }
        CheckPopups = false;



        //     if (jackpot > 0)
        //     {
        //         for (int i = 0; i < Tempimages.Count; i++)
        //         {
        //             for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
        //             {
        //                 StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         for (int i = 0; i < points_AnimString.Count; i++)
        //         {
        //             points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

        //             for (int k = 0; k < points_anim.Count; k++)
        //             {
        //                 if (points_anim[k] >= 10)
        //                 {
        //                     StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
        //                 }
        //                 else
        //                 {
        //                     StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
        //                 }
        //             }
        //         }
        //     }
        //     WinningsAnim(true);
        // }
        // else
        // {

        //     if (audioController) audioController.StopWLAaudio();
        // }
        // CheckSpinAudio = false;

    }
    private void CheckForFeaturesAnimation()
    {
        bool playJackpot = false;
        bool playScatter = false;
        bool playBonus = false;
        bool playFreespin = false;


        if (SocketManager.resultData.bonus.isTriggered)
        {
            playBonus = true;
        }

        PlayFeatureAnimation(playJackpot, playScatter, playBonus, playFreespin);
    }
    private void PlayFeatureAnimation(bool jackpot = false, bool scatter = false, bool bonus = false, bool freeSpin = false)
    {
        for (int i = 0; i < SocketManager.resultData.matrix.Count; i++)
        {
            for (int j = 0; j < SocketManager.resultData.matrix[i].Count; j++)
            {

                if (int.TryParse(SocketManager.resultData.matrix[i][j], out int parsedNumber))
                {

                    if (bonus && parsedNumber == 6)
                    {
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }

                }

            }
        }
    }


    private void GenerateMatrix(int value)
    {
        for (int j = 0; j < 3; j++)
        {
            Tempimages[value].slotImages.Add(images[value].slotImages[images[value].slotImages.Count - 5 + j]);
        }
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * (IconSizeFactor + SpacingFactor)) - (IconSizeFactor + (2 * SpacingFactor));
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpacingFactor > 0 ? SpacingFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
        //yield return new WaitForSeconds(0.2f);
        if (!isStop)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return null;
        }
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion
    internal void GambleCollect()
    {
        SocketManager.GambleCollect();
    }
}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}


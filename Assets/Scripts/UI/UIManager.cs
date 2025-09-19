using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;


public class UIManager : MonoBehaviour
{
    [Header("scripts")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotBehaviour slotManager;
    [SerializeField] private SocketIOManager socketIOManager;

    [SerializeField]
    private Button SkipWinAnimation;


    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("Paytable Popup")]
    [SerializeField]
    private GameObject PaytablePopup_Object;
    [SerializeField]
    private Button PaytableExit_Button;
    [SerializeField]
    private Image Info_Image;
    [SerializeField]
    private TMP_Text[] SymbolsText;
   [SerializeField] private TMP_Text[] SymbolsText1;
    [SerializeField] private TMP_Text[] SymbolsText2;

    [SerializeField]
    private TMP_Text m_Wild_Text;
    [SerializeField]
    private TMP_Text m_Bonus_Text;
    [SerializeField]
    private Button Info_Button;

    [Header("Win Popup")]
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private TMP_Text Win_Text;
    [SerializeField] private Image Win_Image;
    [SerializeField] private Sprite HugeWin_Sprite;
    [SerializeField] private Sprite MegaWin_Sprite;
    [SerializeField] private Sprite BigWin_Sprite;

    [Header("Splash screen")]
    [SerializeField] private GameObject SplashScreen;
    [SerializeField] private Transform progressbar;
    [SerializeField] private TMP_Text loading_text;

    [Header("disconnection popup")]
    [SerializeField] private GameObject disconnecitonPopUp_object;
    [SerializeField] private Button disconnection_close;
    [SerializeField] private GameObject reconnecitonPopUp_object;

    [Header("low balance popup")]
    [SerializeField] private GameObject LBPopup_Object;
    [SerializeField] private Button LBExit_Button;

    [Header("quit popup")]
    [SerializeField] private GameObject QuitPopup_Object;
    [SerializeField] private Button GameExit_Button;
    [SerializeField] private Button YesQuit_Button;
    [SerializeField] private Button NoQuit_Button;
    [SerializeField] private Button CancelQuit_Button;

    [Header("menu popup")]
    [SerializeField] private Transform MenuGrp;
    [SerializeField] private Button MenuButton;
    [SerializeField] private Sprite MenuOpenSprite;
    [SerializeField] private Sprite MenuCloseSprite;

    [Header("Settings Popup")]
    [SerializeField] private GameObject Settings_object;
    [SerializeField] private Button Setting_button;
    [SerializeField] private Button Setting_clsoe_button;
    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SoundSlider;

    [Header("AnotherDevice Popup")]
    [SerializeField] private Button CloseAD_Button;
    [SerializeField] private GameObject ADPopup_Object;

    [SerializeField] private Button m_AwakeGameButton;

    private bool isOpen;
    private Tween WinPopupTextTween;
    private Tween ClosePopupTween;
    internal int FreeSpins;
    private bool isExit = false;

    private void Start()
    {
        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

        if (Info_Button) Info_Button.onClick.RemoveAllListeners();
        if (Info_Button) Info_Button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });


        if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
        if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (disconnection_close) disconnection_close.onClick.RemoveAllListeners();
        if (disconnection_close) disconnection_close.onClick.AddListener(delegate { CallOnExitFunction(); socketIOManager.ReactNativeCallOnFailedToConnect(); }); //BackendChanges

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

        if (NoQuit_Button) NoQuit_Button.onClick.RemoveAllListeners();
        if (NoQuit_Button) NoQuit_Button.onClick.AddListener(delegate { if (!isExit) ClosePopup(QuitPopup_Object); });

        if (CancelQuit_Button) CancelQuit_Button.onClick.RemoveAllListeners();
        if (CancelQuit_Button) CancelQuit_Button.onClick.AddListener(delegate { if (!isExit) ClosePopup(QuitPopup_Object); });

        if (YesQuit_Button) YesQuit_Button.onClick.RemoveAllListeners();
        if (YesQuit_Button) YesQuit_Button.onClick.AddListener(CallOnExitFunction);

        if (MenuButton) MenuButton.onClick.RemoveAllListeners();
        if (MenuButton) MenuButton.onClick.AddListener(delegate { OnMenuClick(); });

        if (Setting_clsoe_button) Setting_clsoe_button.onClick.RemoveAllListeners();
        if (Setting_clsoe_button) Setting_clsoe_button.onClick.AddListener(delegate { ClosePopup(Settings_object); });

        if (Setting_button) Setting_button.onClick.RemoveAllListeners();
        if (Setting_button) Setting_button.onClick.AddListener(delegate { OpenPopup(Settings_object); });

        if (SoundSlider) SoundSlider.onValueChanged.AddListener(delegate { ToggleSound(); });
        if (MusicSlider) MusicSlider.onValueChanged.AddListener(delegate { ToggleMusic(); });

        if (CloseAD_Button) CloseAD_Button.onClick.RemoveAllListeners();
        if (CloseAD_Button) CloseAD_Button.onClick.AddListener(CallOnExitFunction);

        if (SkipWinAnimation) SkipWinAnimation.onClick.RemoveAllListeners();
        if (SkipWinAnimation) SkipWinAnimation.onClick.AddListener(SkipWin);
    }

    private void ToggleMusic()
    {
        float value = MusicSlider.value;
        audioController.ToggleMute(value, "bg");

    }

    private void ToggleSound()
    {

        float value = SoundSlider.value;
        if (audioController) audioController.ToggleMute(value, "button");
        if (audioController) audioController.ToggleMute(value, "wl");
        if (audioController) audioController.ToggleMute(value, "ghost");


    }

    void SkipWin()
    {
        Debug.Log("Skip win called");
        if (ClosePopupTween != null)
        {
            ClosePopupTween.Kill();
            ClosePopupTween = null;
        }
        if (WinPopupTextTween != null)
        {
            WinPopupTextTween.Kill();
            WinPopupTextTween = null;
        }
        ClosePopup(WinPopup_Object);
        slotManager.CheckPopups = false;
    }

    internal void LowBalPopup()
    {
        OpenPopup(LBPopup_Object);
    }

    internal void PopulateWin(int value, double amount)
    {
        switch (value)
        {
            case 1:
                if (Win_Image) Win_Image.sprite = BigWin_Sprite;
                break;
            case 2:
                if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
                break;
            case 3:
                if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
                break;

        }


        StartPopupAnim(amount);
    }



    void OnMenuClick()
    {

        if (audioController) audioController.PlayButtonAudio();
        isOpen = !isOpen;
        if (isOpen)
        {
            MenuButton.image.sprite = MenuOpenSprite;
            for (int i = 0; i < MenuGrp.childCount - 2; i++)
            {
                MenuGrp.GetChild(i).DOLocalMoveX(-130 * (i + 1), 0.1f * (i + 1));
            }
        }
        else
        {
            MenuButton.image.sprite = MenuCloseSprite;

            for (int i = 0; i < MenuGrp.childCount - 2; i++)
            {
                MenuGrp.GetChild(i).DOLocalMoveX(0 * (i + 1), 0.1f * (i + 1));

            }

        }


    }




    private void StartPopupAnim(double amount)
    {
        double initAmount = 0;
        if (WinPopup_Object) WinPopup_Object.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        WinPopupTextTween = DOTween.To(() => initAmount, (val) => initAmount = val, (int)amount, 5f).OnUpdate(() =>
        {
            if (Win_Text) Win_Text.text = initAmount.ToString("F3");
        });

        ClosePopupTween = DOVirtual.DelayedCall(6f, () =>
        {
            ClosePopup(WinPopup_Object);
            Win_Text.text = "";
            // if (WinPopup_Object) WinPopup_Object.SetActive(false);

            // if (!disconnecitonPopUp_object.activeSelf)
            // {
            //     if (MainPopup_Object) MainPopup_Object.SetActive(false);
            // }

            // if (MainPopup_Object) MainPopup_Object.SetActive(false);
            slotManager.CheckPopups = false;
            //slotManager.CheckBonusGame();
        });
    }


    // internal void DisconnectionPopup(bool isReconnection)
    // {

    //     if (!isExit)
    //     {
    //         OpenPopup(disconnecitonPopUp_object);
    //     }

    // }

    internal void DisconnectionPopup()
    {
        if (!isExit)
        {
            OpenPopup(disconnecitonPopUp_object);
        }
    }

    internal void ReconnectionPopup()
    {
        OpenPopup(reconnecitonPopUp_object);
    }

    internal void CheckAndClosePopups()
    {
        if (reconnecitonPopUp_object.activeInHierarchy)
        {
            ClosePopup(reconnecitonPopUp_object);
        }
        if (disconnecitonPopUp_object.activeInHierarchy)
        {
            ClosePopup(disconnecitonPopUp_object);
        }
    }
    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }
    private void CallOnExitFunction()
    {
        isExit = true;
        slotManager.CallCloseSocket();
        //Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {

        if (audioController) audioController.PlayButtonAudio();

        if (Popup) Popup.SetActive(false);
        if (!disconnecitonPopUp_object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    internal void InitialiseUIData(Paylines symbolsText)
    {
        PopulateSymbolsPayout(symbolsText);
    }


    private void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            string text = null;
            if (paylines.symbols[i].multiplier[0] != 0)
            {
                if (SymbolsText[i]) SymbolsText[i].text = (paylines.symbols[i].multiplier[0] * socketIOManager.initialData.bets[slotManager.BetCounter]) + "x";
            }
            if (paylines.symbols[i].multiplier[1] != 0)
            {
                if (SymbolsText1[i]) SymbolsText1[i].text = (paylines.symbols[i].multiplier[1] * socketIOManager.initialData.bets[slotManager.BetCounter]) + "x";
            }
            if (paylines.symbols[i].multiplier[2] != 0)
            {
                 if (SymbolsText2[i]) SymbolsText2[i].text =  (paylines.symbols[i].multiplier[2] * socketIOManager.initialData.bets[slotManager.BetCounter]) + "x";
            }
        }

        for (int i = 0; i < paylines.symbols.Count; i++)
        {
            if (paylines.symbols[i].name.ToUpper() == "WILD")
            {
                if (m_Wild_Text) m_Wild_Text.text = paylines.symbols[i].description.ToString();
            }
            if (paylines.symbols[i].name.ToUpper() == "BONUS")
            {
                if (m_Bonus_Text) m_Bonus_Text.text = paylines.symbols[i].description.ToString();
            }
        }
    }


}

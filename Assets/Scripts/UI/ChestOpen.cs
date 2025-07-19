using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Sockets;

public class ChestOpen : MonoBehaviour
{
    
   public Button Chest;
    [SerializeField] private TMP_Text text;
    [SerializeField]
    private GameObject Chest_Opening;
    [SerializeField] private ImageAnimation imageAnimation;
    [SerializeField] private BonusController _bonusManager;
    [SerializeField] private AudioController audioController;

    [SerializeField]
    internal bool isOpen;

    //[Header("FreeSpins Popup")]
    //[SerializeField]
    private GameObject FreeSpinPopup_Object;
    //[SerializeField]
    private TMP_Text Free_Text;
    private int FreeSpins;
   // [SerializeField]
    private Button FreeSpin_Button;
    //[SerializeField]
    private GameObject Bonus_Object;

    //[SerializeField]
    private SlotBehaviour slotManager;
    private double value;

    [SerializeField] private SocketIOManager socketmanager;
    [SerializeField] private int chestIndex;
    void Start()
    {
        if (Chest) Chest.onClick.RemoveAllListeners();
        if (Chest) Chest.onClick.AddListener(()=> StartCoroutine(OpenCase()));

        if (FreeSpin_Button) FreeSpin_Button.onClick.RemoveAllListeners();
        if (FreeSpin_Button) FreeSpin_Button.onClick.AddListener(delegate { StartFreeSpins(FreeSpins); });
    }

    internal void ResetCase()
    {
        isOpen = false;
        text.gameObject.SetActive(false);
        Chest_Opening.SetActive(false);
        Chest.gameObject.SetActive(true);
        imageAnimation.StopAnimation();
    }

    IEnumerator OpenCase()
    {
        _bonusManager.BonusHidePanel.gameObject.SetActive(true);
        if (isOpen) yield break;
        if (_bonusManager.isOpening) yield break;
        if (_bonusManager.isFinisdhed) yield break;
        socketmanager.isResultdone = false;
        socketmanager.AccumulateTapBonusResult(chestIndex);
        yield return new WaitUntil(() => socketmanager.isResultdone);
         _bonusManager.BonusHidePanel.gameObject.SetActive(false);
        PopulateCase();
        Chest_Opening.SetActive(true);
        Chest.gameObject.SetActive(false);
        StartCoroutine(setCase());
    }

    void PopulateCase()
    {
        value = 0;

        double payout = socketmanager.bonusData.payload.payout;
        print("value " + value);
        if (payout > 0)
        {
            value = socketmanager.bonusData.payload.winAmount;
            text.text = socketmanager.bonusData.payload.winAmount.ToString("f2");
        }
        else
        {
            text.text = "Game Over";
        }
       
    }

    IEnumerator setCase()
    {
        _bonusManager.isOpening = true;
        audioController.PlaySpinBonusAudio("bonus");
        yield return new WaitUntil(() => !imageAnimation.isplaying);
        yield return new WaitForSeconds(0.7f);
        audioController.StopApinBonusAudio();
        text.gameObject.SetActive(true);
        isOpen = true;

        _bonusManager.totalWin += value;
        Debug.Log($"total Bonus Win :" + _bonusManager.totalWin);
        if (value == 0)
            _bonusManager.isFinisdhed = true;

        if (text.text == "Game Over")
        {
            yield return new WaitForSeconds(1f);
            _bonusManager.GameOver();
        }
        _bonusManager.isOpening = false;
        _bonusManager.BonusHidePanel.gameObject.SetActive(false);
    }

    private void StartFreeSpins(int spins)
    {
        if (Bonus_Object) Bonus_Object.SetActive(false);
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(false);
        slotManager.FreeSpin(spins);
    }

    internal void FreeSpinProcess(int spins)
    {
        FreeSpins = spins;
        if (FreeSpinPopup_Object) FreeSpinPopup_Object.SetActive(true);
        if (Free_Text) Free_Text.text = spins.ToString();
        if (Bonus_Object) Bonus_Object.SetActive(true);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Best.SocketIO;

public class BonusController : MonoBehaviour
{
    [SerializeField]
    private GameObject Bonus_Object;
    [SerializeField]
    private SlotBehaviour slotManager;
    [SerializeField] SocketIOManager socketmanager;
    [SerializeField]
    private List<ChestOpen> BonusCases;
    [SerializeField]
    private AudioController _audioManager;

    [SerializeField]
    private List<int> CaseValues;

    [SerializeField] private GameObject BonusWin;
    [SerializeField] private Transform BonusWinObject;
    [SerializeField] private TMP_Text win_text;


    int index = 0;
    internal bool isOpening;
    internal bool isFinisdhed;
    internal double bet { get; private set; }
    internal double totalWin;
    public Image BonusHidePanel;

    internal void ChestOpen() //List<int> values,double betAmount
    {
        index = 0;
        CaseValues.Clear();
        CaseValues.TrimExcess();
        // CaseValues = values;

        foreach (ChestOpen cases in BonusCases)
        {
            cases.ResetCase();
        }

        //for (int i = 0; i < CaseValues.Count; i++)
        //{
        //    if (CaseValues[i] == 0)
        //    {
        //        CaseValues.RemoveAt(i);
        //        CaseValues.Add(0);
        //    }
        //}
        //  bet = betAmount;
        StartBonus();
    }

    internal void GameOver()
    {
        win_text.text = totalWin.ToString("f2");

        BonusWinObject.localScale = Vector3.zero;
        BonusWin.SetActive(true);
        BonusWinObject.DOScale(Vector3.one, 0.3f);
        if (totalWin > 0)
            _audioManager.PlayWLAudio("bonuswin");
        else
        {
            _audioManager.PlayWLAudio("bonuslose");


        }
        socketmanager.resultData.payload.winAmount = socketmanager.bonusData.payload.winAmount;
        socketmanager.resultData.player.balance = socketmanager.bonusData.player.balance;
        DOVirtual.DelayedCall(2f, () =>
        {
            slotManager.CheckPopups = false;
            isFinisdhed = false;
            bet = 0;
            if (Bonus_Object) Bonus_Object.SetActive(false);
            BonusWin.SetActive(false);
            win_text.text = "";
            totalWin = 0;
        });

    }

       internal int GetValue()
    {
        int value = 0;

        value = CaseValues[index];

        index++;

        return value;
    }


    internal void PlayWinLooseSound(bool isWin)
    {
    }

    private void StartBonus()
    {
        if (Bonus_Object) Bonus_Object.SetActive(true);
    }
}

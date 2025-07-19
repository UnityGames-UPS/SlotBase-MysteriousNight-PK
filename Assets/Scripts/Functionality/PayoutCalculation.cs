using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class PayoutCalculation : MonoBehaviour
{
    [SerializeField]
    private int x_Distance;
    [SerializeField]
    private int y_Distance;

    [SerializeField]
    private Transform LineContainer;
    [SerializeField]
    private GameObject[] Lines_Object;
    internal int currrentLineIndex;
    internal int currentLineIndex;
    [SerializeField] internal List<int> LineList;
    internal List<int> DontDestroy = new List<int>();

    [SerializeField] private Button[] left_buttons;
    [SerializeField] private Button[] right_buttons;

    [SerializeField]
    private Vector2 InitialLinePosition = new Vector2(-315, 100);
    [SerializeField] internal List<int> DontDestroyLines = new List<int>();
    GameObject TempObj = null;

    [SerializeField] private SlotBehaviour slotmanager;

    internal void GeneratePayoutLinesBackend(int index = -1, bool DestroyFirst = true)
    {

        if (DestroyFirst)
            ResetStaticLine();

        if (index >= 0)
        {
            Lines_Object[index].SetActive(true);
            print("line object name" + Lines_Object[index].name);
            return;
        }
        DontDestroyLines.Clear();
        for (int i = 0; i < LineList[currentLineIndex]; i++)
        {
            Lines_Object[i].SetActive(true);


        }


    }



    internal void SetButtonActive(int LineCounter)
    {


        currrentLineIndex = LineCounter;

        for (int i = 0; i < LineCounter; i++)
        {
            Lines_Object[i].SetActive(true);
            left_buttons[i].interactable = true;
            right_buttons[i].interactable = true;
        }


        for (int j = LineCounter; j < left_buttons.Length; j++)
        {
            left_buttons[j].interactable = false;
            right_buttons[j].interactable = false;
        }
    }

    internal void ResetStaticLine()
    {
        if (TempObj != null)
        {
            TempObj.SetActive(false);
            TempObj = null;
        }
    }

    internal void ResetLines()
    {
        foreach (GameObject child in Lines_Object)
        {
            child.SetActive(false);
        }
    }

    internal void ResetStaticline()
    {
        for (int i = 0; i < Lines_Object.Length; i++)
        {
            Lines_Object[i].SetActive(slotmanager.dynamicLinesIndex.Contains(i));
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class ManageLineButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
	[SerializeField]
	private SlotBehaviour slotManager;
	[SerializeField]
	private PayoutCalculation payManager;
	[SerializeField]
	private GameObject _ConnectedLine;

	private bool isEnabled = false;
	[SerializeField]
	private int num;
	private Button btn;

	private void Start()
	{
		btn = this.GetComponent<Button>();

	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		// if (num-1 <= payManager.currrentLineIndex)
		// {
		// 	Debug.Log($" lineeeee 0 :"+ num + "   "+ payManager.currentLineIndex);
		// 	isEnabled = true;
		// }
		// else
		// {
		// 	Debug.Log($" lineeeee 1 :"+ num + "   "+ payManager.currentLineIndex);
		// 	isEnabled = false;
		// }
		// if (isEnabled)
			payManager.GeneratePayoutLinesBackend(num - 1);
	}
	public void OnPointerExit(PointerEventData eventData)
	{
		//if (isEnabled)
		//payManager.ResetLines();
		payManager.ResetStaticline();
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
		{
			this.gameObject.GetComponent<Button>().Select();
			Debug.Log("run on pointer down");
			payManager.GeneratePayoutLinesBackend(num - 1);
		}

	}
	public void OnPointerUp(PointerEventData eventData)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
		{
			Debug.Log("run on pointer up");
			//payManager.ResetLines();
			payManager.ResetStaticline();
			DOVirtual.DelayedCall(0.1f, () =>
			{
				this.gameObject.GetComponent<Button>().spriteState = default;
				EventSystem.current.SetSelectedGameObject(null);
			});
		}
	}


}

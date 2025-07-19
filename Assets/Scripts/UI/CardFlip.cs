using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardFlip : MonoBehaviour
{
    [SerializeField] internal Sprite cardImage;
    [SerializeField] internal Button Card_Button;

    [SerializeField] private GambleController gambleController;

    private RectTransform Card_transform;

    internal bool once = false;
    private Tween shakeTween;

    private void Start()
    {
        Card_transform = Card_Button.GetComponent<RectTransform>();
        if (Card_Button) Card_Button.onClick.RemoveAllListeners();
        if (Card_Button) Card_Button.onClick.AddListener(FlipMainCard);
    }

    internal void FlipMyObject()
    {
        if (!once && gambleController.gambleStart)
        {
            Card_transform.localEulerAngles = new Vector3(0, 180, 0);
            Card_transform.DORotate(new Vector3(0, 0, 0), 1, RotateMode.FastBeyond360);
            once = true;
            DOVirtual.DelayedCall(0.3f, changeSprite);
        }
    }

    private void FlipMainCard()
    {
        gambleController.SetCardButtonsInteractable(false);
        StartCoroutine(FlipMainCardRoutine());
    }

    private IEnumerator FlipMainObject()
    {
        // gambleController.RunOnCollect();
        // yield return new WaitUntil(() => gambleController.isResult);
        yield return null;
        cardImage = gambleController.GetCard();
        FlipMyObject();
    }
     private IEnumerator FlipMainCardRoutine()
    {
        // Start shaking
        StartShake();

        // Wait for the first coroutine to complete
        yield return StartCoroutine(gambleController.GambleCoroutine(true));

        // Stop shaking and reset position
        StopShake();

        // Proceed to the actual flip
        yield return StartCoroutine(FlipMainObject());
    }

    
    private void StartShake()
    {
        // Make sure no other shake is active
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();

        // Shake the card indefinitely
        shakeTween = Card_transform.DOShakeRotation(999f, new Vector3(0, 0, 15), 20, 90, true)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StopShake()
    {
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
            shakeTween = null;
        }

        // Reset rotation to original
        Card_transform.localRotation = Quaternion.identity;
    }


    private void changeSprite()
    {
        if (Card_Button)
        {
            Card_Button.image.sprite = cardImage;
            Card_Button.interactable = false;
            gambleController.FlipAllCard();
        }
    }

    internal void Reset()
    {
        Card_Button.interactable = true;
        once = false;
    }
}

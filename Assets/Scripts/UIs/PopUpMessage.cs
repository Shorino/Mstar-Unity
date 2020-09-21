using UnityEngine;
using DG.Tweening;

public class PopUpMessage : MonoBehaviour
{
    [Header("Initialization")]
    public CanvasGroup alphaBoard;

    RectTransform rectTrans;
    [HideInInspector] public CanvasGroup canvasGroup;
    Ease popUpAnimation = Ease.InOutQuint;
    float duration;

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        duration = 0.3f;

        canvasGroup.alpha = 0;
        rectTrans.localScale = Vector3.zero;
        if(alphaBoard != null)
            alphaBoard.alpha = 0;
    }

    private void Update()
    {
        
    }

    public void PopUp(bool yes)
    {
        if (yes)
        {
            if (alphaBoard != null)
            {
                alphaBoard.DOComplete();
                alphaBoard.gameObject.SetActive(true);
                alphaBoard.DOFade(1, duration).SetEase(popUpAnimation);
            }

            rectTrans.DOComplete();
            canvasGroup.DOComplete();
            gameObject.SetActive(true);
            rectTrans.DOScale(1, duration).SetEase(popUpAnimation);
            canvasGroup.DOFade(1, duration).SetEase(popUpAnimation);
        }
        else
        {
            if (alphaBoard != null)
            {
                alphaBoard.DOComplete();
                alphaBoard.DOFade(0, duration).SetEase(popUpAnimation).OnComplete(() =>
                {
                    alphaBoard.gameObject.SetActive(false);
                });
            }

            rectTrans.DOComplete();
            canvasGroup.DOComplete();
            rectTrans.DOScale(0, duration).SetEase(popUpAnimation);
            canvasGroup.DOFade(0, duration).SetEase(popUpAnimation).OnComplete(() => {
                gameObject.SetActive(false);
            });
        }
    }

    public bool AlphaBoardShowing()
    {
        return !(alphaBoard.alpha == 0);
    }
}

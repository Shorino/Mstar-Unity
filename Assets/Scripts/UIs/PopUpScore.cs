using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class PopUpScore : MonoBehaviour
{
    RectTransform rectTrans;
    TextMeshProUGUI textMeshPro;

    float duration = 0.3f;
    Vector3 initPos = new Vector3(-60, -50, 0);
    string text = "+100";

    public Ease animEase;
    public Ease fadeEase;

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        textMeshPro = GetComponent<TextMeshProUGUI>();

        PlayAnimation();
        textMeshPro.text = text;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.E))
    //    {
    //        PlayAnimation();
    //    }
    //}

    void PlayAnimation()
    {
        //rectTrans.localScale = new Vector3(0.5f,0.5f,0.5f);
        rectTrans.localPosition = initPos;
        textMeshPro.DOFade(1, 0);

        //rectTrans.DOScale(1, duration).SetEase(animEase);
        rectTrans.DOLocalMoveY(50, duration).SetEase(animEase);
        textMeshPro.DOFade(0, duration * 2).SetEase(fadeEase).OnComplete(() =>
        {
            Destroy(transform.parent.gameObject);
        });
    }

    public void SetText(string score)
    {
        text = score;
    }
}

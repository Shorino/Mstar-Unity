using UnityEngine;
using TMPro;
using DG.Tweening;

public class ComboIndicator : MonoBehaviour
{
    public static ComboIndicator singleton;

    TextMeshProUGUI comboText;
    RectTransform rectTrans;

    int previousCombo;
    public float duration;
    public Ease easeType;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;

        comboText = GetComponent<TextMeshProUGUI>();
        rectTrans = GetComponent<RectTransform>();

        previousCombo = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.singleton.combo == 0 || GameplaySettingsPanel.singleton.isStarted == false)
        {
            previousCombo = 0;
            comboText.enabled = false;
        }
        else
        {
            comboText.enabled = true;
            UpdateComboNumber(GameManager.singleton.combo);
        }
    }

    void UpdateComboNumber(int combo)
    {
        string comboString = "Combo x";
        if (GameManager.singleton.fullCombo)
        {
            comboString = "Full Combo x";
        }
        else
        {
            comboString = "Combo x";
        }
        comboText.text = comboString + combo.ToString();
        if (combo != previousCombo)
        {
            rectTrans.DOScale(1.2f, duration).SetEase(easeType).OnComplete(() =>
            {
                rectTrans.DOScale(1, duration).SetEase(easeType);
            });
        }
        previousCombo = combo;
    }
}

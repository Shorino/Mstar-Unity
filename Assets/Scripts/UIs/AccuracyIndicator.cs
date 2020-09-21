using UnityEngine;
using TMPro;
using DG.Tweening;

public class AccuracyIndicator : MonoBehaviour
{
    public static AccuracyIndicator singleton;
    [HideInInspector] public TextMeshProUGUI textMeshPro;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;

        textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.alpha = 0;
    }

    void Update()
    {
        if(textMeshPro.alpha > 0)
        {
            textMeshPro.alpha -= Time.deltaTime;
        }
    }

    public void printText(string message)
    {
        textMeshPro.alpha = 1;
        textMeshPro.text = message;
    }
}

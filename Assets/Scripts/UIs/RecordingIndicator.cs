using UnityEngine;
using UnityEngine.UI;

public class RecordingIndicator : MonoBehaviour
{
    public Image orginalRedCircle;
    CanvasGroup canvasGroup;
    Image redCircle;

    bool isEditing = false;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        redCircle = transform.GetChild(0).GetComponent<Image>();

        canvasGroup.alpha = 0;

        
    }

    // Update is called once per frame
    void Update()
    {
        redCircle.color = orginalRedCircle.color;

        if (EditingSettingsPanel.singleton != null)
        {
            isEditing = EditingSettingsPanel.singleton.isStarted;
        }

        if (GameManager.singleton.recording || isEditing)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }
}

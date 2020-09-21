using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BubbleOutline : MonoBehaviour
{
    [HideInInspector] public Image image;
    [HideInInspector] public RectTransform rectTrans;
    Bubble parentBubble;
    public static float popCircleRadius; // 100
    public static float originalOutline = 400;
    public static float smallestOutline = originalOutline;

    public bool selectedBubble; // is this bubble selected to pop?
    [HideInInspector] public Color red, blue;

    // for editor scene
    float sizeOutput;
    float sizeDiff;
    [HideInInspector] public float currentDurationAlive;
    public float initOutlineSize;

    // Start is called before the first frame update
    void Start()
    {
        #region Init Objects
        image = GetComponent<Image>();
        rectTrans = image.rectTransform;
        parentBubble = transform.parent.GetComponent<Bubble>();
        #endregion

        if (parentBubble.bubbleMode == Scene.gameplay || parentBubble.bubbleMode == Scene.editor)
        {
            #region Init Values
            popCircleRadius = Bubble.circleRadius - 55;
            red = new Color(1f, 0.7f, 0.7f);
            blue = new Color(0.5f, 0.78f, 1f);
            DecideOutlineColor();

            if (parentBubble.bubbleMode == Scene.gameplay)
            {
                rectTrans.DOSizeDelta(new Vector2(popCircleRadius - 10, popCircleRadius - 10), GameManager.singleton.bubbleStayDuration).SetEase(Ease.Linear).SetId("BubbleOutline");
            }
            else if (parentBubble.bubbleMode == Scene.editor)
            {
                sizeOutput = 400;
                sizeDiff = 400 - popCircleRadius + 20; // 250
                currentDurationAlive = 0;
                UpdateDeltaSize();
                initOutlineSize = rectTrans.sizeDelta.x;
            }
            #endregion
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (parentBubble.bubbleMode == Scene.gameplay)
        {
            if (rectTrans.sizeDelta.x < smallestOutline)
            {
                smallestOutline = rectTrans.sizeDelta.x;
            }

            if (smallestOutline == rectTrans.sizeDelta.x)
            {
                selectedBubble = true;
            }
            else
            {
                selectedBubble = false;
            }
        }
        else if(parentBubble.bubbleMode == Scene.editor)
        {
            UpdateDeltaSize();
        }
    }

    void UpdateDeltaSize()
    {
        currentDurationAlive = AudioManager.singleton.GetSeek() -
                (GameManager.singleton.spawnNote[(int)parentBubble.index].time - GameManager.singleton.outlineShrinkDuration);
        if (sizeOutput > 130)
        {
            sizeOutput = 400 - (sizeDiff * currentDurationAlive / GameManager.singleton.outlineShrinkDuration);
        }
        rectTrans.sizeDelta = new Vector2(sizeOutput, sizeOutput);
    }

    void OnDestroy()
    {
        if (parentBubble.bubbleMode == Scene.gameplay)
            smallestOutline = originalOutline;
    }

    void DecideOutlineColor()
    {
        switch (parentBubble.direction)
        {
            case BubbleDirection.up:
                image.color = red;
                parentBubble.colorParticleMiss = red;
                break;
            case BubbleDirection.down:
                image.color = red;
                parentBubble.colorParticleMiss = red;
                break;
            case BubbleDirection.left:
                image.color = blue;
                parentBubble.colorParticleMiss = blue;
                break;
            case BubbleDirection.right:
                image.color = blue;
                parentBubble.colorParticleMiss = blue;
                break;
        }
    }
}

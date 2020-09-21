using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public enum BubbleDirection
{// 1    2     3      4
    up, down, left, right, random
};

public enum Scene
{
    gameplay, recorder, editor
};

public class Bubble : MonoBehaviour
{
    #region init
    [Header("Initialization")]
    public Scene bubbleMode = Scene.gameplay;
    #endregion

    #region gameplay
    [Header("For Gameplay Scene")]
    public GameObject prefabParticlePop;
    public GameObject prefabParticleMiss;
    Image image, arrowImage;
    BubbleOutline bubbleOutline;
    public static float circleRadius;
    [HideInInspector] public Color colorParticlePop, colorParticleMiss;
    [HideInInspector] public ChainCondition chainCondition;
    //[HideInInspector] public GameObject chain;
    #endregion

    #region recorder
    [Header("For Recorder Scene")]
    public BubbleDirection direction;
    public Ease expandAnimation;
    public Ease fadeAnimation;
    CanvasGroup canvasGroup;
    #endregion

    #region editor
    [Header("For Editor Scene")]
    public float index;
    [HideInInspector] public bool canDrag;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (bubbleMode == Scene.gameplay)
        {
            #region Init Objects
            bubbleOutline = transform.GetChild(0).GetComponent<BubbleOutline>();
            image = transform.GetChild(1).GetComponent<Image>();
            arrowImage = transform.GetChild(2).GetComponent<Image>();
            #endregion

            #region Init Values
            circleRadius = image.rectTransform.sizeDelta.x;
            #endregion

            RotateArrowImage();
            DecideBubbleColor();
        }
        else if (bubbleMode == Scene.recorder)
        {
            #region Init Objects
            canvasGroup = GetComponent<CanvasGroup>();
            image = transform.GetChild(0).GetComponent<Image>();
            arrowImage = transform.GetChild(1).GetComponent<Image>();
            #endregion

            #region Init Values
            canvasGroup.alpha = 0.5f;
            #endregion

            StartCoroutine(InitRecorderBubble());
        }
        else if (bubbleMode == Scene.editor)
        {
            #region Init Objects
            bubbleOutline = transform.GetChild(0).GetComponent<BubbleOutline>();
            image = transform.GetChild(2).GetComponent<Image>();
            arrowImage = transform.GetChild(3).GetComponent<Image>();
            canvasGroup = GetComponent<CanvasGroup>();
            #endregion

            #region Init Values
            circleRadius = image.rectTransform.sizeDelta.x;
            if (index != EditingPanel.singleton.currentlySelectedBubbleIndex)
            {
                canDrag = false;
            }
            else
            {
                canDrag = true;
            }
            #endregion

            RotateArrowImage();
            DecideBubbleColor();
        }
    }

    IEnumerator InitRecorderBubble()
    {
        yield return GameManager.singleton == null;
        RotateArrowImage();
        DecideBubbleColor();
    }

    // Update is called once per frame
    void Update()
    {
        if (bubbleMode == Scene.gameplay)
        {
            UpdateInput();
            KillBubble();
        }
        else if (bubbleMode == Scene.recorder)
        {
            RecorderVisualFeedback();
        }
        else if (bubbleMode == Scene.editor)
        {
            KillBubbleEditor();
            #region Select/Deselect self
            if (index != EditingPanel.singleton.currentlySelectedBubbleIndex)
            { // deselect
                canvasGroup.enabled = true;
                canDrag = false;
            }
            else
            { // select
                UpdateVisual();
            }
            #endregion
        }
    }

    void OnDestroy()
    {
        if (bubbleMode == Scene.gameplay)
        {
            GameManager.singleton.ResetInput();
        }
    }

    void OnMouseDown()
    {
        if (bubbleMode == Scene.editor && EditingSettingsPanel.singleton.cantChainMessage.canvasGroup.alpha != 1)
        {
            SelectSelf(true);
        }
    }

    void OnMouseUp()
    {
        if (bubbleMode == Scene.editor && EditingSettingsPanel.singleton.cantChainMessage.canvasGroup.alpha != 1)
        {
            canDrag = true;
        }
    }

    void OnMouseDrag()
    {
        if (bubbleMode == Scene.editor && EditingSettingsPanel.singleton.cantChainMessage.canvasGroup.alpha != 1)
        {
            if (canDrag)
            {
                #region update visual position
                Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                position.z = 0;
                gameObject.GetComponent<RectTransform>().position = position;
                #endregion

                #region update position in spawnNote
                GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)index];
                if (index % 1 == 0)
                { // front
                    GameManager.singleton.tempNote.position = position;
                }
                else
                { // back
                    GameManager.singleton.tempNote.position2 = position;
                }
                GameManager.singleton.spawnNote[(int)index] = GameManager.singleton.tempNote;
                #endregion
            }
        }
    }

    #region Gameplay
    void UpdateInput()
    {
        bool trigger = Pressed();
        if (GameManager.singleton.autoPlay)
        {
            trigger = bubbleOutline.rectTrans.sizeDelta.x <= circleRadius;
        }
        if (AudioManager.singleton.IsPlaying() == false)
        {
            trigger = false;
        }
        if (trigger && bubbleOutline.selectedBubble)
        {
            AudioManager.singleton.PlayPop();
            float currentOutlineRadius = bubbleOutline.rectTrans.sizeDelta.x;
            if(currentOutlineRadius < circleRadius - 35)
            { // below 115 radius
                AccuracyIndicator.singleton.printText("GOOD");
                GameObject popUpScore = Instantiate(GameManager.singleton.prefabPopUpScore, transform.position, Quaternion.identity, GameplayCanvas.canvas.transform);
                GameManager.singleton.combo++;
                long score = 4 * GameManager.singleton.combo;
                popUpScore.transform.GetChild(0).GetComponent<PopUpScore>().SetText("+" + score.ToString());
                ScoreText.score += score;
            }
            else if (currentOutlineRadius >= circleRadius - 35 && currentOutlineRadius < circleRadius - 15)
            { // within 115 -  135 radius
                AccuracyIndicator.singleton.printText("GREAT");
                GameObject popUpScore = Instantiate(GameManager.singleton.prefabPopUpScore, transform.position, Quaternion.identity, GameplayCanvas.canvas.transform);
                GameManager.singleton.combo++;
                long score = 8 * GameManager.singleton.combo;
                popUpScore.transform.GetChild(0).GetComponent<PopUpScore>().SetText("+" + score.ToString());
                ScoreText.score += score;
            }
            else if (currentOutlineRadius >= circleRadius - 15 && currentOutlineRadius < circleRadius + 15)
            { // within 135 - 165 radius
                AccuracyIndicator.singleton.printText("PERFECT");
                GameObject popUpScore = Instantiate(GameManager.singleton.prefabPopUpScore, transform.position, Quaternion.identity, GameplayCanvas.canvas.transform);
                GameManager.singleton.combo++;
                long score = 10 * GameManager.singleton.combo;
                popUpScore.transform.GetChild(0).GetComponent<PopUpScore>().SetText("+" + score.ToString());
                ScoreText.score += score;
            }
            else if (currentOutlineRadius >= circleRadius + 15 && currentOutlineRadius < circleRadius + 35)
            { // within 165 - 185 radius
                AccuracyIndicator.singleton.printText("GREAT");
                GameObject popUpScore = Instantiate(GameManager.singleton.prefabPopUpScore, transform.position, Quaternion.identity, GameplayCanvas.canvas.transform);
                GameManager.singleton.combo++;
                long score = 8 * GameManager.singleton.combo;
                popUpScore.transform.GetChild(0).GetComponent<PopUpScore>().SetText("+" + score.ToString());
                ScoreText.score += score;
            }
            else if (currentOutlineRadius >= circleRadius + 35 && currentOutlineRadius < circleRadius + 55)
            { // within 185 - 205 radius
                AccuracyIndicator.singleton.printText("GOOD");
                GameObject popUpScore = Instantiate(GameManager.singleton.prefabPopUpScore, transform.position, Quaternion.identity, GameplayCanvas.canvas.transform);
                GameManager.singleton.combo++;
                long score = 4 * GameManager.singleton.combo;
                popUpScore.transform.GetChild(0).GetComponent<PopUpScore>().SetText("+" + score.ToString());
                ScoreText.score += score;
            }
            else if (currentOutlineRadius >= circleRadius + 55)
            { // within 205 and above
                AccuracyIndicator.singleton.printText("TOO EARLY!");
                GameObject popUpScore = Instantiate(GameManager.singleton.prefabPopUpScore, transform.position, Quaternion.identity, GameplayCanvas.canvas.transform);
                GameManager.singleton.combo = 0;
                GameManager.singleton.fullCombo = false;
                popUpScore.transform.GetChild(0).GetComponent<PopUpScore>().SetText("+1");
                ScoreText.score += 2;
            }
            RemoveSelfFromSpawnedNoteOnScene();
            SpawnPopParticle();
            SpawnMissParticle();
            Destroy(gameObject);
        }
    }

    bool Pressed()
    {
        if (chainCondition == ChainCondition.singleNote)
        {
            if ((direction == BubbleDirection.up &&
                GameManager.singleton.IsPressingUp() &&
                !GameManager.singleton.IsPressingDown() &&
                !GameManager.singleton.IsPressingLeft() &&
                !GameManager.singleton.IsPressingRight()) ||
               (direction == BubbleDirection.down &&
               GameManager.singleton.IsPressingDown() &&
               !GameManager.singleton.IsPressingUp() &&
               !GameManager.singleton.IsPressingLeft() &&
               !GameManager.singleton.IsPressingRight()) ||
               (direction == BubbleDirection.left &&
               GameManager.singleton.IsPressingLeft() &&
               !GameManager.singleton.IsPressingDown() &&
               !GameManager.singleton.IsPressingUp() &&
               !GameManager.singleton.IsPressingRight()) ||
               (direction == BubbleDirection.right &&
               GameManager.singleton.IsPressingRight() &&
               !GameManager.singleton.IsPressingDown() &&
               !GameManager.singleton.IsPressingLeft() &&
               !GameManager.singleton.IsPressingUp()))
            {
                return true;
            }
        }
        else
        {
            switch (chainCondition)
            {
                case ChainCondition.upperLeft:
                    if (GameManager.singleton.IsPressingUpLeft())
                        return true;
                    break;
                case ChainCondition.upperRight:
                    if (GameManager.singleton.IsPressingUpRight())
                        return true;
                    break;
                case ChainCondition.bottomLeft:
                    if (GameManager.singleton.IsPressingDownLeft())
                        return true;
                    break;
                case ChainCondition.bottomRight:
                    if (GameManager.singleton.IsPressingDownRight())
                        return true;
                    break;
                case ChainCondition.upDown:
                    if (GameManager.singleton.IsPressingUpDown())
                        return true;
                    break;
                case ChainCondition.leftRight:
                    if (GameManager.singleton.IsPressingLeftRight())
                        return true;
                    break;
            }
        }
        return false;
    }

    void KillBubble()
    {
        if (bubbleOutline.rectTrans.sizeDelta.x <= BubbleOutline.popCircleRadius)
        {
            AudioManager.singleton.PlayMiss();
            AccuracyIndicator.singleton.printText("MISS!");
            GameObject popUpScore = Instantiate(GameManager.singleton.prefabPopUpScore, transform.position, Quaternion.identity, GameplayCanvas.canvas.transform);
            popUpScore.transform.GetChild(0).GetComponent<PopUpScore>().SetText("+0");
            GameManager.singleton.combo = 0;
            GameManager.singleton.fullCombo = false;

            SpawnMissParticle();
            RemoveSelfFromSpawnedNoteOnScene();
            Destroy(gameObject);
        }
    }

    void RemoveSelfFromSpawnedNoteOnScene()
    {
        foreach (GameObject spawnedNotes in GameManager.singleton.spawnedNoteOnScene)
        {
            if (spawnedNotes != null)
            {
                if (spawnedNotes.GetComponent<Bubble>().index == index)
                {
                    GameManager.singleton.spawnedNoteOnScene.Remove(spawnedNotes);
                    break;
                }
            }
        }
    }

    public void DecideDirection(BubbleDirection dir)
    {
        if (dir == BubbleDirection.random)
        {
            switch ((int)Random.Range(0, 4))
            {
                case 0:
                    direction = BubbleDirection.up;
                    break;
                case 1:
                    direction = BubbleDirection.down;
                    break;
                case 2:
                    direction = BubbleDirection.left;
                    break;
                case 3:
                    direction = BubbleDirection.right;
                    break;
            }
        }
        else
        {
            direction = dir;
        }
    }

    void RotateArrowImage()
    {
        switch (direction)
        {
            case BubbleDirection.up:
                arrowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case BubbleDirection.down:
                arrowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case BubbleDirection.left:
                arrowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
            case BubbleDirection.right:
                arrowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 270);
                break;
        }
    }

    void DecideBubbleColor()
    {
        switch (direction)
        {
            case BubbleDirection.up:
                image.color = GameManager.singleton.red;
                colorParticlePop = GameManager.singleton.red;
                break;
            case BubbleDirection.down:
                image.color = GameManager.singleton.red;
                colorParticlePop = GameManager.singleton.red;
                break;
            case BubbleDirection.left:
                image.color = GameManager.singleton.blue;
                colorParticlePop = GameManager.singleton.blue;
                break;
            case BubbleDirection.right:
                image.color = GameManager.singleton.blue;
                colorParticlePop = GameManager.singleton.blue;
                break;
        }
    }

    void SpawnPopParticle()
    {
        GameObject particle = Instantiate(prefabParticlePop,
            new Vector3(image.rectTransform.position.x, image.rectTransform.position.y, 0),
            Quaternion.identity);
        particle.GetComponent<ParticleSystem>().startColor = colorParticlePop;
    }

    void SpawnMissParticle()
    {
        GameObject particle = Instantiate(prefabParticleMiss,
            new Vector3(image.rectTransform.position.x, image.rectTransform.position.y, 0),
            Quaternion.identity);
        particle.GetComponent<ParticleSystem>().startColor = colorParticleMiss;
    }
    #endregion

    #region Recorder
    void RecorderVisualFeedback()
    {
        if (GameManager.singleton.recording)
        {
            if (direction == BubbleDirection.up && GameManager.singleton.IsPressingUp() ||
                direction == BubbleDirection.down && GameManager.singleton.IsPressingDown() ||
                direction == BubbleDirection.left && GameManager.singleton.IsPressingLeft() ||
                direction == BubbleDirection.right && GameManager.singleton.IsPressingRight())
            {
                SpawnPopParticle();
                ExpandAlpha(0.5f);
            }
        }
    }

    void ExpandAlpha(float duration)
    {
        Vector2 big = new Vector2(1.2f, 1.2f);
        image.rectTransform.DOScale(big, 0);
        image.rectTransform.DOScale(Vector2.one, duration).SetEase(expandAnimation);

        canvasGroup.DOComplete();
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0.5f, duration).SetEase(fadeAnimation);
    }
    #endregion

    #region Editor
    void KillBubbleEditor()
    {
        if (GameManager.singleton.spawnNote[(int)index].absoluteKill)
        {
            DestroySelf(true, true);
        }
    }

    public void DestroySelf(bool auto, bool deselectSelf)
    {
        try
        {
            if (auto)
            {
                AudioManager.singleton.PlayPop();
                SpawnPopParticle();
                RemoveSelfFromSpawnedNoteOnScene();
            }
            if (deselectSelf)
            {
                SelectSelf(false);
            }
            Destroy(gameObject);
        }
        catch
        {

        }
    }

    public void SelectSelf(bool yes)
    {
        if (yes)
        {
            EditingPanel.singleton.Pause(true);
            EditingPanel.singleton.currentlySelectedBubbleIndex = index;
            EditingPanel.singleton.selectedTools.SetActive(true);
            EditingPanel.singleton.playButton.interactable = false;
            EditingPanel.singleton.addButton.interactable = false;
            UpdateVisual();
        }
        else
        {
            if (EditingPanel.singleton != null)
            {
                EditingPanel.singleton.currentlySelectedBubbleIndex = -1;
                EditingPanel.singleton.selectedTools.SetActive(false);
                EditingPanel.singleton.playButton.interactable = true;
                EditingPanel.singleton.addButton.interactable = true;
            }
        }
    }

    public void UpdateVisual()
    {
        canvasGroup.enabled = false;
        SelectedTools.singleton.ShowPopTime(index);
        SelectedTools.singleton.ShowPosX(index);
        SelectedTools.singleton.ShowPosY(index);
    }
    #endregion
}

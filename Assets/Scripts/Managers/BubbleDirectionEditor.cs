using UnityEngine;
using UnityEngine.UI;

public class BubbleDirectionEditor : MonoBehaviour
{
    public static BubbleDirectionEditor singleton;

    public BubbleDirection selectedDirection;
    Image up, down, left, right;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;
        up = transform.GetChild(0).GetComponent<Image>();
        down = transform.GetChild(1).GetComponent<Image>();
        left = transform.GetChild(2).GetComponent<Image>();
        right = transform.GetChild(3).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject spawnedNoteOnScene = GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex);
        if (spawnedNoteOnScene != null)
        {
            selectedDirection = spawnedNoteOnScene.GetComponent<Bubble>().direction;

        }
        #region update visual UI
        switch (selectedDirection)
        {
            case BubbleDirection.up:
                up.enabled = false;
                down.enabled = true;
                left.enabled = true;
                right.enabled = true;
                break;
            case BubbleDirection.down:
                up.enabled = true;
                down.enabled = false;
                left.enabled = true;
                right.enabled = true;
                break;
            case BubbleDirection.left:
                up.enabled = true;
                down.enabled = true;
                left.enabled = false;
                right.enabled = true;
                break;
            case BubbleDirection.right:
                up.enabled = true;
                down.enabled = true;
                left.enabled = true;
                right.enabled = false;
                break;
        }
        #endregion
    }

    public void OnMouseClick_UpArrow()
    {
        UpdateDirection(BubbleDirection.up);
    }

    public void OnMouseClick_DownArrow()
    {
        UpdateDirection(BubbleDirection.down);
    }

    public void OnMouseClick_LeftArrow()
    {
        UpdateDirection(BubbleDirection.left);
    }

    public void OnMouseClick_RightArrow()
    {
        UpdateDirection(BubbleDirection.right);
    }

    void UpdateDirection(BubbleDirection direction)
    {
        int directionInNo = 0;
        switch (direction)
        {
            case BubbleDirection.up:
                directionInNo = 1;
                break;
            case BubbleDirection.down:
                directionInNo = 2;
                break;
            case BubbleDirection.left:
                directionInNo = 3;
                break;
            case BubbleDirection.right:
                directionInNo = 4;
                break;
        }
        GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
        if(EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
        { // front
            GameManager.singleton.tempNote.direction = directionInNo;
        }
        else
        {
            GameManager.singleton.tempNote.direction2 = directionInNo;
        }
        int chainCondition = SelectedTools.singleton.DecideChainCondition(GameManager.singleton.tempNote.direction, GameManager.singleton.tempNote.direction2);
        if(chainCondition != -1)
        {
            GameManager.singleton.tempNote.chainCondition = chainCondition;
            GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
            GameManager.singleton.KillAllBubbleOnScene(false);
        }
    }
}

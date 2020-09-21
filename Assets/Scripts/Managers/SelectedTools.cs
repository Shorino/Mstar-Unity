using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedTools : MonoBehaviour
{
    public static SelectedTools singleton;

    Button removeButton;
    Button confirmButton;
    TMP_InputField popTime, posX, posY;
    BubbleDirectionEditor bubbleDirection;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;

        removeButton = transform.GetChild(0).GetComponent<Button>();
        popTime = transform.GetChild(1).GetChild(0).GetComponent<TMP_InputField>();
        posX = transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>();
        posY = transform.GetChild(3).GetChild(0).GetComponent<TMP_InputField>();
        bubbleDirection = transform.GetChild(4).GetComponent<BubbleDirectionEditor>();
        confirmButton = transform.GetChild(5).GetComponent<Button>();

        bubbleDirection.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        #region Keyboard Shortcut
        if (Input.GetKeyDown(KeyCode.Z))
        {
            RemoveNote();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            DeselectCurrentNote();
        }
        MoveSelectedBubbleUsingHotkey(0.05f);
        #endregion
    }

    #region Remove
    public void RemoveNote()
    {
        if (gameObject.activeInHierarchy && removeButton.interactable)
        {
            foreach (GameObject spawnedBubbleOnScene in GameManager.singleton.spawnedNoteOnScene)
            {
                if (spawnedBubbleOnScene != null)
                {
                    if (spawnedBubbleOnScene.GetComponent<Bubble>().index == EditingPanel.singleton.currentlySelectedBubbleIndex)
                    {
                        spawnedBubbleOnScene.GetComponent<Bubble>().DestroySelf(false, false);
                        #region Remove self from spawnNote
                        if (GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].chainCondition == 1)
                        { // it is single note
                            GameManager.singleton.spawnNote.RemoveAt((int)EditingPanel.singleton.currentlySelectedBubbleIndex);
                        }
                        else
                        { // it is chained note
                            GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
                            GameManager.singleton.tempNote.chainCondition = 1;
                            if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
                            { // index is an integer
                                GameManager.singleton.tempNote.direction = GameManager.singleton.tempNote.direction2;
                                GameManager.singleton.tempNote.position = GameManager.singleton.tempNote.position2;
                                GameManager.singleton.tempNote.direction2 = 0;
                                GameManager.singleton.tempNote.position2 = Vector2.zero;
                            }
                            else
                            { // index is a float
                                GameManager.singleton.tempNote.direction2 = 0;
                                GameManager.singleton.tempNote.position2 = Vector2.zero;
                            }
                            GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
                        }
                        #endregion
                        DeselectCurrentNote();
                        break;
                    }
                }
            }
            GameManager.singleton.KillAllBubbleOnScene(true);
        }
    }
    #endregion

    #region Deselect
    public void DeselectCurrentNote()
    {
        if (gameObject.activeInHierarchy)
        {
            EditingPanel.singleton.currentlySelectedBubbleIndex = -1;
            EditingPanel.singleton.selectedTools.SetActive(false);
            EditingPanel.singleton.playButton.interactable = true;
            EditingPanel.singleton.addButton.interactable = true;
        }
    }
    #endregion

    #region Pop time
    public void ShowPopTime(float index)
    {
        if (gameObject.activeInHierarchy)
        {
            if(popTime.isFocused == false)
                popTime.text = GameManager.singleton.spawnNote[(int)index].time.ToString("F4");
        }
    }

    public void AdjustPopTime()
    {
        ModifySpawnNotePopTime();
        GameManager.singleton.KillAllBubbleOnScene(true);
    }

    void ModifySpawnNotePopTime()
    {
        try
        {
            float time = float.Parse(popTime.text);

            if (time == GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].time)
                return;

            #region single/chained note to chained note
            for (int i = 0; i < GameManager.singleton.spawnNote.Count; i++)
            {
                if (i != (int)EditingPanel.singleton.currentlySelectedBubbleIndex)
                { // current iteration is not the selected bubble
                    if (GameManager.singleton.spawnNote[i].time == time)
                    { // same time with exisitng notes
                        GameManager.singleton.tempNote = GameManager.singleton.spawnNote[i];
                        if (GameManager.singleton.tempNote.chainCondition == 1)
                        { // original is single note
                          // de-attach the replacer
                            if (GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].chainCondition == 1)
                            {  // replacer is single note
                                GameManager.singleton.tempNote.direction2 = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].direction;
                                GameManager.singleton.tempNote.position2 = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].position;
                                int chainCondition = DecideChainCondition(GameManager.singleton.tempNote.direction, GameManager.singleton.tempNote.direction2);
                                if(chainCondition != -1)
                                {
                                    GameManager.singleton.tempNote.chainCondition = chainCondition;
                                    GameManager.singleton.spawnNote[i] = GameManager.singleton.tempNote;
                                    GameManager.singleton.spawnNote.RemoveAt((int)EditingPanel.singleton.currentlySelectedBubbleIndex);
                                    EditingPanel.singleton.currentlySelectedBubbleIndex = GameManager.singleton.spawnNote.IndexOf(GameManager.singleton.tempNote) + 0.5f;
                                }
                                else
                                {
                                    // cant be chained
                                    EditingSettingsPanel.singleton.cantChainMessage.PopUp(true);
                                }
                                return;
                            }
                            else
                            { // replacer is a chained note
                                if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
                                { // de-attached from front
                                    GameManager.singleton.tempNote.direction2 = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].direction;
                                    GameManager.singleton.tempNote.position2 = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].position;
                                    int chainCondition = DecideChainCondition(GameManager.singleton.tempNote.direction, GameManager.singleton.tempNote.direction2);
                                    if(chainCondition != -1)
                                    {
                                        GameManager.singleton.tempNote.chainCondition = chainCondition;
                                        GameManager.singleton.spawnNote[i] = GameManager.singleton.tempNote;
                                        GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
                                        GameManager.singleton.tempNote.direction = GameManager.singleton.tempNote.direction2;
                                        GameManager.singleton.tempNote.position = GameManager.singleton.tempNote.position2;
                                        GameManager.singleton.tempNote.direction2 = 0;
                                        GameManager.singleton.tempNote.position2 = Vector2.zero;
                                        GameManager.singleton.tempNote.chainCondition = 1;
                                        GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
                                        EditingPanel.singleton.currentlySelectedBubbleIndex = i + 0.5f;
                                    }
                                    else
                                    {
                                        // cant be chained
                                        EditingSettingsPanel.singleton.cantChainMessage.PopUp(true);
                                    }
                                    return;
                                }
                                else
                                { // de-attached from back
                                    GameManager.singleton.tempNote.direction2 = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].direction2;
                                    GameManager.singleton.tempNote.position2 = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].position2;
                                    int chainCondition = DecideChainCondition(GameManager.singleton.tempNote.direction, GameManager.singleton.tempNote.direction2);
                                    if(chainCondition != -1)
                                    {
                                        GameManager.singleton.tempNote.chainCondition = chainCondition;
                                        GameManager.singleton.spawnNote[i] = GameManager.singleton.tempNote;
                                        GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
                                        GameManager.singleton.tempNote.direction2 = 0;
                                        GameManager.singleton.tempNote.position2 = Vector2.zero;
                                        GameManager.singleton.tempNote.chainCondition = 1;
                                        GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
                                        EditingPanel.singleton.currentlySelectedBubbleIndex = i + 0.5f;
                                    }
                                    else
                                    {
                                        // cant be chained
                                        EditingSettingsPanel.singleton.cantChainMessage.PopUp(true);
                                    }
                                    return;
                                }
                            }
                        }
                        else
                        {
                            // cant be chained
                            EditingSettingsPanel.singleton.cantChainMessage.PopUp(true);
                        }
                        return;
                    }
                }
            }
            #endregion

            #region single/chained note to single note
            // after finished the loop, can't find same time with existing notes
            GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];

            // de-attach the replacer
            if (GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex].chainCondition == 1)
            {  // replacer is single note
                GameManager.singleton.tempNote.time = time;
                GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
                return;
            }
            else
            { // replacer is a chained note
                if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
                { // de-attached from front
                    int direction = GameManager.singleton.tempNote.direction;
                    Vector2 position = GameManager.singleton.tempNote.position;
                    GameManager.singleton.tempNote.direction = GameManager.singleton.tempNote.direction2;
                    GameManager.singleton.tempNote.position = GameManager.singleton.tempNote.position2;
                    GameManager.singleton.tempNote.direction2 = 0;
                    GameManager.singleton.tempNote.position2 = Vector2.zero;
                    GameManager.singleton.tempNote.chainCondition = 1;
                    GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
                    GameManager.singleton.tempNote.direction = direction;
                    GameManager.singleton.tempNote.position = position;
                    GameManager.singleton.tempNote.time = time;
                    GameManager.singleton.spawnNote.Add(GameManager.singleton.tempNote);
                    EditingPanel.singleton.currentlySelectedBubbleIndex = GameManager.singleton.spawnNote.Count - 1;
                    return;
                }
                else
                { // de-attached from back
                    int direction2 = GameManager.singleton.tempNote.direction2;
                    Vector2 position2 = GameManager.singleton.tempNote.position2;
                    GameManager.singleton.tempNote.direction2 = 0;
                    GameManager.singleton.tempNote.position2 = Vector2.zero;
                    GameManager.singleton.tempNote.chainCondition = 1;
                    GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
                    GameManager.singleton.tempNote.direction = direction2;
                    GameManager.singleton.tempNote.position = position2;
                    GameManager.singleton.tempNote.time = time;
                    GameManager.singleton.spawnNote.Add(GameManager.singleton.tempNote);
                    EditingPanel.singleton.currentlySelectedBubbleIndex = GameManager.singleton.spawnNote.Count - 1;
                    return;
                }
            }
            #endregion
        }
        catch
        {

        }
    }

    public int DecideChainCondition(int direction, int direction2)
    {
        if (direction2 == 0) // back is blank, it is single note
            return 1;
        switch (direction)
        {
            case 1: // up
                switch (direction2)
                {
                    case 1: // up
                        return -1;
                    case 2: // down
                        return 7;
                    case 3: // left
                        return 2;
                    case 4: // right
                        return 3;
                }
                break;
            case 2: // down
                switch (direction2)
                {
                    case 1: // up
                        return 7;
                    case 2: // down
                        return -1;
                    case 3: // left
                        return 4;
                    case 4: // right
                        return 5;
                }
                break;
            case 3: // left
                switch (direction2)
                {
                    case 1: // up
                        return 2;
                    case 2: // down
                        return 4;
                    case 3: // left
                        return -1;
                    case 4: // right
                        return 6;
                }
                break;
            case 4: // right
                switch (direction2)
                {
                    case 1: // up
                        return 3;
                    case 2: // down
                        return 5;
                    case 3: // left
                        return 6;
                    case 4: // right
                        return -1;
                }
                break;
        }
        return -1;
    }
    #endregion

    #region Position
    public void ShowPosX(float index)
    {
        if (gameObject.activeInHierarchy)
        {
            if(posX.isFocused == false)
            {
                if(index % 1 == 0)
                {
                    posX.text = GameManager.singleton.spawnNote[(int)index].position.x.ToString("F4");
                }
                else
                {
                    posX.text = GameManager.singleton.spawnNote[(int)index].position2.x.ToString("F4");
                }
            }
        }
    }

    public void ShowPosY(float index)
    {
        if (gameObject.activeInHierarchy)
        {
            if (posY.isFocused == false)
            {
                if (index % 1 == 0)
                {
                    posY.text = GameManager.singleton.spawnNote[(int)index].position.y.ToString("F4");
                }
                else
                {
                    posY.text = GameManager.singleton.spawnNote[(int)index].position2.y.ToString("F4");
                }
            }
        }
    }

    public void AdjustPosX()
    {
        float value = 0;
        try
        {
            value = float.Parse(posX.text);
        }
        catch
        {
            return;
        }
        GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
        if(EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
        { // front
            GameManager.singleton.tempNote.position.x = value;
            GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position;

        }
        else
        { // back
            GameManager.singleton.tempNote.position2.x = value;
            GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position2;
        }
        GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
    }

    public void AdjustPosY()
    {
        float value = 0;
        try
        {
            value = float.Parse(posY.text);
        }
        catch
        {
            return;
        }
        GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
        if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
        { // front
            GameManager.singleton.tempNote.position.y = value;
            GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position;
        }
        else
        { // back
            GameManager.singleton.tempNote.position2.y = value;
            GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position2;
        }
        GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
    }

    void MoveSelectedBubbleUsingHotkey(float moveDistance)
    {
        if (gameObject.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
                if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
                { // front
                    GameManager.singleton.tempNote.position.y += moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position;
                }
                else
                { // back
                    GameManager.singleton.tempNote.position2.y += moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position2;
                }
                GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
                if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
                { // front
                    GameManager.singleton.tempNote.position.y -= moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position;
                }
                else
                { // back
                    GameManager.singleton.tempNote.position2.y -= moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position2;
                }
                GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
                if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
                { // front
                    GameManager.singleton.tempNote.position.x -= moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position;
                }
                else
                { // back
                    GameManager.singleton.tempNote.position2.x -= moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position2;
                }
                GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                GameManager.singleton.tempNote = GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex];
                if (EditingPanel.singleton.currentlySelectedBubbleIndex % 1 == 0)
                { // front
                    GameManager.singleton.tempNote.position.x += moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position;
                }
                else
                { // back
                    GameManager.singleton.tempNote.position2.x += moveDistance;
                    GameManager.singleton.GetSpawnedNoteOnScene(EditingPanel.singleton.currentlySelectedBubbleIndex).GetComponent<RectTransform>().position = GameManager.singleton.tempNote.position2;
                }
                GameManager.singleton.spawnNote[(int)EditingPanel.singleton.currentlySelectedBubbleIndex] = GameManager.singleton.tempNote;
            }
        }
    }
    #endregion
}

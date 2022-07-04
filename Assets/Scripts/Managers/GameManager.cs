using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.IO;

public enum ChainCondition
{//             1           2          3           4           5           6          7
    random, singleNote, upperLeft, upperRight, bottomLeft, bottomRight, leftRight, upDown
};

public struct SpawnNote
{
    public float time;
    public int chainCondition;
    public int direction, direction2;
    public Vector2 position, position2;

    // for editor only
    public bool isDisplaying;
    public bool absoluteKill;
}

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;

    #region init
    [Header("Initialization")]
    public GameObject prefabBubble;
    public GameObject prefabBubbleEditor;
    public GameObject prefabElectric;
    public GameObject prefabPopUpScore;
    public Material mat_ring;
    #endregion

    #region game settings
    [Header("Game Settings")]
    [Range(0.5f, 1.5f)]
    public float outlineShrinkDuration = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float effectVolume = 1;
    [Range(0, 1)]
    public float outlineBloomLevel = 0.137f;
    [HideInInspector] public bool isPause;
    [HideInInspector] public float bubbleStayDuration;
    ChainCondition chainCondition;
    Color redChain, blueChain, purpleChain;
    [HideInInspector] public Color red, blue;
    bool isPressingUp, isPressingDown, isPressingLeft, isPressingRight;
    [HideInInspector] public float refreshTimer, refreshCooldown;
    [HideInInspector] public int combo;
    [HideInInspector] public bool fullCombo;
    public bool autoPlay;
    public float noteSpawnError;
    #endregion

    #region game settings file
    public static string folderPath = Directory.GetCurrentDirectory() + "/UserData/GameSettings.txt";
    public static DirectoryInfo dir = new DirectoryInfo(folderPath);
    int column;
    string tempString;
    bool read;
    #endregion

    #region recording & editing
    [HideInInspector] public bool recording = false;
    [HideInInspector] public List<SpawnNote> spawnNote = new List<SpawnNote>();
    [HideInInspector] public SpawnNote tempNote;
    [HideInInspector] public List<GameObject> spawnedNoteOnScene = new List<GameObject>();
    Vector2 posSpawnUp = new Vector2(0, 3);
    Vector2 posSpawnDown = new Vector2(0, -3);
    Vector2 posSpawnLeft = new Vector2(-5, 0);
    Vector2 posSpawnRight = new Vector2(5, 0);
    Vector2 posZero = new Vector2(0, 0);
    #endregion
     
    #region spawn bubble based on text file
    [HideInInspector] public bool isSpawningBubble = false;
    ChainCondition condition = ChainCondition.random;
    BubbleDirection direction1 = BubbleDirection.random;
    BubbleDirection direction2 = BubbleDirection.random;
    Vector3 pos1;
    Vector3 pos2;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;
        else Destroy(gameObject);

        isPause = false;
        redChain = new Color(1f, 0.7f, 0f);
        blueChain = new Color(0f, 0.66f, 1f);
        purpleChain = new Color(1f, 0.5f, 1f);
        red = new Color(1f, 0.15f, 0.15f);
        blue = new Color(0f, 0.66f, 1f);
        refreshCooldown = 1;
        refreshTimer = 0;
        combo = 0;
        fullCombo = true;
        autoPlay = false;

        ResetInput();
        ReadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        #region Update Music Volume
        AudioManager.singleton.SetMusicVolume(musicVolume);
        AudioManager.singleton.SetSFXVolume(effectVolume);
        #endregion

        #region Update refresh timer
        if(refreshTimer < refreshCooldown)
        {
            refreshTimer += Time.deltaTime;
        }
        #endregion

        if (SceneManager.GetActiveScene().name == "MstarGameplay")
        {
            #region Update Bubble Stay Duration
            bubbleStayDuration = outlineShrinkDuration / (400 - Bubble.circleRadius) * (400 - (BubbleOutline.popCircleRadius - 10));
            #endregion

            BubbleSpawner();
        }
        else if (SceneManager.GetActiveScene().name == "NoteRecorder")
        {
            Recorder();
        }
    }

    private void OnApplicationQuit()
    {
        WriteSettings();
    }

    #region Key Pressed
    public bool IsPressingUp()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            return true;
        return false;
    }

    public bool IsPressingDown()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            return true;
        return false;
    }

    public bool IsPressingLeft()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            return true;
        return false;
    }

    public bool IsPressingRight()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            return true;
        return false;
    }

    public bool IsPressingUpLeft()
    {
        if (!(IsPressingDown() || IsPressingRight()))
        {
            if (IsPressingUp() && IsPressingLeft())
                return true;

            if (IsPressingUp())
            {
                isPressingUp = true;
            }
            if (isPressingUp && GameManager.singleton.IsPressingLeft())
            {
                return true;
            }

            if (IsPressingLeft())
            {
                isPressingLeft = true;
            }
            if (isPressingLeft && IsPressingUp())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPressingUpRight()
    {
        if (!(IsPressingDown() || IsPressingLeft()))
        {
            if (IsPressingUp() && IsPressingRight())
                return true;

            if (IsPressingUp())
            {
                isPressingUp = true;
            }
            if (isPressingUp && GameManager.singleton.IsPressingRight())
            {
                return true;
            }

            if (IsPressingRight())
            {
                isPressingRight = true;
            }
            if (isPressingRight && IsPressingUp())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPressingDownLeft()
    {
        if (!(IsPressingUp() || IsPressingRight()))
        {
            if (IsPressingDown() && IsPressingLeft())
                return true;

            if (IsPressingDown())
            {
                isPressingDown = true;
            }
            if (isPressingDown && IsPressingLeft())
            {
                return true;
            }

            if (IsPressingLeft())
            {
                isPressingLeft = true;
            }
            if (isPressingLeft && IsPressingDown())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPressingDownRight()
    {
        if (!(IsPressingUp() || IsPressingLeft()))
        {
            if (IsPressingDown() && IsPressingRight())
                return true;

            if (IsPressingDown())
            {
                isPressingDown = true;
            }
            if (isPressingDown && IsPressingRight())
            {
                return true;
            }

            if (IsPressingRight())
            {
                isPressingRight = true;
            }
            if (isPressingRight && IsPressingDown())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPressingLeftRight()
    {
        if (!(IsPressingUp() || IsPressingDown()))
        {
            if (IsPressingLeft() && IsPressingRight())
                return true;

            if (IsPressingLeft())
            {
                isPressingLeft = true;
            }
            if (isPressingLeft && IsPressingRight())
            {
                return true;
            }

            if (IsPressingRight())
            {
                isPressingRight = true;
            }
            if (isPressingRight && IsPressingLeft())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPressingUpDown()
    {
        if (!(IsPressingLeft() || IsPressingRight()))
        {
            if (IsPressingUp() && IsPressingDown())
                return true;

            if (IsPressingUp())
            {
                isPressingUp = true;
            }
            if (isPressingUp && IsPressingDown())
            {
                return true;
            }

            if (IsPressingDown())
            {
                isPressingDown = true;
            }
            if (isPressingDown && IsPressingUp())
            {
                return true;
            }
        }

        return false;
    }

    public void ResetInput()
    {
        isPressingUp = false;
        isPressingDown = false;
        isPressingLeft = false;
        isPressingRight = false;
    }
    #endregion

    #region Notes Spawning
    void DecideChainCondition(ChainCondition condition)
    {
        if (condition == ChainCondition.random)
        {
            switch ((int)Random.Range(0, 7))
            {
                case 0:
                    chainCondition = ChainCondition.singleNote;
                    break;
                case 1:
                    chainCondition = ChainCondition.upperLeft;
                    break;
                case 2:
                    chainCondition = ChainCondition.upperRight;
                    break;
                case 3:
                    chainCondition = ChainCondition.bottomLeft;
                    break;
                case 4:
                    chainCondition = ChainCondition.bottomRight;
                    break;
                case 5:
                    chainCondition = ChainCondition.leftRight;
                    break;
                case 6:
                    chainCondition = ChainCondition.upDown;
                    break;
                default:
                    chainCondition = ChainCondition.singleNote;
                    break;
            }
        }
        else
        {
            chainCondition = condition;
        }

    }

    void SpawnNote(BubbleDirection dir, BubbleDirection dir2, Vector3 position, Vector2 position2, bool editor, int index)
    {
        GameObject bubble, bubble2, chain;
        GameObject prefabBubble;
        Transform canvas;

        if (editor)
        {
            prefabBubble = prefabBubbleEditor;
            canvas = EditorCanvas.canvasEditor.transform;
        }
        else
        {
            prefabBubble = this.prefabBubble;
            canvas = GameplayCanvas.canvas.transform;
        }

        switch (chainCondition)
        {
            case ChainCondition.singleNote:
                bubble = Instantiate(prefabBubble, position, Quaternion.identity, canvas);
                bubble.GetComponent<Bubble>().DecideDirection(dir);
                bubble.GetComponent<Bubble>().chainCondition = ChainCondition.singleNote;
                bubble.GetComponent<Bubble>().index = index;
                bubble.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble);
                break;
            case ChainCondition.upperLeft:
                bubble = Instantiate(prefabBubble, position, Quaternion.identity, canvas);
                bubble.GetComponent<Bubble>().DecideDirection(dir);
                bubble.GetComponent<Bubble>().chainCondition = ChainCondition.upperLeft;
                bubble.GetComponent<Bubble>().index = index;
                bubble.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble);

                bubble2 = Instantiate(prefabBubble, position2, Quaternion.identity, canvas);
                bubble2.GetComponent<Bubble>().DecideDirection(dir2);
                bubble2.GetComponent<Bubble>().chainCondition = ChainCondition.upperLeft;
                bubble2.GetComponent<Bubble>().index = index + 0.5f;
                bubble2.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble2);

                chain = SpawnChain(position, position2, canvas);
                if (chain != null)
                {
                    chain.GetComponent<ElectricColor>().ChangeColor(purpleChain);
                    chain.transform.SetAsFirstSibling();
                    chain.GetComponent<ElectricBehaviour>().bubble = bubble;
                    chain.GetComponent<ElectricBehaviour>().bubble2 = bubble2;
                }
                break;
            case ChainCondition.upperRight:
                bubble = Instantiate(prefabBubble, position, Quaternion.identity, canvas);
                bubble.GetComponent<Bubble>().DecideDirection(dir);
                bubble.GetComponent<Bubble>().chainCondition = ChainCondition.upperRight;
                bubble.GetComponent<Bubble>().index = index;
                bubble.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble);

                bubble2 = Instantiate(prefabBubble, position2, Quaternion.identity, canvas);
                bubble2.GetComponent<Bubble>().DecideDirection(dir2);
                bubble2.GetComponent<Bubble>().chainCondition = ChainCondition.upperRight;
                bubble2.GetComponent<Bubble>().index = index + 0.5f;
                bubble2.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble2);

                chain = SpawnChain(position, position2, canvas);
                if (chain != null)
                {
                    chain.GetComponent<ElectricColor>().ChangeColor(purpleChain);
                    chain.transform.SetAsFirstSibling();
                    chain.GetComponent<ElectricBehaviour>().bubble = bubble;
                    chain.GetComponent<ElectricBehaviour>().bubble2 = bubble2;
                }
                break;
            case ChainCondition.bottomLeft:
                bubble = Instantiate(prefabBubble, position, Quaternion.identity, canvas);
                bubble.GetComponent<Bubble>().DecideDirection(dir);
                bubble.GetComponent<Bubble>().chainCondition = ChainCondition.bottomLeft;
                bubble.GetComponent<Bubble>().index = index;
                bubble.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble);

                bubble2 = Instantiate(prefabBubble, position2, Quaternion.identity, canvas);
                bubble2.GetComponent<Bubble>().DecideDirection(dir2);
                bubble2.GetComponent<Bubble>().chainCondition = ChainCondition.bottomLeft;
                bubble2.GetComponent<Bubble>().index = index + 0.5f;
                bubble2.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble2);

                chain = SpawnChain(position, position2, canvas);
                if (chain != null)
                {
                    chain.GetComponent<ElectricColor>().ChangeColor(purpleChain);
                    chain.transform.SetAsFirstSibling();
                    chain.GetComponent<ElectricBehaviour>().bubble = bubble;
                    chain.GetComponent<ElectricBehaviour>().bubble2 = bubble2;
                }
                break;
            case ChainCondition.bottomRight:
                bubble = Instantiate(prefabBubble, position, Quaternion.identity, canvas);
                bubble.GetComponent<Bubble>().DecideDirection(dir);
                bubble.GetComponent<Bubble>().chainCondition = ChainCondition.bottomRight;
                bubble.GetComponent<Bubble>().index = index;
                bubble.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble);

                bubble2 = Instantiate(prefabBubble, position2, Quaternion.identity, canvas);
                bubble2.GetComponent<Bubble>().DecideDirection(dir2);
                bubble2.GetComponent<Bubble>().chainCondition = ChainCondition.bottomRight;
                bubble2.GetComponent<Bubble>().index = index + 0.5f;
                bubble2.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble2);

                chain = SpawnChain(position, position2, canvas);
                if (chain != null)
                {
                    chain.GetComponent<ElectricColor>().ChangeColor(purpleChain);
                    chain.transform.SetAsFirstSibling();
                    chain.GetComponent<ElectricBehaviour>().bubble = bubble;
                    chain.GetComponent<ElectricBehaviour>().bubble2 = bubble2;
                }
                break;
            case ChainCondition.leftRight:
                bubble = Instantiate(prefabBubble, position, Quaternion.identity, canvas);
                bubble.GetComponent<Bubble>().DecideDirection(dir);
                bubble.GetComponent<Bubble>().chainCondition = ChainCondition.leftRight;
                bubble.GetComponent<Bubble>().index = index;
                bubble.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble);

                bubble2 = Instantiate(prefabBubble, position2, Quaternion.identity, canvas);
                bubble2.GetComponent<Bubble>().DecideDirection(dir2);
                bubble2.GetComponent<Bubble>().chainCondition = ChainCondition.leftRight;
                bubble2.GetComponent<Bubble>().index = index + 0.5f;
                bubble2.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble2);

                chain = SpawnChain(position, position2, canvas);
                if (chain != null)
                {
                    chain.GetComponent<ElectricColor>().ChangeColor(blueChain);
                    chain.transform.SetAsFirstSibling();
                    chain.GetComponent<ElectricBehaviour>().bubble = bubble;
                    chain.GetComponent<ElectricBehaviour>().bubble2 = bubble2;
                }
                break;
            case ChainCondition.upDown:
                bubble = Instantiate(prefabBubble, position, Quaternion.identity, canvas);
                bubble.GetComponent<Bubble>().DecideDirection(dir);
                bubble.GetComponent<Bubble>().chainCondition = ChainCondition.upDown;
                bubble.GetComponent<Bubble>().index = index;
                bubble.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble);

                bubble2 = Instantiate(prefabBubble, position2, Quaternion.identity, canvas);
                bubble2.GetComponent<Bubble>().DecideDirection(dir2);
                bubble2.GetComponent<Bubble>().chainCondition = ChainCondition.upDown;
                bubble2.GetComponent<Bubble>().index = index + 0.5f;
                bubble2.transform.SetAsFirstSibling();
                spawnedNoteOnScene.Add(bubble2);

                chain = SpawnChain(position, position2, canvas);
                if (chain != null)
                {
                    chain.GetComponent<ElectricColor>().ChangeColor(redChain);
                    chain.transform.SetAsFirstSibling();
                    chain.GetComponent<ElectricBehaviour>().bubble = bubble;
                    chain.GetComponent<ElectricBehaviour>().bubble2 = bubble2;
                }
                break;
        }
    }

    public void SpawnNote(int index, bool editor)
    {
        switch (spawnNote[index].chainCondition)
        {
            case 1:
                condition = ChainCondition.singleNote;
                break;
            case 2:
                condition = ChainCondition.upperLeft;
                break;
            case 3:
                condition = ChainCondition.upperRight;
                break;
            case 4:
                condition = ChainCondition.bottomLeft;
                break;
            case 5:
                condition = ChainCondition.bottomRight;
                break;
            case 6:
                condition = ChainCondition.leftRight;
                break;
            case 7:
                condition = ChainCondition.upDown;
                break;
        }
        switch (spawnNote[index].direction)
        {
            case 1:
                direction1 = BubbleDirection.up;
                break;
            case 2:
                direction1 = BubbleDirection.down;
                break;
            case 3:
                direction1 = BubbleDirection.left;
                break;
            case 4:
                direction1 = BubbleDirection.right;
                break;
        }
        switch (spawnNote[index].direction2)
        {
            case 1:
                direction2 = BubbleDirection.up;
                break;
            case 2:
                direction2 = BubbleDirection.down;
                break;
            case 3:
                direction2 = BubbleDirection.left;
                break;
            case 4:
                direction2 = BubbleDirection.right;
                break;
        }
        pos1 = new Vector3(spawnNote[index].position.x, spawnNote[index].position.y, 0);
        pos2 = new Vector3(spawnNote[index].position2.x, spawnNote[index].position2.y, 0);
        DecideChainCondition(condition);
        SpawnNote(direction1, direction2, pos1, pos2, editor, index);
    }

    GameObject SpawnChain(Vector3 position, Vector3 position2, Transform canvas)
    {
        float x1 = position.x;
        float y1 = position.y;

        float x2 = position2.x;
        float y2 = position2.y;

        if (x1 == x2 && y1 == y2)
        { // both bubble are in the same position
            return null; // doesn't need to spawn
        }

        float distanceX = x2 - x1;
        float distanceY = y2 - y1;
        float distance = Mathf.Sqrt((distanceX * distanceX) + (distanceY * distanceY));
        float angle = -1;

        if (distanceX == 0)
        { // vertically line up
            if (distanceY > 0)
            { // above
                angle = 90;
            }
            else
            { // below
                angle = 270;
            }
        }
        else if (distanceY == 0)
        { // horizontally line up
            if (distanceX > 0)
            { // right
                angle = 0;
            }
            else
            { // left
                angle = 180;
            }
        }

        if (angle == -1)
        {
            angle = Mathf.Abs((Mathf.Atan(distanceY / distanceX)) * Mathf.Rad2Deg);
            if (distanceX < 0 && distanceY > 0)
            { // upper left quad
                angle = 180 - angle;
            }
            else if (distanceX < 0 && distanceY < 0)
            { // lower left quad
                angle += 180;
            }
            else if (distanceX > 0 && distanceY < 0)
            { // lower right quad
                angle = -angle;
            }
        }

        GameObject electric = Instantiate(prefabElectric, position, Quaternion.identity, canvas);
        electric.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, angle);
        electric.GetComponent<RectTransform>().localScale = new Vector3(distance + 0.5f, 1, 1);
        electric.transform.SetAsFirstSibling();
        return electric;
    }

    public void KillAllBubbleOnScene(bool deselectSelf)
    {
        foreach (GameObject spawnedBubbleOnScene in spawnedNoteOnScene)
        {
            if(spawnedBubbleOnScene != null)
            {
                spawnedBubbleOnScene.GetComponent<Bubble>().DestroySelf(false, deselectSelf);
            }
        }
        spawnedNoteOnScene.Clear();
        for (int i = 0; i < spawnNote.Count; i++)
        {
            tempNote = spawnNote[i];
            tempNote.isDisplaying = false;
            tempNote.absoluteKill = true;
            spawnNote[i] = tempNote;
        }
    }

    public GameObject GetSpawnedNoteOnScene(float index)
    {
        for(int i = 0; i < spawnedNoteOnScene.Count; i++)
        {
            if (spawnedNoteOnScene[i] != null)
            {
                if (spawnedNoteOnScene[i].GetComponent<Bubble>().index == index)
                {
                    return spawnedNoteOnScene[i];
                }
            }
        }
        return null;
    }
    #endregion

    #region Notes Recording
    void Recorder()
    {
        if (recording)
        {
            if (IsPressingUp())
            {
                tempNote.time = AudioManager.singleton.GetSeek();
                tempNote.chainCondition = 1;
                tempNote.direction = 1;
                tempNote.position = posSpawnUp;
                tempNote.direction2 = 0;
                tempNote.position2 = posZero;
                spawnNote.Add(tempNote);
            }
            else if (IsPressingDown())
            {
                tempNote.time = AudioManager.singleton.GetSeek();
                tempNote.chainCondition = 1;
                tempNote.direction = 2;
                tempNote.position = posSpawnDown;
                tempNote.direction2 = 0;
                tempNote.position2 = posZero;
                spawnNote.Add(tempNote);
            }
            else if (IsPressingLeft())
            {
                tempNote.time = AudioManager.singleton.GetSeek();
                tempNote.chainCondition = 1;
                tempNote.direction = 3;
                tempNote.position = posSpawnLeft;
                tempNote.direction2 = 0;
                tempNote.position2 = posZero;
                spawnNote.Add(tempNote);
            }
            else if (IsPressingRight())
            {
                tempNote.time = AudioManager.singleton.GetSeek();
                tempNote.chainCondition = 1;
                tempNote.direction = 4;
                tempNote.position = posSpawnRight;
                tempNote.direction2 = 0;
                tempNote.position2 = posZero;
                spawnNote.Add(tempNote);
            }
        }
    }

    public void StartRecord(int songIndex)
    {
        recording = true;
        spawnNote.Clear();
        AudioManager.singleton.PlayMusic(AudioManager.singleton.songList[songIndex]);
    }

    public void StopRecord(string filename)
    {
        recording = false;
        TextFileManager.singleton.WriteNew(filename);
        AudioManager.singleton.StopMusic(1);
    }
    #endregion

    #region Note Spawning Based On Text File
    void BubbleSpawner()
    {
        if (isSpawningBubble && !isPause)
        {
            if (spawnNote.Count == 0)
            {
                isSpawningBubble = false;
                return;
            }

            if (AudioManager.singleton.GetSeek() >= spawnNote[0].time - outlineShrinkDuration - noteSpawnError)
            {
                SpawnNote(0, false);
                spawnNote.RemoveAt(0);
            }
        }
    }

    public void StartSpawner(int index)
    {
        TextFileManager.singleton.Read(index);
        isSpawningBubble = true;
        AudioManager.singleton.PlayCurrentlySelectedMusic();
    }

    public void StopSpawner()
    {
        isSpawningBubble = false;
        isPause = false;
        AudioManager.singleton.StopMusic(1);
        KillAllBubbleOnScene(true);
    }

    public void PauseSpawner(bool yes)
    {
        if (yes)
        {
            isPause = true;
            AudioManager.singleton.PauseMusic();
            DOTween.Pause("BubbleOutline");
        }
        else
        {
            isPause = false;
            AudioManager.singleton.UnpauseMusic();
            DOTween.Play("BubbleOutline");
        }
    }
    #endregion

    #region Refresh Songs and Notes
    public void Refresh()
    {
        refreshTimer = 0;
        AudioManager.singleton.ReadSongFiles();
        TextFileManager.singleton.ReadNoteFiles();
    }

    public bool CanRefresh()
    {
        return refreshTimer >= refreshCooldown;
    }
    #endregion

    #region Game Settings File
    void WriteSettings()
    {
        File.WriteAllText(folderPath, ""); // clear file before write
        StreamWriter writer = new StreamWriter(folderPath, true); // init stream writer

        writer.WriteLine("Music Volume:" + musicVolume.ToString() + ";");
        writer.WriteLine("Sound Volume:" + effectVolume.ToString() + ";");
        writer.WriteLine("Bubble Shrink Duration:" + outlineShrinkDuration.ToString() + ";");
        writer.WriteLine("Outline Bloom Level:" + outlineBloomLevel.ToString() + ";");
        writer.WriteLine("#");
        writer.Close();
    }

    void ReadSettings()
    {
        // read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(folderPath);
        string fullString = reader.ReadToEnd();
        reader.Close();

        if (fullString == "")
        {
            return;
        }

        column = 0;
        tempString = "";
        read = false;
        for (int i = 0; fullString[i] != '#'; i++)
        {
            if (fullString[i] == ':')
            {
                read = true;
                continue;
            }
            else if (fullString[i] == ';')
            {
                read = false;
                SwitchColumn();
                column++;
                tempString = "";
                continue;
            }

            if (read)
            {
                tempString += fullString[i];
            }
        }
    }

    void SwitchColumn()
    {
        switch (column)
        {
            case 0:
                musicVolume = float.Parse(tempString);
                break;
            case 1:
                effectVolume = float.Parse(tempString);
                break;
            case 2:
                outlineShrinkDuration = float.Parse(tempString);
                break;
            case 3:
                outlineBloomLevel = float.Parse(tempString);
                break;
        }
    }
    #endregion
}

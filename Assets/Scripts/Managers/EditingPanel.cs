using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class EditingPanel : MonoBehaviour
{
    public static EditingPanel singleton;

    [Header("Initialization")]
    public Sprite playImage;
    public Sprite pauseImage;

    [HideInInspector] public Button playButton;
    Image playButtonImage;
    Slider songSeeker, songSeekerModifier;
    TMP_InputField timeInputField;
    [HideInInspector] public GameObject selectedTools;
    [HideInInspector] public Button addButton;

    [Header("Game Flow")]
    public bool isPause;
    bool isSelectedTime;
    public float currentlySelectedBubbleIndex;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;
        else Destroy(gameObject);

        playButton = transform.GetChild(0).GetComponent<Button>();
        playButtonImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        timeInputField = transform.GetChild(1).GetComponent<TMP_InputField>();
        songSeeker = transform.GetChild(2).GetComponent<Slider>();
        songSeekerModifier = transform.GetChild(3).GetComponent<Slider>();
        selectedTools = transform.GetChild(5).gameObject;
        addButton = transform.GetChild(4).GetComponent<Button>();

        playButton.interactable = false;
        addButton.interactable = false;

        isPause = false;
        currentlySelectedBubbleIndex = -1;

        selectedTools.SetActive(false);
    }

    IEnumerator DeactiveSelectedTools()
    {
        yield return GameManager.singleton == null;
    }

    // Update is called once per frame
    void Update()
    {
        #region Update Song Seeker
        if (EditingSettingsPanel.singleton.isStarted)
        { // start editing
            songSeeker.value = AudioManager.singleton.GetSeek() / AudioManager.singleton.GetSongLength();
        }
        #endregion

        #region Update Time Input Field
        if (!isSelectedTime)
        {
            timeInputField.text = AudioManager.singleton.GetSeek().ToString("F4");
        }
        #endregion

        #region Keyboard Shortcut
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            AddNote();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetSongSeeker(true, 1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetSongSeeker(false, 1);
        }
        #endregion

        #region Bubble Spawner
        BubbleBehaviour();
        #endregion
    }

    public void StartEdit(bool yes, bool save)
    {
        if (yes)
        {
            TextFileManager.singleton.Read(EditingSettingsPanel.singleton.noteToSpawn);
            AudioManager.singleton.PlayCurrentlySelectedMusic();
            EnableInteraction(true);
        }
        else
        {
            AudioManager.singleton.StopMusic(1);
            EnableInteraction(false);
            GameManager.singleton.KillAllBubbleOnScene(true);
            if (save)
            {
                ArrangeSpawnNote();
                TextFileManager.singleton.Write(EditingSettingsPanel.singleton.noteToSpawn);
            }
        }
    }

    #region UI
    public void SetSongSeeker()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && EditingSettingsPanel.singleton.isStarted && !selectedTools.gameObject.activeInHierarchy)
        { // if start editing and user left clicking
            float time = songSeekerModifier.value * AudioManager.singleton.GetSongLength();
            AudioManager.singleton.SetSeek(time);
        }
    }

    public void SetSongSeeker(bool left, float interval)
    {
        float time;
        if (left)
        {
            time = AudioManager.singleton.GetSeek() - interval;
        }
        else
        {
            time = AudioManager.singleton.GetSeek() + interval;
        }
        AudioManager.singleton.SetSeek(time);
    }

    public void SetSongSeekerNumber()
    {
        try
        {
            AudioManager.singleton.SetSeek(float.Parse(timeInputField.text));
        }
        catch
        {

        }
        SelectedTimeInputField(false);
    }

    public void TogglePause()
    {
        if (isPause)
        {
            Pause(false);
        }
        else
        {
            Pause(true);
        }
    }

    public void SelectedTimeInputField(bool yes)
    {
        if (yes)
        {
            isSelectedTime = true;
        }
        else
        {
            isSelectedTime = false;
        }
    }

    public void Pause(bool yes)
    {
        if (playButton.interactable)
        {
            if (yes)
            {
                isPause = true;
                AudioManager.singleton.PauseMusic();
                playButtonImage.sprite = playImage;
            }
            else
            {
                isPause = false;
                AudioManager.singleton.UnpauseMusic();
                playButtonImage.sprite = pauseImage;
            }
        }
    }

    void EnableInteraction(bool yes)
    {
        if (yes)
        {
            playButton.interactable = true;
            playButtonImage.sprite = pauseImage;
            addButton.interactable = true;
        }
        else
        {
            playButton.interactable = false;
            playButtonImage.sprite = playImage;
            addButton.interactable = false;
        }
    }
    #endregion

    #region Bubble Spawner
    void BubbleBehaviour()
    {
        if (EditingSettingsPanel.singleton.isStarted)
        {
            for (int i = 0; i < GameManager.singleton.spawnNote.Count; i++)
            {
                if (GameManager.singleton.spawnNote[i].time - GameManager.singleton.outlineShrinkDuration - GameManager.singleton.noteSpawnError <= AudioManager.singleton.GetSeek()
                && AudioManager.singleton.GetSeek() < GameManager.singleton.spawnNote[i].time - GameManager.singleton.noteSpawnError)
                {
                    if (GameManager.singleton.spawnNote[i].isDisplaying == false)
                    {
                        GameManager.singleton.SpawnNote(i, true);
                        GameManager.singleton.tempNote = GameManager.singleton.spawnNote[i];
                        GameManager.singleton.tempNote.absoluteKill = false;
                        GameManager.singleton.tempNote.isDisplaying = true;
                        GameManager.singleton.spawnNote[i] = GameManager.singleton.tempNote;
                    }
                }
                else
                {
                    GameManager.singleton.tempNote = GameManager.singleton.spawnNote[i];
                    GameManager.singleton.tempNote.absoluteKill = true;
                    GameManager.singleton.tempNote.isDisplaying = false;
                    GameManager.singleton.spawnNote[i] = GameManager.singleton.tempNote;
                }
            }
        }
    }
    #endregion

    #region Add Bubble
    public void AddNote()
    {
        if (addButton.interactable)
        {
            Pause(true);
            CreateNewNoteOnScene(AudioManager.singleton.GetSeek() + (GameManager.singleton.outlineShrinkDuration / 2));
        }
    }

    void CreateNewNoteOnScene(float currentTime)
    {
        foreach (SpawnNote spawnote in GameManager.singleton.spawnNote)
        {
            if (currentTime == spawnote.time)
            {
                return;
            }
        }

        #region create an entry in spawnNote
        GameManager.singleton.tempNote.chainCondition = 1;
        GameManager.singleton.tempNote.direction = 1;
        GameManager.singleton.tempNote.direction2 = 0;
        GameManager.singleton.tempNote.position = Vector2.zero;
        GameManager.singleton.tempNote.position2 = Vector2.zero;
        GameManager.singleton.tempNote.time = currentTime;
        GameManager.singleton.tempNote.isDisplaying = false;
        GameManager.singleton.spawnNote.Add(GameManager.singleton.tempNote);
        #endregion

        #region Selected current note
        currentlySelectedBubbleIndex = GameManager.singleton.spawnNote.Count - 1;
        selectedTools.SetActive(true);
        playButton.interactable = false;
        addButton.interactable = false;
        #endregion
    }
    #endregion

    #region Write File
    void ArrangeSpawnNote()
    {
        List<SpawnNote> tempSpawnNote = new List<SpawnNote>();
        while (GameManager.singleton.spawnNote.Count > 0)
        {
            float smallestTime = 9999999999999999999;
            int smallestIndex = 0;
            for (int i = 0; i < GameManager.singleton.spawnNote.Count; i++)
            {
                if (GameManager.singleton.spawnNote[i].time < smallestTime)
                {
                    smallestTime = GameManager.singleton.spawnNote[i].time;
                    smallestIndex = i;
                }
            }
            SpawnNote temp = GameManager.singleton.spawnNote[smallestIndex];
            tempSpawnNote.Add(temp);
            GameManager.singleton.spawnNote.RemoveAt(smallestIndex);
        }
        GameManager.singleton.spawnNote = tempSpawnNote;
    }
    #endregion
}
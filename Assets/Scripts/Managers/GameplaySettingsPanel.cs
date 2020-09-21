using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameplaySettingsPanel : MonoBehaviour
{
    public static GameplaySettingsPanel singleton;

    [Header("Initialization")]
    public PopUpMessage pauseMessage;
    public PopUpMessage blankNoteMessage;
    public PopUpMessage startTimerMessage;
    public TextMeshProUGUI startTimerText;

    RectTransform rectTrans, arrowTrans;
    Dropdown dropdownSongList, dropdownNoteList;
    Slider musicVolume, effectVolume, bubbleShrinkDuration, outlineBloomLevel;
    Button startButton, backButton, refreshButton;

    [HideInInspector] public bool isShowing;
    [HideInInspector] public bool isStarted;
    int noteToSpawn;
    float startTimer;

    public List<int> textNameIndex = new List<int>();
    public List<string> textNameList = new List<string>();

    string startGameString = "[F1] Start Game";
    string pauseGameString = "[F2] Pause";
    string resumeGameString = "[F2] Resume";
    string stopGameString = "[F1] Stop Game";
    string backToMenuString = "Back To Menu";

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;
        else Destroy(gameObject);

        rectTrans = GetComponent<RectTransform>();
        arrowTrans = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        dropdownSongList = transform.GetChild(2).GetChild(0).GetComponent<Dropdown>();
        dropdownNoteList = transform.GetChild(3).GetChild(0).GetComponent<Dropdown>();
        musicVolume = transform.GetChild(4).GetChild(0).GetComponent<Slider>();
        effectVolume = transform.GetChild(5).GetChild(0).GetComponent<Slider>();
        refreshButton = transform.GetChild(6).GetComponent<Button>();
        startButton = transform.GetChild(7).GetComponent<Button>();
        backButton = transform.GetChild(8).GetComponent<Button>();
        bubbleShrinkDuration = transform.GetChild(9).GetChild(2).GetComponent<Slider>();
        outlineBloomLevel = transform.GetChild(10).GetChild(0).GetComponent<Slider>();

        isStarted = false;
        noteToSpawn = 0;
        isShowing = true;
        startTimer = 0;
        GameManager.singleton.refreshTimer = 0;

        StartCoroutine(DeactiveMessageBlock());
        StartCoroutine(InitDropdownSongList());

        AudioManager.singleton.StopMusic(1);
    }

    IEnumerator InitDropdownSongList()
    {
        dropdownSongList.ClearOptions();
        yield return GameManager.singleton == null; // if haven't init game manager, return true
        dropdownSongList.AddOptions(AudioManager.singleton.songName);
        dropdownSongList.value = AudioManager.singleton.currentlySelectedSongIndex;
        if (dropdownSongList.value == 0)
        { // because the first one was defaulty selected, so it won't trigger on value changed function, need to manually called.
            StartCoroutine(InitDropdownNoteList());
        }
    }

    IEnumerator InitDropdownNoteList()
    {
        dropdownNoteList.ClearOptions();
        yield return GameManager.singleton == null; // if haven't init game manager, return true
        dropdownSongList.value = AudioManager.singleton.currentlySelectedSongIndex;

        // init textNameList variable
        int i = 0;
        textNameList.Clear();
        textNameIndex.Clear();
        foreach (string name in TextFileManager.singleton.songName)
        {
            if (name == AudioManager.singleton.songName[AudioManager.singleton.currentlySelectedSongIndex])
            {
                textNameList.Add(TextFileManager.singleton.notesPath[i].name);
                textNameIndex.Add(i);
            }
            i++;
        }
        dropdownNoteList.AddOptions(textNameList);

        if (textNameIndex.Count == 0)
        {
            blankNoteMessage.PopUp(true);
        }
        else
        {
            UpdateNoteToSpawn();
        }

    }

    IEnumerator DeactiveMessageBlock()
    {
        yield return pauseMessage == null && blankNoteMessage == null;
        pauseMessage.gameObject.SetActive(false);
        blankNoteMessage.gameObject.SetActive(false);
        startTimerMessage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        #region Escape dock settings panel
        if (Input.GetKeyDown(KeyCode.Escape) && !blankNoteMessage.AlphaBoardShowing())
        {
            isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
        }
        #endregion

        #region Update Volume
        musicVolume.value = GameManager.singleton.musicVolume;
        effectVolume.value = GameManager.singleton.effectVolume;
        bubbleShrinkDuration.value = GameManager.singleton.outlineShrinkDuration;
        outlineBloomLevel.value = GameManager.singleton.outlineBloomLevel;
        #endregion

        #region F5 to refresh
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Refresh();
        }
        #endregion

        #region Disable start button
        if (textNameIndex.Count == 0)
        {
            startButton.interactable = false;
        }
        else
        {
            startButton.interactable = true;
        }
        #endregion

        #region F1 to start/end
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PressedF1();
        }
        #endregion

        #region End song
        if (IsEnding())
        {
            EndSong();
        }
        #endregion

        #region F2 to pause/unpause
        if (isStarted && Input.GetKeyDown(KeyCode.F2))
        {
            BackToMenu();
        }
        #endregion

        #region Enter to close message
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (blankNoteMessage.canvasGroup.alpha == 1)
            {
                blankNoteMessage.PopUp(false);
            }
        }
        #endregion

        #region Update start timer
        if (startTimer > 0)
        {
            startTimer -= Time.deltaTime;
            startTimerText.text = ((int)startTimer + 1).ToString();
        }
        #endregion
    }

    void OnMouseDown()
    {
        if (!blankNoteMessage.AlphaBoardShowing())
        {
            isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
        }
    }

    bool IsEnding()
    {
        return isStarted && !GameManager.singleton.isPause && GameManager.singleton.spawnNote.Count == 0 && !AudioManager.singleton.IsPlaying();
    }

    void EndSong()
    {
        isStarted = false;
        EnableInteraction(true);
        GameManager.singleton.StopSpawner();
        startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = startGameString;
        backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = backToMenuString;
    }

    public void PressedF1()
    {
        if (startButton.IsInteractable() && GameManager.singleton.CanRefresh())
        {
            if (!isStarted)
            {
                StartCoroutine(StartPlayAfter3Seconds());
            }
            else
            {
                if (!isShowing)
                {
                    isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
                }

                if (GameManager.singleton.isPause)
                {
                    pauseMessage.PopUp(false);
                    GameManager.singleton.PauseSpawner(false);
                }
                EndSong();
            }
        }
    }

    void Resume()
    {
        StartCoroutine(ResumeAfter3Seconds());
    }

    void StartPlay()
    {
        isStarted = true;
        EnableInteraction(false);
        startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = stopGameString;
        backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pauseGameString;
        GameManager.singleton.StartSpawner(noteToSpawn);
        ScoreText.ResetScore();
    }

    IEnumerator StartPlayAfter3Seconds()
    {
        if (startTimerMessage.gameObject.activeInHierarchy == false)
        {
            startTimerMessage.PopUp(true);
            startTimer = 3;
            GameManager.singleton.combo = 0;
            GameManager.singleton.fullCombo = true;
            if (isShowing)
                isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
            yield return new WaitForSeconds(3);
            startTimerMessage.PopUp(false);
            StartPlay();
        }
    }

    IEnumerator ResumeAfter3Seconds()
    {
        if (pauseMessage.gameObject.activeInHierarchy)
        {
            startTimerMessage.PopUp(true);
            pauseMessage.PopUp(false);
            startTimer = 3;
            if (isShowing)
                isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
            yield return new WaitForSeconds(3);
            startTimerMessage.PopUp(false);
            backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pauseGameString;
            GameManager.singleton.PauseSpawner(false);
        }
    }

    public void UpdateMusicVolume()
    {
        GameManager.singleton.musicVolume = musicVolume.value;
    }

    public void UpdateSoundVolume()
    {
        GameManager.singleton.effectVolume = effectVolume.value;
        AudioManager.singleton.PlayPop();
    }

    public void UpdateBubbleShrinkDuration()
    {
        GameManager.singleton.outlineShrinkDuration = bubbleShrinkDuration.value;
    }

    public void UpdateOutlineBloomLevel()
    {
        GameManager.singleton.outlineBloomLevel = outlineBloomLevel.value;
        GameManager.singleton.mat_ring.SetColor("_color", new Color(outlineBloomLevel.value, outlineBloomLevel.value, outlineBloomLevel.value, 0));
    }

    public void Refresh()
    {
        if (GameManager.singleton.CanRefresh())
        {
            GameManager.singleton.Refresh();
            StartCoroutine(InitDropdownSongList());
        }
    }

    public void UpdateCurrentlySelectedSong()
    {
        AudioManager.singleton.currentlySelectedSongIndex = dropdownSongList.value;
        StartCoroutine(InitDropdownNoteList());
    }

    public void BackToMenu()
    {
        if (GameManager.singleton.CanRefresh())
        {
            if (isStarted)
            {
                if (!GameManager.singleton.isPause)
                { // pause the game
                    pauseMessage.PopUp(true);
                    backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = resumeGameString;
                    GameManager.singleton.PauseSpawner(true);
                }
                else
                { // resume the game
                    Resume();
                }
            }
            else
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
        }
    }

    public void UpdateNoteToSpawn()
    {
        noteToSpawn = textNameIndex[dropdownNoteList.value];
    }

    void EnableInteraction(bool yes)
    {
        if (yes)
        {
            dropdownSongList.interactable = true;
            dropdownNoteList.interactable = true;
            refreshButton.interactable = true;
        }
        else
        {
            dropdownSongList.interactable = false;
            dropdownNoteList.interactable = false;
            refreshButton.interactable = false;
        }
    }
}

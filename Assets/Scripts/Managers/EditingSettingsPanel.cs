using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class EditingSettingsPanel : MonoBehaviour
{
    public static EditingSettingsPanel singleton;

    [Header("Initialization")]
    public PopUpMessage blankNoteMessage;
    public PopUpMessage cantChainMessage;

    RectTransform rectTrans, arrowTrans;
    Dropdown dropdownSongList, dropdownNoteList;
    Slider musicVolume, effectVolume, bubbleShrinkDuration;
    Button startButton, refreshButton, backButton;
    Image imageRecordingCircle;

    [HideInInspector] public bool isShowing;
    [HideInInspector] public bool isStarted;
    [HideInInspector] public int noteToSpawn;

    [Header("Game Flow")]
    public List<int> textNameIndex = new List<int>();
    public List<string> textNameList = new List<string>();

    string startEditingString = "[F1] Start Editing";
    string stopEditingString = "[F1] Save & Stop Editing";
    string discardEditingString = "[F2] Discard & Stop Editing";
    string backToMenuString = "Back To Menu";
    Color imageRecordingCircleTemp;
    float imageRecordingCircleTimer;

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
        bubbleShrinkDuration = transform.GetChild(6).GetChild(2).GetComponent<Slider>();
        refreshButton = transform.GetChild(7).GetComponent<Button>();
        startButton = transform.GetChild(8).GetComponent<Button>();
        backButton = transform.GetChild(9).GetComponent<Button>();
        imageRecordingCircle = transform.GetChild(8).GetChild(1).GetComponent<Image>();

        isShowing = true;
        isStarted = false;
        noteToSpawn = 0;
        GameManager.singleton.refreshTimer = 0;

        StartCoroutine(DeactiveMessageBlock());
        StartCoroutine(InitDropdownSongList());

        AudioManager.singleton.StopMusic(1);
    }

    IEnumerator DeactiveMessageBlock()
    {
        yield return blankNoteMessage == null && cantChainMessage == null;
        blankNoteMessage.gameObject.SetActive(false);
        cantChainMessage.gameObject.SetActive(false);
    }

    IEnumerator InitDropdownSongList()
    {
        dropdownSongList.ClearOptions();
        yield return GameManager.singleton == null; // if haven't init game manager, return true
        dropdownSongList.AddOptions(AudioManager.singleton.songName);
        dropdownSongList.value = AudioManager.singleton.currentlySelectedSongIndex;
        if (dropdownSongList.value == 0)
        {
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

    // Update is called once per frame
    void Update()
    {
        #region Escape dock settings panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
        }
        #endregion

        #region Update Volume
        musicVolume.value = GameManager.singleton.musicVolume;
        effectVolume.value = GameManager.singleton.effectVolume;
        bubbleShrinkDuration.value = GameManager.singleton.outlineShrinkDuration;
        #endregion

        #region Keyboard Input
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PressedF1();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PressedF2();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Refresh();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (blankNoteMessage.canvasGroup.alpha == 1)
            {
                blankNoteMessage.PopUp(false);
            }
            if (cantChainMessage.canvasGroup.alpha == 1)
            {
                cantChainMessage.PopUp(false);
            }
        }
        #endregion

        #region Update recording circle
        if (isStarted)
        {
            imageRecordingCircleTimer += Time.deltaTime * 2;
            imageRecordingCircleTemp = imageRecordingCircle.color;
            if ((int)imageRecordingCircleTimer % 2 == 0)
            {
                imageRecordingCircleTemp.a = 1;
            }
            else if (imageRecordingCircleTimer >= 0.5f)
            {
                imageRecordingCircleTemp.a = 0;
            }
            imageRecordingCircle.color = imageRecordingCircleTemp;
        }
        else
        {
            imageRecordingCircle.DOComplete();
            imageRecordingCircleTemp = imageRecordingCircle.color;
            imageRecordingCircleTemp.a = 0;
            imageRecordingCircle.color = imageRecordingCircleTemp;
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
    }

    void OnMouseDown()
    {
        isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
    }

    public void UpdateCurrentlySelectedSong()
    {
        AudioManager.singleton.currentlySelectedSongIndex = dropdownSongList.value;
        StartCoroutine(InitDropdownNoteList());
    }

    public void UpdateNoteToSpawn()
    {
        noteToSpawn = textNameIndex[dropdownNoteList.value];
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

    public void Refresh()
    {
        if (GameManager.singleton.CanRefresh())
        {
            GameManager.singleton.Refresh();
            StartCoroutine(InitDropdownSongList());
        }
    }

    public void PressedF1()
    {
        if (startButton.IsInteractable() && GameManager.singleton.CanRefresh())
        {
            if (!isStarted)
            {
                if (!AudioManager.singleton.IsPlaying())
                {
                    isStarted = true;
                    EnableInteraction(false);
                    startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = stopEditingString;
                    backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = discardEditingString;
                    if (isShowing)
                        isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
                    EditingPanel.singleton.StartEdit(true, true);
                }
            }
            else
            {
                isStarted = false;
                EnableInteraction(true);
                startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = startEditingString;
                backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = backToMenuString;
                if (!isShowing)
                    isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
                EditingPanel.singleton.StartEdit(false, true);
            }
        }
    }

    void PressedF2()
    {
        BackToMenu(true);
    }

    public void BackToMenu(bool useHotkey)
    {
        if (backButton.IsInteractable() && GameManager.singleton.CanRefresh())
        {
            if (isStarted)
            {
                isStarted = false;
                EnableInteraction(true);
                startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = startEditingString;
                backButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = backToMenuString;
                if (!isShowing)
                    isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
                EditingPanel.singleton.StartEdit(false, false);
            }
            else
            {
                if(useHotkey == false)
                    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
        }
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

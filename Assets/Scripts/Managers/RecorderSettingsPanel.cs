using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public static class SettingsPanel
{
    public static Vector3 showPos = new Vector3(700,0,0);
    public static Vector3 hidePos = new Vector3(1460, 0, 0);
    public static Ease dockAnimation = Ease.InOutBack;
    public static float dockDuration = 0.5f;

    public static bool Docking(RectTransform rectTrans, RectTransform arrowTrans, bool isShowing)
    {
        if (rectTrans.localPosition == showPos)
        {
            rectTrans.DOLocalMove(hidePos, dockDuration).SetEase(dockAnimation);
            arrowTrans.DOScaleX(1, dockDuration).SetEase(dockAnimation);
            return false;
        }
        else if (rectTrans.localPosition == hidePos)
        {
            rectTrans.DOLocalMove(showPos, dockDuration).SetEase(dockAnimation);
            arrowTrans.DOScaleX(-1, dockDuration).SetEase(dockAnimation);
            return true;
        }
        return isShowing;
    }
}

public class RecorderSettingsPanel : MonoBehaviour
{
    public static RecorderSettingsPanel singleton;

    [Header("Initialization")]
    public PopUpMessage duplicateFileMessage;
    public PopUpMessage blankFileNameMessage;
    public PopUpMessage doneRecordingMessage;

    RectTransform rectTrans, arrowTrans;
    TMP_InputField fileName;
    Dropdown dropdownSongList;
    TextMeshProUGUI textRecording;
    Image imageRecordingCircle;
    Button refreshButton, backButton;
    Slider musicVolume, effectVolume;
    
    [HideInInspector] public bool isShowing;

    string startRecordingString = "[F1] Start Recording";
    string stopRecordingString = "[F1] Stop Recording";
    string confirmedFileName;
    Color imageRecordingCircleTemp;
    float imageRecordingCircleTimer;
    bool interaction;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;
        else Destroy(gameObject);

        rectTrans = GetComponent<RectTransform>();
        arrowTrans = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        fileName = transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>();
        dropdownSongList = transform.GetChild(3).GetChild(0).GetComponent<Dropdown>();
        textRecording = transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>();
        imageRecordingCircle = transform.GetChild(5).GetChild(1).GetComponent<Image>();
        refreshButton = transform.GetChild(4).GetComponent<Button>();
        backButton = transform.GetChild(6).GetComponent<Button>();
        musicVolume = transform.GetChild(7).GetChild(0).GetComponent<Slider>();
        effectVolume = transform.GetChild(8).GetChild(0).GetComponent<Slider>();

        rectTrans.localPosition = SettingsPanel.showPos;
        isShowing = true;
        interaction = true;
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
    }

    IEnumerator DeactiveMessageBlock()
    {
        yield return duplicateFileMessage == null && blankFileNameMessage == null && doneRecordingMessage == null;
        duplicateFileMessage.gameObject.SetActive(false);
        blankFileNameMessage.gameObject.SetActive(false);
        doneRecordingMessage.gameObject.SetActive(false);
    }

    void Update()
    {
        #region Escape dock settings panel
        if (Input.GetKeyDown(KeyCode.Escape) && !duplicateFileMessage.AlphaBoardShowing())
        {
            isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
        }
        #endregion

        #region Update recording circle
        if (GameManager.singleton.recording)
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

        #region F1 to record
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OnClickRecordButton();
        }
        #endregion

        #region F5 to refresh
        if (Input.GetKeyDown(KeyCode.F5) && interaction)
        {
            Refresh();
        }
        #endregion

        #region Pop Up Message Input
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (duplicateFileMessage.canvasGroup.alpha == 1)
            {
                duplicateFileMessage.PopUp(false);
            }
            if (blankFileNameMessage.canvasGroup.alpha == 1)
            {
                blankFileNameMessage.PopUp(false);
            }
            if (doneRecordingMessage.canvasGroup.alpha == 1)
            {
                doneRecordingMessage.PopUp(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (duplicateFileMessage.canvasGroup.alpha == 1)
            {
                StartRecording();
                duplicateFileMessage.PopUp(false);
            }
        }
        #endregion

        #region Update Volume
        musicVolume.value = GameManager.singleton.musicVolume;
        effectVolume.value = GameManager.singleton.effectVolume;
        #endregion
    }

    void OnMouseDown()
    {
        if (!duplicateFileMessage.AlphaBoardShowing())
        {
            isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
        }
    }

    public void UpdateCurrentlySelectedSong()
    {
        AudioManager.singleton.currentlySelectedSongIndex = dropdownSongList.value;
    }

    public bool CheckFileName()
    {
        if (IsFileNameBlank())
        {
            blankFileNameMessage.PopUp(true);
            return false;
        }

        confirmedFileName = fileName.text;
        foreach (FilePath notepath in TextFileManager.singleton.notesPath)
        {
            if (fileName.text == notepath.name)
            {
                duplicateFileMessage.PopUp(true);
                return false;
            }
        }
        return true;
    }

    bool IsFileNameBlank()
    {
        if(fileName.text == "")
        {
            return true;
        }
        return false;
    }

    public void OnClickRecordButton()
    {
        if (GameManager.singleton.CanRefresh() && doneRecordingMessage.AlphaBoardShowing() == false)
        {
            if (GameManager.singleton.recording)
            { // actions: stop the recording
                EnableInteraction(true);
                textRecording.text = startRecordingString;
                GameManager.singleton.StopRecord(confirmedFileName);
                doneRecordingMessage.PopUp(true);
                if (!isShowing)
                    isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
            }
            else
            { // actions: start the recording
                if (CheckFileName())
                {
                    StartRecording();
                }
            }
        }
    }

    void EnableInteraction(bool yes)
    {
        if (yes)
        {
            interaction = true;
            refreshButton.interactable = true;
            backButton.interactable = true;
            dropdownSongList.interactable = true;
            fileName.interactable = true;
        }
        else
        {
            interaction = false;
            refreshButton.interactable = false;
            backButton.interactable = false;
            dropdownSongList.interactable = false;
            fileName.interactable = false;
        }
    }

    public void StartRecording()
    {
        EnableInteraction(false);
        textRecording.text = stopRecordingString;
        GameManager.singleton.StartRecord(AudioManager.singleton.currentlySelectedSongIndex);
        imageRecordingCircleTimer = 0;
        if (isShowing)
            isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
    }

    public void Refresh()
    {
        if (GameManager.singleton.CanRefresh())
        {
            GameManager.singleton.Refresh();
            StartCoroutine(InitDropdownSongList());
        }
    }

    public void BackToMenu()
    {
        if (GameManager.singleton.CanRefresh())
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
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
}

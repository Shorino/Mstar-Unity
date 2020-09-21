using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSettingsPanel : MonoBehaviour
{
    public static MenuSettingsPanel singleton;

    [Header("Initialization")]
    public Sprite playSprite;
    public Sprite pauseSprite;

    RectTransform rectTrans, arrowTrans;
    Dropdown dropdownSongList;
    Slider musicVolume, effectVolume, bubbleShrinkDuration;
    Image playPauseImage;

    [HideInInspector] public bool isShowing;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;
        else Destroy(gameObject);

        isShowing = true;

        rectTrans = GetComponent<RectTransform>();
        arrowTrans = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        dropdownSongList = transform.GetChild(8).GetComponent<Dropdown>();
        playPauseImage = transform.GetChild(9).GetChild(0).GetComponent<Image>();
        musicVolume = transform.GetChild(10).GetChild(0).GetComponent<Slider>();
        effectVolume = transform.GetChild(11).GetChild(0).GetComponent<Slider>();
        bubbleShrinkDuration = transform.GetChild(12).GetChild(2).GetComponent<Slider>();

        StartCoroutine(InitDropdownSongList());
    }
    IEnumerator InitDropdownSongList()
    {
        dropdownSongList.ClearOptions();
        yield return GameManager.singleton == null; // if haven't init game manager, return true
        dropdownSongList.AddOptions(AudioManager.singleton.songName);
        dropdownSongList.value = AudioManager.singleton.currentlySelectedSongIndex;
        UpdateCurrentlySelectedSong(true);

        if (AudioManager.singleton.IsPlaying())
        {
            playPauseImage.sprite = pauseSprite;
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

        #region Menu Shortcut Button
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LoadSinglePlayerScene();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadMultiplayerScene();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            LoadRecorderScene();
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            LoadEditorScene();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PausePlayMusic();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            StartCoroutine(InitDropdownSongList());
        }
        #endregion

        #region Update Volume
        musicVolume.value = GameManager.singleton.musicVolume;
        effectVolume.value = GameManager.singleton.effectVolume;
        bubbleShrinkDuration.value = GameManager.singleton.outlineShrinkDuration;
        #endregion
    }

    void OnMouseDown()
    {
        if (true)
        {
            isShowing = SettingsPanel.Docking(rectTrans, arrowTrans, isShowing);
        }
    }

    public void LoadSinglePlayerScene()
    {
        if(GameManager.singleton.CanRefresh())
            SceneManager.LoadScene("MstarGameplay", LoadSceneMode.Single);
    }

    public void LoadMultiplayerScene()
    {

    }

    public void LoadRecorderScene()
    {
        if (GameManager.singleton.CanRefresh())
            SceneManager.LoadScene("NoteRecorder", LoadSceneMode.Single);
    }

    public void LoadEditorScene()
    {
        if (GameManager.singleton.CanRefresh())
            SceneManager.LoadScene("NoteEditor", LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
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

    public void UpdateCurrentlySelectedSong(bool random)
    {
        if (random)
        {
            int randomNo = Random.Range(0, dropdownSongList.options.Count);
            dropdownSongList.value = randomNo;
        }
        AudioManager.singleton.currentlySelectedSongIndex = dropdownSongList.value;
        AudioManager.singleton.PlayCurrentlySelectedMusic();
        playPauseImage.sprite = pauseSprite;
    }

    public void PausePlayMusic()
    {
        if (AudioManager.singleton.IsPlaying())
        {
            AudioManager.singleton.PauseMusic();
            playPauseImage.sprite = playSprite;
        }
        else
        {
            AudioManager.singleton.UnpauseMusic();
            playPauseImage.sprite = pauseSprite;
        }
    }
}

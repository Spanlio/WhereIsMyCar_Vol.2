using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuScript : MonoBehaviour
{
    public GameObject settingsPopup;

    public AudioSource musicSource;

    public Slider musicSlider;
    public Slider soundSlider;


    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundVolumeKey = "SoundVolume";


    void Start()
    {
        float savedMusicVolume =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                musicSource != null ? musicSource.volume : 0.3f
            );

        float savedSoundVolume =
            PlayerPrefs.GetFloat(
                SoundVolumeKey,
                1f
            );


        // Set saved values
        if (musicSource != null)
        {
            musicSource.volume = savedMusicVolume;
        }

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.SetValueWithoutNotify(savedMusicVolume);

            musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        }

        if (soundSlider != null)
        {
            soundSlider.minValue = 0f;
            soundSlider.maxValue = 1f;
            soundSlider.SetValueWithoutNotify(savedSoundVolume);

            soundSlider.onValueChanged.AddListener(ChangeSoundVolume);
        }
    }


    public void PlayGame()
    {
        SceneManager.LoadScene("CityScene");
    }


    public void OpenSettings()
    {
        settingsPopup.SetActive(true);
    }


    public void CloseSettings()
    {
        settingsPopup.SetActive(false);
    }


    void ChangeMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();
    }


    void ChangeSoundVolume(float volume)
    {
        PlayerPrefs.SetFloat(SoundVolumeKey, volume);
        PlayerPrefs.Save();
    }


    public void ExitGame()
    {
        Application.Quit();
    }


    void OnDestroy()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(ChangeMusicVolume);
        }

        if (soundSlider != null)
        {
            soundSlider.onValueChanged.RemoveListener(ChangeSoundVolume);
        }
    }
}
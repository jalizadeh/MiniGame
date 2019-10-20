using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;

    int activeScreenResIndex;
    

    //load saved data from player's preferences
    private void Start()
    {
        print("hi start");
        activeScreenResIndex = PlayerPrefs.GetInt("screen res index");
        bool isFullscreen = (PlayerPrefs.GetInt("fullscreen") == 1) ? true : false;

        for (int i=0; i<resolutionToggles.Length; i++)
        {
            resolutionToggles[i].isOn = (i == activeScreenResIndex);
        }
        
        fullscreenToggle.isOn = isFullscreen;

        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;
    }


    public void Play() {
        SceneManager.LoadScene("Game");
    }


    public void Quit() {
        Application.Quit();
    }

    public void OptionsMenu() {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }


    public void MainMenu() {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionToggles[i].isOn)
        {
            print("activeScreenResIndex: " + activeScreenResIndex + " / " + "i: " + i);
            activeScreenResIndex = i;

            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);

            PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullcreaan(bool isFullscreen)
    {
        for(int i = 0; i<resolutionToggles.Length; i++)
        {
            resolutionToggles[i].interactable = !isFullscreen;
        }

        if (isFullscreen)
        {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResultion = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxResultion.width, maxResultion.height, true);
        }
        else
        {
            SetScreenResolution(activeScreenResIndex);
        }

        PlayerPrefs.SetInt("fullscreen", (isFullscreen) ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
    }
}


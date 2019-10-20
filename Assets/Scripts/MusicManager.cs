using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string sceneName;


    // Start is called before the first frame update
    void Start()
    {
        //it should be run manually for the first time (starting scene)
        OnLevelWasLoaded(0);
    }


    private void OnLevelWasLoaded(int level)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        if(newSceneName != sceneName)
        {
            sceneName = newSceneName;
            Invoke("PlayMusic", 0.2f);
        }

    }

    //Play the level's music
    void PlayMusic()
    {
        AudioClip clipToPlay = null;

        if(sceneName == "Menu")
        {
            clipToPlay = menuTheme;
        } else if (sceneName == "Game")
        {
            clipToPlay = mainTheme;
        }

        if (clipToPlay != null)
        {
            AudioManager.instance.PlayMusic(clipToPlay, 2f);
            Invoke("PlayMusic", clipToPlay.length);
        }
    }
}

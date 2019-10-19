using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //because I have only one audio manager in this project, and I need to access it from all other classes
    // with this, make it a singleton
    public static AudioManager instance;
    SoundLibrary library;

    //these variables can be get globally, but can be set only locally
    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    AudioSource[] musicSources;
    AudioSource sfx2DSource;
    Transform audioListener;
    Transform playerT;

    int activeMusicSourceIndex;

    public enum AudioChannel { Master, Sfx, Music};

    /*
     * If this is the first scene, the AudioManager is instantiated and
     * if I go to next scene, it doesn't stop the playing music.
     * How? by checking if it already exists or not
     */
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSources = new AudioSource[2];
            library = GetComponent<SoundLibrary>();

            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            //this is a 2D Audio Source for playing Level Complete
            GameObject newSfx2Dsource = new GameObject("2D sfx source");
            sfx2DSource = newSfx2Dsource.AddComponent<AudioSource>();
            sfx2DSource.transform.parent = transform;


            audioListener = FindObjectOfType<AudioListener>().transform;

            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }

            //load the previous settings from preferrences
            masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1f);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1f);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", 1f);
        }
    }

    private void Update()
    {
        if(playerT != null)
            audioListener.position = playerT.position;
    }

    public void SetVolume(float volumePercent, AudioChannel channel) {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        //set the current musics
        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        //save the latest settings
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlaySound(AudioClip clip, Vector3 position) {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolumePercent * masterVolumePercent);
        }
    }

    public void PlaySound2D(string soundName) {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }

    public void PlaySound(string name, Vector3 position) {
        PlaySound(library.GetClipFromName(name), position);
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1f) {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }


    IEnumerator AnimateMusicCrossfade(float fadeDuration) {
        float percent = 0;
        
        while(percent < 1)
        {
            percent += Time.deltaTime * (1 / fadeDuration);
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 -activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //because I have only one audio manager in this project, and I need to access it from all other classes
    // with this, make it a singleton
    public static AudioManager instance;


    float masterVolumePercent = 1f;
    float sfxVolumePercent = 1f;
    float musicVolumePercent = 1f;

    AudioSource[] musicSources;
    Transform audioListener;
    Transform playerT;

    int activeMusicSourceIndex;

    private void Awake()
    {
        instance = this;

        musicSources = new AudioSource[2];

        for (int i = 0; i < 2; i++) {
            GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
            musicSources[i] = newMusicSource.AddComponent<AudioSource>();
            newMusicSource.transform.parent = transform;
        }

        audioListener = FindObjectOfType<AudioListener>().transform;
        playerT = FindObjectOfType<Player>().transform;
    }

    private void Update()
    {
        if(playerT != null)
            audioListener.position = playerT.position;
    }

    public void PlaySound(AudioClip clip, Vector3 position) {
        AudioSource.PlayClipAtPoint(clip, position, sfxVolumePercent * masterVolumePercent);
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

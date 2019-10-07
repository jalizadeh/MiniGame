using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject flashHolder;
    public SpriteRenderer[] renderers;
    public Sprite[] sprites;

    public float timeout = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        Activate();    
    }

    public void Activate() {
        flashHolder.SetActive(true);

        int rndSprite = Random.Range(0, sprites.Length);
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].sprite = sprites[rndSprite];
        }

        Invoke("Deactivate", timeout);
    }

    void Deactivate() {
        flashHolder.SetActive(false);
    }
}

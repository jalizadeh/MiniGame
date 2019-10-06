using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public Image fade;
    public GameObject gameOver;

    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }


    void OnGameOver() {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOver.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float second) {
        float speed = 1 / second;
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * speed;
            fade.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }


    public void StartNewGame() {
        SceneManager.LoadScene("SampleScene");
    }
}

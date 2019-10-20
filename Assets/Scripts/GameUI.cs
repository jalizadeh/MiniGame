using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public Image fade;

    [Header("Game Over UI")]
    public GameObject gameOver;
    public Text gameOverScore;

    [Header("New Wave UI")]
    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemiesCount;

    [Header("Health Bar & Score")]
    public Text score;
    public RectTransform healthBar;

    Player player;

    //to access each new wave
    Spawner spawner;



    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }


    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }


    private void Update()
    {
        score.text = ScoreKeeper.score.ToString("D6");

        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
    }


    public void OnNewWave(int waveNumber) {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";

        string enemyCount = ((spawner.waves[waveNumber - 1].infinite) ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount + "");
        /*
        if (spawner.waves[waveNumber - 1].infinite)
        {
            enemyCount = "Infinite";
        } else
        {
            spawner.waves[waveNumber - 1].enemyCount;
        }
        */
        newWaveEnemiesCount.text = "Enemies: " + enemyCount;

        StopCoroutine("AnimateBanner");
        StartCoroutine("AnimateBanner");
    }


    IEnumerator AnimateBanner() {
        float percent = 0;
        float speed = 2.5f;
        int dir = 1;
        float delayTime = 1f;
        

        while (percent >= 0) {
            percent += Time.deltaTime * (1 / speed) * dir;
            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-200, 40, percent);

            if (percent > 1) {
                dir = -1;
                yield return new WaitForSeconds(delayTime);
            }

            yield return null;
        }
    }


    void OnGameOver() {
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, 0.95f), 1));
        score.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
        gameOverScore.text = score.text;
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


    public void PlayAgain() {

        SceneManager.LoadScene("Game");
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}

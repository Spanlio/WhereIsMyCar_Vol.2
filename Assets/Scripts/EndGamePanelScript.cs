using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndGamePanelScript : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Text")]
    public TMP_Text titleText;
    public TMP_Text timeText;

    [Header("Stars")]
    public Image[] stars = new Image[3];
    public Sprite filledStar;
    public Sprite emptyStar;

    private const string GAME_SCENE = "CityScene";
    private const string MENU_SCENE = "StartScene";

    void Awake()
    {
        if (panel == null)
            panel = gameObject;

        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowWin(string formattedTime, int score)
    {
        if (titleText != null)
            titleText.text = "YOU WIN!";

        if (timeText != null)
            timeText.text = "Time: " + formattedTime;

        SetStars(score);
        Show();
    }

    public void ShowLose(string formattedTime)
    {
        if (titleText != null)
            titleText.text = "GAME OVER";

        if (timeText != null)
            timeText.text = "Time: " + formattedTime;

        SetStars(0);
        Show();
    }

    private void Show()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    private void SetStars(int score)
    {
        score = Mathf.Clamp(score, 0, 3);

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            bool earned = i < score;

            if (filledStar != null && emptyStar != null)
            {
                stars[i].sprite = earned
                    ? filledStar
                    : emptyStar;

                stars[i].enabled = true;
            }
            else
            {
                // If no star sprites are assigned, hide unearned stars.
                stars[i].enabled = earned;
            }
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GAME_SCENE);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MENU_SCENE);
    }
}

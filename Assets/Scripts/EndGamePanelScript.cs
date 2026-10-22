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
    public TMP_Text resultMessageText;

    [Header("Stars")]
    public Image[] stars = new Image[3];
    public Sprite filledStar;
    public Sprite emptyStar;

    private const string GAME_SCENE = "CityScene";
    private const string MENU_SCENE = "StartScene";

    void Awake()
    {
        // No Hide() here: the panel starts inactive, so Awake only runs when
        // Show() activates it - hiding here would close it immediately.
        // GameObjectsScript.Start() hides the panel at the start of the game.
        if (panel == null)
            panel = gameObject;
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowWin(string formattedTime, int score)
    {
        if (titleText != null)
            titleText.text = "YOU WIN!";

        if (timeText != null)
            timeText.text = "Time: " + formattedTime;

        if (resultMessageText != null)
            resultMessageText.text = "Nice work! The cars survived the chaos.";

        SetStars(score);
        Show();
    }

    public void ShowLose(string formattedTime)
    {
        if (titleText != null)
            titleText.text = "GAME OVER";

        if (timeText != null)
            timeText.text = "Time: " + formattedTime;

        if (resultMessageText != null)
            resultMessageText.text = "You ran out of hearts.";

        SetStars(0);
        Show();
    }

    public void ShowDNF(string formattedTime)
    {
        if (titleText != null)
            titleText.text = "DNF";

        if (timeText != null)
            timeText.text = "Time: " + formattedTime;

        if (resultMessageText != null)
        {
            resultMessageText.text =
                "4 cars down. At this point, the cars are trying to escape you.";
        }

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
                stars[i].sprite = earned ? filledStar : emptyStar;
                stars[i].enabled = true;
            }
            else
            {
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

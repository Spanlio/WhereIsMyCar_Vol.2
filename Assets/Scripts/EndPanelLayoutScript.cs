using UnityEngine;

// Lays out the end game panel differently for landscape and portrait:
// landscape = wide panel with the buttons side by side,
// portrait  = tall panel with the buttons stacked.
// FitToScreenScript on the same object then shrinks it to fit if needed.
public class EndPanelLayoutScript : MonoBehaviour
{
    public RectTransform title;
    public RectTransform time;
    public RectTransform stars;
    public RectTransform message;
    public RectTransform playAgainButton;
    public RectTransform exitButton;

    private RectTransform panel;
    private RectTransform parentRect;
    private bool? lastWasPortrait;

    void OnEnable()
    {
        panel = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        lastWasPortrait = null;
        ApplyLayout();
    }

    void Update()
    {
        ApplyLayout();
    }

    void ApplyLayout()
    {
        if (parentRect == null)
            return;

        Vector2 screen = parentRect.rect.size;
        bool portrait = screen.y > screen.x;

        if (lastWasPortrait == portrait)
            return;

        lastWasPortrait = portrait;

        if (portrait)
        {
            panel.sizeDelta = new Vector2(800f, 1150f);
            Place(title, 0f, 440f, 720f, 120f);
            Place(time, 0f, 330f, 600f, 60f);
            Place(stars, 0f, 170f);
            Place(message, 0f, -10f, 700f, 140f);
            Place(playAgainButton, 0f, -230f, 520f, 110f);
            Place(exitButton, 0f, -380f, 520f, 110f);
        }
        else
        {
            panel.sizeDelta = new Vector2(1100f, 700f);
            Place(title, 0f, 245f, 900f, 110f);
            Place(time, 0f, 160f, 600f, 60f);
            Place(stars, 0f, 40f);
            Place(message, 0f, -95f, 950f, 70f);
            Place(playAgainButton, -170f, -230f, 300f, 90f);
            Place(exitButton, 170f, -230f, 300f, 90f);
        }

        FitToScreenScript fit = GetComponent<FitToScreenScript>();
        if (fit != null)
            fit.Fit();
    }

    static void Place(RectTransform rect, float x, float y)
    {
        if (rect != null)
            rect.anchoredPosition = new Vector2(x, y);
    }

    static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        if (rect == null)
            return;

        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }
}

using UnityEngine;

// Scales a fixed-size UI panel down (never up) so it always fits on screen,
// in portrait and landscape. Put it on a panel inside a Screen Space canvas.
public class FitToScreenScript : MonoBehaviour
{
    [Tooltip("Largest part of the screen width/height the panel may use.")]
    [Range(0.5f, 1f)]
    public float maxScreenFraction = 0.92f;

    private RectTransform rect;
    private RectTransform parentRect;
    private Vector2 lastParentSize;
    private Vector2 lastSize;

    void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        Fit();
    }

    void Update()
    {
        if (parentRect == null)
            return;

        // Re-fit when the phone is rotated or the panel's own size changes.
        if (parentRect.rect.size != lastParentSize || rect.rect.size != lastSize)
            Fit();
    }

    public void Fit()
    {
        if (rect == null || parentRect == null)
            return;

        lastParentSize = parentRect.rect.size;
        lastSize = rect.rect.size;

        if (lastSize.x <= 0f || lastSize.y <= 0f)
            return;

        Vector2 available = lastParentSize * maxScreenFraction;
        float scale = Mathf.Min(1f, available.x / lastSize.x, available.y / lastSize.y);
        rect.localScale = new Vector3(scale, scale, 1f);
    }
}

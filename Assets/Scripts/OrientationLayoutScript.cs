using UnityEngine;

// Moves/resizes UI elements between a landscape and a portrait layout
// when the screen orientation changes. Both layouts are set in the Inspector.
public class OrientationLayoutScript : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public RectTransform target;
        public Vector2 landscapePosition;
        public Vector2 landscapeSize;
        public Vector2 portraitPosition;
        public Vector2 portraitSize;
    }

    public Entry[] entries;

    private RectTransform canvasRect;
    private bool? lastWasPortrait;

    void OnEnable()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRect = canvas.rootCanvas.transform as RectTransform;

        lastWasPortrait = null;
        ApplyLayout();
    }

    void Update()
    {
        ApplyLayout();
    }

    void ApplyLayout()
    {
        if (canvasRect == null || entries == null)
            return;

        Vector2 screen = canvasRect.rect.size;
        bool portrait = screen.y > screen.x;

        if (lastWasPortrait == portrait)
            return;

        lastWasPortrait = portrait;

        foreach (Entry entry in entries)
        {
            if (entry.target == null)
                continue;

            entry.target.anchoredPosition = portrait ? entry.portraitPosition : entry.landscapePosition;
            entry.target.sizeDelta = portrait ? entry.portraitSize : entry.landscapeSize;
        }
    }
}

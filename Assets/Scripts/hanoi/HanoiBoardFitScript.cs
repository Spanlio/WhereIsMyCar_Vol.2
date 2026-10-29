using UnityEngine;

// The board is laid out in the art's own pixels (1280x600), so disk and pole
// positions match the picture. This scales it to fit its parent, keeping the aspect.
[ExecuteAlways]
public class HanoiBoardFitScript : MonoBehaviour
{
    void Update()
    {
        Fit();
    }

    public void Fit()
    {
        RectTransform rect = (RectTransform)transform;
        RectTransform parent = rect.parent as RectTransform;
        if (parent == null)
            return;

        Vector2 area = parent.rect.size;
        Vector2 size = rect.sizeDelta;
        if (size.x <= 0f || size.y <= 0f)
            return;

        float scale = Mathf.Min(area.x / size.x, area.y / size.y);
        rect.localScale = new Vector3(scale, scale, 1f);
    }
}

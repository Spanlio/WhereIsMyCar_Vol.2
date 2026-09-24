using System.Collections;
using UnityEngine;

public class BombBoomerangScript : MonoBehaviour
{
    [Header("Boomerang Settings")]
    public float diveDistance = 180f;
    public float diveDuration = 0.45f;
    public float returnDuration = 0.55f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("BombBoomerangScript requires a RectTransform.");
            return;
        }

        startPosition = rectTransform.anchoredPosition;

        StartCoroutine(Boomerang());
    }

    private IEnumerator Boomerang()
    {
        // --------------------------------
        // DIVE DOWN
        // --------------------------------

        Vector2 divePosition =
            startPosition + Vector2.down * diveDistance;

        float time = 0f;

        while (time < diveDuration)
        {
            time += Time.deltaTime;

            float t = time / diveDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    divePosition,
                    t
                );

            yield return null;
        }

        rectTransform.anchoredPosition = divePosition;


        // --------------------------------
        // RETURN
        // --------------------------------

        time = 0f;

        while (time < returnDuration)
        {
            time += Time.deltaTime;

            float t = time / returnDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(
                    divePosition,
                    startPosition,
                    t
                );

            yield return null;
        }

        rectTransform.anchoredPosition = startPosition;
    }
}
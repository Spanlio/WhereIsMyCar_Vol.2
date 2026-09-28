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
    private GameObjectsScript gameObjectsScript;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();

        if (rectTransform == null)
        {
            Debug.LogError("BombBoomerangScript requires a RectTransform.");
            return;
        }

        startPosition = rectTransform.anchoredPosition;

        if (gameObjectsScript == null || !gameObjectsScript.GameFinished)
            StartCoroutine(Boomerang());
    }

    private IEnumerator Boomerang()
    {
        Vector2 divePosition = startPosition + Vector2.down * diveDistance;
        float time = 0f;

        while (time < diveDuration)
        {
            if (gameObjectsScript != null && gameObjectsScript.GameFinished)
                yield break;

            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / diveDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, divePosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = divePosition;
        time = 0f;

        while (time < returnDuration)
        {
            if (gameObjectsScript != null && gameObjectsScript.GameFinished)
                yield break;

            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / returnDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(divePosition, startPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = startPosition;
    }
}

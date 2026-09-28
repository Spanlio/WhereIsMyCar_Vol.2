using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyingObjectControllerScript : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;

    public float fadeDuration = 1.5f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;

    private GameObjectsScript gameObjectsScript;
    private ScreenBoundariesScript screenBoundariesScript;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Image image;

    private Color originalColor;

    private bool isFadingOut = false;
    private bool isExploading = false;

    // Prevents one bomb from taking multiple lives.
    private bool hasHitDraggedCar = false;

    void Start()
    {
        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform =
            GetComponent<RectTransform>();

        image =
            GetComponent<Image>();

        if (image != null)
            originalColor =
                image.color;

        gameObjectsScript =
            Object.FindFirstObjectByType<GameObjectsScript>();

        screenBoundariesScript =
            Object.FindFirstObjectByType<ScreenBoundariesScript>();

        StartCoroutine(FadeIn());
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (gameObjectsScript == null)
            return;

        if (gameObjectsScript.GameFinished)
            return;

        // =====================================================
        // MOVEMENT
        // =====================================================

        float waveOffset =
            Mathf.Sin(
                Time.time * waveFrequency
            ) * waveAmplitude;

        rectTransform.anchoredPosition +=
            new Vector2(
                -speed * Time.deltaTime,
                waveOffset * Time.deltaTime
            );

        // =====================================================
        // SCREEN BOUNDARIES
        // =====================================================

        if (speed > 0 &&
            transform.position.x <
            screenBoundariesScript.minX + 80 &&
            !isFadingOut)
        {
            isFadingOut = true;

            StartCoroutine(
                FadeOutAndDestroy()
            );

            return;
        }

        if (speed < 0 &&
            transform.position.x >
            screenBoundariesScript.maxX - 80 &&
            !isFadingOut)
        {
            isFadingOut = true;

            StartCoroutine(
                FadeOutAndDestroy()
            );

            return;
        }

        // =====================================================
        // BOMB
        // =====================================================

        if (CompareTag("Bomb"))
        {
            // If a car is being dragged,
            // ONLY actual car/bomb overlap matters.
            if (GameObjectsScript.isDragging &&
                GameObjectsScript.lastDragged != null)
            {
                CheckBombVsDraggedCar();
            }
            else
            {
                // Nobody is dragging.
                // Keep the old cursor bomb interaction.
                CheckBombVsCursor();
            }
        }
    }

    // =========================================================
    // CURSOR VS BOMB
    // =========================================================

    void CheckBombVsCursor()
    {
        if (isExploading ||
            isFadingOut)
            return;

        if (RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                Camera.main))
        {
            Debug.Log(
                "Cursor collided with bomb."
            );

            TriggerExplosion();
        }
    }

    // =========================================================
    // BOMB VS DRAGGED CAR
    // =========================================================

    void CheckBombVsDraggedCar()
    {
        if (hasHitDraggedCar)
            return;

        if (isFadingOut ||
            isExploading)
            return;

        GameObject car =
            GameObjectsScript.lastDragged;

        if (car == null)
            return;

        if (!car.activeSelf)
            return;

        // Already correctly placed cars are safe.
        if (gameObjectsScript.IsCarCorrectlyPlaced(car))
            return;

        RectTransform carRect =
            car.GetComponent<RectTransform>();

        if (carRect == null)
            return;

        // =====================================================
        // CONVERT BOTH OBJECTS TO SCREEN SPACE
        // =====================================================

        Vector2 bombPosition =
            RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                rectTransform.position
            );

        Vector2 carPosition =
            RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                carRect.position
            );

        // =====================================================
        // BOMB SIZE
        // =====================================================

        float bombWidth =
            rectTransform.rect.width *
            rectTransform.lossyScale.x;

        float bombHeight =
            rectTransform.rect.height *
            rectTransform.lossyScale.y;

        // =====================================================
        // CAR SIZE
        // =====================================================

        float carWidth =
            carRect.rect.width *
            carRect.lossyScale.x;

        float carHeight =
            carRect.rect.height *
            carRect.lossyScale.y;

        // =====================================================
        // SCREEN RECTANGLES
        // =====================================================

        Rect bombRect =
            new Rect(
                bombPosition.x -
                    bombWidth * 0.5f,

                bombPosition.y -
                    bombHeight * 0.5f,

                bombWidth,
                bombHeight
            );

        Rect carRectScreen =
            new Rect(
                carPosition.x -
                    carWidth * 0.5f,

                carPosition.y -
                    carHeight * 0.5f,

                carWidth,
                carHeight
            );

        // =====================================================
        // ACTUAL OVERLAP
        // =====================================================

        if (bombRect.Overlaps(
                carRectScreen,
                true))
        {
            HitDraggedCar(car);
        }
    }

    // =========================================================
    // BOMB HIT CAR
    // =========================================================

    void HitDraggedCar(GameObject car)
    {
        if (hasHitDraggedCar)
            return;

        hasHitDraggedCar = true;

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "BOMB HIT DRAGGED CAR: " +
            car.name
        );

        Debug.Log(
            "================================"
        );

        // =====================================================
        // LOSE ONE LIFE
        // =====================================================

        gameObjectsScript.LoseLife();

        // =====================================================
        // STOP DRAGGING
        // =====================================================

        GameObjectsScript.lastDragged = null;
        GameObjectsScript.isDragging = false;

        // =====================================================
        // RESTORE CAR RAYCAST
        // =====================================================

        CanvasGroup carCanvasGroup =
            car.GetComponent<CanvasGroup>();

        if (carCanvasGroup != null)
        {
            carCanvasGroup.alpha = 1f;
            carCanvasGroup.blocksRaycasts = true;
        }

        // =====================================================
        // REMOVE BOMB
        // =====================================================

        StartAndDestroy(Color.red);
    }

    // =========================================================
    // BOMB EXPLOSION
    // =========================================================

    public void TriggerExplosion()
    {
        if (isExploading)
            return;

        isExploading = true;

        // =====================================================
        // SOUND
        // =====================================================

        if (gameObjectsScript != null &&
            gameObjectsScript.carSoundSource != null &&
            gameObjectsScript.sounds != null &&
            gameObjectsScript.sounds.Length > 6 &&
            gameObjectsScript.sounds[6] != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(
                gameObjectsScript.sounds[6],
                5f
            );
        }

        // =====================================================
        // ANIMATION
        // =====================================================

        Animator animator =
            GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool(
                "explode",
                true
            );
        }

        // =====================================================
        // COLOR
        // =====================================================

        if (image != null)
        {
            image.color =
                Color.red;

            StartCoroutine(
                RecoverColor(0.6f)
            );
        }

        StartCoroutine(Vibrate());

        StartCoroutine(
            WaitBeforeExplode()
        );
    }

    // =========================================================
    // EXPLOSION WAIT
    // =========================================================

    IEnumerator WaitBeforeExplode()
    {
        float radius = 0f;

        CircleCollider2D circle =
            GetComponent<CircleCollider2D>();

        if (circle != null)
        {
            radius =
                circle.radius *
                transform.localScale.x;
        }

        ExplodeAndDestroyNearbyObjects(
            radius
        );

        yield return new WaitForSeconds(1f);

        ExplodeAndDestroyNearbyObjects(
            radius
        );

        Destroy(gameObject);
    }

    // =========================================================
    // EXPLODE NEARBY FLYING OBJECTS
    // =========================================================

    void ExplodeAndDestroyNearbyObjects(
        float radius)
    {
        Collider2D[] hit =
            Physics2D.OverlapCircleAll(
                transform.position,
                radius
            );

        foreach (Collider2D collider in hit)
        {
            if (collider == null)
                continue;

            if (collider.gameObject == gameObject)
                continue;

            FlyingObjectControllerScript flyingObject =
                collider.GetComponent<
                    FlyingObjectControllerScript
                >();

            if (flyingObject != null &&
                !flyingObject.isExploading)
            {
                flyingObject.StartAndDestroy(
                    Color.cyan
                );
            }
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    public void StartAndDestroy(
        Color color)
    {
        if (isFadingOut)
            return;

        isFadingOut = true;

        if (image != null)
        {
            image.color =
                color;

            StartCoroutine(
                RecoverColor(0.5f)
            );
        }

        StartCoroutine(Vibrate());

        if (gameObjectsScript != null &&
            gameObjectsScript.carSoundSource != null &&
            gameObjectsScript.sounds != null &&
            gameObjectsScript.sounds.Length > 5 &&
            gameObjectsScript.sounds[5] != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(
                gameObjectsScript.sounds[5]
            );
        }

        StartCoroutine(
            FadeOutAndDestroy()
        );
    }

    // =========================================================
    // FADE IN
    // =========================================================

    IEnumerator FadeIn()
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            canvasGroup.alpha =
                Mathf.Lerp(
                    0f,
                    1f,
                    time / fadeDuration
                );

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    // =========================================================
    // FADE OUT
    // =========================================================

    IEnumerator FadeOutAndDestroy()
    {
        float time = 0f;

        float startAlpha =
            canvasGroup.alpha;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    time / fadeDuration
                );

            yield return null;
        }

        canvasGroup.alpha = 0f;

        Destroy(gameObject);
    }

    // =========================================================
    // COLOR RECOVERY
    // =========================================================

    IEnumerator RecoverColor(
        float seconds)
    {
        yield return new WaitForSeconds(
            seconds
        );

        if (image != null)
            image.color =
                originalColor;
    }

    // =========================================================
    // VIBRATION
    // =========================================================

    IEnumerator Vibrate()
    {
        Vector2 originalPosition =
            rectTransform.anchoredPosition;

        float duration = 0.4f;
        float elapsed = 0f;
        float intensity = 5f;

        while (elapsed < duration)
        {
            rectTransform.anchoredPosition =
                originalPosition +
                Random.insideUnitCircle *
                intensity;

            elapsed +=
                Time.deltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition =
            originalPosition;
    }
}


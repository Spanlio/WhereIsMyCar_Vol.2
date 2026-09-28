using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyingObjectControllerScript : MonoBehaviour
{
    [HideInInspector] public float speed = 1f;

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
    private bool hasHitDraggedCar = false;

    public bool IsExploding => isExploading;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image != null)
            originalColor = image.color;

        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();

        if (gameObjectsScript != null && !gameObjectsScript.GameFinished)
            StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (gameObjectsScript == null || gameObjectsScript.GameFinished)
            return;

        if (screenBoundariesScript == null || rectTransform == null)
            return;

        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;

        rectTransform.anchoredPosition +=
            new Vector2(
                -speed * Time.deltaTime,
                waveOffset * Time.deltaTime
            );

        // =====================================================
        // SCREEN EDGES
        // =====================================================

        if (speed > 0 &&
            transform.position.x < screenBoundariesScript.minX + 80 &&
            !isFadingOut)
        {
            isFadingOut = true;
            StartCoroutine(FadeOutAndDestroy());
            return;
        }

        if (speed < 0 &&
            transform.position.x > screenBoundariesScript.maxX - 80 &&
            !isFadingOut)
        {
            isFadingOut = true;
            StartCoroutine(FadeOutAndDestroy());
            return;
        }

        // =====================================================
        // BOMB + CURSOR
        // =====================================================
        // Bombs can still be detonated by the cursor when NO car
        // is being dragged. While dragging, bomb damage is handled
        // by CheckBombVsDraggedCar() instead.

        if (CompareTag("Bomb"))
        {
            if (GameObjectsScript.isDragging &&
                GameObjectsScript.lastDragged != null)
            {
                CheckBombVsDraggedCar();
            }
            else
            {
                CheckBombVsCursor();
            }
        }

        // =====================================================
        // CLOUD / AIRPLANE + CURSOR
        // =====================================================
        // This is the old behaviour that allows the cursor to
        // crash/delete a cloud or airplane while dragging a car.
        // Bombs are intentionally excluded from this block.

        if (!CompareTag("Bomb") &&
            GameObjectsScript.isDragging &&
            GameObjectsScript.lastDragged != null &&
            !isFadingOut &&
            RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                Camera.main))
        {
            Debug.Log("Cursor hit flying object: " + gameObject.name);

            GameObject car = GameObjectsScript.lastDragged;

            // Currently crashing into a cloud/airplane destroys
            // the car being dragged, matching the previous game
            // behaviour. We can later turn this into a score/win
            // condition without changing the collision itself.
            StartCoroutine(
                ShrinkAndDestroy(
                    car,
                    0.5f
                )
            );

            GameObjectsScript.lastDragged = null;
            GameObjectsScript.isDragging = false;

            StartAndDestroy(Color.cyan);
        }

        // =====================================================
        // PHYSICAL CAR COLLISION
        // =====================================================

        CheckCarCollision();
    }

    // =========================================================
    // BOMB VS CURSOR
    // =========================================================

    void CheckBombVsCursor()
    {
        if (isExploading || isFadingOut)
            return;

        if (RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                Camera.main))
        {
            Debug.Log("Cursor collided with bomb.");
            TriggerExplosion();
        }
    }

    // =========================================================
    // BOMB VS DRAGGED CAR
    // =========================================================

    void CheckBombVsDraggedCar()
    {
        if (hasHitDraggedCar || isFadingOut || isExploading)
            return;

        GameObject car = GameObjectsScript.lastDragged;

        if (car == null || !car.activeSelf)
            return;

        if (gameObjectsScript.IsCarCorrectlyPlaced(car))
            return;

        RectTransform carRect =
            car.GetComponent<RectTransform>();

        if (carRect == null)
            return;

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

        float bombWidth =
            rectTransform.rect.width *
            rectTransform.lossyScale.x;

        float bombHeight =
            rectTransform.rect.height *
            rectTransform.lossyScale.y;

        float carWidth =
            carRect.rect.width *
            carRect.lossyScale.x;

        float carHeight =
            carRect.rect.height *
            carRect.lossyScale.y;

        Rect bombRect = new Rect(
            bombPosition.x - bombWidth * 0.5f,
            bombPosition.y - bombHeight * 0.5f,
            bombWidth,
            bombHeight
        );

        Rect carScreenRect = new Rect(
            carPosition.x - carWidth * 0.5f,
            carPosition.y - carHeight * 0.5f,
            carWidth,
            carHeight
        );

        if (bombRect.Overlaps(carScreenRect, true))
            HitDraggedCar(car);
    }

    // =========================================================
    // BOMB HIT CAR
    // =========================================================

    void HitDraggedCar(GameObject car)
    {
        if (hasHitDraggedCar)
            return;

        hasHitDraggedCar = true;

        Debug.Log("BOMB HIT DRAGGED CAR: " + car.name);

        gameObjectsScript.LoseLife();

        GameObjectsScript.lastDragged = null;
        GameObjectsScript.isDragging = false;

        CanvasGroup carCanvasGroup =
            car.GetComponent<CanvasGroup>();

        if (carCanvasGroup != null)
        {
            carCanvasGroup.alpha = 1f;
            carCanvasGroup.blocksRaycasts = true;
        }

        StartAndDestroy(Color.red);
    }

    // =========================================================
    // CLOUD / AIRPLANE PHYSICAL COLLISION
    // =========================================================
    // Keeps the previous life-loss collision if the actual flying
    // object overlaps the dragged car. Cursor collision above is
    // separate from this.

    void CheckCarCollision()
    {
        if (gameObjectsScript == null)
            return;

        if (isFadingOut)
            return;

        if (!GameObjectsScript.isDragging)
            return;

        GameObject car = GameObjectsScript.lastDragged;

        if (car == null || !car.activeSelf)
            return;

        if (gameObjectsScript.IsCarCorrectlyPlaced(car))
            return;

        RectTransform carRect =
            car.GetComponent<RectTransform>();

        if (carRect == null)
            return;

        Vector2 flyingObjectScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                rectTransform.position
            );

        if (RectTransformUtility.RectangleContainsScreenPoint(
            carRect,
            flyingObjectScreenPosition,
            Camera.main))
        {
            Debug.Log(
                "Flying object hit dragged car: " +
                car.name
            );

            gameObjectsScript.LoseLife();

            GameObjectsScript.lastDragged = null;
            GameObjectsScript.isDragging = false;

            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }
    }

    // =========================================================
    // SHRINK + DESTROY CAR
    // =========================================================

    IEnumerator ShrinkAndDestroy(
        GameObject obj,
        float duration)
    {
        if (obj == null)
            yield break;

        Vector3 originalScale =
            obj.transform.localScale;

        float time = 0f;

        while (time < duration)
        {
            if (gameObjectsScript != null &&
                gameObjectsScript.GameFinished)
                yield break;

            if (obj == null)
                yield break;

            time += Time.deltaTime;

            obj.transform.localScale =
                Vector3.Lerp(
                    originalScale,
                    Vector3.zero,
                    time / duration
                );

            float angle =
                Mathf.Lerp(
                    0f,
                    360f,
                    time / duration
                );

            obj.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );

            yield return null;
        }

        if (obj != null)
            Destroy(obj);
    }

    // =========================================================
    // BOMB EXPLOSION
    // =========================================================

    public void TriggerExplosion()
    {
        if (isExploading ||
            isFadingOut ||
            gameObjectsScript == null ||
            gameObjectsScript.GameFinished)
            return;

        isExploading = true;

        if (gameObjectsScript.carSoundSource != null &&
            gameObjectsScript.sounds != null &&
            gameObjectsScript.sounds.Length > 6 &&
            gameObjectsScript.sounds[6] != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(
                gameObjectsScript.sounds[6],
                5f
            );
        }

        Animator animator =
            GetComponent<Animator>();

        if (animator != null)
            animator.SetBool("explode", true);

        if (image != null)
        {
            image.color = Color.red;

            StartCoroutine(
                RecoverColor(0.6f)
            );
        }

        StartCoroutine(Vibrate());
        StartCoroutine(WaitBeforeExplode());
    }

    // =========================================================
    // EXPLOSION
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

        ExplodeAndDestroyNearbyObjects(radius);

        yield return new WaitForSeconds(1f);

        if (gameObjectsScript != null &&
            gameObjectsScript.GameFinished)
            yield break;

        ExplodeAndDestroyNearbyObjects(radius);

        Destroy(gameObject);
    }

    void ExplodeAndDestroyNearbyObjects(
        float radius)
    {
        if (gameObjectsScript != null &&
            gameObjectsScript.GameFinished)
            return;

        Collider2D[] hit =
            Physics2D.OverlapCircleAll(
                transform.position,
                radius
            );

        foreach (Collider2D collider in hit)
        {
            if (collider == null ||
                collider.gameObject == gameObject)
                continue;

            FlyingObjectControllerScript flyingObject =
                collider.GetComponent<
                    FlyingObjectControllerScript
                >();

            if (flyingObject != null &&
                !flyingObject.IsExploding)
            {
                flyingObject.StartAndDestroy(
                    Color.cyan
                );
            }
        }
    }

    // =========================================================
    // START AND DESTROY FLYING OBJECT
    // =========================================================

    public void StartAndDestroy(Color color)
    {
        if (isFadingOut ||
            (gameObjectsScript != null &&
             gameObjectsScript.GameFinished))
            return;

        isFadingOut = true;

        if (image != null)
        {
            image.color = color;

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

        StartCoroutine(FadeOutAndDestroy());
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
        float startAlpha = canvasGroup.alpha;

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

    IEnumerator RecoverColor(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (image != null)
            image.color = originalColor;
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
            if (gameObjectsScript != null &&
                gameObjectsScript.GameFinished)
                yield break;

            rectTransform.anchoredPosition =
                originalPosition +
                Random.insideUnitCircle * intensity;

            elapsed += Time.deltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition =
            originalPosition;
    }
}

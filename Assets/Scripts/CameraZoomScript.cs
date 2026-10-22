using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraZoomScript : MonoBehaviour
{
    [Header("Zoom")]
    public float maxZoom = 530f;
    public float minZoom = 150f;
    public float pinchZoomSpeed = 0.9f;
    public float mouseScrollSpeed = 150f;

    [Header("Pan")]
    public float mouseFollowSpeed = 1f;
    public float touchPanSpeed = 1f;

    [Header("Double tap zoom")]
    public float doubleTapMaxDelay = 0.4f;
    public float doubleTapMaxDistance = 100f;
    [Tooltip("Zoomed-in size as a fraction of the fully zoomed-out size.")]
    [Range(0.1f, 1f)]
    public float doubleTapZoomFactor = 0.5f;
    public float doubleTapZoomDuration = 0.25f;
    // A touch only counts as a tap if it is short and barely moves (otherwise it is a pan).
    public float tapMaxDuration = 0.3f;
    public float tapMaxMove = 30f;

    [Header("References")]
    public ScreenBoundariesScript screenBoundariesScript;

    public Camera mainCamera;

    Vector2 lastTouchPos;
    int panFingerId = -1;
    bool isTouchPan = false;

    bool tapCandidate = false;
    float tapStartTime;
    Vector2 tapStartPos;
    float lastTapTime = -10f;
    Vector2 lastTapPos;

    Coroutine zoomRoutine;

    GameObjectsScript gameObjectsScript;
    bool endScreenZoomStarted = false;

    void Awake()
    {
        gameObjectsScript = FindFirstObjectByType<GameObjectsScript>();

        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
        }

        if (screenBoundariesScript == null)
        {
            screenBoundariesScript = FindFirstObjectByType<ScreenBoundariesScript>();
        }
    }

    private void Start()
    {
        if (screenBoundariesScript == null)
            return;

        screenBoundariesScript.RecalculateBounds();
        transform.position = screenBoundariesScript.GetClampedCameraPosition(transform.position);
    }

    void Update()
    {
        if (screenBoundariesScript == null)
            return;

        // Game over: zoom out to the whole map so the end panel (in the middle
        // of the map) is fully visible, and stop following the cursor/fingers.
        if (gameObjectsScript != null && gameObjectsScript.GameFinished)
        {
            if (!endScreenZoomStarted)
            {
                endScreenZoomStarted = true;
                UpdateMaxZoom();
                Vector2 center = screenBoundariesScript.worldBounds.center;
                StopZoomRoutine();
                zoomRoutine = StartCoroutine(ZoomSmooth(maxZoom,
                    new Vector3(center.x, center.y, transform.position.z)));
            }
            return;
        }

        if (ObjectTransformationScript.isTransforming)
        {
            ResetTouchState();
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        DesktopFollowCursor();
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if ((Mathf.Abs(scroll) > Mathf.Epsilon))
        {
            StopZoomRoutine();
            mainCamera.orthographicSize -= scroll * mouseScrollSpeed;
        }

        // Double click = same as double tap, so it can be tested in the editor.
        if (Input.touchCount == 0 && Input.GetMouseButtonDown(0) &&
            !GameObjectsScript.isDragging && !IsOverUIOrCar(Input.mousePosition))
        {
            RegisterTap(Input.mousePosition);
        }
#else
        HandleTouch();
#endif
        if (Input.touchCount == 2)
        {
            StopZoomRoutine();
            HandlePinch();
        }

        UpdateMaxZoom();
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        screenBoundariesScript.RecalculateBounds();
        transform.position = screenBoundariesScript.GetClampedCameraPosition(transform.position);
    }

    void DesktopFollowCursor()
    {
        FollowScreenPoint(Input.mousePosition);
    }

    void FollowScreenPoint(Vector2 pointerPos)
    {
        if (pointerPos.x < 0 || pointerPos.x > Screen.width ||
           pointerPos.y < 0 || pointerPos.y > Screen.height)
        {
            return;
        }

        Vector3 screenPoint = new Vector3(pointerPos.x, pointerPos.y, mainCamera.nearClipPlane);
        Vector3 targetWorld = mainCamera.ScreenToWorldPoint(screenPoint);
        Vector3 targetPosition = new Vector3(targetWorld.x, targetWorld.y, transform.position.z);

        transform.position =
            Vector3.Lerp(transform.position, targetPosition, mouseFollowSpeed * Time.unscaledDeltaTime);
    }

    void HandleTouch()
    {
        // Pinch (2 fingers) or nothing: forget the pan, so the finger that stays
        // after a pinch starts a fresh pan instead of jumping or being ignored.
        if (Input.touchCount != 1)
        {
            ResetTouchState();
            return;
        }

        Touch touch = Input.GetTouch(0);

        // While a car is dragged, the camera follows the finger (like the cursor on desktop)
        // so the car can be carried across the map on a portrait screen.
        if (GameObjectsScript.isDragging)
        {
            ResetTouchState();
            FollowScreenPoint(touch.position);
            return;
        }

        if (touch.phase == TouchPhase.Began)
        {
            bool onUI = IsOverUIOrCar(touch.position);

            panFingerId = touch.fingerId;
            isTouchPan = !onUI;
            lastTouchPos = touch.position;

            tapCandidate = !onUI;
            tapStartTime = Time.unscaledTime;
            tapStartPos = touch.position;
            return;
        }

        // A finger we never saw begin (left over from a pinch, or a car drag
        // that ended): take it over as a pan from where it is now.
        if (touch.fingerId != panFingerId)
        {
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                panFingerId = touch.fingerId;
                isTouchPan = true;
                lastTouchPos = touch.position;
                tapCandidate = false;
            }
            return;
        }

        if (touch.phase == TouchPhase.Moved)
        {
            if (Vector2.Distance(touch.position, tapStartPos) > tapMaxMove)
                tapCandidate = false;

            if (isTouchPan)
            {
                StopZoomRoutine();
                // Move the camera opposite to the finger, so the map sticks to the finger.
                Vector2 delta = touch.position - lastTouchPos;
                transform.Translate(-ScreenDeltaToWorldDelta(delta) * touchPanSpeed, Space.World);
            }
            lastTouchPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            if (tapCandidate &&
                Time.unscaledTime - tapStartTime <= tapMaxDuration &&
                Vector2.Distance(touch.position, tapStartPos) <= tapMaxMove)
            {
                RegisterTap(touch.position);
            }
            ResetTouchState();
        }
        else if (touch.phase == TouchPhase.Canceled)
        {
            ResetTouchState();
        }
    }

    void ResetTouchState()
    {
        isTouchPan = false;
        panFingerId = -1;
        tapCandidate = false;
    }

    void RegisterTap(Vector2 screenPos)
    {
        if (Time.unscaledTime - lastTapTime <= doubleTapMaxDelay &&
            Vector2.Distance(screenPos, lastTapPos) <= doubleTapMaxDistance)
        {
            lastTapTime = -10f;
            ToggleZoom(screenPos);
        }
        else
        {
            lastTapTime = Time.unscaledTime;
            lastTapPos = screenPos;
        }
    }

    // Double tap: zoomed out -> zoom in on the tapped point; zoomed in -> zoom back out.
    void ToggleZoom(Vector2 screenPos)
    {
        UpdateMaxZoom();
        float zoomedIn = Mathf.Max(minZoom, maxZoom * doubleTapZoomFactor);
        bool isZoomedOut = mainCamera.orthographicSize > (zoomedIn + maxZoom) * 0.5f;

        Vector3 target = transform.position;
        float targetZoom = maxZoom;

        if (isZoomedOut)
        {
            Vector3 world = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane));
            target = new Vector3(world.x, world.y, transform.position.z);
            targetZoom = zoomedIn;
        }

        StopZoomRoutine();
        zoomRoutine = StartCoroutine(ZoomSmooth(targetZoom, target));
    }

    void StopZoomRoutine()
    {
        if (zoomRoutine != null)
        {
            StopCoroutine(zoomRoutine);
            zoomRoutine = null;
        }
    }

    void HandlePinch()
    {
        Touch touch0 = Input.GetTouch(0),
            touch1 = Input.GetTouch(1);
        float prevDistance =
        ((touch0.position - touch0.deltaPosition) -
        (touch1.position - touch1.deltaPosition)).magnitude;
        float currentDistnace = (touch0.position - touch1.position).magnitude;
        mainCamera.orthographicSize -= (currentDistnace - prevDistance) * pinchZoomSpeed;
    }

    List<RaycastResult> RaycastUI(Vector2 screenPosition)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        if (EventSystem.current == null)
            return results;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;
        EventSystem.current.RaycastAll(pointerData, results);
        return results;
    }

    // Buttons or a car: a tap there must not count towards a double tap zoom.
    bool IsOverUIOrCar(Vector2 screenPosition)
    {
        foreach (RaycastResult result in RaycastUI(screenPosition))
        {
            if (result.gameObject.GetComponentInParent<Selectable>() != null ||
                result.gameObject.GetComponentInParent<EventTrigger>() != null ||
                result.gameObject.GetComponentInParent<DragAndDropScript>() != null)
            {
                return true;
            }
        }

        return false;
    }

    Vector3 ScreenDeltaToWorldDelta(Vector2 delta)
    {
        float worldPerPixel = (2f * mainCamera.orthographicSize) / Screen.height;
        return new Vector3(delta.x * worldPerPixel, delta.y * worldPerPixel, 0f);
    }

    IEnumerator ZoomSmooth(float targetZoom, Vector3 targetPosition)
    {
        float elapsed = 0f;
        float startZoom = mainCamera.orthographicSize;
        Vector3 startPosition = transform.position;

        while (elapsed < doubleTapZoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / doubleTapZoomDuration);
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            screenBoundariesScript.RecalculateBounds();
            transform.position =
                screenBoundariesScript.GetClampedCameraPosition(transform.position);

            yield return null;
        }

        mainCamera.orthographicSize = targetZoom;
        screenBoundariesScript.RecalculateBounds();
        transform.position =
            screenBoundariesScript.GetClampedCameraPosition(transform.position);
        zoomRoutine = null;
    }

    void UpdateMaxZoom()
    {
        if(screenBoundariesScript == null || mainCamera == null)
        {
            return;
        }

        Rect wb = screenBoundariesScript.worldBounds;

        float maxZoomByHeight = wb.height / 2f;
        float maxZoomByWidth = (wb.width / 2f) / mainCamera.aspect;
        maxZoom = Mathf.Min(maxZoomByHeight, maxZoomByWidth);
    }
}

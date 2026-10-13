using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("References")]
    public ScreenBoundariesScript screenBoundariesScript;

    public Camera mainCamera;
    Vector2 lastTouchPos;
    int panFingerId = -1;
    bool isTouchPan = false;

    float lastTapTime = 0f;
    public float doubleTapMaxDelay = 0.4f;
    public float doubleTapMaxDistance = 100f;


    void Awake()
    {
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

        if (ObjectTransformationScript.isTransforming)
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        DesktopFollowCursor();
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if ((Mathf.Abs(scroll) > Mathf.Epsilon))
        {
            mainCamera.orthographicSize -= scroll * mouseScrollSpeed;
        }
#else
    HandleTouch();
#endif
        if (Input.touchCount == 2)
            HandlePinch();

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
        if (Input.touchCount != 1)
            return;

        // While a car is dragged, the camera follows the finger (like the cursor on desktop)
        // so the car can be carried across the map on a portrait screen.
        if (GameObjectsScript.isDragging)
        {
            isTouchPan = false;
            panFingerId = -1;
            FollowScreenPoint(Input.GetTouch(0).position);
            return;
        }

        Touch touch = Input.GetTouch(0);
        if (IsTouchUIButton(touch.position))
        {
            return;
        }

        if(touch.phase == TouchPhase.Began)
        {
            float dt = Time.time - lastTapTime;
            if (dt <= doubleTapMaxDelay &&
                Vector2.Distance(touch.position, lastTouchPos) <= doubleTapMaxDistance)
            {
                StartCoroutine(ResetZoomSmooth());
                lastTapTime = 0f;

            }else{
                lastTapTime = Time.time;
            }

            lastTouchPos = touch.position;
            panFingerId = touch.fingerId;
            isTouchPan = true;

            
        }else if (touch.phase == TouchPhase.Moved && isTouchPan &&
            touch.fingerId == panFingerId)
        {
            Vector2 delta = touch.position - lastTouchPos;
            transform.Translate(ScreenDeltaToWorldDelta(delta) * touchPanSpeed, Space.World);
            lastTouchPos = touch.position;
        }else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isTouchPan = false;
            panFingerId = -1;
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

    bool IsTouchUIButton(Vector2 touchPosition)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = touchPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach(RaycastResult result in results)
        {
            if(result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
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

    IEnumerator ResetZoomSmooth()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float currentZoom = mainCamera.orthographicSize;
        float targetZoom = maxZoom;

        while(elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            mainCamera.orthographicSize =
                Mathf.Lerp(currentZoom, targetZoom, elapsed / duration);
            screenBoundariesScript.RecalculateBounds();
            transform.position = 
                screenBoundariesScript.GetClampedCameraPosition(transform.position);

            yield return null;
        }

        mainCamera.orthographicSize = targetZoom;
        screenBoundariesScript.RecalculateBounds();
        transform.position =
            screenBoundariesScript.GetClampedCameraPosition(transform.position);
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
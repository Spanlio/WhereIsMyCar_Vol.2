using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObjectsScript gameObjectsScript;
    private CanvasGroup canvasGroup;
    public ScreenBoundariesScript screenBoundariesScript;

    private Vector3 dragOffsetWorld;
    private Camera mainCamera;
    private Canvas canvas;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if(gameObjectsScript == null)
        {
            gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        }

        if(screenBoundariesScript == null)
        {
            screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();
        }

        canvas = GetComponentInParent<Canvas>();
        if(canvas != null)
        {
            mainCamera = canvas.worldCamera;
        }
        else
        {
            Debug.LogWarning("Canvas not found in parent hierarchy of " + gameObject.name);
        }


    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked on " + gameObject.name);

        PlayDragSound();
    }

    private void PlayDragSound()
    {
        if (gameObjectsScript != null &&
            gameObjectsScript.carSoundSource != null &&
            gameObjectsScript.sounds != null &&
            gameObjectsScript.sounds.Length > 0 &&
            gameObjectsScript.sounds[0] != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameObjectsScript != null && gameObjectsScript.GameFinished)
            return;

        Debug.Log("Start drag!");
        GameObjectsScript.isDragging = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        int lastIndex = transform.parent.childCount - 1;
        int positionIndex = Mathf.Max(0, lastIndex - 1);
        transform.SetSiblingIndex(positionIndex);

        if(ScreenToWorldPoint(eventData.position, out Vector3 cursorWorldPos))
        {
            dragOffsetWorld = transform.position - cursorWorldPos;
        }
        else
        {
            dragOffsetWorld = Vector3.zero;
        }

        GameObjectsScript.lastDragged = eventData.pointerDrag;
    }

    private bool ScreenToWorldPoint(Vector2 screenPosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if(mainCamera == null)
        {
            Debug.LogWarning("Main camera is not assigned.");
            return false;
        }
        float z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, z);
        worldPosition = mainCamera.ScreenToWorldPoint(screenPoint);
        return true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (gameObjectsScript != null && gameObjectsScript.GameFinished)
            return;

        Vector3 pointerWorld;
        if(!ScreenToWorldPoint(eventData.position, out pointerWorld))
        {
            return;
        }
        Vector3 desired = pointerWorld + dragOffsetWorld;
        desired.z = transform.position.z;

        if (screenBoundariesScript != null)
        {
            screenBoundariesScript.RecalculateBounds();
            Vector2 clamped = screenBoundariesScript.GetClampedPosition(desired);
            desired.x = clamped.x;
            desired.y = clamped.y;
        }

        transform.position = desired;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PlayDragSound();
        GameObjectsScript.isDragging = false;
        Debug.Log("OnEndDrag called for " + gameObject.name);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (gameObjectsScript.inRightPlace)
        {
            canvasGroup.blocksRaycasts = false;
            GameObjectsScript.lastDragged = null;
        }
        // Otherwise the dropped car stays selected (lastDragged) so the transform
        // buttons still work on it. isDragging is false, so it can't collide.

        gameObjectsScript.inRightPlace = false;
    }

    void Update()
    {
        if (gameObjectsScript == null || !gameObjectsScript.GameFinished)
            return;

        // If the game ends during a drag, immediately make this car inert.
        if (GameObjectsScript.lastDragged == gameObject)
        {
            GameObjectsScript.lastDragged = null;
            GameObjectsScript.isDragging = false;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
    }
}

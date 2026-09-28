using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObjectsScript gameObjectsScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    public ScreenBoundariesScript screenBoundariesScript;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameObjectsScript == null || gameObjectsScript.GameFinished)
            return;

        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            Debug.Log("Left mouse button clicked on " + gameObject.name);

            if (gameObjectsScript.carSoundSource != null &&
                gameObjectsScript.sounds != null &&
                gameObjectsScript.sounds.Length > 0 &&
                gameObjectsScript.sounds[0] != null)
            {
                gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameObjectsScript == null || gameObjectsScript.GameFinished)
            return;

        if (!Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            return;

        GameObjectsScript.isDragging = true;
        GameObjectsScript.lastDragged = gameObject;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        int lastIndex = transform.parent.childCount - 1;
        int positionIndex = Mathf.Max(0, lastIndex - 1);
        transform.SetSiblingIndex(positionIndex);

        Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            screenBoundariesScript.screenPoint.z));

        rectTransform.position = cursorWorldPos;

        screenBoundariesScript.screenPoint =
            Camera.main.WorldToScreenPoint(rectTransform.localPosition);

        screenBoundariesScript.offset =
            rectTransform.localPosition -
            Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                screenBoundariesScript.screenPoint.z));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (gameObjectsScript == null || gameObjectsScript.GameFinished)
            return;

        if (!GameObjectsScript.isDragging ||
            GameObjectsScript.lastDragged != gameObject)
            return;

        if (!Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            return;

        Vector3 cursScreenPoint = new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            screenBoundariesScript.screenPoint.z);

        Vector3 curPosition =
            Camera.main.ScreenToWorldPoint(cursScreenPoint) +
            screenBoundariesScript.offset;

        rectTransform.position =
            screenBoundariesScript.GetClampedPosition(curPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (gameObjectsScript == null)
            return;

        // A bomb may have already ended this drag.
        if (GameObjectsScript.lastDragged != gameObject &&
            !GameObjectsScript.isDragging)
            return;

        GameObjectsScript.isDragging = false;

        Debug.Log("OnEndDrag called for " + gameObject.name);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (gameObjectsScript.inRightPlace)
        {
            canvasGroup.blocksRaycasts = false;
            GameObjectsScript.lastDragged = null;
        }
        else if (!gameObjectsScript.GameFinished)
        {
            GameObjectsScript.lastDragged = null;
        }

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
